using System.Reflection;
using Asp.Versioning;
using AutoMapper;
using Jobs.Common.Contracts;
using Jobs.Common.Extentions;
using Jobs.Common.Helpers;
using Jobs.Common.Options;
using Jobs.Core.Contracts;
using Jobs.Core.Contracts.Providers;
using Jobs.Core.Extentions;
using Jobs.Core.Middleware;
using Jobs.Core.Observability.Options;
using Jobs.Core.Providers;
using Jobs.Core.Services;
using Jobs.Entities.DTO;
using Jobs.Entities.Models;
using Jobs.VacancyApi.Contracts;
using Jobs.VacancyApi.Data;
using Jobs.VacancyApi.Features.Commands;
using Jobs.VacancyApi.Features.Notifications;
using Jobs.VacancyApi.Features.Queries;
using Jobs.VacancyApi.Middleware;
using Jobs.VacancyApi.Repository;
using Jobs.VacancyApi.Services;
using MediatR;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    builder.Host.UseSerilog((context, loggerConfiguration) =>
    {
        loggerConfiguration.WriteTo.Console();
        loggerConfiguration.ReadFrom.Configuration(context.Configuration);
    });
    
    Log.Information("Starting WebApi Vacancy Service.");

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddApiVersionService();

    builder.Services.AddDbContext<JobsDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    
    CryptOptions cryptOptions = new();

    builder.Configuration
        .GetRequiredSection(nameof(CryptOptions))
        .Bind(cryptOptions);

    builder.Services.AddScoped<IGenericRepository<Vacancy>, VacancyRepository>();
    builder.Services.AddScoped<IGenericRepository<WorkType>, WorkTypeRepository>();
    builder.Services.AddScoped<IGenericRepository<EmploymentType>, EmploymentTypeRepository>();
    builder.Services.AddScoped<IGenericRepository<Category>, CategoryRepository>();
    builder.Services.AddScoped<IMiniGenericRepository<VacancyWorkTypes>, VacancyWorkTypesRepository>();
    builder.Services.AddScoped<IMiniGenericRepository<VacancyEmploymentTypes>, VacancyEmploymentTypesRepository>();
    
    
    builder.Services.AddScoped<IApiKeyStorageServiceProvider, MemoryApiKeyStorageServiceProvider>();
    builder.Services.AddScoped<ISecretApiKeyRepository, SecretApiKeyRepository>();
    builder.Services.AddScoped<IProcessingService, ProcessingService>();
    builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
    builder.Services.AddScoped<IEncryptionService, NaiveEncryptionService>(p => 
        p.ResolveWith<NaiveEncryptionService>(Convert.FromBase64String(cryptOptions.PKey), Convert.FromBase64String(cryptOptions.IV)));
    builder.Services.AddScoped<ISignedNonceService, SignedNonceService>();
    

//builder.Services.AddAutoMapper(Assembly.GetEntryAssembly()); // AutoMapper registration
    builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly()); // AutoMapper registration
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.WriteIndented = true;
        options.SerializerOptions.IncludeFields = true;
    });

    //builder.Services.AddRateLimiterService();
    builder.Services.AddWindowRateLimiterService();
    
    ObservabilityOptions observabilityOptions = new();

    builder.Configuration
        .GetRequiredSection(nameof(ObservabilityOptions))
        .Bind(observabilityOptions);
    
    builder.AddSerilog(observabilityOptions);
    builder.Services
        .AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService(observabilityOptions.ServiceName))
        .AddMetrics(observabilityOptions)
        .AddTracing(observabilityOptions);
    /*
    builder.Services.AddOpenTelemetry().WithMetrics(opts => opts
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
    );
    
    builder.Services.AddOpenTelemetry().WithMetrics(opts => opts
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("JobStore.WebApi"))
        .AddMeter(builder.Configuration.GetValue<string>("JobsStoreMeterName"))
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddOtlpExporter(opts =>
        {
            opts.Endpoint = new Uri(builder.Configuration["Otlp:Endpoint"]);
        })); */
    
    // forward headers configuration for reverse proxy
    builder.Services.Configure<ForwardedHeadersOptions>(options => {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    builder.Services.AddResponseCompressionService();
    
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(build => {
            build.WithOrigins("https://localhost:7111","http://localhost:5206");
            build.AllowAnyMethod();
            //builder.WithMethods("GET", "POST", "PUT", "DELETE");
            build.AllowAnyHeader();
        });
    });

    var app = builder.Build();
    
    app.UseResponseCompression();
    app.UseForwardedHeaders();
    app.UseMiddleware<AdminSafeListMiddleware>(builder.Configuration["AdminSafeList"]);
    
// Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

// Get the Automapper, we can share this too
    var mapper = app.Services.GetService<IMapper>();
    if (mapper == null)
    {
        throw new InvalidOperationException("Mapper not found");
    }

//app.UseLogHeaders(); // add here right after you create app
    app.UseExceptionHandlers();
//app.UseSecurityHeaders();

//app.UseErrorHandler(); // add here right after you create app

    if (!builder.Environment.IsDevelopment())
    {
        builder.Services.AddHttpsRedirection(options =>
        {
            options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
            options.HttpsPort = 443;
        });
    }
    
    app.UseHttpsRedirection();
    
    app.UseStaticFiles(); // 🔴 here it is
    app.UseRouting(); // 🔴 here it is

    app.UseCors();
    /*app.UseCors(options =>
        options
            .WithOrigins("https://localhost:7111","http://localhost:5206")
            .AllowAnyMethod()
            .AllowAnyHeader());*/
    
//app.UseMiddleware<ErrorHandlerMiddleware>(); 
    // Enable compression
    

    app.UseRateLimiter();

// Ensure database is created during application startup
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<JobsDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

// Request Logging Middleware
    app.Use(async (context, next) =>
    {
        try
        {
            var key = context.Request.Headers["x-api-key"];
            var nonce = context.Request.Headers["s-nonce"];
            Log.Information($"Incoming Request: {context.Request.Protocol} {context.Request.Method} {context.Request.Path}{context.Request.QueryString}");
            Log.Information($"Key - {key}, Nonce - {nonce}");
        }
        catch (Exception)
        {
            Log.Information($"Api Key or Nonce not found in Header.");
        }
        
        await next();
    });

    app.Use(async (context, next) =>
    {
        context.Response.OnStarting(async () =>
        {
            using var scope = app.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IApiKeyService>();
            var cryptService = scope.ServiceProvider.GetRequiredService<IEncryptionService>();

            if (context.Response.StatusCode == StatusCodes.Status200OK || 
                context.Response.StatusCode == StatusCodes.Status201Created ||
                context.Response.StatusCode == StatusCodes.Status204NoContent
               )
            {
                var apiKey = await service.GenerateApiKeyAsync();
                var cryptedApiKey = cryptService.Encrypt(apiKey.Key);
                context.Response.Headers.Append("x-api-key", cryptedApiKey);
            }
        });
        await next.Invoke();
    });

    var version1 = new ApiVersion(1);

    var apiVersionSet = app.NewApiVersionSet()
        .HasApiVersion(version1)
        //.HasApiVersion(new ApiVersion(2))
        .ReportApiVersions()
        .Build();
    
    app.MapGet("api/v{version:apiVersion}/categories", async (HttpContext context, 
            [FromServices] ISender mediatr, 
            [FromServices] IApiKeyService service, 
            [FromServices] IEncryptionService cryptService,
            [FromServices] ISignedNonceService signedNonceService,
            [FromHeader(Name = "x-api-key")] string apiKey,
            [FromHeader(Name = "s-nonce")] string signedNonce,
            [FromHeader(Name = "x-api-secret")] string apiSecret) =>
        {
            var (longNonce ,resultParse) = signedNonceService.IsSignedNonceValid(signedNonce);

            if (builder.Environment.IsDevelopment())
            {
                longNonce = DateTime.UtcNow.Ticks;
            }

            if (!resultParse)
            {
                return Results.BadRequest();
            }
            
            // apiKey must be in Base64
            var realApiKey = cryptService.Decrypt(apiKey);
            var realApiSecret = cryptService.Decrypt(apiSecret);
 
            if (!service.IsValid(realApiKey, longNonce, realApiSecret))
            {
                return Results.BadRequest();
            }
            
            var ipAddress = context.Request.GetIpAddress();
            Log.Information($"ClientIPAddress - {ipAddress}.");
            
            var items = await mediatr.Send(new ListCategoriesQuery());
            return Results.Ok(items);
        }).WithName("GetCategories")
        .MapApiVersion(apiVersionSet, version1)
        .RequireRateLimiting("FixedWindow")
        .WithOpenApi();
    
    app.MapGet("api/v{version:apiVersion}/employmenttypes", async (HttpContext context, 
            [FromServices] ISender mediatr, 
            [FromServices] IApiKeyService service, 
            [FromServices] IEncryptionService cryptService,
            [FromServices] ISignedNonceService signedNonceService,
            [FromHeader(Name = "x-api-key")] string apiKey,
            [FromHeader(Name = "s-nonce")] string signedNonce,
            [FromHeader(Name = "x-api-secret")] string apiSecret) =>
        {
            var (longNonce ,resultParse) = signedNonceService.IsSignedNonceValid(signedNonce);

            if (builder.Environment.IsDevelopment())
            {
                longNonce = DateTime.UtcNow.Ticks;
            }

            if (!resultParse)
            {
                return Results.BadRequest();
            }
            
            // apiKey must be in Base64
            var realApiKey = cryptService.Decrypt(apiKey);
            var realApiSecret = cryptService.Decrypt(apiSecret);
 
            if (!service.IsValid(realApiKey, longNonce, realApiSecret))
            {
                return Results.BadRequest();
            }
            
            var ipAddress = context.Request.GetIpAddress();
            Log.Information($"ClientIPAddress - {ipAddress}.");
            
            var items = await mediatr.Send(new ListEmploymentTypesQuery());
            return Results.Ok(items);
        }).WithName("GetEmploymentTypes")
        .MapApiVersion(apiVersionSet, version1)
        .RequireRateLimiting("FixedWindow")
        .WithOpenApi();
    
    app.MapGet("api/v{version:apiVersion}/employmenttypes/{id:int}", async (int id, 
            HttpContext context, 
            [FromServices] ISender mediatr, 
            [FromServices] IApiKeyService service, 
            [FromServices] IEncryptionService cryptService,
            [FromServices] ISignedNonceService signedNonceService,
            [FromHeader(Name = "x-api-key")] string apiKey,
            [FromHeader(Name = "s-nonce")] string signedNonce,
            [FromHeader(Name = "x-api-secret")] string apiSecret) =>
        {
            var (longNonce ,resultParse) = signedNonceService.IsSignedNonceValid(signedNonce);

            if (builder.Environment.IsDevelopment())
            {
                longNonce = DateTime.UtcNow.Ticks;
            }

            if (!resultParse)
            {
                return Results.BadRequest();
            }
            
            if (id <= 0)
            {
                return Results.BadRequest();
            }
            
            // apiKey must be in Base64
            var realApiKey = cryptService.Decrypt(apiKey);
            var realApiSecret = cryptService.Decrypt(apiSecret);
 
            if (!service.IsValid(realApiKey, longNonce, realApiSecret))
            {
                return Results.BadRequest();
            }
            
            var ipAddress = context.Request.GetIpAddress();
            Log.Information($"ClientIPAddress - {ipAddress}.");
            
            var result = await mediatr.Send(new GetEmploymentTypeQuery(id));
            return result != null ? Results.Ok(result) : Results.NotFound();
        }).WithName("GetEmploymentType")
        .MapApiVersion(apiVersionSet, version1)
        .RequireRateLimiting("FixedWindow")
        .WithOpenApi();
    
    app.MapGet("api/v{version:apiVersion}/worktypes", async (HttpContext context, 
            [FromServices] ISender mediatr, 
            [FromServices] IApiKeyService service, 
            [FromServices] IEncryptionService cryptService,
            [FromServices] ISignedNonceService signedNonceService,
            [FromHeader(Name = "x-api-key")] string apiKey,
            [FromHeader(Name = "s-nonce")] string signedNonce,
            [FromHeader(Name = "x-api-secret")] string apiSecret) =>
        {
            var (longNonce ,resultParse) = signedNonceService.IsSignedNonceValid(signedNonce);

            if (builder.Environment.IsDevelopment())
            {
                longNonce = DateTime.UtcNow.Ticks;
            }

            if (!resultParse)
            {
                return Results.BadRequest();
            }
            
            // apiKey must be in Base64
            var realApiKey = cryptService.Decrypt(apiKey);
            var realApiSecret = cryptService.Decrypt(apiSecret);
 
            if (!service.IsValid(realApiKey, longNonce, realApiSecret))
            {
                return Results.BadRequest();
            }
            
            var ipAddress = context.Request.GetIpAddress();
            Log.Information($"ClientIPAddress - {ipAddress}.");
            
            var items = await mediatr.Send(new ListWorkTypesQuery());
            return Results.Ok(items);
        }).WithName("GetWorkTypes")
        .MapApiVersion(apiVersionSet, version1)
        .RequireRateLimiting("FixedWindow")
        .WithOpenApi();
    
    app.MapGet("api/v{version:apiVersion}/worktypes/{id:int}", async (int id, 
            HttpContext context, 
            [FromServices] ISender mediatr, 
            [FromServices] IApiKeyService service, 
            [FromServices] IEncryptionService cryptService,
            [FromServices] ISignedNonceService signedNonceService,
            [FromHeader(Name = "x-api-key")] string apiKey,
            [FromHeader(Name = "s-nonce")] string signedNonce,
            [FromHeader(Name = "x-api-secret")] string apiSecret) =>
        {
            var (longNonce ,resultParse) = signedNonceService.IsSignedNonceValid(signedNonce);

            if (builder.Environment.IsDevelopment())
            {
                longNonce = DateTime.UtcNow.Ticks;
            }

            if (!resultParse)
            {
                return Results.BadRequest();
            }
            
            if (id <= 0)
            {
                return Results.BadRequest();
            }
            
            // apiKey must be in Base64
            var realApiKey = cryptService.Decrypt(apiKey);
            var realApiSecret = cryptService.Decrypt(apiSecret);
 
            if (!service.IsValid(realApiKey, longNonce, realApiSecret))
            {
                return Results.BadRequest();
            }
            
            var ipAddress = context.Request.GetIpAddress();
            Log.Information($"ClientIPAddress - {ipAddress}.");
            
            var result = await mediatr.Send(new GetWorkTypeQuery(id));
            return result != null ? Results.Ok(result) : Results.NotFound();
        }).WithName("GetWorkType")
        .MapApiVersion(apiVersionSet, version1)
        .RequireRateLimiting("FixedWindow")
        .WithOpenApi();

//api/v{version:apiVersion}
    app.MapGet("api/v{version:apiVersion}/vacancies", async (HttpContext context, 
            [FromServices] ISender mediatr, 
            [FromServices] IApiKeyService service, 
            [FromServices] IEncryptionService cryptService,
            [FromServices] ISignedNonceService signedNonceService,
            [FromHeader(Name = "x-api-key")] string apiKey,
            [FromHeader(Name = "s-nonce")] string signedNonce,
            [FromHeader(Name = "x-api-secret")] string apiSecret) =>
        {
            var (longNonce ,resultParse) = signedNonceService.IsSignedNonceValid(signedNonce);

            if (builder.Environment.IsDevelopment())
            {
                longNonce = DateTime.UtcNow.Ticks;
            }

            if (!resultParse)
            {
                return Results.BadRequest();
            }
            
            // apiKey must be in Base64
            var realApiKey = cryptService.Decrypt(apiKey);
            var realApiSecret = cryptService.Decrypt(apiSecret);
 
            if (!service.IsValid(realApiKey, longNonce, realApiSecret))
            {
                return Results.BadRequest();
            }
            
            var ipAddress = context.Request.GetIpAddress();
            Log.Information($"ClientIPAddress - {ipAddress}.");
            
            var products = await mediatr.Send(new ListVacanciesQuery());
            return Results.Ok(products);
        }).WithName("GetVacancies")
        .MapApiVersion(apiVersionSet, version1)
        .RequireRateLimiting("FixedWindow")
        .WithOpenApi();

    app.MapGet("api/v{version:apiVersion}/vacancies/{id:int}", async (int id, 
            [FromServices] ISender mediatr,
            [FromServices] IApiKeyService service, 
            [FromServices] IEncryptionService cryptService,
            [FromServices] ISignedNonceService signedNonceService,
            [FromHeader(Name = "x-api-key")] string apiKey,
            [FromHeader(Name = "s-nonce")] string signedNonce,
            [FromHeader(Name = "x-api-secret")] string apiSecret) =>
        {
            var (longNonce ,resultParse) = signedNonceService.IsSignedNonceValid(signedNonce);

            if (builder.Environment.IsDevelopment())
            {
                longNonce = DateTime.UtcNow.Ticks;
            }

            if (!resultParse)
            {
                return Results.BadRequest();
            }
            
            // apiKey must be in Base64
            var realApiKey = cryptService.Decrypt(apiKey);
            var realApiSecret = cryptService.Decrypt(apiSecret);
 
            if (!service.IsValid(realApiKey, longNonce, realApiSecret))
            {
                return Results.BadRequest();
            }

            if (id <= 0)
            {
                return Results.BadRequest();
            }

            var vacancy = await mediatr.Send(new GetVacancyQuery(id));
            return vacancy == null ? Results.NotFound() : Results.Ok(vacancy);
        }).WithName("GetVacancy")
        .MapApiVersion(apiVersionSet, version1)
        .RequireRateLimiting("FixedWindow")
        .WithOpenApi();
    
    VacancyInDto SanitizeVacancyInDto(VacancyInDto entity) => entity with { 
        VacancyTitle = HtmlSanitizerHelper.Sanitize(entity.VacancyTitle), 
        VacancyDescription = HtmlSanitizerHelper.Sanitize(entity.VacancyDescription) 
    };
    
    app.MapPost("api/v{version:apiVersion}/vacancies", async (VacancyInDto vacancy,
            [FromServices] IApiKeyService service, 
            [FromServices] IEncryptionService cryptService,
            [FromServices] ISender mediatr, 
            [FromServices] IPublisher publisher, 
            [FromServices] ISignedNonceService signedNonceService,
            [FromHeader(Name = "x-api-key")] string apiKey,
            [FromHeader(Name = "s-nonce")] string signedNonce,
            [FromHeader(Name = "x-api-secret")] string apiSecret) =>
        {
            var (longNonce ,resultParse) = signedNonceService.IsSignedNonceValid(signedNonce);

            if (builder.Environment.IsDevelopment())
            {
                longNonce = DateTime.UtcNow.Ticks;
            }

            if (!resultParse)
            {
                return Results.BadRequest();
            }
            
            // apiKey must be in Base64
            var realApiKey = cryptService.Decrypt(apiKey);
            var realApiSecret = cryptService.Decrypt(apiSecret);
 
            if (!service.IsValid(realApiKey, longNonce, realApiSecret))
            {
                return Results.BadRequest();
            }
            
            if (vacancy.VacancyId != 0)
            {
                return Results.BadRequest();
            }
            
            if (!vacancy.IsValid())
            {
                return Results.BadRequest();
            }

            var sanitized = SanitizeVacancyInDto(vacancy);

            var result = await mediatr.Send(new CreateVacancyCommand(sanitized));

            if (0 == result.VacancyId) return Results.BadRequest();
            
            await publisher.Publish(new VacancyCreatedNotification(result.VacancyId));

            return Results.Created($"/vacancies/{result.VacancyId}", result);
        }).WithName("AddVacancy")
        .MapApiVersion(apiVersionSet, version1)
        .RequireRateLimiting("FixedWindow")
        .WithOpenApi();
    
    app.MapPut("api/v{version:apiVersion}/vacancies/{id:int}", async (int id, 
            [FromBody] VacancyInDto vacancy, 
            [FromServices] ISender mediatr,
            [FromServices] IApiKeyService service, 
            [FromServices] IEncryptionService cryptService,
            [FromServices] ISignedNonceService signedNonceService,
            [FromHeader(Name = "x-api-key")] string apiKey,
            [FromHeader(Name = "s-nonce")] string signedNonce,
            [FromHeader(Name = "x-api-secret")] string apiSecret) =>
        {
            var (longNonce ,resultParse) = signedNonceService.IsSignedNonceValid(signedNonce);

            if (builder.Environment.IsDevelopment())
            {
                longNonce = DateTime.UtcNow.Ticks;
            }

            if (!resultParse)
            {
                return Results.BadRequest();
            }
            
            // apiKey must be in Base64
            var realApiKey = cryptService.Decrypt(apiKey);
            var realApiSecret = cryptService.Decrypt(apiSecret);
 
            if (!service.IsValid(realApiKey, longNonce, realApiSecret))
            {
                return Results.BadRequest();
            }

            if (id != vacancy.VacancyId || id <= 0)
            {
                return Results.BadRequest();
            }

            if (!vacancy.IsValid())
            {
                return Results.BadRequest();
            }
            
            var sanitized = SanitizeVacancyInDto(vacancy);

            var result = await mediatr.Send(new UpdateVacancyCommand(sanitized));

            return result > 0 ? Results.NoContent() : Results.NotFound();
        }).WithName("ChangeVacancy")
        .MapApiVersion(apiVersionSet, version1)
        .RequireRateLimiting("FixedWindow")
        .WithOpenApi();

    app.MapDelete("api/v{version:apiVersion}/vacancies/{id:int}", async (int id, 
            [FromServices] ISender mediatr,
            [FromServices] IApiKeyService service, 
            [FromServices] IEncryptionService cryptService,
            [FromServices] ISignedNonceService signedNonceService,
            [FromHeader(Name = "x-api-key")] string apiKey,
            [FromHeader(Name = "s-nonce")] string signedNonce,
            [FromHeader(Name = "x-api-secret")] string apiSecret) =>
        {
            var (longNonce ,resultParse) = signedNonceService.IsSignedNonceValid(signedNonce);

            if (builder.Environment.IsDevelopment())
            {
                longNonce = DateTime.UtcNow.Ticks;
            }

            if (!resultParse)
            {
                return Results.BadRequest();
            }
            
            // apiKey must be in Base64
            var realApiKey = cryptService.Decrypt(apiKey);
            var realApiSecret = cryptService.Decrypt(apiSecret);
 
            if (!service.IsValid(realApiKey, longNonce, realApiSecret))
            {
                return Results.BadRequest();
            }

            if (id <= 0)
            {
                return Results.BadRequest();
            }

            var result = await mediatr.Send(new DeleteVacancyCommand(id));
            return result == -1 ? Results.NotFound() : Results.NoContent();
        }).WithName("RemoveVacancy")
        .MapApiVersion(apiVersionSet, version1)
        .RequireRateLimiting("FixedWindow")
        .WithOpenApi();
    
    app.UseSerilogRequestLogging();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "server terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
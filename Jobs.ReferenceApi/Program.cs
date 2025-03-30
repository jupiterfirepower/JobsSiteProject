using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Asp.Versioning;
using AutoMapper;
using Jobs.Common.Constants;
using Jobs.Common.Contracts;
using Jobs.Common.Extentions;
using Jobs.Common.Helpers;
using Jobs.Common.Options;
using Jobs.Core.Contracts;
using Jobs.Core.Contracts.Providers;
using Jobs.Core.Extentions;
using Jobs.Core.Filters;
using Jobs.Core.Handlers;
using Jobs.Core.Managers;
using Jobs.Core.Middleware;
using Jobs.Core.Observability.Options;
using Jobs.Core.Providers;
using Jobs.Core.Services;
using Jobs.DTO;
using Jobs.DTO.In;
using Jobs.Entities.Models;
using Jobs.ReferenceApi.Contracts;
using Jobs.ReferenceApi.Data;
using Jobs.ReferenceApi.Features.Queries;
using Jobs.ReferenceApi.Repositories;
using Jobs.ReferenceApi.Services;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
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
    
    Log.Information("Starting WebApi Reference Service.");

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddApiVersionService();

    builder.Services.AddDbContext<JobsDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    
    //var vacancySecretKey = builder.Configuration["VacancyApiService:SecretKey"];
    var referenceSecretKey = "12345678910111213141151617";
    
    CryptOptions cryptOptions = new();

    builder.Configuration
        .GetRequiredSection(nameof(CryptOptions))
        .Bind(cryptOptions);
    
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<ICacheService, LocalCacheService>();
    
    builder.Services.AddScoped<IGenericRepository<WorkType>, WorkTypeRepository>();
    builder.Services.AddScoped<IGenericRepository<EmploymentType>, EmploymentTypeRepository>();
    builder.Services.AddScoped<IGenericRepository<Category>, CategoryRepository>();
    builder.Services.AddScoped<IApiKeyStorageServiceProvider, MemoryApiKeyStorageServiceProvider>();
    builder.Services.AddScoped<IApiKeyManagerServiceProvider, ApiKeyManagerServiceProvider>();
    builder.Services.AddScoped<ISecretApiKeyRepository, SecretApiKeyRepository>();
    builder.Services.AddScoped<IProcessingService, ProcessingService>();
    builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
    builder.Services.AddScoped<IEncryptionService, NaiveEncryptionService>(p => 
        p.ResolveWith<NaiveEncryptionService>(Convert.FromBase64String(cryptOptions.PKey), Convert.FromBase64String(cryptOptions.IV)));
    builder.Services.AddScoped<ISignedNonceService, SignedNonceService>();
    builder.Services.AddScoped<ISecretApiService, SecretApiService>(p => p.ResolveWith<SecretApiService>(referenceSecretKey));
    

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
    
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();
    
    /*builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", settings =>
            settings.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
    });*/
    
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(build => {
            build.WithOrigins("https://localhost:7006");
            build.AllowAnyMethod();
            build.AllowAnyHeader();
        });
    });

    var app = builder.Build();
    
    app.UseResponseCompression();
    app.UseForwardedHeaders();
    app.UseMiddleware<AdminSafeListMiddleware>(builder.Configuration["HostsSafeList"]);
    
    // Global Exception Handler.
    app.UseExceptionHandler();
    
   
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


   // app.UseExceptionHandlers();
   
   
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

    //app.UseCors("AllowAll");
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
            var key = context.Request.Headers[HttpHeaderKeys.XApiHeaderKey];
            var nonce = context.Request.Headers[HttpHeaderKeys.SNonceHeaderKey];
            var secret = context.Request.Headers[HttpHeaderKeys.XApiSecretHeaderKey];
            Log.Information($"Incoming Request: {context.Request.Protocol} {context.Request.Method} {context.Request.Path}{context.Request.QueryString}");
            Log.Information($"Key - {key}, Nonce - {nonce}, Secret - {secret}");
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
                var cryptApiKey = cryptService.Encrypt(apiKey.Key);
                context.Response.Headers.Append(HttpHeaderKeys.XApiHeaderKey, cryptApiKey);
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
    
    bool IsBadRequest(IHttpContextAccessor httpContextAccessor, 
        IEncryptionService cryptService,
        ISignedNonceService signedNonceService,
        IApiKeyService service,
        string apiKey, 
        string signedNonce,
        string apiSecret)
    {
        if (!UserAgentConstant.AppUserAgent.Equals(httpContextAccessor.HttpContext?.Request.Headers.UserAgent))
        {
            return true;
        }
        
        var (longNonce , resultParse) = signedNonceService.IsSignedNonceValid(signedNonce);

        if (builder.Environment.IsDevelopment())
        {
            longNonce = DateTime.UtcNow.Ticks;
        }

        if (!resultParse)
        {
            return true;
        }
            
        // apiKey must be in Base64
        var realApiKey = cryptService.Decrypt(apiKey);
        var realApiSecret = cryptService.Decrypt(apiSecret);
            
        if (!service.IsValid(realApiKey, longNonce, realApiSecret))
        {
            return true;
        }

        return false;
    }
   
    void Guards(ISender mediatr, IApiKeyService service,
        IEncryptionService cryptService, ISignedNonceService signedNonceService,
        IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(mediatr, nameof(mediatr));
        ArgumentNullException.ThrowIfNull(service, nameof(service));
        ArgumentNullException.ThrowIfNull(cryptService, nameof(cryptService));
        ArgumentNullException.ThrowIfNull(signedNonceService, nameof(signedNonceService));
        ArgumentNullException.ThrowIfNull(httpContextAccessor, nameof(httpContextAccessor));
    }
    
    // Create the group
    var group = app
        .MapGroup("dict-endpoints")
        .WithTags("DB Dictionary Endpoints");
    
    app.MapGet("api/v{version:apiVersion}/categories", async Task<Results<Ok<List<CategoryDto>>, BadRequest>> (HttpContext context, 
            [FromServices] ISender mediatr, 
            [FromServices] IApiKeyService service, 
            [FromServices] IEncryptionService cryptService,
            [FromServices] ISignedNonceService signedNonceService,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            [FromHeader(Name = HttpHeaderKeys.XApiHeaderKey), Required, 
             StringLength(HttpHeaderKeys.XApiHeaderKeyMaxLength, MinimumLength = HttpHeaderKeys.XApiHeaderKeyMinLength)] string apiKey,
            [FromHeader(Name = HttpHeaderKeys.SNonceHeaderKey), Required, 
             StringLength(HttpHeaderKeys.SNonceHeaderKeyMaxLength, MinimumLength = HttpHeaderKeys.SNonceHeaderKeyMinLength)] string signedNonce,
            [FromHeader(Name = HttpHeaderKeys.XApiSecretHeaderKey), Required, 
             StringLength(HttpHeaderKeys.XApiSecretHeaderKeyMaxLength, MinimumLength = HttpHeaderKeys.XApiSecretHeaderKeyMinLength)] string apiSecret) =>
        {
            Guards(mediatr, service, cryptService, signedNonceService, httpContextAccessor);
            Console.WriteLine("Start Get Categories.");
           
            if (IsBadRequest(httpContextAccessor, 
                    cryptService, signedNonceService, service, 
                    apiKey, signedNonce, apiSecret))
            {
                return TypedResults.BadRequest();
            }
            
            var ipAddress = context.Request.GetIpAddress();
            Log.Information($"ClientIPAddress - {ipAddress}.");
            
            var items = await mediatr.Send(new ListCategoriesQuery());
            return TypedResults.Ok(items);
        }).WithName("GetCategories")
        .MapApiVersion(apiVersionSet, version1)
        .RequireRateLimiting("FixedWindow")
        .WithOpenApi();
    
    app.MapGet("api/v{version:apiVersion}/emp-types", async Task<Results<Ok<List<EmploymentTypeDto>>, BadRequest>> (HttpContext context, 
            [FromServices] ISender mediatr, 
            [FromServices] IApiKeyService service, 
            [FromServices] IEncryptionService cryptService,
            [FromServices] ISignedNonceService signedNonceService,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            [FromHeader(Name = HttpHeaderKeys.XApiHeaderKey), Required, 
             StringLength(HttpHeaderKeys.XApiHeaderKeyMaxLength, MinimumLength = HttpHeaderKeys.XApiHeaderKeyMinLength)] string apiKey,
            [FromHeader(Name = HttpHeaderKeys.SNonceHeaderKey), Required, 
             StringLength(HttpHeaderKeys.SNonceHeaderKeyMaxLength, MinimumLength = HttpHeaderKeys.SNonceHeaderKeyMinLength)] string signedNonce,
            [FromHeader(Name = HttpHeaderKeys.XApiSecretHeaderKey), Required, 
             StringLength(HttpHeaderKeys.XApiSecretHeaderKeyMaxLength, MinimumLength = HttpHeaderKeys.XApiSecretHeaderKeyMinLength)] string apiSecret) =>
        {
            Guards(mediatr, service, cryptService, signedNonceService, httpContextAccessor);
            
            if (IsBadRequest(httpContextAccessor, 
                    cryptService, signedNonceService, service, 
                    apiKey, signedNonce, apiSecret))
            {
                return TypedResults.BadRequest();
            }
            
            var ipAddress = context.Request.GetIpAddress();
            Log.Information($"ClientIPAddress - {ipAddress}.");
            
            var items = await mediatr.Send(new ListEmploymentTypesQuery());
            return TypedResults.Ok(items);
        }).WithName("GetEmploymentTypes")
        .MapApiVersion(apiVersionSet, version1)
        .RequireRateLimiting("FixedWindow")
        .WithOpenApi();
    
    app.MapGet("api/v{version:apiVersion}/emp-types/{id:int}", async Task<Results<Ok<EmploymentTypeDto>, BadRequest, NotFound>> (int id, 
            HttpContext context, 
            [FromServices] ISender mediatr, 
            [FromServices] IApiKeyService service, 
            [FromServices] IEncryptionService cryptService,
            [FromServices] ISignedNonceService signedNonceService,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            [FromHeader(Name = HttpHeaderKeys.XApiHeaderKey), Required, 
             StringLength(HttpHeaderKeys.XApiHeaderKeyMaxLength, MinimumLength = HttpHeaderKeys.XApiHeaderKeyMinLength)] string apiKey,
            [FromHeader(Name = HttpHeaderKeys.SNonceHeaderKey), Required, 
             StringLength(HttpHeaderKeys.SNonceHeaderKeyMaxLength, MinimumLength = HttpHeaderKeys.SNonceHeaderKeyMinLength)] string signedNonce,
            [FromHeader(Name = HttpHeaderKeys.XApiSecretHeaderKey), Required, 
             StringLength(HttpHeaderKeys.XApiSecretHeaderKeyMaxLength, MinimumLength = HttpHeaderKeys.XApiSecretHeaderKeyMinLength)] string apiSecret) =>
        {
            Guards(mediatr, service, cryptService, signedNonceService, httpContextAccessor);
            
            if (IsBadRequest(httpContextAccessor, 
                    cryptService, signedNonceService, service, 
                    apiKey, signedNonce, apiSecret))
            {
                return TypedResults.BadRequest();
            }
            
            var ipAddress = context.Request.GetIpAddress();
            Log.Information($"ClientIPAddress - {ipAddress}.");
            
            var result = await mediatr.Send(new GetEmploymentTypeQuery(id));
            return result != null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .AddEndpointFilter(async (context, next) =>
        {
            var id = context.GetArgument<int>(0);
   
            if (id <= 0)
            {
                return TypedResults.BadRequest();
            }

            return await next(context);
        })
        .WithName("GetEmploymentType")
        .MapApiVersion(apiVersionSet, version1)
        .RequireRateLimiting("FixedWindow")
        .WithOpenApi();
    
    app.MapGet("api/v{version:apiVersion}/work-types", async Task<Results<Ok<List<WorkTypeDto>>, BadRequest>> (HttpContext context, 
            [FromServices] ISender mediatr, 
            [FromServices] IApiKeyService service, 
            [FromServices] IEncryptionService cryptService,
            [FromServices] ISignedNonceService signedNonceService,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            [FromHeader(Name = HttpHeaderKeys.XApiHeaderKey), Required, 
             StringLength(HttpHeaderKeys.XApiHeaderKeyMaxLength, MinimumLength = HttpHeaderKeys.XApiHeaderKeyMinLength)] string apiKey,
            [FromHeader(Name = HttpHeaderKeys.SNonceHeaderKey), Required, 
             StringLength(HttpHeaderKeys.SNonceHeaderKeyMaxLength, MinimumLength = HttpHeaderKeys.SNonceHeaderKeyMinLength)] string signedNonce,
            [FromHeader(Name = HttpHeaderKeys.XApiSecretHeaderKey), Required, 
             StringLength(HttpHeaderKeys.XApiSecretHeaderKeyMaxLength, MinimumLength = HttpHeaderKeys.XApiSecretHeaderKeyMinLength)] string apiSecret) =>
        {
            Guards(mediatr, service, cryptService, signedNonceService, httpContextAccessor);
            
            if (IsBadRequest(httpContextAccessor, 
                    cryptService, signedNonceService, service, 
                    apiKey, signedNonce, apiSecret))
            {
                return TypedResults.BadRequest();
            }
            
            var ipAddress = context.Request.GetIpAddress();
            Log.Information($"ClientIPAddress - {ipAddress}.");
            
            var items = await mediatr.Send(new ListWorkTypesQuery());
            return TypedResults.Ok(items);
        }).WithName("GetWorkTypes")
        .MapApiVersion(apiVersionSet, version1)
        .RequireRateLimiting("FixedWindow")
        .WithOpenApi();
    
    app.MapGet("api/v{version:apiVersion}/work-types/{id:int}", async Task<Results<Ok<WorkTypeDto>, BadRequest, NotFound>> (int id, 
            HttpContext context, 
            [FromServices] ISender mediatr, 
            [FromServices] IApiKeyService service, 
            [FromServices] IEncryptionService cryptService,
            [FromServices] ISignedNonceService signedNonceService,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            [FromHeader(Name = HttpHeaderKeys.XApiHeaderKey), Required, 
             StringLength(HttpHeaderKeys.XApiHeaderKeyMaxLength, MinimumLength = HttpHeaderKeys.XApiHeaderKeyMinLength)] string apiKey,
            [FromHeader(Name = HttpHeaderKeys.SNonceHeaderKey), Required, 
             StringLength(HttpHeaderKeys.SNonceHeaderKeyMaxLength, MinimumLength = HttpHeaderKeys.SNonceHeaderKeyMinLength)] string signedNonce,
            [FromHeader(Name = HttpHeaderKeys.XApiSecretHeaderKey), Required, 
             StringLength(HttpHeaderKeys.XApiSecretHeaderKeyMaxLength, MinimumLength = HttpHeaderKeys.XApiSecretHeaderKeyMinLength)] string apiSecret) =>
        {
            Guards(mediatr, service, cryptService, signedNonceService, httpContextAccessor);
            
            if (IsBadRequest(httpContextAccessor, 
                    cryptService, signedNonceService, service, 
                    apiKey, signedNonce, apiSecret))
            {
                return TypedResults.BadRequest();
            }
            
            var ipAddress = context.Request.GetIpAddress();
            Log.Information($"ClientIPAddress - {ipAddress}.");
            
            var result = await mediatr.Send(new GetWorkTypeQuery(id));
            return result != null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .AddEndpointFilter(async (context, next) =>
        {
            var id = context.GetArgument<int>(0);
   
            if (id <= 0)
            {
                return TypedResults.BadRequest();
            }

            return await next(context);
        })
        .WithName("GetWorkType")
        .MapApiVersion(apiVersionSet, version1)
        .RequireRateLimiting("FixedWindow")
        .WithOpenApi();

//api/v{version:apiVersion}
    
    
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

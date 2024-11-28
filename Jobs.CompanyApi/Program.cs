using System.Reflection;
using System.Security.Claims;
using Asp.Versioning;
using AutoMapper;
using Jobs.Common.Constants;
using Jobs.Common.Contracts;
using Jobs.Common.Extentions;
using Jobs.Common.Options;
using Jobs.Common.Settings;
using Jobs.CompanyApi.DBContext;
using Jobs.CompanyApi.DTOModels;
using Jobs.CompanyApi.DTOModels.Helpers;
using Jobs.CompanyApi.Features.Commands;
using Jobs.CompanyApi.Features.Notifications;
using Jobs.CompanyApi.Features.Queries;
using Jobs.CompanyApi.Repositories;
using Jobs.CompanyApi.Services;
using Jobs.CompanyApi.Services.Contracts;
using Jobs.Core.Contracts;
using Jobs.Core.Contracts.Providers;
using Jobs.Core.Extentions;
using Jobs.Core.Managers;
using Jobs.Core.Middleware;
using Jobs.Core.Observability.Options;
using Jobs.Core.Providers;
using Jobs.Core.Services;
using Jobs.Entities.Models;
using Keycloak.AuthServices.Authentication;
using MediatR;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration.WriteTo.Console();
    loggerConfiguration.ReadFrom.Configuration(context.Configuration);
});

Log.Information("Starting WebApi Company Service.");

var companySecretKey = builder.Configuration["JobsCompanyApi:SecretKey"];
Console.WriteLine($"CompanySecretKey - {companySecretKey}");

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo{ Title = "My API", Version = "v1" });
    options.AddSecurityDefinition("Keycloak", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Implicit = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("http://localhost:9001/realms/mjobs/protocol/openid-connect/auth"),
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "openid" },
                    { "profile", "profile" }
                }
            }
        }
    });
    
    OpenApiSecurityScheme keycloakSecurityScheme = new()
    {
        Reference = new OpenApiReference
        {
            Id = "Keycloak",
            Type = ReferenceType.SecurityScheme,
        },
        In = ParameterLocation.Header,
        Name = "Bearer",
        Scheme = "Bearer",
    };

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { keycloakSecurityScheme, Array.Empty<string>() },
    });
    
    //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    //options.IncludeXmlComments(xmlPath);
});

builder.Services.AddApiVersionService();

builder.Services.AddDbContext<CompanyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

CryptOptions cryptOptions = new();

builder.Configuration
    .GetRequiredSection(nameof(CryptOptions))
    .Bind(cryptOptions);

builder.Services.AddScoped<IGenericRepository<Company>, CompanyRepository>();
builder.Services.AddScoped<IApiKeyStorageServiceProvider, MemoryApiKeyStorageServiceProvider>();
builder.Services.AddScoped<IApiKeyManagerServiceProvider, ApiKeyManagerServiceProvider>();
builder.Services.AddScoped<ISecretApiKeyRepository, SecretApiKeyRepository>();
builder.Services.AddScoped<IProcessingService, ProcessingService>();
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
builder.Services.AddScoped<ISignedNonceService, SignedNonceService>();
builder.Services.AddScoped<IEncryptionService, NaiveEncryptionService>(p => 
    p.ResolveWith<NaiveEncryptionService>(Convert.FromBase64String(cryptOptions.PKey), Convert.FromBase64String(cryptOptions.IV)));
builder.Services.AddScoped<ISecretApiService, SecretApiService>(p => 
    p.ResolveWith<SecretApiService>(companySecretKey));


builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly()); // AutoMapper registration
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.IncludeFields = true;
});

//builder.Services.AddRateLimiterService();
builder.Services.AddWindowRateLimiterService();

builder.Services.AddHttpClient();

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

// forward headers configuration for reverse proxy
builder.Services.Configure<ForwardedHeadersOptions>(options => {
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddResponseCompressionService();
builder.Services.AddHttpContextAccessor();

// Keycloak Auth.
builder.Services.AddKeycloakWebApiAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

CorsSettings corsSettings = new();

builder.Configuration
    .GetRequiredSection(nameof(CorsSettings))
    .Bind(corsSettings);
    
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(build => {
        build.WithOrigins(corsSettings.CorsAllowedOrigins);
        build.AllowAnyMethod();
        build.AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //app.UseSwaggerUI();
    
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        options.OAuthClientId("confmjobs");
    });
}

app.UseResponseCompression();
app.UseForwardedHeaders();
app.UseMiddleware<AdminSafeListMiddleware>(builder.Configuration["AdminSafeList"]);

//app.UseHttpsRedirection();

// Get the Automapper, we can share this too
var mapper = app.Services.GetService<IMapper>();
if (mapper == null)
{
    throw new InvalidOperationException("Mapper not found");
}

//app.UseLogHeaders(); // add here right after you create app
//app.UseExceptionHandlers();

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
            var cryptedApiKey = cryptService.Encrypt(apiKey.Key);
            context.Response.Headers.Append(HttpHeaderKeys.XApiHeaderKey, cryptedApiKey);
        }
    });
    
    await next.Invoke();
});

app.UseCors();
app.UseRateLimiter();

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
        
    var (longNonce ,resultParse) = signedNonceService.IsSignedNonceValid(signedNonce);

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

app.MapGet("api/v{version:apiVersion}/companies", async (HttpContext context, 
        ClaimsPrincipal user,
        [FromServices] ISender mediatr, 
        [FromServices] IApiKeyService service,
        [FromServices] ISignedNonceService signedNonceService,
        [FromServices] IEncryptionService cryptService,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromHeader(Name = HttpHeaderKeys.XApiHeaderKey)] string apiKey,
        [FromHeader(Name = HttpHeaderKeys.SNonceHeaderKey)] string signedNonce,
        [FromHeader(Name = HttpHeaderKeys.XApiSecretHeaderKey)] string apiSecret) =>
    {
        app.Logger.LogInformation($"UserName: {user.Identity?.Name}");
        Console.WriteLine($"UserAgent - {httpContextAccessor.HttpContext?.Request.Headers.UserAgent}");
        
        if (IsBadRequest(httpContextAccessor, 
                cryptService, signedNonceService, service, 
                apiKey, signedNonce, apiSecret))
        {
            return Results.BadRequest();
        }

        var ipAddress = context.Request.GetIpAddress();
        Log.Information($"ClientIPAddress - {ipAddress}.");
        
        var companies = await mediatr.Send(new ListCompaniesQuery());
        return Results.Ok(companies);
    }).WithName("GetCompanies")
    .MapApiVersion(apiVersionSet, version1)
    .RequireRateLimiting("FixedWindow")
    .RequireAuthorization()
    .WithOpenApi();

app.MapGet("api/v{version:apiVersion}/companies/{id:int}", async (int id,
        ISender mediatr,
        [FromServices] IApiKeyService service, 
        [FromServices] ISignedNonceService signedNonceService,
        [FromServices] IEncryptionService cryptService,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromHeader(Name = HttpHeaderKeys.XApiHeaderKey)] string apiKey,
        [FromHeader(Name = HttpHeaderKeys.SNonceHeaderKey)] string signedNonce,
        [FromHeader(Name = HttpHeaderKeys.XApiSecretHeaderKey)] string apiSecret) =>
    {
        if (IsBadRequest(httpContextAccessor, 
                cryptService, signedNonceService, service, 
                apiKey, signedNonce, apiSecret))
        {
            return Results.BadRequest();
        }

        if (id <= 0)
        {
            return Results.BadRequest();
        }

        var vacancy = await mediatr.Send(new GetCompanyQuery(id));
        return vacancy == null ? Results.NotFound() : Results.Ok(vacancy);
    }).WithName("GetCompany")
    .MapApiVersion(apiVersionSet, version1)
    .RequireRateLimiting("FixedWindow")
    .WithOpenApi();

app.MapPost("api/v{version:apiVersion}/companies", async ([FromBody] CompanyInDto company,
        [FromServices] IApiKeyService service, 
        [FromServices] ISignedNonceService signedNonceService,
        [FromServices] IEncryptionService cryptService,
        [FromServices] ISender mediatr, 
        [FromServices] IPublisher publisher, 
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromHeader(Name = HttpHeaderKeys.XApiHeaderKey)] string apiKey,
        [FromHeader(Name = HttpHeaderKeys.SNonceHeaderKey)] string signedNonce,
        [FromHeader(Name = HttpHeaderKeys.XApiSecretHeaderKey)] string apiSecret) =>
        {
            if (IsBadRequest(httpContextAccessor, 
                    cryptService, signedNonceService, service, 
                    apiKey, signedNonce, apiSecret))
            {
                return Results.BadRequest();
            }
            
            if (company.CompanyId != 0)
            {
                return Results.BadRequest();
            }
                
            if (!company.IsValid())
            {
                return Results.BadRequest();
            }
            
            var sanitized = SanitizerDtoHelper.SanitizeCompanyInDto(company);

            var result = await mediatr.Send(new CreateCompanyCommand(sanitized));
            if (0 == result.CompanyId) return Results.BadRequest();
                
            await publisher.Publish(new CompanyCreatedNotification(result.CompanyId));
            
            return Results.Created($"/companies/{result.CompanyId}", result);
    }).WithName("AddCompany")
    .MapApiVersion(apiVersionSet, version1)
    .RequireRateLimiting("FixedWindow")
    .WithOpenApi();

app.MapPut("api/v{version:apiVersion}/companies/{id:int}", async (int id, 
        CompanyInDto company, 
        [FromServices] ISender mediatr,
        [FromServices] IApiKeyService service, 
        [FromServices] ISignedNonceService signedNonceService,
        [FromServices] IEncryptionService cryptService,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromHeader(Name = HttpHeaderKeys.XApiHeaderKey)] string apiKey,
        [FromHeader(Name = HttpHeaderKeys.SNonceHeaderKey)] string signedNonce,
        [FromHeader(Name = HttpHeaderKeys.XApiSecretHeaderKey)] string apiSecret) =>
    {
        if (IsBadRequest(httpContextAccessor, 
                cryptService, signedNonceService, service, 
                apiKey, signedNonce, apiSecret))
        {
            return Results.BadRequest();
        }

        if (id != company.CompanyId || id <= 0)
        {
            return Results.BadRequest();
        }

        if (!company.IsValid())
        {
            return Results.BadRequest();
        }
            
        var sanitized = SanitizerDtoHelper.SanitizeCompanyInDto(company);
        var result = await mediatr.Send(new UpdateCompanyCommand(sanitized));

        return result > 0 ? Results.NoContent() : Results.NotFound();
    }).WithName("ChangeCompany")
    .MapApiVersion(apiVersionSet, version1)
    .RequireRateLimiting("FixedWindow")
    .WithOpenApi();

app.MapDelete("api/v{version:apiVersion}/companies/{id:int}", async (int id, 
        ISender mediatr,
        [FromServices] IApiKeyService service, 
        [FromServices] ISignedNonceService signedNonceService,
        [FromServices] IEncryptionService cryptService,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromHeader(Name = HttpHeaderKeys.XApiHeaderKey)] string apiKey,
        [FromHeader(Name = HttpHeaderKeys.SNonceHeaderKey)] string signedNonce,
        [FromHeader(Name = HttpHeaderKeys.XApiSecretHeaderKey)] string apiSecret) =>
    {
        if (IsBadRequest(httpContextAccessor, 
                cryptService, signedNonceService, service, 
                apiKey, signedNonce, apiSecret))
        {
            return Results.BadRequest();
        }

        if (id <= 0)
        {
            return Results.BadRequest();
        }
        
        var result = await mediatr.Send(new DeleteCompanyCommand(id));
        return result == -1 ? Results.NotFound() : Results.NoContent();
    }).WithName("RemoveCompany")
    .MapApiVersion(apiVersionSet, version1)
    .RequireRateLimiting("FixedWindow")
    .WithOpenApi();
    
app.UseSerilogRequestLogging();

app.Run();


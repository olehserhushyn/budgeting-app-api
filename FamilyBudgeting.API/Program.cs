using FamilyBudgeting.API.Configuration;
using FamilyBudgeting.API.Filters;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Data.Users;
using FamilyBudgeting.Domain.Interfaces;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Infrastructure.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Resend;
using System.Text;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<LogActionFilter>();
});

// Register Database Context
builder.Services.AddPostgresWithEnums(
    builder.Configuration,
    AppConstants.DBConnStringName);

// Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure JWT Options
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(nameof(JwtOptions)));
builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<JwtOptions>>().Value);

var jwtOptions = builder.Configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT options are missing in the configuration.");

// Log JWT configuration (without exposing the secret key)
var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<Program>();
logger.LogInformation("JWT Configuration loaded - Issuer: {Issuer}, Audience: {Audience}, ExpiresHours: {ExpiresHours}", 
    jwtOptions.Issuer, jwtOptions.Audience, jwtOptions.ExpiresHours);

// Authentication Setup
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
        ClockSkew = TimeSpan.FromMinutes(1),
        RequireExpirationTime = true,
        ValidateTokenReplay = false
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Cookies[AppConstants.JwtCookieName];
            var logger = context.HttpContext.RequestServices.GetService<ILogger<Program>>();
            
            if (string.IsNullOrEmpty(token))
            {
                logger?.LogWarning("No JWT token found in cookies for request: {Path}", context.Request.Path);
                logger?.LogDebug("Available cookies: {Cookies}", string.Join(", ", context.Request.Cookies.Keys));
            }
            else
            {
                logger?.LogDebug("JWT token found in cookies for request: {Path}, Token length: {Length}", context.Request.Path, token.Length);
            }
            
            context.Token = token;
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetService<ILogger<Program>>();
            logger?.LogError("JWT authentication failed: {Error}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetService<ILogger<Program>>();
            logger?.LogDebug("JWT token validated successfully for user: {UserId}", context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Task.CompletedTask;
        }
    };
});

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppConstants.AuthConfirmedEmailPolicyName, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
        {
            var emailConfirmedClaim = context.User.Claims.FirstOrDefault(c => c.Type == "email_confirmed");
            return emailConfirmedClaim != null && emailConfirmedClaim.Value == "True";
        });
    });
});

// Swagger Configuration with JWT Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Family Budgeting API",
        Version = "v1"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer {your_token_here}'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };

    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { { securityScheme, new List<string>() } });
});

builder.Services.AddHttpContextAccessor();

// Logging config as you had it
builder.Services.AddLogging(configure =>
{
    configure.ClearProviders();
    configure.AddConsole();
    configure.AddFilter("LogActionFilter", LogLevel.Information);
    configure.SetMinimumLevel(LogLevel.Debug);
});
builder.Logging.AddFilter("LogActionFilter", LogLevel.Information);
builder.Logging.AddFilter("Npgsql", LogLevel.Trace);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.InjectRepositories();
builder.Services.InjectQueryServices();
builder.Services.InjectServices();

builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(o =>
{
    o.ApiToken = builder.Configuration["Resend:ApiToken"]
            ?? throw new InvalidOperationException("Resend API token is missing in configuration");
});
builder.Services.AddScoped<IResend, ResendClient>();
builder.Services.AddScoped<IEmailService, FamilyBudgeting.Infrastructure.EmailService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173",
                "http://107.170.72.240",
                "https://107.170.72.240"
             )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders("Token-Expired"));
});

builder.Services.AddMemoryCache();

builder.AddServiceDefaults();

var app = builder.Build();

// Add this BEFORE UseRouting() and BEFORE UseAuthentication()
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
};

// Since your nginx runs locally, clear KnownNetworks and KnownProxies to trust localhost proxy headers
forwardedHeadersOptions.KnownNetworks.Clear(); // Remove default loopback restrictions
forwardedHeadersOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedHeadersOptions);

app.UseRouting();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Comment out HTTPS redirection in development to allow HTTP requests
    // app.UseHttpsRedirection();
    app.UseDeveloperExceptionPage();
}
else
{
    
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapDefaultEndpoints();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IAppInitializer>();
    await initializer.InitializeAsync();
}

app.Run();

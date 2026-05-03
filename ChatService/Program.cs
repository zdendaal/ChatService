using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;
using System.Text;
using ChatService;
using ChatService.Database;
using ChatService.Realtime;
using ChatService.Services;
using System.IdentityModel.Tokens.Jwt;



var builder = WebApplication.CreateBuilder(args);

// Bind appsettings to Configuration class and register it to the DI container
builder.Services.Configure<JWTConf>(conf => 
    builder.Configuration.GetSection("JWT").Bind(conf)
    );

builder.Services.AddDbContext<BusinessData>(options =>
    options.UseMySQL(
        builder.Configuration.GetValue<string>("DefaultConnection")?? throw new ArgumentNullException("DefaultConnection appsettings.json value is null"))
);

// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Authentication using JWT Bearer tokens
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT:JWTValidIssuer"] ?? throw new ArgumentNullException("ValidIssuer cannot be null in appsettings.json"),
            ValidAudience = builder.Configuration["JWT:JWTValidAudience"] ?? throw new ArgumentNullException("ValidAudience cannot be null in appsettings.json"),
            NameClaimType = JwtRegisteredClaimNames.Sub
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                builder.Configuration["JWT:JWTSecret"]?? throw new ArgumentNullException("JWTSecret cannot be null in appsettings.json")
                )
            );

        options.TokenValidationParameters.IssuerSigningKey = key;
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"].FirstOrDefault();
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chat"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        // 1. Definice zabezpečení (Bearer)
        document.Components = new OpenApiComponents()
        {
            SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>()
        };

        document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme."
        });

        // Create a security requirement referencing the scheme
        var securityRequirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        };
        return Task.CompletedTask;
    });
});


// Cors policy rule name
var allowedOrigins = "myAllowedOrigins";
// Alowwed origins for CORS
string[] origins = new string[] {
    "https://localhost:7069",
    "http://localhost:5034",
};
builder.Services.AddCors(options =>
options.AddPolicy(allowedOrigins, policy => {

    policy.WithOrigins(origins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    })
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddServerSideBlazor();
builder.Services.AddRazorPages();

builder.Services.AddScoped<Token>();



// Building the application, follows request pipeline configuration
var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.MapOpenApi();
    app.MapScalarApiReference("scalar/v1", options =>
    {
        options.WithTitle("ChatService API documentation");
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors(allowedOrigins);
app.UseAuthentication();
app.UseAuthorization();

// Global exception handling middleware
app.UseExceptionHandler(error =>
{
    error.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "text/plain";

        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        if (exceptionHandlerPathFeature?.Error != null)
        {
            Log.Error(exceptionHandlerPathFeature.Error, "Unhandled exception occurred.");
            await context.Response.WriteAsync("Internal server error.");
        }
    });
});

app.MapControllers()
    .RequireCors(allowedOrigins)
    .RequireAuthorization();

// Blazor Server (frontend)
app.MapBlazorHub()
    .RequireCors(allowedOrigins);


app.MapHub<Chat>("/chat")
    .RequireCors(allowedOrigins)
    .RequireAuthorization();

app.Run();

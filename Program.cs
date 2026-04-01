using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using System.Reflection;
using Microsoft.AspNetCore.SignalR;
using VideoStreamingService.Realtime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using VideoStreamingService;
using VideoStreamingService.Database;
using MySql.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Serilog;



var builder = WebApplication.CreateBuilder(args);

// Bind appsettings to Configuration class and register it to the DI container
builder.Services.Configure<Configuration>(conf => 
    builder.Configuration.GetSection("Configuration").Bind(conf)
    );

builder.Services.AddDbContext<BusinessData>(options =>
    options.UseMySQL(
        builder.Configuration.GetConnectionString("DefaultConnection")?? throw new ArgumentNullException("DefaultConnection appsettings.json value is null"))
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
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
            ValidAudience = builder.Configuration["JWT:ValidAudience"]
        };
    });

builder.Services.AddSwaggerGen(options =>
{
    // To load XML comments and display them in Swagger UI
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    options.SwaggerDoc("v0", new OpenApiInfo
    {
        Version = "v0",
        Title = "VideoStreamingService Api Documentation",
        Description = "Documentation"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter your JWT token in the text box below. Example: '12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.OAuth2,
        Scheme = "Bearer",
        BearerFormat = "JWT",
    });

    /*
    // 🔐 Security requirement
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    */

});

// Cors policy rule name
var allowedOrigins = "myAllowedOrigins";
// Alowwed origins for CORS
string[] origins = new string[] {
    "https://localhost:7069",
    "http://localhost:5173",
    "https://localhost:5173"
};
builder.Services.AddCors(options =>
options.AddPolicy(allowedOrigins, policy => {

    policy.WithOrigins(origins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    })
);

builder.Services.AddControllers();
builder.Services.AddSignalR();



// Building the application, follows request pipeline configuration
var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty; // Sets Swagger at the app's root (index.html));
    });
}

app.UseHttpsRedirection();
app.UseCors();

app.UseRouting();

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
            Log.Error(exceptionHandlerPathFeature.Error, "Unhandled exception occurred");
            await context.Response.WriteAsync("Internal server error");
        }
    });
});

app.MapControllers()
    .RequireCors(allowedOrigins);

app.MapHub<Chat>("/chat")
    .RequireCors(allowedOrigins);

app.Run();

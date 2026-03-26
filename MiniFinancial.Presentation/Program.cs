using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using MiniFinancial.Application;
using MiniFinancial.Domain.Commom;
using MiniFinancial.Infrastructure.Persistence;
using MiniFinancial.Presentation.Middlewares;
using Scalar.AspNetCore;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;


#region ConfigureService
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Scalar precisa do OpenAPI generator interno
builder.Services.AddOpenApi();

builder.Services.AddHttpContextAccessor();

// Forwarded Headers
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),

        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }

            return Task.CompletedTask;
        }
    };
});

//Configura os Json
builder.Services.AddControllers()
                .AddJsonOptions(opt =>
                {
                    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

//Configura o Logging Estrutural
Log.Logger = new LoggerConfiguration()
             .Enrich.FromLogContext() 
             .Enrich.WithMachineName()
             .Enrich.WithThreadId()
             .WriteTo.Console()
             .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
             .CreateLogger();
             
builder.Host.UseSerilog();
#endregion

#region Adiciona Dependencias
var app = builder.Build();

app.UseForwardedHeaders();
app.UseHttpsRedirection();

app.UseRouting();

//Add Midlewares
app.UseMiddleware<AbuseMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "Mini Financial API";
});

app.Run();
#endregion

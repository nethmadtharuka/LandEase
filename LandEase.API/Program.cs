using FluentValidation;
using FluentValidation.AspNetCore;
using LandEase.API.Middleware;
using LandEase.Application.Interfaces;
using LandEase.Application.Validators;
using LandEase.Infrastructure;
using LandEase.Infrastructure.Data;
using LandEase.Infrastructure.ExternalServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using LandEase.Infrastructure.Hubs;


var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("DefaultConnection"))));

// ── Application Services ──────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IKycService, KycService>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IServiceListingService, ServiceListingService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ICommunityService, CommunityService>();
builder.Services.AddSignalR();
builder.Services.AddScoped<ISosService, SosService>();
builder.Services.AddScoped<IAiChatService, AiChatService>();
builder.Services.AddScoped<GeminiAiService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<IFraudDetectionService, FraudDetectionService>();
builder.Services.AddScoped<IPlaceRecognitionService, PlaceRecognitionService>();
// ── FluentValidation ──────────────────────────────────────────
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<KycSubmissionValidator>();

// ── File Upload Limit ─────────────────────────────────────────
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 15 * 1024 * 1024;
});

// ── JWT Authentication ────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Secret"]!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// ── Swagger ───────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LandEase API",
        Version = "v1",
        Description = "Migration Support Platform — Internship Portfolio Project"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization. Enter: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
var app = builder.Build();

// ── Middleware Pipeline ───────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "LandEase API v1");
    c.RoutePrefix = string.Empty;
});
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<SosHub>("/hubs/sos");

app.Run();
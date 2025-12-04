using CRM_DocumentIA.Server.Application.Services;
using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;
using CRM_DocumentIA.Server.Domain.Interfaces.Repositories;
using CRM_DocumentIA.Server.Infrastructure.Database;
using CRM_DocumentIA.Server.Infrastructure.Rag;
using CRM_DocumentIA.Server.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// 1. Configurar política de CORS
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// ✅ SERVICIOS CORREGIDOS - SIN DUPLICADOS
builder.Services.AddScoped<SmtpEmailService>();
builder.Services.AddScoped<TwoFactorService>();
builder.Services.AddScoped<JWTService>();

// Conexión a SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.EnableSensitiveDataLogging();
});

// RAG Http Service
builder.Services.AddHttpClient<IRagClient, RagClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:8000/"); // Ajusta según tu FastAPI/Docker
    client.Timeout = TimeSpan.FromSeconds(30);
});


// Agregar HttpClient para el servicio RAG
builder.Services.AddHttpClient<ProcesoIAService>();

// Servicios normales
builder.Services.AddScoped<DocumentoService>();
builder.Services.AddScoped<ProcesoIAService>();
builder.Services.AddScoped<IDocumentoRepository, DocumentoRepository>();
// ... otros servicios

// Repositorios
builder.Services.AddScoped<IDocumentoRepository, DocumentoRepository>();
builder.Services.AddScoped<IProcesoIARepository, ProcesoIARepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IInsightRepository, InsightRepository>();
builder.Services.AddScoped<IRolRepository, RolRepository>();
builder.Services.AddScoped<IInsightsHistoRepository, InsightsHistoRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();

// 🔥 AGREGAR HTTPCLIENT PARA PROCESOIASERVICE
builder.Services.AddHttpClient<ProcesoIAService>();

// Servicios
builder.Services.AddScoped<DocumentoService>();
builder.Services.AddScoped<ProcesoIAService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<InsightService>();
builder.Services.AddScoped<RolService>();
builder.Services.AddScoped<AutenticacionService>();
builder.Services.AddScoped<InsightsHistoService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<AnalyticsService>();

// 2. Configuración de JWT Bearer (CRUCIAL)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var secretKey = builder.Configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("Clave JWT no configurada.");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// Controladores y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 3. CONFIGURACIÓN SWAGGER
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CRM DocumentIA API",
        Version = "v1",
        Description = "API para CRM DocumentIA - Compatible con Linux"
    });

    c.MapType<IFormFile>(() => new OpenApiSchema { Type = "string", Format = "binary" });
    c.MapType<IFormFileCollection>(() => new OpenApiSchema { Type = "string", Format = "binary" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
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
            new string[] {}
        }
    });
});

var app = builder.Build();

// 4. Configurar pipeline HTTP
app.UseCors(MyAllowSpecificOrigins);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CRM DocumentIA API v1");
        c.RoutePrefix = "swagger";
        c.ConfigObject.AdditionalItems["syntaxHighlight"] = new Dictionary<string, object>
        {
            ["activated"] = false
        };
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
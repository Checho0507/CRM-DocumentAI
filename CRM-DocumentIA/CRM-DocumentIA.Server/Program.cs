using System.Text;
using CRM_DocumentIA.Server.Application.Services;
using CRM_DocumentIA.Server.Infrastructure.Repositories;
using CRM_DocumentIA.Server;
using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;
using CRM_DocumentIA.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar política de CORS
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "https://localhost:49431")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// Servicios de Email
builder.Services.AddScoped<JWTService>();
builder.Services.AddScoped<TwoFactorService, TwoFactorService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<SmtpEmailService, SmtpEmailService>();

// Conexión a SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.EnableSensitiveDataLogging();
});

// Repositorios
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IDocumentoRepository, DocumentoRepository>();
builder.Services.AddScoped<IProcesoIARepository, ProcesoIARepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IInsightRepository, InsightRepository>();

// Servicios
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<DocumentoService>();
builder.Services.AddScoped<ProcesoIAService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<InsightService>();
builder.Services.AddScoped<JWTService>();

// 2. Configuración de JWT Bearer
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

builder.Services.AddScoped<AutenticacionService>();

// Controladores y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 3. CONFIGURACIÓN SWAGGER PARA LINUX
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "CRM DocumentIA API", 
        Version = "v1",
        Description = "API para CRM DocumentIA - Compatible con Linux"
    });
    
    // Agregar el filtro para manejar IFormFile en Linux
    c.OperationFilter<SwaggerFileOperationFilter>();
    
    // ✅ SOLO ESTAS 2 LÍNEAS NUEVAS:
    c.MapType<IFormFile>(() => new OpenApiSchema { Type = "string", Format = "binary" });
    c.MapType<IFormFileCollection>(() => new OpenApiSchema { Type = "string", Format = "binary" });
    
    // Configuración de seguridad JWT
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
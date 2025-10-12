using System.Text;
using CRM_DocumentIA.Application.Services;
using CRM_DocumentIA.Infrastructure.Repositories;
using CRM_DocumentIA.Server.Application.Services;
using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;
using CRM_DocumentIA.Server.Infrastructure.Database;
using CRM_DocumentIA.Server.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar política de CORS
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:3000") // 👈 tu frontend
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // opcional si usas cookies o auth
        });
});

builder.Services.AddSingleton<SmtpEmailService, SmtpEmailService>();

builder.Services.AddScoped<JWTService>();


builder.Services.AddScoped<TwoFactorService, TwoFactorService>();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<SmtpEmailService, SmtpEmailService>();



// Conexión a SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

    // ⚠️ Muestra los valores de los parámetros en la consola
    options.EnableSensitiveDataLogging();
});

// Repositorios
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IDocumentoRepository, DocumentoRepository>();
builder.Services.AddScoped<IProcesoIARepository, ProcesoIARepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IInsightRepository, InsghtRepository>();


// Servicios
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<DocumentoService>();
builder.Services.AddScoped<ProcesoIAService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<InsightService>();
builder.Services.AddScoped<JWTService>();
// 2. Configuración de JWT Bearer (CRUCIAL)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Obtener la clave secreta de appsettings.json
    var secretKey = builder.Configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("Clave JWT no configurada.");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        // Parámetros a validar
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero // Recomendado para evitar problemas de desfase horario
    };
});
builder.Services.AddScoped<AutenticacionService>();

// Controladores y herramientas
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 2. Usar la política de CORS antes del routing
app.UseCors(MyAllowSpecificOrigins);
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

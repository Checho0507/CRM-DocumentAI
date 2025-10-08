using System.Text;
using CRM_DocumentIA.Application.Services;
using CRM_DocumentIA.Infrastructure.Repositories;
using CRM_DocumentIA.Server.Application.Services;
using CRM_DocumentIA.Server.Domain.Interfaces;
using CRM_DocumentIA.Server.Infrastructure.Database;
using CRM_DocumentIA.Server.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Conexión a SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

using CRM_DocumentIA.Server.Application.Services;
using CRM_DocumentIA.Server.Domain.Interfaces;
using CRM_DocumentIA.Server.Domain.Interfaces.Repositories;
using CRM_DocumentIA.Server.Infrastructure.Database;
using CRM_DocumentIA.Server.Infrastructure.Rag;
using CRM_DocumentIA.Server.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 🔥 1. CONFIGURAR SERVICIOS DEL CONTENEDOR

// Configurar controladores con manejo de referencias circulares
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// 🔥 2. CONFIGURAR CORS
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins(
                    "http://localhost:3000",     // Frontend Next.js
                    "http://localhost:8080",     // Otros posibles frontends
                    "http://localhost:5173"      // Vite/React
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

// 🔥 3. CONFIGURAR BASE DE DATOS
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    options.LogTo(Console.WriteLine, LogLevel.Information);
});

// 🔥 4. CONFIGURAR AUTENTICACIÓN JWT
var jwtSecret = builder.Configuration["JwtSettings:Secret"] 
    ?? throw new InvalidOperationException("❌ Clave JWT no configurada. Agrega 'JwtSettings:Secret' en appsettings.json");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "CRM_DocumentIA",
        ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "CRM_DocumentIA_Users",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero, // Sin margen de tiempo para expiración
        RoleClaimType = "role",    // Para trabajar con roles
        NameClaimType = "name"     // Para obtener el nombre del usuario
    };

    // 🔥 AGREGAR LOGS PARA DEPURACIÓN
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"❌ Autenticación fallida: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"✅ Token validado para usuario: {context.Principal?.Identity?.Name}");
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            Console.WriteLine($"🔍 Token recibido: {(string.IsNullOrEmpty(token) ? "NO" : "SÍ")}");
            return Task.CompletedTask;
        }
    };
});

// 🔥 5. CONFIGURAR HTTP CLIENTS
builder.Services.AddHttpClient<IRagClient, RagClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["RagService:BaseUrl"] ?? "http://localhost:8000/");
    client.Timeout = TimeSpan.FromSeconds(60);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Configurar HttpClient para otros servicios
builder.Services.AddHttpClient<ProcesoIAService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});

// 🔥 6. REGISTRAR REPOSITORIOS
builder.Services.AddScoped<IDocumentoRepository, DocumentoRepository>();
builder.Services.AddScoped<IProcesoIARepository, ProcesoIARepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IInsightRepository, InsightRepository>();
builder.Services.AddScoped<IRolRepository, RolRepository>();
builder.Services.AddScoped<IInsightsHistoRepository, InsightsHistoRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();

// 🔥 7. REGISTRAR SERVICIOS DE APLICACIÓN
builder.Services.AddScoped<SmtpEmailService>();
builder.Services.AddScoped<TwoFactorService>();
builder.Services.AddScoped<JWTService>();
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

// 🔥 8. REGISTRAR SERVICIOS DE DOCUMENTOS COMPARTIDOS
builder.Services.AddScoped<IDocumentoCompartidoRepository, DocumentoCompartidoRepository>();
builder.Services.AddScoped<DocumentoCompartidoService, DocumentoCompartidoService>();
// En la sección de repositorios, AGREGA:
builder.Services.AddScoped<IDocumentoCompartidoRepository, DocumentoCompartidoRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
// En la sección de servicios, AGREGA:
builder.Services.AddScoped<DocumentoCompartidoService, DocumentoCompartidoService>();
// 🔥 9. CONFIGURAR SWAGGER/OPENAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CRM DocumentIA API",
        Version = "v1",
        Description = "API para CRM DocumentIA - Gestión documental con IA",
        Contact = new OpenApiContact
        {
            Name = "Soporte CRM DocumentIA",
            Email = "soporte@crmdocumentia.com"
        }
    });

    // Mapear IFormFile para Swagger
    c.MapType<IFormFile>(() => new OpenApiSchema { Type = "string", Format = "binary" });
    c.MapType<IFormFileCollection>(() => new OpenApiSchema { Type = "string", Format = "binary" });

    // 🔥 CONFIGURAR AUTENTICACIÓN JWT EN SWAGGER
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
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

    // Incluir comentarios XML
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// 🔥 10. CONFIGURAR PIPELINE DE MIDDLEWARE

// 🔥 IMPORTANTE: EL ORDEN ES CRÍTICO

// 1. Usar CORS (debe ir primero)
app.UseCors(MyAllowSpecificOrigins);

// 2. Swagger solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CRM DocumentIA API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "CRM DocumentIA API Documentation";
        
        // Configuraciones adicionales para mejor visualización
        c.DefaultModelsExpandDepth(-1); // Ocultar modelos por defecto
        c.DisplayRequestDuration();
        c.EnableFilter();
        
        // Configurar para evitar problemas de sintaxis
        c.ConfigObject.AdditionalItems["syntaxHighlight"] = new Dictionary<string, object>
        {
            ["activated"] = false
        };
    });
    
    Console.WriteLine("✅ Swagger disponible en: http://localhost:8085/swagger");
}

// 3. Redirección HTTPS (opcional en desarrollo)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 🔥 4. MIDDLEWARE DE AUTENTICACIÓN Y AUTORIZACIÓN
// EL ORDEN ES CRÍTICO: UseRouting -> UseAuthentication -> UseAuthorization

app.UseRouting();

app.UseAuthentication(); // 🔥 DEBE IR ANTES DE UseAuthorization
app.UseAuthorization();

// 5. Mapear controladores
app.MapControllers();

// 6. Middleware para manejar errores
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// 7. Middleware personalizado para logs
app.Use(async (context, next) =>
{
    var endpoint = context.GetEndpoint();
    if (endpoint != null)
    {
        Console.WriteLine($"📞 Endpoint: {endpoint.DisplayName}");
    }
    
    await next();
});

// 🔥 8. AGREGAR UN ENDPOINT DE SALUD
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    service = "CRM DocumentIA API",
    version = "1.0.0"
}));

// 🔥 9. AGREGAR UN ENDPOINT PARA VERIFICAR AUTENTICACIÓN
app.MapGet("/api/auth/test", () => Results.Ok(new
{
    message = "API funcionando correctamente",
    authenticated = false,
    timestamp = DateTime.UtcNow
})).RequireAuthorization(); // Solo accesible con token

// 🔥 10. INICIALIZAR BASE DE DATOS Y MOSTRAR INFORMACIÓN
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Verificar conexión a la base de datos
        if (dbContext.Database.CanConnect())
        {
            Console.WriteLine("✅ Conexión a la base de datos establecida");
            
            // Aplicar migraciones pendientes
            dbContext.Database.Migrate();
            Console.WriteLine("✅ Migraciones aplicadas");
        }
        else
        {
            Console.WriteLine("⚠️ No se pudo conectar a la base de datos");
        }
    }
    
    Console.WriteLine($"🚀 Servidor iniciado en: {app.Urls}");
    Console.WriteLine($"🔗 Swagger UI: http://localhost:8085/swagger");
    Console.WriteLine($"🔗 Health Check: http://localhost:8085/health");
    Console.WriteLine($"🔗 Test Auth: http://localhost:8085/api/auth/test");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error al inicializar la aplicación: {ex.Message}");
}

app.Run();
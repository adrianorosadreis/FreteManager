using FreteManager.Data;
using FreteManager.Extensions;
using FreteManager.Middleware;
using FreteManager.Repositories;
using FreteManager.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using System.Reflection;
using System.Text;

public class Program
{
    private static void Main(string[] args)
    {
        // Configuração inicial do NLog
        var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
        logger.Debug("Inicializando aplicação...");

        // Garantir que o diretório de logs existe
        var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        // Registrar manipulador de exceções não tratadas
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            logger.Error(args.ExceptionObject as Exception, "Exceção não tratada");
            NLog.LogManager.Shutdown();
        };

        var builder = WebApplication.CreateBuilder(args);

        // Configurar o NLog para DI
        builder.Logging.ClearProviders();
        builder.Host.UseNLog();

        // Add services to the container.
        builder.Services.AddControllers();

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
        builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
        builder.Services.AddScoped<IClienteService, ClienteService>();
        builder.Services.AddScoped<IPedidoService, PedidoService>();
        builder.Services.AddScoped<IFreteService, FreteService>();
        builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IFreteService, FreteService>();

        // Adicionar HttpClient para o serviço Frete
        builder.Services.AddHttpClient<IFreteService, FreteService>();

        // Adicionar cache de memória
        builder.Services.AddMemoryCache();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Secret"])),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };
        });

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "FreteManager API",
                Version = "v1",
                Description = "API para gerenciamento de pedidos de transporte e logística",
                Contact = new OpenApiContact
                {
                    Name = "Adriano Reis",
                    Email = "adrianorosadreis@gmail.com"
                }
            });

            // Configuração para incluir comentários XML
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);

            // Configuração para autenticação JWT
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Authorization: Bearer {token}\"",
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
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            // Habilitar o Swagger em ambiente de desenvolvimento
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "FreteManager API v1");
                // Configuração opcional para iniciar no Swagger
                c.RoutePrefix = string.Empty;
            });
        }
        app.InitializeDatabase();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseRouting();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}

using FreteManager.Data;
using FreteManager.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FreteManager.Tests.Integration
{
    /// <summary>
    /// Fábrica customizada para testes de integração
    /// Permite configurar o ambiente de teste de forma isolada
    /// </summary>
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        /// <summary>
        /// Configurações personalizadas para o ambiente de teste
        /// </summary>
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remover o DbContext original e suas opções
                services.RemoveAll(typeof(ApplicationDbContext)); 
                var dbContextOptionsDescriptors = services
                    .Where(d => d.ServiceType.Name.Contains("DbContextOptions"))
                    .ToList();
                
                foreach (var descriptor in dbContextOptionsDescriptors)
                {
                    services.Remove(descriptor);
                }

                // Adicionar o banco de dados em memória com o tipo específico de DbContextOptions<ApplicationDbContext>
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryTestDatabase");
                });

                // Construir o provedor de serviços
                var serviceProvider = services.BuildServiceProvider();

                // Inicializar o banco de dados
                using (var scope = serviceProvider.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<ApplicationDbContext>();

                    // A chamada para EnsureCreated() já é feita no construtor,
                    // mas não custa nada garantir
                    db.Database.EnsureCreated();

                    // Popular dados de teste
                    PopularDadosDeTeste(db);
                }
            });
        }

        /// <summary>
        /// Método para popular o banco de dados com dados de teste
        /// </summary>
        private void PopularDadosDeTeste(ApplicationDbContext context)
        {
            // Adicionar dados iniciais para testes de integração
            if (!context.Usuarios.Any())
            {
                context.Usuarios.Add(new Usuario
                {
                    Nome = "Usuário Teste",
                    Email = "teste@integracao.com",
                    Senha = "SenhaTesteSecurity123!", // Em um cenário real, usar hash
                    Role = "Usuario"
                });
                context.SaveChanges();
            }

            if (!context.Clientes.Any())
            {
                context.Clientes.Add(new Cliente
                {
                    Nome = "Cliente Teste Integração",
                    Email = "cliente.teste@exemplo.com",
                    Telefone = "1234567890",
                    Endereco = "Rua Teste, 123"
                });
                context.SaveChanges();
            }
        }
    }
}
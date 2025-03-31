using FreteManager.Data;
using Microsoft.EntityFrameworkCore;

namespace FreteManager.Tests.Data
{
    /// <summary>
    /// Versão do ApplicationDbContext específica para testes em memória
    /// </summary>
    public class InMemoryApplicationDbContext : ApplicationDbContext
    {
        public InMemoryApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Configuração específica para o contexto de testes
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        /// <summary>
        /// Ajusta o modelo específico para testes
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Chama o modelo base primeiro
            base.OnModelCreating(modelBuilder);
        }
    }
}

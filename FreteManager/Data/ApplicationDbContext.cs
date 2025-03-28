using FreteManager.Models;
using Microsoft.EntityFrameworkCore;

namespace FreteManager.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Construtor que recebe as opções de configuração
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        // DbSets para nossas entidades
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
    }
}
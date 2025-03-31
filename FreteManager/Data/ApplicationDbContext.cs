using FreteManager.Models;
using Microsoft.EntityFrameworkCore;

namespace FreteManager.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Pacote> Pacotes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar o relacionamento unidirecional entre Pedido e Pacote
            modelBuilder.Entity<Pacote>()
                .HasOne<Pedido>()  // Pacote pertence a um Pedido
                .WithMany(p => p.Pacotes)  // Pedido tem muitos Pacotes
                .HasForeignKey(p => p.PedidoId)  // Chave estrangeira em Pacote
                .OnDelete(DeleteBehavior.Cascade);  // Excluir pacotes quando o pedido for excluído

            // Garantir que ValorDeclarado tenha um valor padrão
            modelBuilder.Entity<Pedido>()
                .Property(p => p.ValorDeclarado)
                .HasDefaultValue(100.00m);
        }
    }
}
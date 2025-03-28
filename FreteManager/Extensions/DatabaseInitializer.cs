using FreteManager.Data;

namespace FreteManager.Extensions
{
    public static class DatabaseInitializer
    {
        public static void InitializeDatabase(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

                try
                {
                    // Apenas verifica se consegue conectar ao banco
                    if (dbContext.Database.CanConnect())
                    {
                        logger.LogInformation("Conexão com banco de dados estabelecida com sucesso.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Erro ao conectar ao banco de dados.");
                    throw;
                }
            }
        }
    }
}
using FreteManager.Data;
using FreteManager.Models;
using FreteManager.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FreteManager.Tests.Repositories
{
    /// <summary>
    /// Testes unitários para o repositório de Usuários
    /// Utiliza InMemory Database para isolar testes de banco de dados
    /// </summary>
    public class UsuarioRepositoryTests : IDisposable
    {
        // Contexto de banco de dados em memória
        private readonly ApplicationDbContext _context;

        // Repositório de usuários sendo testado
        private readonly UsuarioRepository _repository;

        /// <summary>
        /// Configuração inicial para cada teste
        /// Cria um banco de dados em memória único para cada execução de teste
        /// </summary>
        public UsuarioRepositoryTests()
        {
            // Configurar opções para banco de dados em memória
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Criar contexto de banco de dados
            _context = new ApplicationDbContext(options);

            // Criar repositório
            _repository = new UsuarioRepository(_context);
        }

        [Fact]
        public async Task AdicionarAsync_NovoUsuario_DeveInserirComSucesso()
        {
            // Arrange: Preparar dados de usuário
            var novoUsuario = new Usuario
            {
                Nome = "Usuário Teste",
                Email = "usuario.teste@exemplo.com",
                Senha = "SenhaTeste123!",
                Role = "Usuario"
            };

            // Act: Adicionar usuário
            var usuarioInserido = await _repository.AdicionarAsync(novoUsuario);

            // Assert: Verificar se o usuário foi inserido corretamente
            Assert.NotNull(usuarioInserido);
            Assert.True(usuarioInserido.Id > 0);
            Assert.Equal(novoUsuario.Nome, usuarioInserido.Nome);
            Assert.Equal(novoUsuario.Email, usuarioInserido.Email);
        }

        [Fact]
        public async Task ObterPorIdAsync_UsuarioExistente_DeveRetornarUsuario()
        {
            // Arrange: Criar e salvar um usuário
            var usuario = new Usuario
            {
                Nome = "Usuário Busca",
                Email = "usuario.busca@exemplo.com",
                Senha = "SenhaTeste456!",
                Role = "Usuario"
            };
            await _repository.AdicionarAsync(usuario);

            // Act: Obter o usuário por ID
            var usuarioEncontrado = await _repository.ObterPorIdAsync(usuario.Id);

            // Assert: Verificar se o usuário foi encontrado corretamente
            Assert.NotNull(usuarioEncontrado);
            Assert.Equal(usuario.Id, usuarioEncontrado.Id);
            Assert.Equal(usuario.Nome, usuarioEncontrado.Nome);
        }

        [Fact]
        public async Task ObterPorIdAsync_UsuarioInexistente_DeveRetornarNull()
        {
            // Act: Tentar obter usuário com ID inexistente
            var usuarioEncontrado = await _repository.ObterPorIdAsync(999999);

            // Assert: Verificar se retorna null
            Assert.Null(usuarioEncontrado);
        }

        [Fact]
        public async Task ObterPorEmailAsync_EmailExistente_DeveRetornarUsuario()
        {
            // Arrange: Criar e salvar um usuário
            var usuario = new Usuario
            {
                Nome = "Usuário Email",
                Email = "usuario.email@exemplo.com",
                Senha = "SenhaTeste789!",
                Role = "Usuario"
            };
            await _repository.AdicionarAsync(usuario);

            // Act: Obter usuário por email
            var usuarioEncontrado = await _repository.ObterPorEmailAsync(usuario.Email);

            // Assert: Verificar se o usuário foi encontrado corretamente
            Assert.NotNull(usuarioEncontrado);
            Assert.Equal(usuario.Email, usuarioEncontrado.Email);
            Assert.Equal(usuario.Nome, usuarioEncontrado.Nome);
        }

        [Fact]
        public async Task ObterPorEmailAsync_EmailInexistente_DeveRetornarNull()
        {
            // Act: Tentar obter usuário com email inexistente
            var usuarioEncontrado = await _repository.ObterPorEmailAsync("naoexiste@exemplo.com");

            // Assert: Verificar se retorna null
            Assert.Null(usuarioEncontrado);
        }

        [Fact]
        public async Task ObterPorEmailAsync_EmailCaseInsensitive_DeveRetornarUsuario()
        {
            // Arrange: Criar usuário com email em caixa baixa
            var usuario = new Usuario
            {
                Nome = "Usuário Case Insensitive",
                Email = "usuario.case@exemplo.com",
                Senha = "SenhaTeste101!",
                Role = "Usuario"
            };
            await _repository.AdicionarAsync(usuario);

            // Act: Tentar obter usuário com email em caixa diferente
            var usuarioEncontrado = await _repository.ObterPorEmailAsync("Usuario.Case@Exemplo.com");

            // Assert: Verificar se o usuário foi encontrado independente da caixa
            Assert.NotNull(usuarioEncontrado);
            Assert.Equal(usuario.Email.ToLower(), usuarioEncontrado.Email.ToLower());
        }

        [Fact]
        public async Task AdicionarAsync_UsuarioComRolePadrao_DeveDefinirRolePadrao()
        {
            // Arrange: Criar usuário sem especificar role
            var novoUsuario = new Usuario
            {
                Nome = "Usuário Sem Role",
                Email = "usuario.semrole@exemplo.com",
                Senha = "SenhaTeste202!"
            };

            // Act: Adicionar usuário
            var usuarioInserido = await _repository.AdicionarAsync(novoUsuario);

            // Assert: Verificar se a role padrão foi definida
            Assert.Equal("Usuario", usuarioInserido.Role);
        }

        /// <summary>
        /// Método para limpeza de recursos após os testes
        /// </summary>
        public void Dispose()
        {
            // Limpar o contexto de banco de dados
            _context.Dispose();
        }
    }
}
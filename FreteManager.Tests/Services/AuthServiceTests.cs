using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using FreteManager.Services;
using FreteManager.Repositories;
using FreteManager.Models;
using FreteManager.Exceptions;

namespace FreteManager.Tests.Services
{
    /// <summary>
    /// Testes unitários para o serviço de autenticação
    /// </summary>
    public class AuthServiceTests
    {
        private readonly Mock<IUsuarioRepository> _mockUsuarioRepository;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            // Configuração inicial para cada teste
            _mockUsuarioRepository = new Mock<IUsuarioRepository>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<AuthService>>();

            // Configurar mock de configuração
            _mockConfiguration
                .Setup(config => config["Jwt:Secret"])
                .Returns("YourSuperSecretKey123!@#$ThisShouldBeAtLeast32BytesLong");

            _authService = new AuthService(
                _mockUsuarioRepository.Object,
                _mockConfiguration.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task RegisterAsync_NovoUsuario_DeveCadastrarComSucesso()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Nome = "Usuário Teste",
                Email = "usuario@teste.com",
                Senha = "Senha123!",
                ConfirmacaoSenha = "Senha123!"
            };

            // Configurar mock do repositório para não encontrar usuário existente
            _mockUsuarioRepository
                .Setup(repo => repo.ObterPorEmailAsync(registerRequest.Email))
                .ReturnsAsync((Usuario)null);

            // Configurar mock do repositório para adicionar usuário
            _mockUsuarioRepository
                .Setup(repo => repo.AdicionarAsync(It.IsAny<Usuario>()))
                .Returns<Usuario>(usuario =>
                {
                    usuario.Id = 1; // Simular atribuição de ID
                    return Task.FromResult(usuario);
                });

            // Act
            var usuarioCriado = await _authService.RegisterAsync(registerRequest);

            // Assert
            Assert.NotNull(usuarioCriado);
            Assert.Equal(1, usuarioCriado.Id);
            Assert.Equal(registerRequest.Nome, usuarioCriado.Nome);
            Assert.Equal(registerRequest.Email, usuarioCriado.Email);
            Assert.NotEqual(registerRequest.Senha, usuarioCriado.Senha); // Deve estar hasheada

            // Verificar chamadas
            _mockUsuarioRepository.Verify(repo => repo.ObterPorEmailAsync(registerRequest.Email), Times.Once);
            _mockUsuarioRepository.Verify(repo => repo.AdicionarAsync(It.IsAny<Usuario>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_EmailJaExistente_DeveLancarExcecao()
        {
            // Arrange
            var usuarioExistente = new Usuario
            {
                Id = 1,
                Email = "existente@teste.com"
            };

            var registerRequest = new RegisterRequest
            {
                Nome = "Usuário Teste",
                Email = "existente@teste.com",
                Senha = "Senha123!",
                ConfirmacaoSenha = "Senha123!"
            };

            // Configurar mock do repositório para encontrar usuário existente
            _mockUsuarioRepository
                .Setup(repo => repo.ObterPorEmailAsync(registerRequest.Email))
                .ReturnsAsync(usuarioExistente);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleViolationException>(
                () => _authService.RegisterAsync(registerRequest)
            );

            // Verificar que o método de adicionar NÃO foi chamado
            _mockUsuarioRepository.Verify(repo => repo.AdicionarAsync(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_CredenciaisValidas_DeveRetornarToken()
        {
            // Arrange
            var usuario = new Usuario
            {
                Id = 1,
                Nome = "Usuário Teste",
                Email = "usuario@teste.com",
                Senha = "", // Será preenchido pelo hash na classe de serviço
                Role = "Usuario"
            };

            var loginRequest = new LoginRequest
            {
                Email = usuario.Email,
                Senha = "Senha123!"
            };

            // Configurar hash da senha para simular validação
            var senhaHasheada = _authService.HashSenha(loginRequest.Senha);
            usuario.Senha = senhaHasheada;

            // Configurar mock do repositório para encontrar usuário
            _mockUsuarioRepository
                .Setup(repo => repo.ObterPorEmailAsync(loginRequest.Email))
                .ReturnsAsync(usuario);

            // Act
            var loginResponse = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.NotNull(loginResponse);
            Assert.NotNull(loginResponse.Token);
            Assert.Equal(usuario.Nome, loginResponse.Nome);
            Assert.Equal(usuario.Email, loginResponse.Email);
            Assert.True(loginResponse.Expiracao > DateTime.UtcNow);

            // Verificar chamadas
            _mockUsuarioRepository.Verify(repo => repo.ObterPorEmailAsync(loginRequest.Email), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_UsuarioInexistente_DeveLancarExcecao()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "naoexiste@teste.com",
                Senha = "Senha123!"
            };

            // Configurar mock do repositório para não encontrar usuário
            _mockUsuarioRepository
                .Setup(repo => repo.ObterPorEmailAsync(loginRequest.Email))
                .ReturnsAsync((Usuario)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => _authService.LoginAsync(loginRequest)
            );
        }

        [Fact]
        public void HashSenha_DeveGerarHashDiferenteDaSenhaOriginal()
        {
            // Arrange
            var senha = "Senha123!";

            // Act
            var hash1 = _authService.HashSenha(senha);
            var hash2 = _authService.HashSenha(senha);

            // Assert
            Assert.NotEqual(senha, hash1);
            Assert.NotEqual(hash1, hash2); // Garantir que não seja um hash estático
        }

        [Fact]
        public void VerificarSenha_SenhaCorreta_DeveRetornarTrue()
        {
            // Arrange
            var senhaOriginal = "Senha123!";
            var senhaHasheada = _authService.HashSenha(senhaOriginal);

            // Act
            var resultado = _authService.VerificarSenha(senhaHasheada, senhaOriginal);

            // Assert
            Assert.True(resultado);
        }

        [Fact]
        public void VerificarSenha_SenhaIncorreta_DeveRetornarFalse()
        {
            // Arrange
            var senhaOriginal = "Senha123!";
            var senhaHasheada = _authService.HashSenha(senhaOriginal);
            var senhaIncorreta = "SenhaErrada456!";

            // Act
            var resultado = _authService.VerificarSenha(senhaHasheada, senhaIncorreta);

            // Assert
            Assert.False(resultado);
        }
    }
}
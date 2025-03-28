using FreteManager.Models;
using FreteManager.Repositories;
using FreteManager.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUsuarioRepository usuarioRepository,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Usuario> RegisterAsync(RegisterRequest model)
    {
        // Verificar se o email já existe
        var usuarioExistente = await _usuarioRepository.ObterPorEmailAsync(model.Email);
        if (usuarioExistente != null)
        {
            throw new InvalidOperationException("Este email já está em uso.");
        }

        // Criar um novo usuário
        var usuario = new Usuario
        {
            Nome = model.Nome,
            Email = model.Email,
            Senha = HashSenha(model.Senha),
            Role = "Usuario" // Definir um perfil padrão
        };

        // Salvar o usuário no banco de dados
        await _usuarioRepository.AdicionarAsync(usuario);

        return usuario;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest model)
    {
        // Buscar o usuário pelo email
        var usuario = await _usuarioRepository.ObterPorEmailAsync(model.Email);
        if (usuario == null)
        {
            throw new KeyNotFoundException("Usuário não encontrado.");
        }

        // Verificar a senha
        if (!VerificarSenha(usuario.Senha, model.Senha))
        {
            throw new InvalidOperationException("Credenciais inválidas.");
        }

        // Gerar o token JWT
        var token = GerarJwtToken(usuario);

        // Retornar a resposta de login
        return new LoginResponse
        {
            Token = token,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Expiracao = DateTime.UtcNow.AddHours(1) // Token válido por 1 hora
        };
    }

    private string GerarJwtToken(Usuario usuario)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Role)
            };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string HashSenha(string senha)
    {
        // Usar bcrypt ou outra técnica segura de hash para senhas
        // Para simplicidade, estamos usando HMACSHA256 aqui, mas em produção use bcrypt
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(senha));
        return Convert.ToBase64String(hashBytes);
    }

    public bool VerificarSenha(string senhaArmazenada, string senhaInformada)
    {
        // Hash a senha informada e compara com a armazenada
        var senhaHash = HashSenha(senhaInformada);
        return senhaArmazenada == senhaHash;
    }
}
using FreteManager.Models;

namespace FreteManager.Services
{
    public interface IAuthService
    {
        Task<Usuario> RegisterAsync(RegisterRequest model);
        Task<LoginResponse> LoginAsync(LoginRequest model);
        string HashSenha(string senha);
        bool VerificarSenha(string senhaArmazenada, string senhaInformada);
    }
}
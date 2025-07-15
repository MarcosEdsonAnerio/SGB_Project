using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using SGB_Project.Models;

namespace SGB_Project.Controllers
{
    public class AuthenticationService
    {
        private readonly UsuarioService _usuarioService;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthenticationService(UsuarioService usuarioService, 
                                    ILocalStorageService localStorage,
                                    AuthenticationStateProvider authStateProvider)
        {
            _usuarioService = usuarioService;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
        }

        public async Task<bool> Login(string email, string senha)
        {
            try 
            {
                var usuario = await _usuarioService.LoginAsync(email, senha);
                
                if (usuario == null)
                    return false;

                await _localStorage.SetItemAsync("userId", usuario.IdUsuario);
                await _localStorage.SetItemAsync("userName", usuario.Nome);
                await _localStorage.SetItemAsync("userEmail", usuario.Email);
                await _localStorage.SetItemAsync("userType", usuario.TipoUsuario);
                await _localStorage.SetItemAsync("isAuthenticated", true);

                // Notificar que o estado de autenticação mudou
                ((CustomAuthenticationStateProvider)_authStateProvider).NotifyAuthenticationStateChanged();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao fazer login: {ex.Message}");
                return false;
            }
        }

        public async Task Logout()
        {
            // Limpar as informações do usuário da sessão
            await _localStorage.RemoveItemAsync("userId");
            await _localStorage.RemoveItemAsync("userName");
            await _localStorage.RemoveItemAsync("userEmail");
            await _localStorage.RemoveItemAsync("userType");
            await _localStorage.SetItemAsync("isAuthenticated", false);

            // Notificar que o estado de autenticação mudou
            ((CustomAuthenticationStateProvider)_authStateProvider).NotifyAuthenticationStateChanged();
        }

        public async Task<bool> IsAuthenticated()
        {
            try 
            {
                if (!OperatingSystem.IsBrowser())
                {
                    // Durante a pré-renderização, considere o usuário como não autenticado
                    return false;
                }
                
                var isAuthenticated = await _localStorage.GetItemAsync<bool>("isAuthenticated");
                return isAuthenticated;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GetUserId()
        {
            return await _localStorage.GetItemAsync<int>("userId");
        }

        public async Task<string> GetUserName()
        {
            return await _localStorage.GetItemAsync<string>("userName") ?? "";
        }

        public async Task<int> GetUserType()
        {
            return await _localStorage.GetItemAsync<int>("userType");
        }

        public async Task<bool> IsAdmin()
        {
            var userType = await GetUserType();
            return userType == 1; // Administrador
        }
    }

    // Provider personalizado para gerenciar o estado de autenticação
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthenticationStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // Durante a pré-renderização, retorne o usuário anônimo para evitar erros de interop JS
                if (!OperatingSystem.IsBrowser())
                {
                    // Durante a pré-renderização, retorne o usuário anônimo
                    return new AuthenticationState(_anonymous);
                }
                
                // Estamos no browser, então é seguro usar localStorage
                var isAuthenticated = await _localStorage.GetItemAsync<bool>("isAuthenticated");
                
                if (!isAuthenticated)
                    return new AuthenticationState(_anonymous);

                var userId = await _localStorage.GetItemAsync<int>("userId");
                var userName = await _localStorage.GetItemAsync<string>("userName");
                var userEmail = await _localStorage.GetItemAsync<string>("userEmail");
                var userType = await _localStorage.GetItemAsync<int>("userType");

                // Criar as claims do usuário
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Name, userName ?? ""),
                    new Claim(ClaimTypes.Email, userEmail ?? ""),
                    new Claim(ClaimTypes.Role, userType.ToString())
                };

                var identity = new ClaimsIdentity(claims, "Custom Authentication");
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch
            {
                return new AuthenticationState(_anonymous);
            }
        }

        public void NotifyAuthenticationStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}

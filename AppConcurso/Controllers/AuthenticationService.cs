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

                Console.WriteLine($"Usuário encontrado: {usuario.Nome} (ID: {usuario.IdUsuario})");

                await _localStorage.SetItemAsync("userId", usuario.IdUsuario);
                await _localStorage.SetItemAsync("userName", usuario.Nome);
                await _localStorage.SetItemAsync("userEmail", usuario.Email);
                await _localStorage.SetItemAsync("userType", usuario.TipoUsuario);
                await _localStorage.SetItemAsync("isAuthenticated", true);

                Console.WriteLine("Dados salvos no LocalStorage");

                // Aguardar um pouco para garantir que o LocalStorage foi atualizado
                await Task.Delay(100);

                // Verificar se os dados foram salvos corretamente
                var savedUserId = await _localStorage.GetItemAsync<int>("userId");
                var savedIsAuthenticated = await _localStorage.GetItemAsync<bool>("isAuthenticated");
                Console.WriteLine($"Verificação pós-salvamento - UserId: {savedUserId}, IsAuthenticated: {savedIsAuthenticated}");

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
                // Tentar acessar o localStorage diretamente, se falhar, estamos em pré-renderização
                var isAuthenticatedValue = await _localStorage.GetItemAsync<bool>("isAuthenticated");
                var userId = await _localStorage.GetItemAsync<int>("userId");
                
                Console.WriteLine($"IsAuthenticated - isAuthenticatedValue: {isAuthenticatedValue}, userId: {userId}");
                
                // Verificar se também temos um userId válido
                var result = isAuthenticatedValue && userId > 0;
                Console.WriteLine($"IsAuthenticated - resultado final: {result}");
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao verificar autenticação (provavelmente pré-renderização): {ex.Message}");
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
                Console.WriteLine("GetAuthenticationStateAsync - Verificando localStorage");
                
                // Tentar acessar o localStorage diretamente
                var isAuthenticated = await _localStorage.GetItemAsync<bool>("isAuthenticated");
                var userId = await _localStorage.GetItemAsync<int>("userId");
                
                Console.WriteLine($"GetAuthenticationStateAsync - isAuthenticated: {isAuthenticated}, userId: {userId}");
                
                if (!isAuthenticated || userId <= 0)
                {
                    Console.WriteLine("GetAuthenticationStateAsync - Não autenticado, retornando anônimo");
                    return new AuthenticationState(_anonymous);
                }

                var userName = await _localStorage.GetItemAsync<string>("userName");
                var userEmail = await _localStorage.GetItemAsync<string>("userEmail");
                var userType = await _localStorage.GetItemAsync<int>("userType");

                Console.WriteLine($"GetAuthenticationStateAsync - Criando usuário autenticado: {userName}");

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
            catch (Exception ex)
            {
                Console.WriteLine($"GetAuthenticationStateAsync - Erro (provavelmente pré-renderização): {ex.Message}");
                return new AuthenticationState(_anonymous);
            }
        }

        public void NotifyAuthenticationStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}

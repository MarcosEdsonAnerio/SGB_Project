using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using SGB_Project.Contexto;
using SGB_Project.Controllers;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// --- Início do Registro de Serviços ---

// Adiciona os serviços padrão do Blazor
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Adiciona serviços de autenticação e armazenamento local
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthenticationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationService>();

// Adiciona os nossos serviços de aplicação, seguindo o seu padrão
// Cada serviço é registrado com um escopo de vida por requisição
builder.Services.AddScoped<LeitorService>();
builder.Services.AddScoped<LivroService>();
builder.Services.AddScoped<EmprestimoService>();
builder.Services.AddScoped<EmprestimoItemService>();

// Configura o Contexto do Banco de Dados para MySQL
// Pega a string de conexão do ficheiro appsettings.json
string mySqlConexao = builder.Configuration.GetConnectionString("BaseConexaoMySql");

// Registra o SGB_ProjectContext usando um Pool (para melhor performance) e o provedor MySQL
builder.Services.AddDbContextPool<SGB_ProjectContext>(options =>
    options.UseMySql(mySqlConexao, ServerVersion.AutoDetect(mySqlConexao))
           .LogTo(_ => {}, LogLevel.Error) // Apenas logs de erro
);

// --- Fim do Registro de Serviços ---

var app = builder.Build();

// Configura o pipeline de requisições HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // O valor padrão de HSTS é de 30 dias.
    app.UseHsts();
}

// Aplica as migrações pendentes no startup da aplicação
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<SGB_ProjectContext>();
        
        // Inicializa o banco de dados silenciosamente
        try
        {
            // Verifica se tabelas existem e cria a tabela EmprestimoStatus se necessário
            bool tablesExist = context.Leitores.AnyAsync().GetAwaiter().GetResult();
            
            if (!tablesExist)
            {
                context.Database.MigrateAsync().GetAwaiter().GetResult();
            }
            
            context.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS EmprestimoStatus (
                    IdEmprestimo INT PRIMARY KEY,
                    DataDevolucao DATETIME NOT NULL,
                    FOREIGN KEY (IdEmprestimo) REFERENCES Emprestimos(IdEmprestimo) ON DELETE CASCADE
                );").GetAwaiter().GetResult();
                
            // Criar a tabela de usuários se não existir
            context.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS Usuarios (
                    IdUsuario INT AUTO_INCREMENT PRIMARY KEY,
                    Nome VARCHAR(100) NOT NULL,
                    Email VARCHAR(100) NOT NULL,
                    Senha VARCHAR(100) NOT NULL,
                    TipoUsuario INT NOT NULL DEFAULT 3,
                    Ativo BOOLEAN NOT NULL DEFAULT TRUE,
                    DataCadastro DATETIME NOT NULL
                );").GetAwaiter().GetResult();
                
            // Criar o primeiro usuário administrador
            var usuarioService = services.GetRequiredService<UsuarioService>();
            usuarioService.CriarPrimeiroUsuarioAsync().GetAwaiter().GetResult();
        }
        catch
        {
            // Silencia erros de inicialização do banco
        }
    }
    catch
    {
        // Silencia erros de inicialização do banco
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
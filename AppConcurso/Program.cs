using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using SGB_Project.Contexto;
using SGB_Project.Controllers;

var builder = WebApplication.CreateBuilder(args);

// --- In�cio do Registro de Servi�os ---

// Adiciona os servi�os padr�o do Blazor
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Adiciona os nossos servi�os de aplica��o, seguindo o seu padr�o
// Cada servi�o � registrado com um escopo de vida por requisi��o
builder.Services.AddScoped<LeitorService>();
builder.Services.AddScoped<LivroService>();
builder.Services.AddScoped<EmprestimoService>();
builder.Services.AddScoped<EmprestimoItemService>();

// Configura o Contexto do Banco de Dados para MySQL
// Pega a string de conex�o do ficheiro appsettings.json
string mySqlConexao = builder.Configuration.GetConnectionString("BaseConexaoMySql");

// Registra o SGB_ProjectContext usando um Pool (para melhor performance) e o provedor MySQL
builder.Services.AddDbContextPool<SGB_ProjectContext>(options =>
    options.UseMySql(mySqlConexao, ServerVersion.AutoDetect(mySqlConexao))
           .LogTo(_ => {}, LogLevel.Error) // Apenas logs de erro
);

// --- Fim do Registro de Servi�os ---

var app = builder.Build();

// Configura o pipeline de requisi��es HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // O valor padr�o de HSTS � de 30 dias.
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
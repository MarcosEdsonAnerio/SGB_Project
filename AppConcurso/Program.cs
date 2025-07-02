using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using SGB_Project.Contexto;
using SGB_Project.Controllers;

var builder = WebApplication.CreateBuilder(args);

// --- Início do Registro de Serviços ---

// Adiciona os serviços padrão do Blazor
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
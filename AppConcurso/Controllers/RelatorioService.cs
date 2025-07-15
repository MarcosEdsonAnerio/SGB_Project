using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SGB_Project.Models;

namespace SGB_Project.Controllers
{
    public class RelatorioService
    {
        private readonly EmprestimoService _emprestimoService;
        private readonly LeitorService _leitorService;
        private readonly LivroService _livroService;

        public RelatorioService(EmprestimoService emprestimoService, LeitorService leitorService, LivroService livroService)
        {
            _emprestimoService = emprestimoService;
            _leitorService = leitorService;
            _livroService = livroService;
            
            // Configurar licença do QuestPDF (para uso comercial é necessário licença)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GerarRelatorioEmprestimosPendentes()
        {
            var emprestimos = await _emprestimoService.BuscarEmprestimosPendentesAsync();
            
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text("Sistema de Gestão de Biblioteca")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);

                            x.Item().Text("Relatório de Empréstimos Pendentes")
                                .FontSize(16).Bold().FontColor(Colors.Black);

                            x.Item().Text($"Data de Geração: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(10).FontColor(Colors.Grey.Darken2);

                            x.Item().Text($"Total de Empréstimos Pendentes: {emprestimos.Count()}")
                                .FontSize(14).Bold();

                            if (emprestimos.Any())
                            {
                                x.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3); // Leitor
                                        columns.RelativeColumn(2); // Data Empréstimo
                                        columns.RelativeColumn(3); // Livros
                                        columns.RelativeColumn(2); // Status
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("Leitor").Bold();
                                        header.Cell().Element(CellStyle).Text("Data Empréstimo").Bold();
                                        header.Cell().Element(CellStyle).Text("Livros").Bold();
                                        header.Cell().Element(CellStyle).Text("Status").Bold();

                                        static IContainer CellStyle(IContainer container)
                                        {
                                            return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                        }
                                    });

                                    foreach (var emprestimo in emprestimos)
                                    {
                                        table.Cell().Element(CellStyle).Text(emprestimo.NomeLeitor);
                                        table.Cell().Element(CellStyle).Text(emprestimo.DataEmprestimo.ToString("dd/MM/yyyy"));
                                        table.Cell().Element(CellStyle).Text(string.Join(", ", emprestimo.Livros.Select(l => l.Titulo)));
                                        
                                        var status = emprestimo.DiasAtraso > 0 ? $"Atrasado ({emprestimo.DiasAtraso} dias)" : "No prazo";
                                        var statusColor = emprestimo.DiasAtraso > 0 ? Colors.Red.Medium : Colors.Green.Medium;
                                        table.Cell().Element(CellStyle).Text(status).FontColor(statusColor);

                                        static IContainer CellStyle(IContainer container)
                                        {
                                            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                                        }
                                    }
                                });
                            }
                            else
                            {
                                x.Item().Text("Não há empréstimos pendentes no momento.")
                                    .FontSize(14).FontColor(Colors.Green.Medium);
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                            x.Span(" de ");
                            x.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> GerarRelatorioLivrosPopulares()
        {
            var livros = await _livroService.BuscarLivrosPopularesAsync();
            
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text("Sistema de Gestão de Biblioteca")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);

                            x.Item().Text("Relatório de Livros Populares")
                                .FontSize(16).Bold().FontColor(Colors.Black);

                            x.Item().Text($"Data de Geração: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(10).FontColor(Colors.Grey.Darken2);

                            if (livros.Any())
                            {
                                x.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1); // Posição
                                        columns.RelativeColumn(4); // Título
                                        columns.RelativeColumn(3); // Autor
                                        columns.RelativeColumn(2); // Empréstimos
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("#").Bold();
                                        header.Cell().Element(CellStyle).Text("Título").Bold();
                                        header.Cell().Element(CellStyle).Text("Autor").Bold();
                                        header.Cell().Element(CellStyle).Text("Empréstimos").Bold();

                                        static IContainer CellStyle(IContainer container)
                                        {
                                            return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                        }
                                    });

                                    int posicao = 1;
                                    foreach (var livro in livros)
                                    {
                                        table.Cell().Element(CellStyle).Text(posicao.ToString());
                                        table.Cell().Element(CellStyle).Text(livro.Titulo);
                                        table.Cell().Element(CellStyle).Text(livro.Autor);
                                        table.Cell().Element(CellStyle).Text(livro.QuantidadeEmprestimos.ToString());

                                        posicao++;

                                        static IContainer CellStyle(IContainer container)
                                        {
                                            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                                        }
                                    }
                                });
                            }
                            else
                            {
                                x.Item().Text("Não há dados de livros populares disponíveis.")
                                    .FontSize(14).FontColor(Colors.Orange.Medium);
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                            x.Span(" de ");
                            x.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> GerarRelatorioAcervo()
        {
            var livros = await _livroService.ListarAsync();
            
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text("Sistema de Gestão de Biblioteca")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);

                            x.Item().Text("Relatório do Acervo")
                                .FontSize(16).Bold().FontColor(Colors.Black);

                            x.Item().Text($"Data de Geração: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(10).FontColor(Colors.Grey.Darken2);

                            x.Item().Text($"Total de Livros no Acervo: {livros.Count()}")
                                .FontSize(14).Bold();

                            if (livros.Any())
                            {
                                x.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1); // ID
                                        columns.RelativeColumn(4); // Título
                                        columns.RelativeColumn(3); // Autor
                                        columns.RelativeColumn(1); // Estoque
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("ID").Bold();
                                        header.Cell().Element(CellStyle).Text("Título").Bold();
                                        header.Cell().Element(CellStyle).Text("Autor").Bold();
                                        header.Cell().Element(CellStyle).Text("Estoque").Bold();

                                        static IContainer CellStyle(IContainer container)
                                        {
                                            return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                        }
                                    });

                                    foreach (var livro in livros)
                                    {
                                        table.Cell().Element(CellStyle).Text(livro.IdLivro.ToString());
                                        table.Cell().Element(CellStyle).Text(livro.Titulo);
                                        table.Cell().Element(CellStyle).Text(livro.Autor);
                                        
                                        var estoqueColor = livro.Estoque == 0 ? Colors.Red.Medium : 
                                                          livro.Estoque <= 2 ? Colors.Orange.Medium : Colors.Black;
                                        table.Cell().Element(CellStyle).Text(livro.Estoque.ToString()).FontColor(estoqueColor);

                                        static IContainer CellStyle(IContainer container)
                                        {
                                            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                                        }
                                    }
                                });
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                            x.Span(" de ");
                            x.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> GerarRelatorioLeitores()
        {
            var leitores = await _leitorService.ListarAsync();
            
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text("Sistema de Gestão de Biblioteca")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);

                            x.Item().Text("Relatório de Leitores")
                                .FontSize(16).Bold().FontColor(Colors.Black);

                            x.Item().Text($"Data de Geração: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(10).FontColor(Colors.Grey.Darken2);

                            x.Item().Text($"Total de Leitores Cadastrados: {leitores.Count()}")
                                .FontSize(14).Bold();

                            if (leitores.Any())
                            {
                                x.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1); // ID
                                        columns.RelativeColumn(4); // Nome
                                        columns.RelativeColumn(3); // CPF
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("ID").Bold();
                                        header.Cell().Element(CellStyle).Text("Nome").Bold();
                                        header.Cell().Element(CellStyle).Text("CPF").Bold();

                                        static IContainer CellStyle(IContainer container)
                                        {
                                            return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                        }
                                    });

                                    foreach (var leitor in leitores)
                                    {
                                        table.Cell().Element(CellStyle).Text(leitor.IdLeitor.ToString());
                                        table.Cell().Element(CellStyle).Text(leitor.Nome);
                                        table.Cell().Element(CellStyle).Text(leitor.CPF);

                                        static IContainer CellStyle(IContainer container)
                                        {
                                            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                                        }
                                    }
                                });
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                            x.Span(" de ");
                            x.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}

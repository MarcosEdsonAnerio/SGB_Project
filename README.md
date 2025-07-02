# AppConcurso

Sistema de gerenciamento de concursos públicos desenvolvido em Blazor Server (.NET 7), como parte da Atividade Avaliativa 3 da disciplina Linguagem de Programação Visual II.

## Descrição

O AppConcurso é uma aplicação web que permite o cadastro e gerenciamento de cargos, candidatos, inscrições, lançamento de notas e geração de resultados de concursos públicos. O sistema foi desenvolvido utilizando o framework Blazor Server, proporcionando uma interface moderna e interativa.

## Funcionalidades

`Cadastro de Cargo`: Permite adicionar novos cargos disponíveis para o concurso.
`Cadastro de Candidato`: Permite cadastrar candidatos interessados em participar do concurso.
`Cadastro de Candidato com Inscrição`: Permite cadastrar um novo candidato já realizando sua inscrição em um cargo.
`Inscrição de Candidato`: Permite que candidatos já cadastrados se inscrevam em cargos disponíveis.
`Lançamento de Notas`: Permite lançar notas de conhecimentos específicos e gerais para cada inscrição.
`Resultado Preliminar`: Exibe a classificação dos candidatos pela soma das notas.
`Resultado Final`: Exibe a classificação final considerando critérios de desempate (nota específica, nota geral, idade).

## Tecnologias Utilizadas

-	.NET 7
-	Blazor Server
-	Entity Framework Core
-	MySQL (banco de dados)
-	C#

## Como Executar

1.	Clone o repositório:
```
git clone https://github.com/seu-usuario/AppConcurso.git
```
3.	Configure a string de conexão do MySQL no arquivo appsettings.json:
```
"ConnectionStrings": {
"BaseConexaoMySql": "server=localhost;database=DBAppConcurso;user=root;password=suasenha"
}
```
3.	Crie o banco de dados e as tabelas utilizando o script SQL fornecido na pasta /docs ou no README.
4.	Acesse a aplicação em http://localhost:5000 (ou porta configurada).

## Estrutura do Projeto

`/Pages`: Páginas Blazor (cadastro, inscrição, notas, resultados)
`/Models`: Modelos de dados (Cargo, Candidato, Inscricao, Nota)
`/Services`: Serviços para manipulação de dados
`/Contexto`: Contexto do Entity Framework

## Telas Principais

-	Cadastro de Cargo
-	Cadastro de Candidato
-	Cadastro de Candidato com Inscrição
-	Inscrição de Candidato
-	Lançamento de Notas
-	Resultado Preliminar
-	Resultado Final

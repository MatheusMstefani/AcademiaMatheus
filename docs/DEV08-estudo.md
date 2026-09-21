# DEV08 — Camada de aplicação

Matheus Marques Stefani

## O que foi acrescentado

A solução agora tem seis projetos: Domain, Domain.Tests, Infrastructure,
Infrastructure.Tests, Application e Application.Tests. Não foi criada uma tela
de cadastro: a DEV08 pede a camada que a futura interface vai chamar.

O fluxo é: interface -> serviço de aplicação -> domínio e repositório -> banco.
Os serviços conhecem interfaces de repositório. Somente a configuração de
injeção de dependência escolhe as implementações de infraestrutura.

## As sete pastas da Application

- `DTOs`: dados recebidos e devolvidos para a interface, sem expor entidades.
- `Enums`: opções de plano, restrições, tipo e vínculo com nomes amigáveis.
- `Interfaces`: operações públicas dos quatro serviços.
- `Mappings`: conversão entre DTOs e entidades, usando as fábricas do domínio.
- `Security`: PasswordHasher com Argon2id e comparação de hash em tempo constante.
- `Services`: cadastro, consulta, edição, exclusão e validações que dependem do banco.
- `DependencyInjection`: registro dos serviços e das fábricas de repositórios.

## Exemplo: cadastrar um aluno

1. A futura tela preenche um AlunoDto com CPF, endereço cadastrado, senha e demais dados.
2. AlunoService verifica CPF e email duplicados e busca o logradouro no banco.
3. O mapeamento chama Aluno.Criar; idade, telefone, email, senha e arquivo são validados.
4. A senha original recebe um salt aleatório e é transformada em hash Argon2id.
5. O repositório salva o aluno. O serviço devolve o DTO com ID gerado e Senha nula.

A aplicação não altera o DTO enviado pelo chamador. A senha original fica apenas
na entrada; não deve ser registrada em logs. Na edição, senha nula/vazia mantém
a anterior e uma nova senha válida gera outro hash. O CPF é imutável na edição.
Um email nulo na edição preserva o anterior; no cadastro, o domínio exige email válido.

## Exemplo: cadastrar uma matrícula

O serviço recebe o ID do aluno no AlunoMatricula e busca o aluno real no banco.
Isso evita inventar uma senha para reconstruir um aluno a partir de um DTO de consulta.
A idade usada na validação também vem do banco, não de valores enviados pela tela.

Não é permitida nova matrícula se o aluno já tem uma ativa. Na edição também
não se permite ativar uma segunda matrícula ao mesmo tempo. Menores de 16 anos
e alunos com restrições precisam de laudo. O domínio calcula DataFim conforme
o plano: 1, 3, 6 ou 12 meses; uma DataFim enviada no DTO não substitui esse cálculo.

Arquivo informado deve ser válido: não se ignora uma foto ou laudo inválido.
Na edição, foto ou laudo nulo mantém o arquivo anterior. Os arrays devolvidos
são cópias, para que mudar um DTO não modifique a entidade em memória.

## Ajustes importantes em relação ao exemplo didático

Os valores zero dos enums são opções válidas: Administrador, CLT, Mensal e None.
Por isso, não usamos `!= default` para decidir se devemos atualizar esses campos.
É possível voltar a Mensal ou remover todas as restrições usando None.
Os DTOs de edição representam os dados completos do cadastro, não um PATCH genérico.

O verificador de senha limita os parâmetros lidos de um hash malformado, evitando
alocações arbitrariamente grandes. Os hashes novos usam três iterações, 64 MiB,
salt de 16 bytes e saída de 32 bytes. O paralelismo fica entre 1 e 4 neste projeto.
A biblioteca usada é [Konscious.Security.Cryptography.Argon2 1.3.1](https://www.nuget.org/packages/Konscious.Security.Cryptography.Argon2/1.3.1).

## Como configurar SQLite em um programa que consuma a aplicação

```csharp
using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddSingleton(new RepositoryConfig
{
    ConnectionString = "Data Source=academia.db;Foreign Keys=True",
    DatabaseType = DatabaseType.Sqlite
});
services.AddApplicationServices();
using var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();
var logradouros = scope.ServiceProvider.GetRequiredService<ILogradouroService>();
var lista = await logradouros.ObterTodosAsync();
```

Esse código é para a inicialização do programa, não para uma tela acessar
repositórios. O escopo libera os repositórios criados pelas fábricas.
MySQL e SQL Server continuam selecionáveis por RepositoryConfig, mas não foram
executados nesta entrega. Nenhum desses servidores precisa ser instalado para SQLite.

## Verificação e estudo

Execute `dotnet test` na pasta da solução. São 350 testes: 235 de domínio,
70 de infraestrutura e 45 da aplicação. Os da aplicação criam e removem um
banco temporário exclusivo por teste, sem reutilizar o banco dos prints.
Os antigos testes de infraestrutura continuam deixando dados fictícios em Dados.

Leia nesta ordem: LogradouroDto, ILogradouroService, LogradouroMappingExtensions,
LogradouroService, ApplicationDependencyInjection e LogradouroServiceTests.
Depois acompanhe AlunoService e MatriculaService para entender senhas e laudos.

Testes passando verificam os cenários escritos, não garantem um sistema pronto
para produção. Por exemplo, uma aplicação multiusuário precisaria tratar a
concorrência de duas matrículas criadas ao mesmo tempo com transações apropriadas.

## Entrega solicitada na DEV08

Uma print do explorador/gerenciador de soluções com os diretórios de Application
expandidos e o nome completo visível, além do ZIP completo baixado do GitHub.
Prints do SQLite e dos testes não são exigidos nesta atividade.

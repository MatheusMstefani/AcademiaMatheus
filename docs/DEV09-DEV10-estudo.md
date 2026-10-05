# Guia de estudo — DEV09 e DEV10

Matheus Marques Stefani

## O que mudou

Até a DEV08, o trabalho tinha regras de domínio, persistência ADO.NET e serviços
de aplicação. Agora existe uma janela para usar esses serviços: MAUI para Windows.
Foi instalado somente o workload Windows, sem emulador Android.

O fluxo é: botão na View → comando do ViewModel → interface da Application →
repositório da Infrastructure → banco. A resposta retorna como DTO; a interface
não precisa manipular entidades do domínio nem montar SQL.

## Arquivos para estudar nesta ordem

1. MauiProgram.cs: registra a configuração do banco, os serviços e a janela.
2. App.xaml e App.xaml.cs: estilos claro/escuro e aplicação do tema salvo.
3. AppShell.cs: menu lateral e navegação entre painel, logradouros e configurações.
4. ViewModels/BaseViewModel.cs: IsBusy impede operações repetidas; Status mostra
   o resultado; try/catch/finally garante que o indicador seja encerrado.
5. DashboardListViewModel.cs: consulta os quatro serviços para obter os totais.
6. LogradouroListViewModel.cs: consulta o banco, filtra e atualiza a coleção observável.
7. LogradouroViewModel.cs: carrega, valida, salva e exclui o DTO selecionado.
8. ConfiguracoesViewModel.cs e Services/ConfigurationHelper.cs: preferências,
   armazenamento seguro de senha e troca da conexão.

## MVVM

View é a tela. ViewModel guarda seus valores e comandos. ObservableObject avisa
a tela quando uma propriedade muda. ObservableCollection avisa quando a lista
muda. RelayCommand gera os comandos assíncronos usados pelos botões.
As Views desta implementação são construídas em C#; os estilos ficam no XAML.
Isso continua sendo uma interface MAUI com bindings e MVVM.

Exemplo: digitar um bairro altera Bairro no ViewModel. Salvar monta LogradouroDto
e chama AtualizarAsync. A Application valida e normaliza os dados; o repositório
executa SQL parametrizado. Depois a tela retorna à lista e consulta novamente.

## SQLite e troca de banco

O aplicativo começa com um arquivo novo em AppData. Por isso os contadores podem
estar zerados. Configurações mostra o caminho. A troca é testada antes de salvar;
um arquivo SQLite novo ganha automaticamente as tabelas.

AppServices abre um escopo de injeção de dependência por operação e o descarta ao
terminar. Assim os repositórios descartáveis não ficam presos na janela por toda
a vida do aplicativo, e a próxima operação usa a nova RepositoryConfig.

Cada troca incrementa uma revisão. Um formulário aberto antes da troca não pode
salvar silenciosamente no novo banco; precisa ser reaberto.

DatabaseSettings, na Application, monta strings com os builders dos provedores.
Isso evita concatenar senha e caminho diretamente em uma string. Os enums da
aplicação são mapeados explicitamente para os enums da infraestrutura.

## Tema e mensagens

Preferences guarda informações simples, como tema, provedor e caminho.
TemaPreferencesUpdatedMessage avisa o App sobre a escolha de Claro, Escuro ou
Sistema; o tema muda sem reiniciar. BancoPreferencesUpdatedMessage notifica a
alteração de banco. WeakReferenceMessenger não precisa manter uma referência
forte permanente às telas inscritas.

Senhas de servidores usam SecureStorage, não Preferences. O campo complementar
rejeita Password/Pwd para não guardar senhas ali por engano.

## O que foi verificado

- Compilação Windows em Debug e Release.
- 235 testes do domínio, 70 da infraestrutura e 59 da aplicação.
- Os testes novos cobrem enums, strings de conexão, validações e troca entre
  dois arquivos SQLite com cadastro, atualização e exclusão reais.
- Abertura do painel e menu confirmada pelas prints do próprio aluno.
- MySQL e SQL Server não foram executados; apenas montagem das configurações
  desses provedores foi testada.
- O percurso completo de interface e os vídeos devem ser conferidos seguindo
  o roteiro de entrega. Teste automatizado do serviço não é gravação da interface.

## Perguntas para treinar

- Qual a diferença entre DTO e entidade?
- Por que não montar SQL na View?
- O que acontece quando o CEP já existe?
- Por que um logradouro vinculado a aluno não pode ser excluído?
- O que muda ao selecionar outro SQLite?
- Por que a senha fica em SecureStorage e o tema em Preferences?
- Como IsBusy e o prazo de cancelamento evitam uma tela travada indefinidamente?

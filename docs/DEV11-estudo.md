# Estudo DEV11 - Matheus Marques Stefani

## O que foi acrescentado

As telas de Aluno e Colaborador agora usam os serviços construídos na DEV08.
AlunoListPage e ColaboradorListPage mostram coleções de DTOs, foto, contato e
endereço. Os formulários permitem criar, consultar, editar e excluir.

PessoaListPage e PessoaPage concentram a estrutura compartilhada. As classes
de Aluno e Colaborador especializam o que muda: admissão, tipo e vínculo são
exclusivos do colaborador. O mesmo vale para os ViewModels. Assim uma correção
no campo de telefone ou foto atende às duas telas.

## Caminho dos dados

O botão Salvar executa um AsyncRelayCommand no ViewModel, valida o formulário
e envia um AlunoDto ou ColaboradorDto à Application. O serviço valida as regras
do domínio e chama o repositório. A infraestrutura executa SQL parametrizado
no banco selecionado. A apresentação continua referenciando só Application.

PessoaCadastroValidation reúne os erros básicos para mostrá-los de uma vez.
As regras do domínio continuam sendo aplicadas pelo serviço, inclusive
verificação dos dígitos do CPF. Uma sequência de 11 números não garante CPF válido.

## Foto, datas e enums

- FotoToImageSourceConverter transforma ArquivoDto.Conteudo em imagem de tela.
  Cada leitura abre um novo MemoryStream para o controle poder carregar a foto.
- PhotoService abre Galeria ou Câmera só após clique do usuário e lê no máximo
  15 MB. Cancelar a seleção mantém a foto anterior.
- DateOnlyToDateTimeConverter adapta as datas do DTO ao DatePicker e vice-versa.
- EnumDisplayConverter usa o atributo Display do enum; por exemplo, Estagio
  aparece como Estagiário. O valor do enum, e não seu texto, é persistido.

As permissões de câmera foram declaradas nos manifestos. A compilação e execução
verificadas são Windows; os arquivos Android/iOS não significam teste nesses
dispositivos. O projeto permanece com alvo Windows para não instalar emuladores.

## Endereço e senha

O CEP precisa existir em Logradouros. Buscar CEP carrega o DTO e seu Id.
Ao trocar o CEP digitado, o vínculo antigo é limpo; isso evita salvar o endereço
anterior por engano. Número e complemento pertencem à pessoa.

CPF não pode ser alterado depois do cadastro. Na edição, a consulta não devolve
senha nem hash. Senha e confirmação vazias preservam a senha armazenada.
Uma nova senha passa por validação e hash antes da persistência.

## Busca, layout e navegação

CPF consulta o serviço com os 11 dígitos normalizados. Há também filtros por
nome, Id e e-mail. Colaborador acrescenta tipo e vínculo. A lista é recarregada
quando a página reaparece. O número de colunas varia conforme a largura da janela;
o tamanho da foto também considera o tipo de dispositivo.

O painel e o menu agora abrem Alunos e Colaboradores. Matrículas permanece para
a próxima etapa. O logo com o nome completo continua na imagem do painel.

## Correções anteriores

O estilo de ContentPage precisava de ApplyToDerivedTypes para alcançar as
classes concretas das páginas. O menu Shell também ganhou cores por tema.
Foram acrescentados botão de edição na lista de logradouros e erro para busca
de CEP sem dígitos.

Essas correções de código não alteram os vídeos já gravados. A evidência de
busca e de mudança do bairro exige uma nova gravação pelo aluno.

## Testes

371 testes passaram: 235 de domínio, 70 de infraestrutura e 66 da aplicação.
Os novos testes usam banco SQLite temporário, com imagem mínima de teste, para
verificar criação, busca por CPF, nome zé dos testes, troca dos bytes da foto e
exclusão. Também verificam validação agrupada e regras do colaborador.

## Para treinar

Explique a diferença entre Id e CPF; por que o CEP precisa ser buscado; o que
acontece ao deixar senha vazia na edição; por que os bytes da foto não são
diretamente uma ImageSource; e por que um teste do serviço não substitui o vídeo
de execução da interface.

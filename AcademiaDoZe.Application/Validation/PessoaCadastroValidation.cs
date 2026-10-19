// Matheus Marques Stefani
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using System.Net.Mail;
namespace AcademiaDoZe.Application.Validation;

// Valida o formulário de uma vez. As regras de domínio continuam nos serviços.
public static class PessoaCadastroValidation
{
    public static IReadOnlyList<string> Validate(PessoaDto person, string confirmation)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(person.Nome)) errors.Add("Informe o nome.");
        if (Digits(person.Cpf).Length != 11) errors.Add("CPF deve conter 11 dígitos.");
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (person.DataNascimento == default || person.DataNascimento > today.AddYears(-12))
            errors.Add("Informe o nascimento; a idade mínima é 12 anos.");
        if (Digits(person.Telefone).Length != 11) errors.Add("Telefone deve conter 11 dígitos com DDD.");
        if (string.IsNullOrWhiteSpace(person.Email) || !MailAddress.TryCreate(person.Email.Trim(), out var mail) ||
            mail.Address != person.Email.Trim() || !mail.Host.Contains('.'))
            errors.Add("Informe um e-mail válido.");
        if (person.Id == 0 || !string.IsNullOrEmpty(person.Senha))
        {
            if (string.IsNullOrWhiteSpace(person.Senha) || person.Senha.Length < 6 || !person.Senha.Any(char.IsUpper))
                errors.Add("A senha deve ter ao menos 6 caracteres e uma letra maiúscula.");
            if (person.Senha != confirmation) errors.Add("A confirmação da senha não confere.");
        }
        else if (!string.IsNullOrEmpty(confirmation)) errors.Add("Preencha a nova senha ou limpe a confirmação.");
        if (person.Endereco is null || person.Endereco.Id <= 0) errors.Add("Busque um CEP cadastrado para vincular o endereço.");
        if (string.IsNullOrWhiteSpace(person.Numero)) errors.Add("Informe o número do endereço.");
        if (person.Foto?.Conteudo is not { Length: > 0 }) errors.Add("Selecione uma foto.");
        else if (person.Foto.Conteudo.Length > 15 * 1024 * 1024) errors.Add("A foto deve ter no máximo 15 MB.");
        if (person is ColaboradorDto employee)
        {
            if (employee.DataAdmissao == default || employee.DataAdmissao > today) errors.Add("A admissão não pode ser futura.");
            if (!Enum.IsDefined(employee.Tipo) || !Enum.IsDefined(employee.Vinculo)) errors.Add("Selecione tipo e vínculo válidos.");
            if (employee.Tipo == AppColaboradorTipo.Administrador && employee.Vinculo != AppColaboradorVinculo.CLT)
                errors.Add("Administrador deve ter vínculo CLT.");
        }
        return errors;
    }
    public static string Digits(string? text) => new((text ?? "").Where(char.IsDigit).ToArray());
}

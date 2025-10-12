using Domain.Utils;
using Shared.Core.Helpers;

namespace Domain.Entities;

public class Rider : BaseEntity
{
    private readonly List<Rental> _rentals;

    protected Rider()
    {
    }

    private Rider(string identifier, string name, string cnpj, DateOnly birthDate, string cnh, string cnhType,
        string cnhImageUrl)
    {
        Identifier = identifier;
        Name = name;
        Cnpj = cnpj;
        BirthDate = birthDate;
        Cnh = cnh;
        CnhType = cnhType;
        CnhImageUrl = cnhImageUrl;
    }

    public string Identifier { get; private set; }
    public string Name { get; private set; }
    public string Cnpj { get; private set; }
    public DateOnly BirthDate { get; private set; }
    public string Cnh { get; private set; }
    public string CnhType { get; private set; }
    public string CnhImageUrl { get; private set; }
    public IReadOnlyCollection<Rental> Rentals => _rentals.AsReadOnly();

    public static Result<Rider> Create(string identifier, string name, string cnpj, DateOnly birthDate, string cnh,
        string cnhType,
        string cnhImageUrl)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return Result.Failure<Rider>(Error.Validation("RIDER.IDENTIFIER.EMPTY",
                "Identificador não pode ser nulo ou vazio"));

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Rider>(Error.Validation("RIDER.NAME.EMPTY", "Nome não pode ser nulo ou vazio"));

        if (string.IsNullOrWhiteSpace(cnpj) || !CnpjValidator.IsValidCnpj(cnpj))
            return Result.Failure<Rider>(Error.Validation("RIDER.CNPJ.INVALID", "CNPJ inválido"));

        if (string.IsNullOrWhiteSpace(cnh))
            return Result.Failure<Rider>(Error.Validation("RIDER.CNH.EMPTY", "CNH não pode ser nulo ou vazio"));

        if (string.IsNullOrWhiteSpace(cnhType))
            return Result.Failure<Rider>(Error.Validation("RIDER.CNH_TYPE.EMPTY",
                "Tipo de CNH não pode ser nulo ou vazio"));

        var normalizedCnhType = cnhType.ToUpper();
        if (!new[] { "A", "B", "A+B" }.Contains(normalizedCnhType))
            return Result.Failure<Rider>(Error.Validation("RIDER.CNH_TYPE.INVALID",
                "Tipo de CNH deve ser A, B, ou A+B"));

        if (string.IsNullOrWhiteSpace(cnhImageUrl))
            return Result.Failure<Rider>(Error.Validation("RIDER.CNH_IMAGE_URL.EMPTY",
                "URL da imagem da CNH não pode ser nulo ou vazio"));

        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
            return Result.Failure<Rider>(Error.Validation("RIDER.BIRTH_DATE.FUTURE",
                "Data de nascimento não pode ser no futuro"));

        return Result.Success(new Rider(identifier, name, cnpj, birthDate, cnh, normalizedCnhType, cnhImageUrl));
    }

    public void UpdateCnhImageUrl(string cnhImageUrl)
    {
        CnhImageUrl = cnhImageUrl;
    }
}
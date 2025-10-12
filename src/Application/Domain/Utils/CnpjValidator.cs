using FluentValidation;

namespace Domain.Utils;

public static class CnpjValidator
{
    public static bool IsValidCnpj(string cnpj)
    {
        if (cnpj.Length != 14 || cnpj.All(c => c == cnpj[0]))
            return false;

        int[] multiplier1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] multiplier2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        var tempCnpj = cnpj[..12];
        var sum = tempCnpj
            .Select((t, i) => int.Parse(t.ToString()) * multiplier1[i])
            .Sum();

        var remainder = sum % 11;
        if (remainder < 2)
            remainder = 0;
        else
            remainder = 11 - remainder;

        if (remainder != int.Parse(cnpj[12].ToString()))
            return false;

        tempCnpj += cnpj[12];
        sum = tempCnpj
            .Select((t, i) => int.Parse(t.ToString()) * multiplier2[i])
            .Sum();

        remainder = sum % 11;
        if (remainder < 2)
            remainder = 0;
        else
            remainder = 11 - remainder;

        return remainder == int.Parse(cnpj[13].ToString());
    }

    public class Validator : AbstractValidator<string>
    {
        public Validator()
        {
            RuleFor(cnpj => cnpj)
                .Must(IsValidCnpj)
                .WithMessage("CNPJ inválido");
        }
    }
}
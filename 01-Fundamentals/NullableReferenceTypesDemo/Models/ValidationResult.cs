namespace NullableReferenceTypesDemo.Models;

public sealed record ValidationResult(bool IsValid, string Message)
{
    public static ValidationResult Success()
    {
        return new ValidationResult(true, "Perfil pronto para cadastro.");
    }

    public static ValidationResult Failure(string message)
    {
        return new ValidationResult(false, message);
    }
}

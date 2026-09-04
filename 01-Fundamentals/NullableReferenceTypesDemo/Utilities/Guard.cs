using System;
using System.Diagnostics.CodeAnalysis;

namespace NullableReferenceTypesDemo.Utilities;

public static class Guard
{
    public static void AgainstNull<TValue>(
        [NotNull] TValue? value,
        string parameterName)
        where TValue : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(parameterName);
        }
    }

    public static void AgainstNullOrWhiteSpace(
        [NotNull] string? value,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Valor obrigatorio nao pode ser nulo ou vazio.", parameterName);
        }
    }
}

using System;
using System.Collections.Generic;
using NullableReferenceTypesDemo.Models;
using NullableReferenceTypesDemo.Services;

namespace NullableReferenceTypesDemo.Demo;

public sealed class NullableDemoRunner
{
    public void Run()
    {
        CustomerProfile completeProfile = new(
            Id: "CUS-001",
            DisplayName: " Ana Lima ",
            Email: " ANA@example.com ",
            PhoneNumber: null,
            Address: new ShippingAddress("Sao Paulo", "SP", "01001000"),
            PreferredChannels: new List<string> { "email" });

        CustomerProfile incompleteProfile = new(
            Id: "CUS-002",
            DisplayName: null,
            Email: null,
            PhoneNumber: null,
            Address: null,
            PreferredChannels: null);

        ProfileFormatter formatter = new();
        RegistrationValidator validator = new();
        ContactPreferenceResolver resolver = new();

        Console.WriteLine("Nullable Reference Types Demo");
        Console.WriteLine();

        PrintFormattedProfile(formatter, completeProfile);
        PrintValidation(validator, completeProfile);
        PrintValidation(validator, incompleteProfile);
        PrintContactPreference(resolver, completeProfile);
        PrintContactPreference(resolver, incompleteProfile);
        PrintFlowAnalysisExample(completeProfile);
    }

    private static void PrintFormattedProfile(ProfileFormatter formatter, CustomerProfile profile)
    {
        Console.WriteLine("== Guard clauses + operadores de null ==");
        Console.WriteLine(formatter.CreateSummary(profile));
        Console.WriteLine();
    }

    private static void PrintValidation(RegistrationValidator validator, CustomerProfile? profile)
    {
        ValidationResult result = validator.Validate(profile);

        Console.WriteLine("== Analise de fluxo do compilador ==");
        Console.WriteLine(result.IsValid ? "Cadastro valido." : result.Message);
        Console.WriteLine();
    }

    private static void PrintContactPreference(ContactPreferenceResolver resolver, CustomerProfile profile)
    {
        ContactPreference preference = resolver.Resolve(profile);

        Console.WriteLine("== Operador ??= para fallback ==");
        Console.WriteLine($"Canal: {preference.Channel}");
        Console.WriteLine($"Destino: {preference.Destination}");
        Console.WriteLine();
    }

    private static void PrintFlowAnalysisExample(CustomerProfile? profile)
    {
        Console.WriteLine("== Pattern matching remove nullable do fluxo ==");

        if (profile?.Email?.Trim() is string email)
        {
            Console.WriteLine($"Email normalizado no fluxo: {email.ToLowerInvariant()}");
            return;
        }

        Console.WriteLine("Email ausente.");
    }
}

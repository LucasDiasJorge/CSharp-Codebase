namespace GoodOrderApi.Domain.ValueObjects;

// ✅ REGRA 8: Máximo duas variáveis de instância
// ✅ REGRA 3: Encapsular primitivos

/// <summary>
/// Value Object que agrupa informações de contato.
/// </summary>
public sealed class ContactInfo
{
    public Email Email { get; }
    public PhoneNumber Phone { get; }

    private ContactInfo(Email email, PhoneNumber phone)
    {
        Email = email;
        Phone = phone;
    }

    public static ContactInfo Create(string email, string phone)
    {
        return new ContactInfo(
            Email.Create(email),
            PhoneNumber.Create(phone)
        );
    }
}

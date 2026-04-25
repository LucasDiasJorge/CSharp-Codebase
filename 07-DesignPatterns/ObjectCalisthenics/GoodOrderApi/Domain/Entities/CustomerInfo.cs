using GoodOrderApi.Domain.ValueObjects;

namespace GoodOrderApi.Domain.Entities;

// ✅ REGRA 8: Máximo duas variáveis de instância (agrupamos em objetos)
// ✅ REGRA 9: Expor comportamento em vez de getters/setters

/// <summary>
/// Entidade que representa informações do cliente em um pedido.
/// </summary>
public sealed class CustomerInfo
{
    public PersonName Name { get; }
    public CustomerContact Contact { get; }

    private CustomerInfo(PersonName name, CustomerContact contact)
    {
        Name = name;
        Contact = contact;
    }

    public static CustomerInfo Create(string name, string email, string phone, string street, string city, string zipCode)
    {
        var personName = PersonName.Create(name);
        var contact = CustomerContact.Create(email, phone, street, city, zipCode);
        return new CustomerInfo(personName, contact);
    }
}

/// <summary>
/// Value Object que agrupa contato e endereço do cliente.
/// ✅ REGRA 8: Apenas duas variáveis de instância.
/// </summary>
public sealed class CustomerContact
{
    public ContactInfo ContactInfo { get; }
    public Address Address { get; }

    private CustomerContact(ContactInfo contactInfo, Address address)
    {
        ContactInfo = contactInfo;
        Address = address;
    }

    public static CustomerContact Create(string email, string phone, string street, string city, string zipCode)
    {
        var contact = ValueObjects.ContactInfo.Create(email, phone);
        var address = ValueObjects.Address.Create(street, city, zipCode);
        return new CustomerContact(contact, address);
    }
}

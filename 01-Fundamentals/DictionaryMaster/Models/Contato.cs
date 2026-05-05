namespace DictionaryMaster.Models;

public class Contato
{
    public Contato(string telefone, string email)
    {
        Telefone = telefone;
        Email = email;
    }

    public string Telefone { get; private set; }

    public string Email { get; private set; }

    public void Atualizar(string telefone, string email)
    {
        Telefone = telefone;
        Email = email;
    }
}

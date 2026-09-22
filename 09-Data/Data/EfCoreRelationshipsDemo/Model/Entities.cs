namespace EfCoreRelationshipsDemo.Model;

/// <summary>
/// Um-para-muitos: um blog tem vários posts. O lado "muitos" carrega a chave
/// estrangeira, e é sempre assim — a coleção fica do lado "um".
/// </summary>
public sealed class Blog
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<Post> Posts { get; set; } = new List<Post>();
}

public sealed class Post
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public int BlogId { get; set; }

    public Blog? Blog { get; set; }

    /// <summary>
    /// Muitos-para-muitos por skip navigation: a partir do EF Core 5 não é preciso
    /// declarar a entidade de junção, embora a tabela continue existindo no banco.
    /// </summary>
    public List<Tag> Tags { get; set; } = new List<Tag>();
}

public sealed class Tag
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<Post> Posts { get; set; } = new List<Post>();
}

/// <summary>
/// Um-para-um com <see cref="AuthorProfile"/>. O lado dependente é quem tem a chave
/// estrangeira — e, no um-para-um, escolher qual dos dois é o dependente é uma decisão
/// de modelagem, não um detalhe: é ela que define quem pode existir sem o outro.
/// </summary>
public sealed class Author
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public AuthorProfile? Profile { get; set; }

    /// <summary>
    /// Owned type: vira colunas na tabela de <see cref="Author"/>, sem tabela nem
    /// chave próprias. Serve para valor que não tem identidade — dois endereços iguais
    /// são o mesmo endereço.
    /// </summary>
    public Address Address { get; set; } = new Address();
}

public sealed class AuthorProfile
{
    public int Id { get; set; }

    public string Bio { get; set; } = string.Empty;

    /// <summary>Chave estrangeira: este é o lado dependente.</summary>
    public int AuthorId { get; set; }

    public Author? Author { get; set; }
}

/// <summary>
/// Tipo possuído (owned). Não tem <c>Id</c> de propósito: sua identidade é a do dono.
/// </summary>
public sealed class Address
{
    public string Street { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;
}

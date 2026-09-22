namespace DatabaseMigrationsDemo.Model;

/// <summary>
/// Cliente. O modelo evolui em três migrations, e cada etapa representa um tipo
/// diferente de mudança de schema — ver o README.
/// </summary>
public sealed class Customer
{
    public int Id { get; set; }

    /// <summary>
    /// Nome completo. Mantido mesmo depois da divisão em
    /// <see cref="FirstName"/>/<see cref="LastName"/>: durante o expand, as duas
    /// formas coexistem para que a versão antiga da aplicação continue funcionando.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Acrescentado na segunda migration. Coluna NOVA e NULA: mudanca aditiva, que a
    /// versao anterior da aplicacao ignora sem quebrar.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Etapa "expand" da divisao do nome. Nula de proposito: a coluna nasce vazia, e
    /// preenchida por um backfill na propria migration, e convive com <see cref="Name"/>
    /// ate que toda a aplicacao tenha migrado.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>Par de <see cref="FirstName"/>; ver a etapa expand no README.</summary>
    public string? LastName { get; set; }
}

namespace ResilientHttpSdk;

public sealed record Product(string Id, string Name, decimal Price);

public sealed record CreateProductRequest(string Name, decimal Price);

/// <summary>
/// A superfície pública do SDK. Interface, para quem consome poder substituir em teste
/// sem precisar de handler HTTP falso.
///
/// Todo método recebe <see cref="CancellationToken"/>: um SDK que não aceita o token
/// impede o chamador de desistir, e nenhum "wrapper" resolve isso depois.
/// </summary>
public interface IProductsClient
{
    Task<Product> GetAsync(string id, CancellationToken cancellationToken = default);

    Task<Product> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
}

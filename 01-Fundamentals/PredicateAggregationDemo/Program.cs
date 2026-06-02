using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PredicateAggregationDemo.Expressions;
using PredicateAggregationDemo.Models;

namespace PredicateAggregationDemo;

public static class Program
{
    public static void Main()
    {
        IReadOnlyList<Product> products = CreateProducts();

        DemonstrateInMemoryPredicates();
        DemonstrateDynamicAndAggregation(products);
        DemonstrateDynamicOrAggregation(products);
    }

    private static void DemonstrateInMemoryPredicates()
    {
        PrintTitle("1. Func<T, bool> para colecoes em memoria");

        List<int> numbers = Enumerable.Range(1, 20).ToList();
        Func<int, bool> isEven = number => number % 2 == 0;
        Func<int, bool> isGreaterThanTen = number => number > 10;
        Func<int, bool> isEvenAndGreaterThanTen =
            number => isEven(number) && isGreaterThanTen(number);

        List<int> filteredNumbers = numbers.Where(isEvenAndGreaterThanTen).ToList();

        Console.WriteLine("Numeros pares e maiores que dez:");
        Console.WriteLine(string.Join(", ", filteredNumbers));
    }

    private static void DemonstrateDynamicAndAggregation(IReadOnlyList<Product> products)
    {
        PrintTitle("2. Expression<Func<T, bool>> com filtros AND dinamicos");

        ProductFilter filter = new ProductFilter(
            activeOnly: true,
            searchTerm: "Notebook",
            maximumPrice: 5000m);

        Expression<Func<Product, bool>> predicate = CreateProductPredicate(filter);
        List<Product> filteredProducts = products
            .AsQueryable()
            .Where(predicate)
            .ToList();

        Console.WriteLine("Arvore de expressao composta:");
        Console.WriteLine(predicate);
        Console.WriteLine();
        Console.WriteLine("Produtos encontrados:");
        PrintProducts(filteredProducts);
    }

    private static void DemonstrateDynamicOrAggregation(IReadOnlyList<Product> products)
    {
        PrintTitle("3. Expression<Func<T, bool>> com filtros OR dinamicos");

        List<string> selectedCategories = new List<string>
        {
            "Acessorios",
            "Livros"
        };

        Expression<Func<Product, bool>> predicate = PredicateBuilder.False<Product>();

        foreach (string category in selectedCategories)
        {
            string selectedCategory = category;
            predicate = predicate.Or(product => product.Category == selectedCategory);
        }

        List<Product> filteredProducts = products
            .AsQueryable()
            .Where(predicate)
            .ToList();

        Console.WriteLine("Arvore de expressao composta:");
        Console.WriteLine(predicate);
        Console.WriteLine();
        Console.WriteLine("Produtos encontrados:");
        PrintProducts(filteredProducts);
    }

    private static Expression<Func<Product, bool>> CreateProductPredicate(ProductFilter filter)
    {
        Expression<Func<Product, bool>> predicate = PredicateBuilder.True<Product>();

        if (filter.ActiveOnly)
        {
            predicate = predicate.And(product => product.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            string searchTerm = filter.SearchTerm;
            predicate = predicate.And(product =>
                product.Name.Contains(searchTerm)
                || product.Description.Contains(searchTerm));
        }

        if (filter.MaximumPrice.HasValue)
        {
            decimal maximumPrice = filter.MaximumPrice.Value;
            predicate = predicate.And(product => product.Price <= maximumPrice);
        }

        return predicate;
    }

    private static IReadOnlyList<Product> CreateProducts()
    {
        return new List<Product>
        {
            new Product("Notebook Pro", "Notebook para trabalho remoto", "Eletronicos", 4500m, true),
            new Product("Notebook Gamer", "Notebook com placa de video dedicada", "Eletronicos", 7200m, true),
            new Product("Notebook Basico", "Notebook de entrada fora de linha", "Eletronicos", 2600m, false),
            new Product("Mouse sem fio", "Acessorio para notebook", "Acessorios", 180m, true),
            new Product("Suporte ajustavel", "Acessorio ergonomico para Notebook", "Acessorios", 250m, true),
            new Product("Clean Code", "Livro sobre boas praticas de desenvolvimento", "Livros", 120m, true),
            new Product("Monitor 4K", "Monitor para produtividade", "Eletronicos", 2100m, true)
        };
    }

    private static void PrintProducts(IEnumerable<Product> products)
    {
        foreach (Product product in products)
        {
            Console.WriteLine(
                $"- {product.Name} | {product.Category} | R$ {product.Price:N2} | Ativo: {product.IsActive}");
        }
    }

    private static void PrintTitle(string title)
    {
        Console.WriteLine();
        Console.WriteLine($"=== {title} ===");
    }
}

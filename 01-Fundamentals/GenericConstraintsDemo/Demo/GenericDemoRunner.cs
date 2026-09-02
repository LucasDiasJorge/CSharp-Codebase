using System;
using System.Collections.Generic;
using GenericConstraintsDemo.Contracts;
using GenericConstraintsDemo.Infrastructure;
using GenericConstraintsDemo.Models;
using GenericConstraintsDemo.Services;
using GenericConstraintsDemo.Utilities;

namespace GenericConstraintsDemo.Demo;

public sealed class GenericDemoRunner
{
    public void Run()
    {
        InMemoryRepository<Product> repository = CreateRepository();

        Console.WriteLine("Generic Constraints Demo");
        Console.WriteLine();

        DemonstrateGenericClass(repository);
        DemonstrateGenericMethod(repository);
        DemonstrateStructConstraint();
        DemonstrateCovariance(repository);
        DemonstrateContravariance(repository);
    }

    private static InMemoryRepository<Product> CreateRepository()
    {
        InMemoryRepository<Product> repository = new();

        Product cleanArchitecture = EntityFactory.CreateEntity<Product>(
            "BK-001",
            static (Product product) =>
            {
                product.Name = "Clean Architecture";
                product.Category = "Books";
                product.Price = 150m;
            });

        Product csharpDepth = EntityFactory.CreateEntity<Product>(
            "BK-002",
            static (Product product) =>
            {
                product.Name = "C# in Depth";
                product.Category = "Books";
                product.Price = 180m;
            });

        repository.Add(cleanArchitecture);
        repository.Add(csharpDepth);

        return repository;
    }

    private static void DemonstrateGenericClass(InMemoryRepository<Product> repository)
    {
        CatalogReport<Product> report = new(repository);

        Console.WriteLine("== Classe generica com constraints ==");
        report.PrintSummary();
        Console.WriteLine();
    }

    private static void DemonstrateGenericMethod(InMemoryRepository<Product> repository)
    {
        Product? product = repository.FindById("BK-001");

        Console.WriteLine("== Metodo generico com class + interface + new() ==");
        Console.WriteLine(product is null
            ? "Produto nao encontrado."
            : ReferenceInspector.DescribeReference(product));
        Console.WriteLine();
    }

    private static void DemonstrateStructConstraint()
    {
        StockSnapshot snapshot = new(Available: 12, Reserved: 4);
        string description = MetricFormatter.FormatStruct(snapshot);

        Console.WriteLine("== Constraint struct + IFormattable ==");
        Console.WriteLine(description);
        Console.WriteLine();
    }

    private static void DemonstrateCovariance(InMemoryRepository<Product> repository)
    {
        IReadableCatalog<Product> productCatalog = repository;
        IReadableCatalog<IEntity> entityCatalog = productCatalog;
        IReadOnlyCollection<IEntity> entities = entityCatalog.GetAll();

        Console.WriteLine("== Variance: covariance com out T ==");
        Console.WriteLine($"Catalogo de Product lido como catalogo de IEntity: {entities.Count} entidades.");
        Console.WriteLine();
    }

    private static void DemonstrateContravariance(InMemoryRepository<Product> repository)
    {
        Product? product = repository.FindById("BK-002");

        if (product is null)
        {
            return;
        }

        EntityAuditSink<IEntity> auditSink = new();
        IEntitySink<Product> productSink = auditSink;
        productSink.Write(product);

        Console.WriteLine("== Variance: contravariance com in T ==");
        foreach (string message in auditSink.Messages)
        {
            Console.WriteLine(message);
        }
    }
}

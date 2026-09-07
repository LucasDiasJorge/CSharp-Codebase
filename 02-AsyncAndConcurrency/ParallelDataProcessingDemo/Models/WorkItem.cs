namespace ParallelDataProcessingDemo.Models;

/// <summary>
/// Unidade de trabalho processada pelas estrategias. <see cref="Seed"/> alimenta o calculo
/// da carga de CPU, para que o resultado dependa do item e nao possa ser otimizado fora.
/// </summary>
public sealed record WorkItem(int Identifier, int Seed);

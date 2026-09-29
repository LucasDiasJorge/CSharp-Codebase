using System;
using System.Collections.Generic;
using System.Linq;

namespace Acme.EnumExtensions.Generator;

/// <summary>
/// O modelo que atravessa o pipeline do gerador.
///
/// <b>Igualdade por valor aqui não é detalhe: é o que faz o cache funcionar.</b> O
/// pipeline incremental compara o modelo produzido em uma execução com o da anterior; se
/// forem iguais, as etapas seguintes não rodam. Um modelo que compare por referência
/// (uma classe sem <c>Equals</c>, ou um que contenha <c>ImmutableArray&lt;T&gt;</c>, cuja
/// igualdade é por referência) derruba o cache sem produzir erro nenhum — o gerador
/// continua correto e fica lento.
/// </summary>
public sealed class EnumToGenerate : IEquatable<EnumToGenerate>
{
    public EnumToGenerate(string? namespaceName, string name, string accessibility, IReadOnlyList<string> members)
    {
        Namespace = namespaceName;
        Name = name;
        Accessibility = accessibility;
        Members = members;
    }

    public string? Namespace { get; }

    public string Name { get; }

    public string Accessibility { get; }

    public IReadOnlyList<string> Members { get; }

    public bool Equals(EnumToGenerate? other)
    {
        if (other is null)
        {
            return false;
        }

        return Namespace == other.Namespace
            && Name == other.Name
            && Accessibility == other.Accessibility
            && Members.SequenceEqual(other.Members);
    }

    public override bool Equals(object? obj) => obj is EnumToGenerate other && Equals(other);

    public override int GetHashCode()
    {
        int hash = 17;

        hash = (hash * 31) + (Namespace?.GetHashCode() ?? 0);
        hash = (hash * 31) + Name.GetHashCode();
        hash = (hash * 31) + Accessibility.GetHashCode();

        foreach (string member in Members)
        {
            hash = (hash * 31) + member.GetHashCode();
        }

        return hash;
    }
}

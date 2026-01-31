using System.ComponentModel;
using System.Reflection;
using OrderRuleConsole.Models;

namespace OrderRuleConsole.Services;

public static class OrderRuleEngine
{
    public static void Apply(RuleInput ruleConfig, Order order)
    {
        if (ruleConfig == null) throw new ArgumentNullException(nameof(ruleConfig));
        if (order == null) throw new ArgumentNullException(nameof(order));

        if (order.Operation != ruleConfig.Operation)
            return;

        if (string.IsNullOrWhiteSpace(ruleConfig.ParameterKey))
            return;

        if (!TryResolvePath(order, ruleConfig.ParameterKey!, out object targetObject, out PropertyInfo? targetProperty))
            return;

        if (targetProperty == null || !targetProperty.CanWrite)
            return;

        if (!ShouldApply(ruleConfig, order, targetObject, targetProperty))
            return;

        targetProperty.SetValue(targetObject, ruleConfig.ParameterValue);
    }

    // Decides which comparative path to use; returns true if allowed to proceed
    private static bool ShouldApply(RuleInput input, Order rootOrder, object targetObject, PropertyInfo targetProperty)
    {
        bool hasKey = !string.IsNullOrWhiteSpace(input.ExceptionParameterKey);
        bool hasLiteral = !string.IsNullOrWhiteSpace(input.ExceptionParameterValue);

        if (!hasKey && !hasLiteral)
            return true;

        if (hasKey)
            return EvaluateWithExceptionKeyCondition(input, rootOrder, targetObject, targetProperty);

        return EvaluateWithLiteralCondition(input, targetObject, targetProperty);
    }

    private static bool EvaluateWithExceptionKeyCondition(RuleInput input, Order rootOrder, object targetObject, PropertyInfo targetProperty)
    {
        if (string.IsNullOrWhiteSpace(input.ExceptionParameterKey)) return true;
        if (!TryResolvePath(rootOrder, input.ExceptionParameterKey!, out object exceptionObject, out PropertyInfo? exceptionProperty) || exceptionProperty == null)
            return false; // cannot evaluate -> block change (safer)
            
        object? exceptionPropertyValue = exceptionProperty.GetValue(exceptionObject); // value from ExceptionParameterKey

        // If a literal ExceptionParameterValue is provided, compare exceptionPropValue vs literal.
        // Else compare target main property vs exception property (previous behavior).
        if (!string.IsNullOrWhiteSpace(input.ExceptionParameterValue))
        {
            object? literal = input.ExceptionParameterValue;
            if (exceptionPropertyValue != null)
            {
                try
                {
                    Type t = exceptionPropertyValue.GetType();
                    var conv = TypeDescriptor.GetConverter(t);
                    if (conv.CanConvertFrom(typeof(string)))
                        literal = conv.ConvertFrom(input.ExceptionParameterValue);
                    else
                        literal = Convert.ChangeType(input.ExceptionParameterValue, t);
                }
                catch { /* fallback keep string */ }
            }
            return EvaluateCondition(input.RuleException, exceptionPropertyValue, literal);
        }
        else
        {
            object? primaryValue = targetProperty.GetValue(targetObject);
            return EvaluateCondition(input.RuleException, primaryValue, exceptionPropertyValue);
        }
    }

    private static bool EvaluateWithLiteralCondition(RuleInput input, object targetObject, PropertyInfo targetProperty)
    {
        if (string.IsNullOrWhiteSpace(input.ExceptionParameterValue)) return true;
        object? primaryValue = targetProperty.GetValue(targetObject);

        object? comparisonValue = input.ExceptionParameterValue; // start as string
        if (primaryValue != null)
        {
            try
            {
                Type t = primaryValue.GetType();
                TypeConverter conv = TypeDescriptor.GetConverter(t);
                if (conv.CanConvertFrom(typeof(string)))
                    comparisonValue = conv.ConvertFrom(input.ExceptionParameterValue);
                else
                    comparisonValue = Convert.ChangeType(input.ExceptionParameterValue, t);
            }
            catch
            {
                // keep string fallback
            }
        }
        return EvaluateCondition(input.RuleException, primaryValue, comparisonValue);
    }

    /// <summary>
    /// Resolve um caminho dot-separated a partir de um objeto raiz até a propriedade alvo.
    /// </summary>
    /// <param name="root">Objeto raiz onde a busca começa (por exemplo, o <c>Order</c>).</param>
    /// <param name="path">Caminho dot-separated para a propriedade (ex.: "Parent.Child.Prop").</param>
    /// <param name="targetOwner">(out) O objeto que possui fisicamente a propriedade final. Este é o objeto onde a propriedade será lida/escrita.</param>
    /// <param name="targetProp">(out) A PropertyInfo da propriedade final a ser lida/escrita.</param>
    /// <returns>True se o caminho foi resolvido com sucesso e <paramref name="targetProp"/> foi localizado; caso contrário false.</returns>
    private static bool TryResolvePath(object root, string path, out object targetOwner, out PropertyInfo? targetProp)
    {
        // Inicialmente devolvemos o root como dono por padrão
        targetOwner = root;
        targetProp = null;
        string[] segments = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0) return false;

        object current = root;
        PropertyInfo? prop = null;
        for (int i = 0; i < segments.Length; i++)
        {
            string name = segments[i];
            prop = current.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null)
                return false;

            // Se não for o último segmento, navegamos para o objeto filho
            if (i < segments.Length - 1)
            {
                object? next = prop.GetValue(current);
                if (next == null) return false; // não é possível navegar caminho nulo
                current = next;
            }
        }

        // Ao final, 'current' refere-se ao objeto que contém a propriedade final
        // Ex.: para path "A.B.C", targetOwner será a instância de B e targetProp será a PropertyInfo de C
        targetOwner = current;
        targetProp = prop;
        return true;
    }

    private static bool EvaluateCondition(string? op, object? a, object? b)
    {
        if (string.IsNullOrWhiteSpace(op)) return true; // backward compatible: no condition means proceed
        op = op.Trim();

        bool EqualsObj(object? x, object? y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            // Case-insensitive for strings
            if (x is string sx && y is string sy)
                return string.Equals(sx, sy, StringComparison.OrdinalIgnoreCase);

            if (x.GetType() == y.GetType())
                return x.Equals(y);

            // attempt simple conversion from y to x's type
            try
            {
                Type tx = x.GetType();
                TypeConverter conv = TypeDescriptor.GetConverter(tx);
                if (conv.CanConvertFrom(y.GetType()))
                {
                    object? y2 = conv.ConvertFrom(y);
                    if (y2 is string sy2 && x is string sx2)
                        return string.Equals(sx2, sy2, StringComparison.OrdinalIgnoreCase);
                    return x.Equals(y2);
                }
                // fallback string compare (case-insensitive)
                return string.Equals(x.ToString(), y.ToString(), StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        return op switch
        {
            "==" => EqualsObj(a, b),
            "!=" => !EqualsObj(a, b),
            _ => true // unknown operator -> do not block
        };
    }

}

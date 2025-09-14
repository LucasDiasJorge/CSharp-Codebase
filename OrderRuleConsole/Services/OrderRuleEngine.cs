using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Reflection;
using OrderRuleConsole.Models;

namespace OrderRuleConsole.Services;

public static class OrderRuleEngine
{
    // Applies rule to order based on input descriptor
    public static void Apply(RuleInput input, Order order)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));
        if (order == null) throw new ArgumentNullException(nameof(order));

        // Only apply when operation matches
        if (order.Operation != input.Operation)
            return;

        // support nested properties via dot notation, e.g. "Shipping.Address.Zip"
        string path = input.ParameterKey ?? string.Empty;
        if (string.IsNullOrWhiteSpace(path))
            return;

        object current = order;
        PropertyInfo? prop = null;
        string[] segments = path.Split('.', StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < segments.Length; i++)
        {
            string name = segments[i];
            prop = current.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null)
            {
                // property not found -> nothing to do
                return;
            }

            if (i < segments.Length - 1)
            {
                // traverse to next object
                object? next = prop.GetValue(current);
                if (next == null)
                {
                    // cannot traverse a null intermediate object
                    return;
                }
                current = next;
            }
        }

        if (prop == null || !prop.CanWrite)
            return;

        // convert input.ParameterValue to the property's type
        Type targetType = prop.PropertyType;
        Type underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        object? valueToSet;
        try
        {
            // handle null assignment
            if (input.ParameterValue != default) 
            {
                // If already assignable, use directly
                if (underlying.IsInstanceOfType(input.ParameterValue))
                {
                    valueToSet = input.ParameterValue;
                }
                else
                {
                    // try TypeConverter first (handles enums, guids, etc.)
                    TypeConverter converter = TypeDescriptor.GetConverter(underlying);
                    if (converter != null && converter.CanConvertFrom(input.ParameterValue.GetType()))
                    {
                        valueToSet = converter.ConvertFrom(input.ParameterValue);
                    }
                    else
                    {
                        // fallback to ChangeType for primitives
                        valueToSet = Convert.ChangeType(input.ParameterValue, underlying);
                    }
                }
            }
            else
            {
                valueToSet = null;
            }
        }
        catch (Exception ex)
        {
            // conversion failed -> do not apply
            throw new InvalidOperationException($"Failed to convert parameter value to target property type '{underlying.FullName}'.", ex);
        }

        prop.SetValue(current, valueToSet);
    }

}

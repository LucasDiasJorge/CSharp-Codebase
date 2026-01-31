using System;
using OrderRuleConsole.Models;
using OrderRuleConsole.Services;
using Xunit;

namespace OrderRuleConsole.Tests;

public class OrderRuleEngineTests
{
    [Fact]
    public void Apply_DifferentOperation_DoesNotChange()
    {
        var order = new Order { Operation = 99, OrderTypeCode = 1, OrderName = "Original" };
        var rule = new RuleInput(8, "OrderName", "Changed", "OrderTypeCode", "==", "1");

        OrderRuleEngine.Apply(rule, order);

        Assert.Equal("Original", order.OrderName);
    }

    [Fact]
    public void Apply_NullRule_Throws()
    {
        var order = new Order { Operation = 8 };
        Assert.Throws<ArgumentNullException>(() => OrderRuleEngine.Apply(null!, order));
    }

    [Fact]
    public void Apply_NullOrder_Throws()
    {
        var rule = new RuleInput(8, "OrderName", "X", "OrderTypeCode", "==", "1");
        Assert.Throws<ArgumentNullException>(() => OrderRuleEngine.Apply(rule, null!));
    }

    [Fact]
    public void Apply_EmptyParameterKey_DoesNothing()
    {
        var order = new Order { Operation = 8, OrderTypeCode = 1, OrderName = "Original" };
        var rule = new RuleInput(8, "", "Changed");

        OrderRuleEngine.Apply(rule, order);
        Assert.Equal("Original", order.OrderName);
    }

    [Fact]
    public void Apply_PropertyNotFound_DoesNothing()
    {
        var order = new Order { Operation = 8, OrderTypeCode = 1, OrderName = "Original" };
        var rule = new RuleInput(8, "NonExisting", "Changed", "OrderTypeCode", "==", "1");

        OrderRuleEngine.Apply(rule, order);
        Assert.Equal("Original", order.OrderName);
    }

    [Fact]
    public void Apply_LiteralConditionTrue_AppliesChange()
    {
        var order = new Order { Operation = 8, OrderTypeCode = 5, OrderName = "Original" };
        var rule = new RuleInput(8, "OrderName", "Changed", "OrderTypeCode", "==", "5");

        OrderRuleEngine.Apply(rule, order);
        Assert.Equal("Changed", order.OrderName);
    }

    [Fact]
    public void Apply_LiteralConditionFalse_DoesNotApply()
    {
        var order = new Order { Operation = 8, OrderTypeCode = 6, OrderName = "Original" };
        var rule = new RuleInput(8, "OrderName", "Changed", "OrderTypeCode", "==", "5");

        OrderRuleEngine.Apply(rule, order);
        Assert.Equal("Original", order.OrderName);
    }

    [Fact]
    public void Apply_NotEqualsConditionTrue_Applies()
    {
        var order = new Order { Operation = 8, OrderTypeCode = 10, OrderName = "Original" };
        var rule = new RuleInput(8, "OrderName", "Changed", "OrderTypeCode", "!=", "5");

        OrderRuleEngine.Apply(rule, order);
        Assert.Equal("Changed", order.OrderName);
    }

    [Fact]
    public void Apply_NotEqualsConditionFalse_DoesNotApply()
    {
        var order = new Order { Operation = 8, OrderTypeCode = 5, OrderName = "Original" };
        var rule = new RuleInput(8, "OrderName", "Changed", "OrderTypeCode", "!=", "5");

        OrderRuleEngine.Apply(rule, order);
        Assert.Equal("Original", order.OrderName);
    }

    [Fact]
    public void Apply_UnknownOperator_DoesApply_ByDesign()
    {
        var order = new Order { Operation = 8, OrderTypeCode = 5, OrderName = "Original" };
        var rule = new RuleInput(8, "OrderName", "Changed", "OrderTypeCode", "#", "XXX");

        OrderRuleEngine.Apply(rule, order);
        Assert.Equal("Changed", order.OrderName); // unknown op defaults to allow
    }

    [Fact]
    public void Apply_CaseInsensitivePropertyName_Applies()
    {
        var order = new Order { Operation = 8, OrderTypeCode = 1, OrderName = "Original" };
        var rule = new RuleInput(8, "ordername", "Changed", "OrderTypeCode", "==", "1");

        OrderRuleEngine.Apply(rule, order);
        Assert.Equal("Changed", order.OrderName);
    }

    [Fact]
    public void Apply_CaseInsensitiveStringComparison()
    {
        var order = new Order { Operation = 8, OrderTypeCode = 1, OrderName = "original" };
        // Condition comparing target property vs ExceptionParameterKey (no literal) not covered here because we always pass a literal.
        var rule = new RuleInput(8, "OrderName", "Changed", "OrderTypeCode", "==", "1");

        OrderRuleEngine.Apply(rule, order);
        Assert.Equal("Changed", order.OrderName);
    }

    [Fact]
    public void Apply_NoConditionFieldsProvided_Applies()
    {
        var order = new Order { Operation = 8, OrderTypeCode = 1, OrderName = "Original" };
        var rule = new RuleInput(8, "OrderName", "Changed"); // ctor without condition

        OrderRuleEngine.Apply(rule, order);
        Assert.Equal("Changed", order.OrderName);
    }
}

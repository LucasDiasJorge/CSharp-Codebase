using FluentAssertions;
using OrderRuleConsole.Models;
using Xunit;
using OrderRuleConsole.Services;

namespace OrderRuleConsole.Tests;

public class OrderRuleEngineTests
{
    private class Order
    {
        // Espelha propriedades esperadas pelo motor (ajuste se o seu Order real diferir)
        public int Operation { get; set; }
        public string? OrderTypeCode { get; set; }
        public string? OrderName { get; set; }
        public string? Other { get; set; }
    }

    [Fact]
    public void Apply_Should_Update_Property_When_Operation_Matches_And_No_Condition()
    {
        var order = new Order { Operation = 8, OrderTypeCode = "X" };
        var input = new RuleInput(8, "OrderTypeCode", "15");

        OrderRuleEngine.Apply(input, order);

        order.OrderTypeCode.Should().Be("15");
    }

    [Fact]
    public void Apply_Should_Not_Update_When_Operation_Differs()
    {
        var order = new Order { Operation = 99, OrderTypeCode = "X" };
        var input = new RuleInput(8, "OrderTypeCode", "15");

        OrderRuleEngine.Apply(input, order);

        order.OrderTypeCode.Should().Be("X");
    }

    [Fact]
    public void Apply_Should_Not_Update_When_ParameterKey_Empty()
    {
        var order = new Order { Operation = 8, OrderTypeCode = "X" };
        var input = new RuleInput(8, "", "15");

        OrderRuleEngine.Apply(input, order);

        order.OrderTypeCode.Should().Be("X");
    }

    [Fact]
    public void Apply_Should_Update_When_ExceptionKey_Condition_Is_True()
    {
        var order = new Order { Operation = 8, OrderTypeCode = "X", OrderName = "Name" };
        var input = new RuleInput(
            operation: 8,
            field: "OrderTypeCode",
            value: "15",
            conditionField: "OrderName",
            comparison: "==",
            targetField: "Name" // usado como ExceptionParameterValue pelo construtor atual
        );

        // Ajuste: construtor atual mapeia:
        // field -> ParameterKey
        // value -> ParameterValue
        // conditionField -> ExceptionParameterKey
        // comparison -> RuleException
        // targetField -> ExceptionParameterValue

        OrderRuleEngine.Apply(input, order);

        order.OrderTypeCode.Should().Be("15");
    }

    [Fact]
    public void Apply_Should_Not_Update_When_ExceptionKey_Condition_Is_False()
    {
        var order = new Order { Operation = 8, OrderTypeCode = "X", OrderName = "DIFF" };
        var input = new RuleInput(
            operation: 8,
            field: "OrderTypeCode",
            value: "15",
            conditionField: "OrderName",
            comparison: "==",
            targetField: "Name"
        );

        OrderRuleEngine.Apply(input, order);

        order.OrderTypeCode.Should().Be("X");
    }

    [Fact]
    public void Apply_Should_Update_When_Literal_Condition_Equals_Current_Value()
    {
        var order = new Order { Operation = 8, OrderTypeCode = "AAA" };
        var input = new RuleInput(8, "OrderTypeCode", "BBB")
        {
            RuleException = "==",
            ExceptionParameterValue = "AAA"
        };

        OrderRuleEngine.Apply(input, order);

        order.OrderTypeCode.Should().Be("BBB");
    }

    [Fact]
    public void Apply_Should_Not_Update_When_Literal_Condition_Fails()
    {
        var order = new Order { Operation = 8, OrderTypeCode = "ZZZ" };
        var input = new RuleInput(8, "OrderTypeCode", "BBB")
        {
            RuleException = "==",
            ExceptionParameterValue = "AAA"
        };

        OrderRuleEngine.Apply(input, order);

        order.OrderTypeCode.Should().Be("ZZZ");
    }

    [Fact]
    public void Apply_Should_Update_When_Operator_Not_Specified()
    {
        var order = new Order { Operation = 8, OrderTypeCode = "X" };
        var input = new RuleInput(8, "OrderTypeCode", "Y")
        {
            // Sem RuleException => ShouldApply retorna true
        };

        OrderRuleEngine.Apply(input, order);

        order.OrderTypeCode.Should().Be("Y");
    }

    [Fact]
    public void Apply_Should_Handle_NotEqual_Operator()
    {
        var order = new Order { Operation = 8, OrderTypeCode = "X", OrderName = "Alpha" };
        var input = new RuleInput(
            operation: 8,
            field: "OrderTypeCode",
            value: "Y",
            conditionField: "OrderName",
            comparison: "!=",
            targetField: "Beta"
        );

        // OrderName = Alpha != Beta => condição true => aplica
        OrderRuleEngine.Apply(input, order);

        order.OrderTypeCode.Should().Be("Y");
    }

    [Fact]
    public void Apply_Should_Not_Update_When_Path_Invalid()
    {
        var order = new Order { Operation = 8, OrderTypeCode = "Orig" };
        var input = new RuleInput(8, "NaoExiste", "Novo");

        OrderRuleEngine.Apply(input, order);

        order.OrderTypeCode.Should().Be("Orig");
    }
}
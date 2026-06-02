using System;
using System.Linq.Expressions;

namespace PredicateAggregationDemo.Expressions;

public static class PredicateBuilder
{
    public static Expression<Func<T, bool>> True<T>()
    {
        return item => true;
    }

    public static Expression<Func<T, bool>> False<T>()
    {
        return item => false;
    }

    public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        return Combine(left, right, Expression.AndAlso);
    }

    public static Expression<Func<T, bool>> Or<T>(
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        return Combine(left, right, Expression.OrElse);
    }

    private static Expression<Func<T, bool>> Combine<T>(
        Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right,
        Func<Expression, Expression, BinaryExpression> merge)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        ArgumentNullException.ThrowIfNull(merge);

        ParameterExpression parameter = left.Parameters[0];
        Expression? replacedBody = new ParameterReplaceVisitor(right.Parameters[0], parameter)
            .Visit(right.Body);

        if (replacedBody is null)
        {
            throw new InvalidOperationException("Nao foi possivel combinar os predicados.");
        }

        BinaryExpression body = merge(left.Body, replacedBody);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private sealed class ParameterReplaceVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _source;
        private readonly ParameterExpression _target;

        public ParameterReplaceVisitor(
            ParameterExpression source,
            ParameterExpression target)
        {
            _source = source;
            _target = target;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _source ? _target : base.VisitParameter(node);
        }
    }
}

namespace CustomMiddleware.Middlewares.Extensions;

public static class UseCustomMiddlewaresExtensions
{
    public static IApplicationBuilder UseCustomMiddlewares(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<RequestTimingMiddleware>();
        builder.UseMiddleware<CustomHeaderMiddleware>();
        builder.UseMiddleware<CorrelationIdMiddleware>();
        
        return builder;
    }
}
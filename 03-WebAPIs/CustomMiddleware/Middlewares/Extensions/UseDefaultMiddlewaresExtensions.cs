using CustomMiddleware.Middlewares;
using Microsoft.Extensions.Options;

public static class UseDefaultMiddlewaresExtensions
{
    public static IApplicationBuilder UseDefaultMiddlewares(this IApplicationBuilder builder)
    {
        builder.UseExceptionHandler("/Error");
        builder.UseMiddleware<RequestResponseLoggingMiddleware>();
        
        return builder;
    }
}
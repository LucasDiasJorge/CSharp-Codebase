using CountryRulesTimeProviderDemo.Contracts;
using CountryRulesTimeProviderDemo.Resolvers;
using CountryRulesTimeProviderDemo.Rules;
using CountryRulesTimeProviderDemo.Services;
using ScalarDocumentationSdk.Extensions;

namespace CountryRulesTimeProviderDemo;

public static class Program
{
	public static void Main()
	{
		WebApplicationBuilder builder = WebApplication.CreateBuilder();
		IServiceCollection services = builder.Services;

		ConfigureServices(services);

		WebApplication app = builder.Build();
		ConfigureEndpoints(app);
		app.Run();
	}

	private static void ConfigureServices(IServiceCollection services)
	{
		services.AddSingleton<TimeProvider>(TimeProvider.System);

		services.AddSingleton<BrazilRules>();
		services.AddSingleton<EnglandRules>();
		services.AddSingleton<KoreaRules>();

		services.AddSingleton<ICountryRuleResolver, BrazilRuleResolver>();
		services.AddSingleton<ICountryRuleResolver, EnglandRuleResolver>();
		services.AddSingleton<ICountryRuleResolver, KoreaRuleResolver>();
		services.AddSingleton<UnsupportedCountryRuleResolver>();

		services.AddSingleton<ICountryTimeService, CountryTimeService>();

		services.AddScalarDocumentationSdk(options =>
		{
			options.DocumentName = "v1";
			options.Title = "Country Rules Time Provider API";
			options.ScalarRoutePrefix = "/scalar";
			options.OpenApiRoutePattern = "/openapi/{documentName}.json";
			options.MapOnlyInDevelopment = true;
		});
	}

	private static void ConfigureEndpoints(WebApplication app)
	{
		app.MapGet(
			"/api/time/{rule}",
			(string rule, ICountryTimeService countryTimeService) =>
			{
				CountryTimeResponse? response;
				CountryTimeErrorResponse? errorResponse;

				bool success = countryTimeService.TryGetNow(rule, out response, out errorResponse);
				if (!success || response is null)
				{
					return Results.NotFound(errorResponse);
				}

				return Results.Ok(response);
			});

		app.MapGet(
			"/api/time",
			(ICountryTimeService countryTimeService) =>
			{
				IReadOnlyCollection<string> supportedRules = countryTimeService.GetSupportedRules();
				return Results.Ok(supportedRules);
			});

		app.MapScalarDocumentationSdk();
	}
}

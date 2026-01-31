using System;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using FluentValidationUserApi.Validators;

namespace FluentValidationUserApi.Extensions
{
    /// <summary>
    /// Extension methods to register application services in a clean, centralised place.
    /// This mimics a "Startup" style registration while keeping Program.cs concise.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers FluentValidation and scans for validators in the current assembly.
        /// Call this from Program.cs to keep bootstrap logic tidy.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <returns>The original <see cref="IServiceCollection"/> for chaining.</returns>
        public static IServiceCollection AddApplicationValidation(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // Enable automatic model validation using FluentValidation.
            services.AddFluentValidationAutoValidation();
            services.AddFluentValidationClientsideAdapters();

            // Register all validators from the assembly that contains UserValidator.
            services.AddValidatorsFromAssemblyContaining<UserValidator>();

            return services;
        }
    }
}

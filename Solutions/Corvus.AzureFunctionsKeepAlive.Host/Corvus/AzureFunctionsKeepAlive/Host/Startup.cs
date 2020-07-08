// <copyright file="Startup.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;
using Corvus.AzureFunctionsKeepAlive.Host.Internal;
using Corvus.Identity.ManagedServiceIdentity.ClientAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: Microsoft.Azure.WebJobs.Hosting.WebJobsStartup(typeof(Corvus.AzureFunctionsKeepAlive.Host.Startup))]

namespace Corvus.AzureFunctionsKeepAlive.Host
{
    /// <summary>
    /// Initialisation code for the function.
    /// </summary>
    public class Startup : IWebJobsStartup
    {
        /// <summary>
        /// Configures the web job builder, primarily by adding services to the container.
        /// </summary>
        /// <param name="builder">The builder to add to.</param>
        public void Configure(IWebJobsBuilder builder)
        {
            IServiceCollection services = builder.Services;
            services.AddLogging();
            services.AddSingleton(sp =>
            {
                IConfiguration config = sp.GetRequiredService<IConfiguration>();
                TargetUri[] targetUris = config.GetSection("TargetUris").Get<TargetUri[]>();

                if (targetUris == null)
                {
                    throw new InvalidOperationException("Could not find or process the configuration section 'TargetUris'");
                }

                return targetUris;
            });

            services.AddAzureManagedIdentityBasedTokenSource(
                sp =>
                {
                    IConfiguration config = sp.GetRequiredService<IConfiguration>();
                    return new AzureManagedIdentityTokenSourceOptions
                    {
                        AzureServicesAuthConnectionString = config["AzureServicesAuthConnectionString"],
                    };
                });

            services.AddApplicationInsightsInstrumentationTelemetry();
            services.AddInstrumentation();

            services.AddSingleton<UriPoller>();
        }
    }
}

// <copyright file="KeepAliveContainerBindings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.AzureFunctionsKeepAlive.Specs.Bindings
{
    using Corvus.AzureFunctionsKeepAlive.Host.Internal;
    using Corvus.Identity.ManagedServiceIdentity.ClientAuthentication;
    using Corvus.Testing.SpecFlow;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Bindings to set up the test container with workflow services so that test setup/teardown can be performed.
    /// </summary>
    [Binding]
    public static class KeepAliveContainerBindings
    {
        /// <summary>
        /// Setup the endjin container for a feature.
        /// </summary>
        /// <param name="featureContext">The feature context for the current feature.</param>
        /// <remarks>We expect features run in parallel to be executing in separate app domains.</remarks>
        [BeforeScenario("@perScenarioContainer", Order = ContainerBeforeScenarioOrder.PopulateServiceCollection)]
        public static void SetupFeature(ScenarioContext featureContext)
        {
            ContainerBindings.ConfigureServices(
                featureContext,
                services =>
                {
                    IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
                        .AddEnvironmentVariables()
                        .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);

                    IConfiguration root = configurationBuilder.Build();

                    services.AddSingleton(root);

                    services.AddLogging();

                    services.AddAzureManagedIdentityBasedTokenSource(
                        sp =>
                        {
                            IConfiguration config = sp.GetRequiredService<IConfiguration>();
                            return new AzureManagedIdentityTokenSourceOptions
                            {
                                AzureServicesAuthConnectionString = config["AzureServicesAuthConnectionString"],
                            };
                        });

                    services.AddSingleton<UriPoller>();
                });
        }
    }
}

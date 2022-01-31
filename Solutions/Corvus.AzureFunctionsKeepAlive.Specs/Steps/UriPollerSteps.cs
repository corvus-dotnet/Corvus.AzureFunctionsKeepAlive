// <copyright file="UriPollerSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.AzureFunctionsKeepAlive.Specs.Steps
{
    using System;
    using System.Threading.Tasks;
    using Corvus.AzureFunctionsKeepAlive.Host.Internal;
    using Corvus.Testing.SpecFlow;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class UriPollerSteps
    {
        private readonly UriPoller uriPoller;
        private readonly ScenarioContext scenarioContext;

        public UriPollerSteps(ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
            this.uriPoller = ContainerBindings.GetServiceProvider(scenarioContext).GetRequiredService<UriPoller>();
        }

        [When("I poll the endpoint called '(.*)' at Url '(.*)'")]
        public async Task WhenIPollTheEndpointCalledAtUrl(string endpointName, string endpointUrl)
        {
            var target = new TargetUri
            {
                Name = endpointName,
                Uri = endpointUrl,
            };

            try
            {
                await this.uriPoller.PollUriAsync(target).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.scenarioContext.Set(ex);
            }
        }

        [Then("the operation should complete successfully")]
        public void ThenTheOperationShouldCompleteSuccessfully()
        {
            Assert.IsFalse(
                this.scenarioContext.TryGetValue<Exception>(out Exception ex),
                $"Expected no exception, but an exception was thrown: {ex}");
        }
    }
}
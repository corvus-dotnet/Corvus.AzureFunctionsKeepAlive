// <copyright file="UriPoller.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.AzureFunctionsKeepAlive.Host.Internal
{
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Corvus.Identity.ManagedServiceIdentity.ClientAuthentication;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Used to poll a specific URL, with optional authentication.
    /// </summary>
    public class UriPoller
    {
        private readonly HttpClient httpClient;
        private readonly IServiceIdentityTokenSource tokenSource;
        private readonly ILogger<UriPoller> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UriPoller"/> class.
        /// </summary>
        /// <param name="tokenSource">Token source for obtaining authentication tokens.</param>
        /// <param name="logger">The logger.</param>
        public UriPoller(IServiceIdentityTokenSource tokenSource, ILogger<UriPoller> logger)
        {
            this.tokenSource = tokenSource ?? throw new ArgumentNullException(nameof(tokenSource));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.httpClient = HttpClientFactory.Create();
        }

        /// <summary>
        /// Polls the given Url.
        /// </summary>
        /// <param name="target">The target Url information.</param>
        /// <returns>A task that can be used to track the progress of the operation.</returns>
        public async Task PollUriAsync(TargetUri target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (string.IsNullOrEmpty(target.Uri))
            {
                throw new ArgumentException(nameof(target.Uri));
            }

            var message = new HttpRequestMessage(HttpMethod.Get, target.Uri);

            if (!string.IsNullOrEmpty(target.ResourceForAadAuthentication))
            {
                string authToken = await this.tokenSource.GetAccessToken(target.ResourceForAadAuthentication).ConfigureAwait(false);
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            }

            var sw = new Stopwatch();
            sw.Start();
            HttpResponseMessage response = await this.httpClient.SendAsync(message).ConfigureAwait(false);
            sw.Stop();

            if (response.IsSuccessStatusCode)
            {
                this.logger.LogInformation(
                    "Successfully requested endpoint '{endpointName}' in '{requestTime}'ms",
                    target.Name,
                    sw.ElapsedMilliseconds);
            }
            else
            {
                this.logger.LogWarning(
                    "Failed to request endpoint '{endpointName}'. Status code '{responseCode}' was returned in '{requestTime}'ms.",
                    target.Name,
                    response.StatusCode,
                    sw.ElapsedMilliseconds);
            }
        }
    }
}
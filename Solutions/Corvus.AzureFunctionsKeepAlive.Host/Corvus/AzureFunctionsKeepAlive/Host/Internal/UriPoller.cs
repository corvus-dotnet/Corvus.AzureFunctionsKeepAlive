// <copyright file="UriPoller.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.AzureFunctionsKeepAlive.Host.Internal
{
    using System.Diagnostics;
    using System.Net;
    using System.Threading.Tasks;
    using Corvus.Identity.ManagedServiceIdentity.ClientAuthentication;
    using Microsoft.Build.Framework;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Used to poll a specific URL, with optional authentication.
    /// </summary>
    public class UriPoller
    {
        private readonly IServiceIdentityTokenSource tokenSource;
        private readonly ILogger<UriPoller> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UriPoller"/> class.
        /// </summary>
        /// <param name="tokenSource">Token source for obtaining authentication tokens.</param>
        /// <param name="logger">The logger.</param>
        public UriPoller(IServiceIdentityTokenSource tokenSource, ILogger<UriPoller> logger)
        {
            this.tokenSource = tokenSource;
            this.logger = logger;
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
                throw new System.ArgumentNullException(nameof(target));
            }

            if (string.IsNullOrEmpty(target.Uri))
            {
                throw new System.ArgumentException(nameof(target.Uri));
            }

            var request = (HttpWebRequest)WebRequest.Create(target.Uri);
            request.Method = "GET";

            if (!string.IsNullOrEmpty(target.ResourceForAadAuthentication))
            {
                string authToken = await this.tokenSource.GetAccessToken(target.ResourceForAadAuthentication).ConfigureAwait(false);
                request.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {authToken}");
            }

            var sw = new Stopwatch();
            sw.Start();

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                sw.Stop();

                this.logger.LogInformation(
                    "Successfully requested endpoint '{endpointName}' in '{requestTime}'ms",
                    target.Name,
                    sw.ElapsedMilliseconds);
            }
            catch (WebException ex)
            {
                sw.Stop();

                var exceptionResponse = (HttpWebResponse)ex.Response;

                this.logger.LogInformation(
                    "Failed to request endpoint '{endpointName}'. Status code '{responseCode}' was returned in '{requestTime}'ms with message '{responseMessage}'",
                    exceptionResponse.StatusCode,
                    target.Name,
                    sw.ElapsedMilliseconds,
                    exceptionResponse.StatusDescription);
            }
        }
    }
}

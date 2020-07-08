// <copyright file="KeepAliveTrigger.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.AzureFunctionsKeepAlive.Host
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Corvus.AzureFunctionsKeepAlive.Host.Internal;
    using Corvus.Monitoring.Instrumentation;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class for the keep alive trigger function.
    /// </summary>
    public class KeepAliveTrigger
    {
        private readonly TargetUri[] targetUris;
        private readonly UriPoller uriPoller;
        private readonly IExceptionsInstrumentation<KeepAliveTrigger> exceptionsInstrumentation;

        /// <summary>
        /// Creates a new instance of the <see cref="KeepAliveTrigger"/> class.
        /// </summary>
        /// <param name="targetUris">Configution for the function.</param>
        /// <param name="uriPoller">Polling helper.</param>
        /// <param name="exceptionsInstrumentation">The exception instrumentation service.</param>
        public KeepAliveTrigger(
            TargetUri[] targetUris,
            UriPoller uriPoller,
            IExceptionsInstrumentation<KeepAliveTrigger> exceptionsInstrumentation)
        {
            this.targetUris = targetUris
                ?? throw new ArgumentNullException(nameof(targetUris));

            this.uriPoller = uriPoller
                ?? throw new ArgumentNullException(nameof(uriPoller));

            this.exceptionsInstrumentation = exceptionsInstrumentation
                ?? throw new ArgumentNullException(nameof(exceptionsInstrumentation));
        }

        /// <summary>
        /// The keep alive trigger function.
        /// </summary>
        /// <param name="myTimer">The timer controlling function execution.</param>
        /// <param name="log">The logger to use.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [FunctionName("Corvus-KeepAlive")]
        public async Task Run([TimerTrigger("0 */3 * * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            IEnumerable<Task> pollingTasks = this.targetUris.Select(target => this.PollAsync(target, log));

            await Task.WhenAll(pollingTasks).ConfigureAwait(false);
        }

        private async Task PollAsync(TargetUri targetUri, ILogger log)
        {
            try
            {
                await this.uriPoller.PollUriAsync(targetUri).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var instrumentationProperties = new Dictionary<string, string>
                {
                    { "TargetUriName", targetUri?.Name },
                    { "TargetUri", targetUri?.Uri },
                };

                this.exceptionsInstrumentation.ReportException(
                    ex,
                    new AdditionalInstrumentationDetail(instrumentationProperties, null));

                log.LogError(
                    "Unexpected exception executing logger for target '{targetName}': {exception}",
                    targetUri.Name,
                    ex.ToString());
            }
        }
    }
}

// <copyright file="TargetUri.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.AzureFunctionsKeepAlive.Host.Internal
{
    /// <summary>
    /// Information about an endpoint to poll.
    /// </summary>
    public class TargetUri
    {
        /// <summary>
        /// Gets or sets the Uri to poll.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Gets or sets the name of the Uri.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Id of the AAD resource to use when authenticating.
        /// </summary>
        public string ResourceForAadAuthentication { get; set; }
    }
}
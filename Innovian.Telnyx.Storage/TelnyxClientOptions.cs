//  -------------------------------------------------------------
//  Copyright (c) 2023 Innovian Corporation. All rights reserved.
//  -------------------------------------------------------------

using Microsoft.Extensions.Options;

namespace Innovian.Telnyx.Storage;

/// <summary>
/// Options used to configure the Telnyx storage client.
/// </summary>
public sealed record TelnyxClientOptions : IOptions<TelnyxClientOptions>
{
    /// <summary>
    /// The default configured <typeparamref name="TOptions" /> instance
    /// </summary>
    public TelnyxClientOptions Value => this;

    /// <summary>
    /// The API key used to authenticate to Telnyx.
    /// </summary>
    public required string TelnyxApiKey { get; set; }
}
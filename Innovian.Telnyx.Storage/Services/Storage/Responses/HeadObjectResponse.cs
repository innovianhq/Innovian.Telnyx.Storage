//  -------------------------------------------------------------
//  Copyright (c) 2023 Innovian Corporation. All rights reserved.
//  -------------------------------------------------------------

namespace Innovian.Telnyx.Storage.Services.Storage.Responses;

public sealed record HeadObjectResponse(string? AcceptRanges = null, string? Etag = null, string? RequestId = null, DateTime? Date = null, string? Server = null);
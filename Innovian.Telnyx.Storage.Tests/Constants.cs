//  -------------------------------------------------------------
//  Copyright (c) 2023 Innovian Corporation. All rights reserved.
//  -------------------------------------------------------------

namespace Innovian.Telnyx.Storage.Tests;

internal static class Constants
{
    /// <summary>
    /// The name of the bucket used for testing.
    /// </summary>
    public static string BucketName = $"integration-testing-{Guid.NewGuid().ToString()[..8].ToLower()}";
}
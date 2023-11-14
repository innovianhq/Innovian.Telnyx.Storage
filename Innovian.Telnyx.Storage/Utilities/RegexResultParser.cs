﻿// -------------------------------------------------------------
// Copyright (c) 2023 Innovian Corporation. All rights reserved.
// -------------------------------------------------------------

using System.Text.RegularExpressions;

namespace Innovian.Telnyx.Storage.Utilities;

internal sealed class RegexResultParser
{
    /// <summary>
    /// Extracts the bucket location out of an XML string.
    /// </summary>
    /// <param name="xml">The XML to parse.</param>
    /// <returns></returns>
    public string? ParseBucketLocationResult(string xml)
    {
        var regex = new Regex(@""">[a-z0-9\-]+<\/LocationConstraint>");
        var match = regex.Match(xml);
        var matchedValue = match.Value.Replace("</LocationConstraint>", string.Empty);
        matchedValue = matchedValue.Substring(2);

        return matchedValue;
    }
}
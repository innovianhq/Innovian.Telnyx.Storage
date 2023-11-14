// -------------------------------------------------------------
// Copyright (c) 2023 Innovian Corporation. All rights reserved.
// -------------------------------------------------------------

namespace Innovian.Telnyx.Storage.Exceptions;

public class ErrorException : Exception
{
    public ErrorException(string code) : base(code) {}

    public string RequestId { get; init; }

    public string HostId { get; init; }
}
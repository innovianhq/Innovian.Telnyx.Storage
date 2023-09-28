//  -------------------------------------------------------------
//  Copyright (c) 2023 Innovian Corporation. All rights reserved.
//  -------------------------------------------------------------

namespace Innovian.Telnyx.Storage.Exceptions;

public sealed class ValidityFailureException : Exception
{
    public ValidityFailureException(string message): base(message) {}
}
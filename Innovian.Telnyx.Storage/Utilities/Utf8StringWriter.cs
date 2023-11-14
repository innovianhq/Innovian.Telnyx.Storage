// -------------------------------------------------------------
// Copyright (c) 2023 Innovian Corporation. All rights reserved.
// -------------------------------------------------------------

using System.Text;

namespace Innovian.Telnyx.Storage.Utilities;

internal class Utf8StringWriter : StringWriter
{
    /// <summary>Gets the <see cref="T:System.Text.Encoding" /> in which the output is written.</summary>
    /// <returns>The <see langword="Encoding" /> in which the output is written.</returns>
    public override Encoding Encoding => new UTF8Encoding(false);
}
// -------------------------------------------------------------
// Copyright (c) 2023 Innovian Corporation. All rights reserved.
// -------------------------------------------------------------

using System.Xml.Serialization;

namespace Innovian.Telnyx.Storage.Services.Storage.Responses;

[XmlRoot("Error")]
public sealed record ErrorResponse
{
    [XmlElement("Code")]
    public string Code { get; set; } = string.Empty;

    [XmlElement("HostId")]
    public string HostId { get; set; } = string.Empty;

    [XmlElement("RequestId")]
    public string RequestId { get; set; } = string.Empty;
}
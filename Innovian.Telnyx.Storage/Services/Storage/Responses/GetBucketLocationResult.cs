//  -------------------------------------------------------------
//  Copyright (c) 2023 Innovian Corporation. All rights reserved.
//  -------------------------------------------------------------

using System.Xml.Serialization;

namespace Innovian.Telnyx.Storage.Services.Storage.Responses;

[XmlRoot("LocationConstraint")]
public sealed record GetBucketLocationResult
{
    [XmlElement("LocationConstraint")]
    public string LocationConstraint { get; init; } = string.Empty;
}
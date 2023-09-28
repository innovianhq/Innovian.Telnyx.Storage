//  -------------------------------------------------------------
//  Copyright (c) 2023 Innovian Corporation. All rights reserved.
//  -------------------------------------------------------------

using System.Xml.Serialization;

namespace Innovian.Telnyx.Storage.Services.Storage.Requests;

[XmlRoot("Delete", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
public sealed record DeleteObjectsRequest
{
    [XmlElement("Object")]
    public List<DeleteObject> Objects { get; set; } = new();
}

public record DeleteObject
{
    [XmlElement("Key")]
    public string Key { get; init; }
}
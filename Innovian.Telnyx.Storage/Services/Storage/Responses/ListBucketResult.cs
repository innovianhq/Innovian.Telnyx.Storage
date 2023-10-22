//  -------------------------------------------------------------
//  Copyright (c) 2023 Innovian Corporation. All rights reserved.
//  -------------------------------------------------------------

using System.Xml.Serialization;

namespace Innovian.Telnyx.Storage.Services.Storage.Responses;

[XmlRoot("ListBucketResult", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
public sealed record ListBucketResult
{
    [XmlElement("Name")]
    public string Name { get; init; } = string.Empty;

    [XmlElement("Contents", IsNullable = true)]
    public Content[]? Contents { get; init; }
}

public sealed record Content
{
    [XmlElement("Key")]
    public string Key { get; init; } = string.Empty;

    [XmlElement("Size")]
    public int Size { get; init; }

    [XmlElement("LastModified")]
    public string LastModifiedRaw { get; init; } = string.Empty;

    [XmlIgnore]
    public DateTime LastModified => DateTime.Parse(LastModifiedRaw);

    //[XmlElement("LastModified")]
    //public DateTime LastModified { get; init; }
}
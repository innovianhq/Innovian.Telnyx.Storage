// -------------------------------------------------------------
// Copyright (c) 2023 Innovian Corporation. All rights reserved.
// -------------------------------------------------------------

using System.Xml.Serialization;

namespace Innovian.Telnyx.Storage.Services.Storage.Responses;

[XmlRoot("ListAllMyBucketsResult", Namespace="http://s3.amazonaws.com/doc/2006-03-01/")]
public sealed record ListAllMyBucketsResult
{
    [XmlArray("Buckets")]
    public List<Bucket> Buckets { get; set; } = new();

    [XmlElement("Owner")]
    public Owner Owner { get; set; }
}

[XmlRoot("Bucket")]
public sealed record Bucket
{
    [XmlElement("CreationDate")]
    public DateTime CreationDate { get; set; }

    [XmlElement("Name")]
    public string Name { get; set; }
}

public sealed record Owner
{
    [XmlElement("DisplayName")]
    public string DisplayName { get; set; }

    [XmlElement("ID")]
    public string ID { get; set; }
}
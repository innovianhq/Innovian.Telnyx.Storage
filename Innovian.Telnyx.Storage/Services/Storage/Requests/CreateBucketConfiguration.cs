//  -------------------------------------------------------------
//  Copyright (c) 2023 Innovian Corporation. All rights reserved.
//  -------------------------------------------------------------

using System.Xml.Serialization;

namespace Innovian.Telnyx.Storage.Services.Storage.Requests;

[XmlRoot(Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
public sealed record CreateBucketConfiguration
{
    [XmlElement("LocationConstraint")]
    public string LocationConstraint { get; init; }
}
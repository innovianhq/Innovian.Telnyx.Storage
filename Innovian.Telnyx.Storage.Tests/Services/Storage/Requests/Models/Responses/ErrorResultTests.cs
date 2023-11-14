// -------------------------------------------------------------
// Copyright (c) 2023 Innovian Corporation. All rights reserved.
// -------------------------------------------------------------

using System.Xml.Serialization;
using Innovian.Telnyx.Storage.Services.Storage.Responses;

namespace Innovian.Telnyx.Storage.Tests.Services.Storage.Requests.Models.Responses;

[TestClass]
public class ErrorResultTests
{
    [TestMethod]
    public void ParseErrorResult()
    {
        const string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Error><Code>NoSuchBucket</Code><BucketName>integration-testing-1255a4d8</BucketName><RequestId>tx000000b46d854832e1324-00655311b7-289ba-da1</RequestId><HostId>289ba-da1-us-central-1</HostId></Error>\r\n";

        var serializer = new XmlSerializer(typeof(ErrorResponse));
        using var sr = new StringReader(xml);
        var result = (ErrorResponse) serializer.Deserialize(sr);

        Assert.IsNotNull(result);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Code));
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.RequestId));
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.HostId));
    }
}
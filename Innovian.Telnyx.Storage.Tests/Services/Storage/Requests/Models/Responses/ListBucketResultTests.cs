//  -------------------------------------------------------------
//  Copyright (c) 2023 Innovian Corporation. All rights reserved.
//  -------------------------------------------------------------

using System.Xml.Serialization;
using Innovian.Telnyx.Storage.Services.Storage.Responses;

namespace Innovian.Telnyx.Storage.Tests.Services.Storage.Requests.Models.Responses;

[TestClass]
public class ListBucketResultTests
{
    [TestMethod]
    public void ParsePopulatedBucket()
    {
        const string xml =
            @"<ListBucketResult xmlns=""http://s3.amazonaws.com/doc/2006-03-01/""><Name>integration-test-bucket</Name><Contents><Key>alt2.txt</Key><LastModified>2023-08-19T01:21:31.958Z</LastModified><Size>1048716</Size></Contents><Contents><Key>onemegabyte.txt</Key><LastModified>2023-08-19T01:21:30.954Z</LastModified><Size>1048716</Size></Contents><Contents><Key>alt.txt</Key><LastModified>2023-08-19T01:21:31.477Z</LastModified><Size>1048716</Size></Contents></ListBucketResult>";

        var serializer = new XmlSerializer(typeof(ListBucketResult));
        using var sr = new StringReader(xml);
        var result = (ListBucketResult)serializer.Deserialize(sr);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Contents);
        Assert.AreEqual(3, result.Contents.Length);
        //Assert.AreEqual(3, result.KeyCount);
    }

    [TestMethod]
    public void ParseEmptyBucket()
    {
        const string xml =
            @"<ListBucketResult xmlns=""http://s3.amazonaws.com/doc/2006-03-01/""><Name>integration-test-bucket</Name></ListBucketResult>";

        var serializer = new XmlSerializer(typeof(ListBucketResult));
        using var sr = new StringReader(xml);
        var result = (ListBucketResult) serializer.Deserialize(sr);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Contents);
        Assert.AreEqual(0, result.Contents.Length);
    }
}
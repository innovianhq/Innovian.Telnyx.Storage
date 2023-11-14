// -------------------------------------------------------------
// Copyright (c) 2023 Innovian Corporation. All rights reserved.
// -------------------------------------------------------------

using System.Xml.Serialization;
using Innovian.Telnyx.Storage.Services.Storage.Responses;

namespace Innovian.Telnyx.Storage.Tests.Services.Storage.Requests.Models.Responses;

[TestClass]
public class ListBucketResultTests
{
    [TestMethod]
    public void ParsePopulatedBucket()
    {
        var xml =
            @$"<ListBucketResult xmlns=""http://s3.amazonaws.com/doc/2006-03-01/""><Name>{Constants.BucketName}</Name><Contents><Key>alt2.txt</Key><LastModified>2023-08-19T01:21:31.958Z</LastModified><Size>1048716</Size></Contents><Contents><Key>onemegabyte.txt</Key><LastModified>2023-08-19T01:21:30.954Z</LastModified><Size>1048716</Size></Contents><Contents><Key>alt.txt</Key><LastModified>2023-08-19T01:21:31.477Z</LastModified><Size>1048716</Size></Contents></ListBucketResult>";

        var serializer = new XmlSerializer(typeof(ListBucketResult));
        using var sr = new StringReader(xml);
        var result = (ListBucketResult)serializer.Deserialize(sr);

        Assert.IsNotNull(result);
        Assert.AreEqual(Constants.BucketName, result.Name);
        Assert.IsNotNull(result.Contents);
        Assert.AreEqual(3, result.Contents.Length);

        Assert.AreEqual("alt2.txt", result.Contents[0].Key);
        Assert.AreEqual(1048716, result.Contents[0].Size);
    }

    [TestMethod]
    public void ParseEmptyBucket()
    {
        var xml =
            @$"<ListBucketResult xmlns=""http://s3.amazonaws.com/doc/2006-03-01/""><Name>{Constants.BucketName}</Name></ListBucketResult>";

        var serializer = new XmlSerializer(typeof(ListBucketResult));
        using var sr = new StringReader(xml);
        var result = (ListBucketResult) serializer.Deserialize(sr);

        Assert.IsNotNull(result);
        Assert.IsNull(result.Contents);
    }
}
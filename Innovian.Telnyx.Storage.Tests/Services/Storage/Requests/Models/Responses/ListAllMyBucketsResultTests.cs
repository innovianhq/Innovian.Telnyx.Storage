// -------------------------------------------------------------
// Copyright (c) 2023 Innovian Corporation. All rights reserved.
// -------------------------------------------------------------

using System.Xml.Serialization;
using Innovian.Telnyx.Storage.Services.Storage.Responses;

namespace Innovian.Telnyx.Storage.Tests.Services.Storage.Requests.Models.Responses;

[TestClass]
public class ListAllMyBucketsResultTests
{
    [TestMethod]
    public void ParseResult()
    {
        const string xml =
            @"<?xml version=""1.0"" encoding=""UTF-8""?><ListAllMyBucketsResult xmlns=""http://s3.amazonaws.com/doc/2006-03-01/""><Owner><ID>f1669028-0b1c-49f8-9e2e-9f021e1f83dc</ID><DisplayName>f1669028-0b1c-49f8-9e2e-9f021e1f83dc</DisplayName></Owner><Buckets><Bucket><Name>empirica-collection-property-configs</Name><CreationDate>2023-11-08T09:16:37.664Z</CreationDate></Bucket><Bucket><Name>empirica-collection-script</Name><CreationDate>2023-11-07T11:57:57.879Z</CreationDate></Bucket><Bucket><Name>empirica-customer-data-backup</Name><CreationDate>2023-10-23T08:25:38.901Z</CreationDate></Bucket><Bucket><Name>empirica-customer-receipt-backup</Name><CreationDate>2023-10-23T08:27:06.574Z</CreationDate></Bucket><Bucket><Name>empirica-dep-tracker-data</Name><CreationDate>2023-10-23T08:25:39.882Z</CreationDate></Bucket><Bucket><Name>empirica-dep-tracker-data-consolidated</Name><CreationDate>2023-10-23T08:26:01.111Z</CreationDate></Bucket><Bucket><Name>empirica-dep-tracker-data-raw</Name><CreationDate>2023-10-23T08:27:05.240Z</CreationDate></Bucket><Bucket><Name>empirica-deployment-scripts</Name><CreationDate>2023-10-23T08:25:55.992Z</CreationDate></Bucket><Bucket><Name>empirica-email-cdn</Name><CreationDate>2023-11-09T06:14:17.394Z</CreationDate></Bucket><Bucket><Name>empirica-generic-cdn</Name><CreationDate>2023-10-23T08:25:57.718Z</CreationDate></Bucket><Bucket><Name>empirica-impl-rule-templates</Name><CreationDate>2023-10-23T08:25:59.047Z</CreationDate></Bucket><Bucket><Name>empirica-organization-logo-images</Name><CreationDate>2023-10-23T08:27:01.829Z</CreationDate></Bucket><Bucket><Name>empirica-subscription-receipts</Name><CreationDate>2023-10-23T08:30:37.863Z</CreationDate></Bucket><Bucket><Name>empirica-user-profile-images</Name><CreationDate>2023-10-23T08:26:28.773Z</CreationDate></Bucket><Bucket><Name>integration-testing-08f1222e</Name><CreationDate>2023-11-14T01:51:59.681Z</CreationDate></Bucket><Bucket><Name>integration-testing-3e48cbe7</Name><CreationDate>2023-11-14T02:17:00.694Z</CreationDate></Bucket><Bucket><Name>integration-testing-447073e7</Name><CreationDate>2023-11-14T02:17:30.211Z</CreationDate></Bucket><Bucket><Name>integration-testing-57acda36</Name><CreationDate>2023-11-14T02:05:27.571Z</CreationDate></Bucket><Bucket><Name>integration-testing-ba5dcbbf</Name><CreationDate>2023-11-14T01:18:46.411Z</CreationDate></Bucket><Bucket><Name>integration-testing-dda03996</Name><CreationDate>2023-11-14T01:17:12.759Z</CreationDate></Bucket><Bucket><Name>invn-support-pub</Name><CreationDate>2023-10-23T08:30:46.169Z</CreationDate></Bucket><Bucket><Name>property-monitor</Name><CreationDate>2023-10-23T08:30:36.880Z</CreationDate></Bucket><Bucket><Name>static-innovian-website-dev</Name><CreationDate>2023-11-14T00:20:15.600Z</CreationDate></Bucket><Bucket><Name>telnyxtest</Name><CreationDate>2023-10-23T08:25:21.842Z</CreationDate></Bucket><Bucket><Name>telnyxtest2</Name><CreationDate>2023-10-23T08:26:28.687Z</CreationDate></Bucket><Bucket><Name>transcription-audio</Name><CreationDate>2023-10-23T08:26:09.216Z</CreationDate></Bucket><Bucket><Name>transcription-files</Name><CreationDate>2023-10-23T08:26:09.361Z</CreationDate></Bucket><Bucket><Name>transcription-raw-transcripts</Name><CreationDate>2023-10-23T08:25:30.064Z</CreationDate></Bucket></Buckets></ListAllMyBucketsResult>";

        var serializer = new XmlSerializer(typeof(ListAllMyBucketsResult));
        using var sr = new StringReader(xml);
        var result = (ListAllMyBucketsResult) serializer.Deserialize(sr);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Owner);
        Assert.IsNotNull(result.Buckets);
        Assert.IsTrue(result.Buckets.Count > 0);
    }
}
// -------------------------------------------------------------
// Copyright (c) 2023 Innovian Corporation. All rights reserved.
// -------------------------------------------------------------

using Innovian.Telnyx.Storage.Utilities;

namespace Innovian.Telnyx.Storage.Tests.Services.Storage.Utilities;

[TestClass]
public class RegexResultParserTests
{
    [TestMethod]
    public void ParseLocationResultTest()
    {
        const string xml = """
                           <?xml version="1.0" encoding="UTF-8"?><LocationConstraint xmlns="http://s3.amazonaws.com/doc/2006-03-01/">us-central-1</LocationConstraint>
                           """;

        var parser = new RegexResultParser();
        var result = parser.ParseBucketLocationResult(xml);

        Assert.IsNotNull(result);
        Assert.AreEqual("us-central-1", result);
    }
}
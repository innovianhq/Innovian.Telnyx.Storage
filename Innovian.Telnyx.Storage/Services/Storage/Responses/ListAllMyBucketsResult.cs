//  -------------------------------------------------------------
//  Copyright (c) 2023 Innovian Corporation. All rights reserved.
//  -------------------------------------------------------------

namespace Innovian.Telnyx.Storage.Services.Storage.Responses;

public sealed record ListAllMyBucketsResult
{
    public List<Bucket> Buckets { get; set; } = new();

    public Owner Owner { get; set; }
}

public sealed record Bucket
{
    public DateTime CreationDate { get; set; }

    public string Name { get; set; }
}

public sealed record Owner
{
    public string DisplayName { get; set; }

    public string ID { get; set; }
}
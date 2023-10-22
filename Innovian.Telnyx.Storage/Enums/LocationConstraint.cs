//  -------------------------------------------------------------
//  Copyright (c) 2023 Innovian Corporation. All rights reserved.
//  -------------------------------------------------------------

using System.Runtime.Serialization;

namespace Innovian.Telnyx.Storage.Enums;

/// <summary>
/// The region to assign a bucket to upon creation.
/// </summary>
public enum LocationConstraint
{
    [EnumMember(Value="us-central-1")]
    Dallas,
}
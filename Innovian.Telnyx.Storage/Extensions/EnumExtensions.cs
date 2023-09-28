//  -------------------------------------------------------------
//  Copyright (c) 2023 Innovian Corporation. All rights reserved.
//  -------------------------------------------------------------

using System.Runtime.Serialization;

namespace Innovian.Telnyx.Storage.Extensions;

internal static class EnumExtensions
{
    /// <summary>
    /// Reads the value of an enum out of the attached <see cref="EnumMemberAttribute"/> attribute.
    /// </summary>
    /// <typeparam name="T">The enum.</typeparam>
    /// <param name="value">The value of the enum to pull the value for.</param>
    /// <returns></returns>
    public static string GetValueFromEnumMember<T>(this T value) where T : Enum
    {
        var type = typeof(T);
        var memberInfo = type.GetMember(value.ToString());
        if (memberInfo.Length <= 0)
            return value.ToString();

        var attributes = memberInfo[0].GetCustomAttributes(typeof(EnumMemberAttribute), false);
        return attributes.Length > 0 ? ((EnumMemberAttribute) attributes[0]).Value : value.ToString();
    }
}
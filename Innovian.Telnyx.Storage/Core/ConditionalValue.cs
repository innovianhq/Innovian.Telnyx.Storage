//  -------------------------------------------------------------
//  Copyright (c) 2023 Innovian Corporation. All rights reserved.
//  -------------------------------------------------------------

namespace Innovian.Telnyx.Storage.Core;

/// <summary>
/// A container that avoids needing to throw an exception when a value isn't available.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public readonly struct ConditionalValue<TValue>
{
    private readonly TValue _value;

    /// <summary>
    /// Indicates that an object value isn't available.
    /// </summary>
    public ConditionalValue()
    {
        HasValue = false;
        _value = default;
    }

    /// <summary>
    /// Indicates that the object value is available.
    /// </summary>
    /// <param name="value"></param>
    public ConditionalValue(TValue value)
    {
        HasValue = true;
        _value = value;
    }

    /// <summary>
    /// Indicates whether or not a value is populated in this object.
    /// </summary>
    public readonly bool HasValue;

    /// <summary>
    /// If a value has been populated, this contains the object's value.
    /// </summary>
    public TValue Value => _value;
}
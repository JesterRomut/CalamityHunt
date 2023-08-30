﻿#nullable enable

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CalamityHunt.Common.Utilities;

/// <summary>
///     Various reflection-related utilities.
/// </summary>
public static class ReflectionUtilities
{
    public delegate object? GetDelegate(object? obj);

    public delegate void SetDelegate(object? obj, object? value);

    /// <summary>
    ///     Resolves the getter and setter for a property.
    ///     <br />
    ///     If a property's getter or setter is not defined, this method
    ///     searches for the property's backing field, which is then used
    ///     instead.
    /// </summary>
    /// <param name="property">
    ///     The property to make a getter and setter for.
    /// </param>
    /// <returns>Getters and setters for the given property.</returns>
    public static unsafe (GetDelegate? getter, SetDelegate? setter) ResolvePropertyGetterAndSetter(this PropertyInfo? property)
    {
        if (property is null)
            return (null, null);

        GetDelegate? getter = null;
        SetDelegate? setter = null;

        FieldInfo? backingField = property.DeclaringType?.GetField($"<{property.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);

        if (property.GetMethod is { } getMethod)
            getter = obj => getMethod.Invoke(obj, null);
        else if (backingField is not null)
            getter = obj => backingField.GetValue(obj);

        if (property.SetMethod is { } setMethod)
        {
            setter = (obj, value) => setMethod.Invoke(obj, new[] { value });
        }
        else if (backingField is not null)
        {
            if (!backingField.IsInitOnly)
            {
                setter = (obj, value) => backingField.SetValue(obj, value);
            }
            else
            {
                setter = (obj, value) =>
                {
                    // Get a reference/pointer to the backing field.
                    if (obj is null)
                    {
                        // Assume static context.
                        /*var fieldPtr = backingField.FieldHandle.Value;

                        if (value is null)
                        {
                            *fieldPtr.ToPointer() = default;
                        }
                        else
                        {
                            fixed (byte* ptr = &Unsafe.As<StrongBox<byte>>(value!).Value)
                            {
                                throw new NotImplementedException();
                            }
                        }*/

                        var fieldPtr = backingField.FieldHandle.Value;
                        
                        if (value is null)
                        {
                            Marshal.WriteIntPtr(fieldPtr, IntPtr.Zero);
                        }
                        else
                        {
                            var objHandle = GCHandle.Alloc(obj, GCHandleType.Pinned);
                            Marshal.WriteIntPtr(fieldPtr, objHandle.AddrOfPinnedObject());
                        }
                    }
                    else
                    {
                        // Assume instance context.
                        fixed (byte* ptr = &Unsafe.As<StrongBox<byte>>(obj).Value)
                        {
                            var fieldPtr = backingField.FieldHandle;
                            throw new NotImplementedException();
                        }
                    }
                };
            }
        }

        return (getter, setter);
    }
}
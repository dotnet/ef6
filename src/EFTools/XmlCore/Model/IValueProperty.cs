// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public interface IValueProperty
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        object ActualValue { get; }
    }

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    /// <typeparam name="T">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</typeparam>
    public interface IValueProperty<T> : IValueProperty
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        T DefaultValue { get; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <remarks>
        ///     The following table indicates return values for expected permutations of IsNotPresent, IsRequired & ActualValue
        ///     | IsNotPresent         |IsNotPresent == false|  IsNotPresent == false   |
        ///     |  == true             |  && ActualValue is  |  ActualValue is not      |
        ///     |                      | convertible to T    |  Convertible to T        |
        ///     -------------|----------------------|-------------------- |--------------------------|
        ///     IsRequired   |  return DefaultValue |  return value       |       throw              |
        ///     -------------|----------------------|---------------------|------------------------- |
        ///     ! IsRequired |  return DefaultValue |  return value       |       throw              |
        ///     -------------|----------------------|---------------------|--------------------------|
        /// </remarks>
        T Value { get; set; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <remarks>
        ///     The following table indicates return values for expected permutations of IsNotPresent, IsRequired & ActualValue
        ///     | IsNotPresent  |IsNotPresent == false|  IsNotPresent == false   |
        ///     |  == true      |  && ActualValue is  |  ActualValue is not      |
        ///     |               | convertible to T    |  Convertible to T        |
        ///     -------------|---------------|-------------------- |--------------------------|
        ///     IsRequired   |  return false |  return true        |  return false            |
        ///     -------------|---------------|---------------------|--------------------------|
        ///     ! IsRequired |   return true |  return true        |  return false            |
        ///     -------------|---------------|---------------------|--------------------------|
        /// </remarks>
        bool TryGetValue(out T value);
    }
}

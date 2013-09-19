// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.EntityClient
{
    using System.Data.Common;

    /// <summary>
    /// Class representing a parameter collection used in EntityCommand
    /// </summary>
    public sealed partial class EntityParameterCollection : DbParameterCollection
    {
        private static readonly Type _itemType = typeof(EntityParameter);
        private bool _isDirty;

        /// <summary>
        /// Constructs the EntityParameterCollection object
        /// </summary>
        internal EntityParameterCollection()
        {
        }

        /// <summary>
        /// Gets the <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> at the specified index.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> at the specified index.
        /// </returns>
        /// <param name="index">The zero-based index of the parameter to retrieve. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The specified index does not exist. </exception>
        public new EntityParameter this[int index]
        {
            get { return (EntityParameter)GetParameter(index); }
            set { SetParameter(index, value); }
        }

        /// <summary>
        /// Gets the <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> with the specified name.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> with the specified name.
        /// </returns>
        /// <param name="parameterName">The name of the parameter to retrieve. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The specified name does not exist. </exception>
        public new EntityParameter this[string parameterName]
        {
            get { return (EntityParameter)GetParameter(parameterName); }
            set { SetParameter(parameterName, value); }
        }

        /// <summary>
        /// Gets whether this collection has been changes since the last reset
        /// </summary>
        internal bool IsDirty
        {
            get
            {
                if (_isDirty)
                {
                    return true;
                }

                // Loop through and return true if any parameter is dirty
                foreach (EntityParameter parameter in this)
                {
                    if (parameter.IsDirty)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Adds the specified <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> object to the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// .
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> object.
        /// </returns>
        /// <param name="value">
        /// The <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> to add to the collection.
        /// </param>
        /// <exception cref="T:System.ArgumentException">
        /// The <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> specified in the  value  parameter is already added to this or another
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// .
        /// </exception>
        /// <exception cref="T:System.InvalidCastException">
        /// The parameter passed was not a <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" />.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">The  value  parameter is null. </exception>
        public EntityParameter Add(EntityParameter value)
        {
            Add((object)value);
            return value;
        }

        /// <summary>
        /// Adds a value to the end of the <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> object.
        /// </returns>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="value">The value to be added.</param>
        public EntityParameter AddWithValue(string parameterName, object value)
        {
            var param = new EntityParameter();
            param.ParameterName = parameterName;
            param.Value = value;
            return Add(param);
        }

        /// <summary>
        /// Adds a <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> to the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// given the parameter name and the data type.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> object.
        /// </returns>
        /// <param name="parameterName">The name of the parameter. </param>
        /// <param name="dbType">
        /// One of the <see cref="T:System.Data.DbType" /> values.
        /// </param>
        public EntityParameter Add(string parameterName, DbType dbType)
        {
            return Add(new EntityParameter(parameterName, dbType));
        }

        /// <summary>
        /// Adds a <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> to the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// with the parameter name, the data type, and the column length.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> object.
        /// </returns>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="dbType">
        /// One of the <see cref="T:System.Data.DbType" /> values.
        /// </param>
        /// <param name="size">The column length.</param>
        public EntityParameter Add(string parameterName, DbType dbType, int size)
        {
            return Add(new EntityParameter(parameterName, dbType, size));
        }

        /// <summary>
        /// Adds an array of <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> values to the end of the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// .
        /// </summary>
        /// <param name="values">
        /// The <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> values to add.
        /// </param>
        public void AddRange(EntityParameter[] values)
        {
            AddRange((Array)values);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> is in this
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// .
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" /> contains the value; otherwise false.
        /// </returns>
        /// <param name="parameterName">
        /// The <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> value.
        /// </param>
        public override bool Contains(string parameterName)
        {
            return IndexOf(parameterName) != -1;
        }

        /// <summary>
        /// Copies all the elements of the current <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" /> to the specified
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// starting at the specified destination index.
        /// </summary>
        /// <param name="array">
        /// The <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" /> that is the destination of the elements copied from the current
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// .
        /// </param>
        /// <param name="index">
        /// A 32-bit integer that represents the index in the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// at which copying starts.
        /// </param>
        public void CopyTo(EntityParameter[] array, int index)
        {
            CopyTo((Array)array, index);
        }

        /// <summary>
        /// Gets the location of the specified <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> in the collection.
        /// </summary>
        /// <returns>
        /// The zero-based location of the specified <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> that is a
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" />
        /// in the collection. Returns -1 when the object does not exist in the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// .
        /// </returns>
        /// <param name="value">
        /// The <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> to find.
        /// </param>
        public int IndexOf(EntityParameter value)
        {
            return IndexOf((object)value);
        }

        /// <summary>
        /// Inserts a <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> object into the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which value should be inserted.</param>
        /// <param name="value">
        /// A <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> object to be inserted in the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// .
        /// </param>
        public void Insert(int index, EntityParameter value)
        {
            Insert(index, (object)value);
        }

        /// <summary>
        /// Marks that this collection has been changed
        /// </summary>
        private void OnChange()
        {
            _isDirty = true;
        }

        /// <summary>
        /// Removes the specified <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> from the collection.
        /// </summary>
        /// <param name="value">
        /// A <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> object to remove from the collection.
        /// </param>
        /// <exception cref="T:System.InvalidCastException">
        /// The parameter is not a <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" />.
        /// </exception>
        /// <exception cref="T:System.SystemException">The parameter does not exist in the collection. </exception>
        public void Remove(EntityParameter value)
        {
            Remove((object)value);
        }

        /// <summary>
        /// Reset the dirty flag on the collection
        /// </summary>
        internal void ResetIsDirty()
        {
            _isDirty = false;

            // Loop through and reset each parameter
            foreach (EntityParameter parameter in this)
            {
                parameter.ResetIsDirty();
            }
        }
    }
}

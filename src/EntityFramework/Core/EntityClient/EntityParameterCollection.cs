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
        /// Gets the parameter from the collection at the specified index
        /// </summary>
        /// <param name="index">The index of the parameter to retrieved</param>
        /// <returns>The parameter at the index</returns>
        public new EntityParameter this[int index]
        {
            get { return (EntityParameter)GetParameter(index); }
            set { SetParameter(index, value); }
        }

        /// <summary>
        /// Gets the parameter with the given name from the collection
        /// </summary>
        /// <param name="parameterName">The name of the parameter to retrieved</param>
        /// <returns>The parameter with the given name</returns>
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
        /// Add a EntityParameter to the collection
        /// </summary>
        /// <param name="value">The parameter to add to the collection</param>
        /// <returns>The index of the new parameter within the collection</returns>
        public EntityParameter Add(EntityParameter value)
        {
            Add((object)value);
            return value;
        }

        /// <summary>
        /// Add a EntityParameter with the given name and value to the collection
        /// </summary>
        /// <param name="parameterName">The name of the parameter to add</param>
        /// <param name="value">The value of the parameter to add</param>
        /// <returns>The index of the new parameter within the collection</returns>
        public EntityParameter AddWithValue(string parameterName, object value)
        {
            var param = new EntityParameter();
            param.ParameterName = parameterName;
            param.Value = value;
            return Add(param);
        }

        /// <summary>
        /// Adds a EntityParameter with the given name and type to the collection
        /// </summary>
        /// <param name="parameterName">The name of the parameter to add</param>
        /// <param name="dbType">The type of the parameter</param>
        /// <returns>The index of the new parameter within the collection</returns>
        public EntityParameter Add(string parameterName, DbType dbType)
        {
            return Add(new EntityParameter(parameterName, dbType));
        }

        /// <summary>
        /// Add a EntityParameter with the given name, type, and size to the collection
        /// </summary>
        /// <param name="parameterName">The name of the parameter to add</param>
        /// <param name="dbType">The type of the parameter</param>
        /// <param name="size">The size of the parameter</param>
        /// <returns>The index of the new parameter within the collection</returns>
        public EntityParameter Add(string parameterName, DbType dbType, int size)
        {
            return Add(new EntityParameter(parameterName, dbType, size));
        }

        /// <summary>
        /// Adds a range of EntityParameter objects to this collection
        /// </summary>
        /// <param name="values">The arary of EntityParameter objects to add</param>
        public void AddRange(EntityParameter[] values)
        {
            AddRange((Array)values);
        }

        /// <summary>
        /// Check if the collection has a parameter with the given parameter name
        /// </summary>
        /// <param name="parameterName">The parameter name to look for</param>
        /// <returns>True if the collection has a parameter with the given name</returns>
        public override bool Contains(string parameterName)
        {
            return IndexOf(parameterName) != -1;
        }

        /// <summary>
        /// Copies the given array of parameters into this collection
        /// </summary>
        /// <param name="array">The array to copy into</param>
        /// <param name="index">The index in the array where the copy starts</param>
        public void CopyTo(EntityParameter[] array, int index)
        {
            CopyTo((Array)array, index);
        }

        /// <summary>
        /// Finds the index in the collection of the given parameter object
        /// </summary>
        /// <param name="value">The parameter to search for</param>
        /// <returns>The index of the parameter, -1 if not found</returns>
        public int IndexOf(EntityParameter value)
        {
            return IndexOf((object)value);
        }

        /// <summary>
        /// Add a EntityParameter with the given value to the collection at a location indicated by the index
        /// </summary>
        /// <param name="index">The index at which the parameter is to be inserted</param>
        /// <param name="value">The value of the parameter</param>
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
        /// Remove a EntityParameter with the given value from the collection
        /// </summary>
        /// <param name="value">The parameter to remove</param>
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

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Validation
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    ///     Represents validation results for single entity.
    /// </summary>
    [Serializable]
    public class DbEntityValidationResult
    {
        /// <summary>
        ///     Entity entry the results applies to. Never null.
        /// </summary>
        [NonSerialized]
        private readonly InternalEntityEntry _entry;

        /// <summary>
        ///     List of <see cref = "DbValidationError" /> instances. Never null. Can be empty meaning the entity is valid.
        /// </summary>
        private readonly List<DbValidationError> _validationErrors;

        /// <summary>
        ///     Creates an instance of <see cref = "DbEntityValidationResult" /> class.
        /// </summary>
        /// <param name = "entry">
        ///     Entity entry the results applies to. Never null.
        /// </param>
        /// <param name = "validationErrors">
        ///     List of <see cref = "DbValidationError" /> instances. Never null. Can be empty meaning the entity is valid.
        /// </param>
        public DbEntityValidationResult(DbEntityEntry entry, IEnumerable<DbValidationError> validationErrors)
        {
            Contract.Requires(entry != null);
            Contract.Requires(validationErrors != null);

            _entry = entry.InternalEntry;
            _validationErrors = validationErrors.ToList();
        }

        /// <summary>
        ///     Creates an instance of <see cref = "DbEntityValidationResult" /> class.
        /// </summary>
        /// <param name = "entry">
        ///     Entity entry the results applies to. Never null.
        /// </param>
        /// <param name = "validationErrors">
        ///     List of <see cref = "DbValidationError" /> instances. Never null. Can be empty meaning the entity is valid.
        /// </param>
        internal DbEntityValidationResult(InternalEntityEntry entry, IEnumerable<DbValidationError> validationErrors)
        {
            Contract.Requires(entry != null);
            Contract.Requires(validationErrors != null);

            _entry = entry;
            _validationErrors = validationErrors.ToList();
        }

        /// <summary>
        ///     Gets an instance of <see cref = "DbEntityEntry" /> the results applies to.
        /// </summary>
        public DbEntityEntry Entry
        {
            get
            {
                // The entry can be null when a DbEntityValidationResult instance was serialized and then deserialized 
                // with DbEntityValidationException it was a part of.
                return _entry != null ? new DbEntityEntry(_entry) : null;
            }
        }

        /// <summary>
        ///     Gets validation errors. Never null.
        /// </summary>
        public ICollection<DbValidationError> ValidationErrors
        {
            get { return _validationErrors; }
        }

        /// <summary>
        ///     Gets an indicator if the entity is valid.
        /// </summary>
        public bool IsValid
        {
            get { return !_validationErrors.Any(); }
        }
    }
}

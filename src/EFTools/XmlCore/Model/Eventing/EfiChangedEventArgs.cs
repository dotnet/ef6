// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Eventing
{
    using System;
    using Microsoft.Data.Entity.Design.Model.Commands;

    /// <summary>
    ///     EventArgs based class for communicating an EfiChangeGroup to those
    ///     subscribing to change events on EFService <see cref="EFService">.
    /// </summary>
    internal class EfiChangedEventArgs : EventArgs
    {
        private readonly EfiChangeGroup _changes;

        internal EfiChangedEventArgs(EfiChangeGroup changes)
        {
            _changes = changes;
        }

        public EfiChangeGroup ChangeGroup
        {
            get { return _changes; }
        }
    }

    internal class EfiChangingEventArgs : EventArgs
    {
        private readonly CommandProcessorContext _cpc;

        internal EfiChangingEventArgs(CommandProcessorContext cpc)
        {
            _cpc = cpc;
        }

        public CommandProcessorContext CommandProcessorContext
        {
            get { return _cpc; }
        }
    }
}

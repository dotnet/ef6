// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
using System;
using System.Data.Common;

namespace SampleEntityFrameworkProvider
{
    public partial class SampleCommand : ICloneable
    {
        object ICloneable.Clone()
        {
            SampleCommand clone = new SampleCommand();
            clone._Connection = this._Connection;

            //Defer to the Clone method on the wrapped SqlCommand
            clone._WrappedCommand = (DbCommand)((ICloneable)this._WrappedCommand).Clone();

            ////An alternate approach is to create a new instance of the Command and
            ////set values of the properties of the new Command to the corresponding
            ////properties of the original command, using code like:
            //clone.Connection = this.Connection;
            //clone.CommandText = this.CommandText;
            //clone.CommandType = this.CommandType;
            //clone.CommandTimeout = this.CommandTimeout;
            //clone.DesignTimeVisible = this.DesignTimeVisible;
            //clone.Transaction = this.Transaction;
            //clone.UpdatedRowSource = this.UpdatedRowSource;
            //foreach (DbParameter p in this.Parameters)
            //    clone.Parameters.Add((DbParameter) ((ICloneable)p).Clone());

            return clone;
        }
    }
}

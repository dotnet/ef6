//---------------------------------------------------------------------
// <copyright file="Command.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------

/*////////////////////////////////////////////////////////////////////////
 * Sample ADO.NET Entity Framework Provider
 *
 * This partial Command class implements ICloneable so the Entity 
 * Framework's provider agnostic logic can clone commands
 */
////////////////////////////////////////////////////////////////////////

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

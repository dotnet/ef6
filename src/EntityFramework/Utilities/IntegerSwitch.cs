using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.Entity.Utilities
{
    internal class IntegerSwitch : Switch
    {
        /// <summary>Gets or sets the current value of this switch.</summary>
        public int CurrentValue
        {
            get { return SwitchSetting; }
            set { SwitchSetting = value; }
        }

        internal IntegerSwitch(string displayName, string description)
            : base(displayName, description) { }

        internal IntegerSwitch(string displayName, string description, string defaultSwitchValue)
            : base(displayName, description, defaultSwitchValue) { }
    }
}
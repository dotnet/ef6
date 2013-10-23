// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Common
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Windows.Forms;

    /// <summary>
    ///     Exception hardening work.  This class can be used to filter messages sent to a control,
    ///     and catch/display all non-critical exceptions.  Otherwise, Watson will
    ///     be invoked and will take down the process, potentially resulting in data loss.  See
    ///     document referenced in bug 427820 for more details.	 Use this class to wrap an existing
    ///     IWindowTarget as follows (c is a Control):
    ///     c.WindowTarget = new SafeWindowTarget(c.WindowTarget);
    /// </summary>
    internal class SafeWindowTarget : IWindowTarget
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IWindowTarget inner;

        internal SafeWindowTarget(IServiceProvider serviceProvider, IWindowTarget inner)
        {
            this.serviceProvider = serviceProvider;
            this.inner = inner;
        }

        void IWindowTarget.OnHandleChange(IntPtr newHandle)
        {
            inner.OnHandleChange(newHandle);
        }

        /// <devdoc>
        ///     The main wndproc for the control.  Wrapped to display non-critical exceptions to the user.
        /// </devdoc>
        void IWindowTarget.OnMessage(ref Message m)
        {
            try
            {
                inner.OnMessage(ref m);
            }
            catch (Exception ex)
            {
                if (CriticalException.ThrowOrShow(serviceProvider, ex))
                {
                    throw;
                }
            }
        }

        /// <summary>
        ///     Replaces the WindowTarget for all child controls of the specified collection.
        ///     In Debug builds, this will assert that any child controls added after this call must have their WindowTarget replaced as well.
        /// </summary>
        /// <param name="serviceProvider">The ServiceProvider.</param>
        /// <param name="controls">The collection of controls to recurse through and replace their target.</param>
        internal static void ReplaceWindowTargetRecursive(IServiceProvider serviceProvider, ICollection controls)
        {
            ReplaceWindowTargetRecursive(serviceProvider, controls, true);
        }

        /// <summary>
        ///     Replaces the WindowTarget for all child controls of the specified collection.
        /// </summary>
        /// <param name="serviceProvider">The ServiceProvider.</param>
        /// <param name="controls">The collection of controls to recurse through and replace their target.</param>
        /// <param name="checkControlAdded">
        ///     If true, in Debug builds, this will assert that any controls
        ///     added after this call must have their WindowTarget replaced as well.
        /// </param>
        internal static void ReplaceWindowTargetRecursive(IServiceProvider serviceProvider, ICollection controls, bool checkControlAdded)
        {
            foreach (Control c in controls)
            {
                c.WindowTarget = new SafeWindowTarget(serviceProvider, c.WindowTarget);

                if (checkControlAdded)
                {
#if DEBUG
                    // For controls added dynamically after form load, we require the derived class to add the SafeWindowTarget. 
                    // In debug mode, attach a ControlAdded handler that asserts that this happens.
                    c.ControlAdded += OnChildControlAdded;
#endif
                }

                if (c.Controls.Count > 0)
                {
                    ReplaceWindowTargetRecursive(serviceProvider, c.Controls, checkControlAdded);
                }
            }
        }

        internal static void OnChildControlAdded(object sender, ControlEventArgs e)
        {
#if DEBUG

            if (e.Control != null
                && (e.Control.WindowTarget == null || !e.Control.WindowTarget.GetType().Name.Contains("SafeWindowTarget")))
            {
                Debug.Fail(
                    "A control was added after Form.Load, but it is not wrapped with a SafeWindowTarget.  This violates our exception hardening policy.  See bug 427820 for more info on using SafeWindowTarget.");
            }
#endif
        }
    }
}

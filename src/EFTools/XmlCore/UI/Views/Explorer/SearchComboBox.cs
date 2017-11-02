// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Explorer
{
    using System.Windows;
    using System.Windows.Automation;
    using System.Windows.Automation.Peers;
    using System.Windows.Controls;

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class SearchComboBox : ComboBox
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new SearchComboBoxAutomationPeer(this);
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            TrySetCaretBrush();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void TrySetCaretBrush()
        {
            try
            {
                if (Template != null)
                {
                    var textBox = Template.FindName("PART_EditableTextBox", this) as TextBox;
                    if (textBox != null)
                    {
                        textBox.CaretBrush = textBox.Foreground;
                        textBox.SetValue(
                            AutomationProperties.NameProperty,
                            this.GetValue(AutomationProperties.NameProperty));
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="e">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property.Name == "Foreground")
            {
                TrySetCaretBrush();
            }
        }
    }

    internal class SearchComboBoxAutomationPeer : ComboBoxAutomationPeer
    {
        internal SearchComboBoxAutomationPeer(SearchComboBox owner)
            : base(owner)
        {
            // do nothing 
        }

        protected override void SetFocusCore()
        {
            Owner.Focus();
        }
    }
}

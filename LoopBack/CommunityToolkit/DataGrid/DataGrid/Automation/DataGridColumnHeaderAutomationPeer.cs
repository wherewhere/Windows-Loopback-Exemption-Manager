// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.WinUI.Controls;
using CommunityToolkit.WinUI.Controls.DataGridInternals;
using CommunityToolkit.WinUI.Controls.Primitives;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;

namespace CommunityToolkit.WinUI.Automation.Peers
{
    /// <summary>
    /// AutomationPeer for DataGridColumnHeader
    /// </summary>
    public partial class DataGridColumnHeaderAutomationPeer : FrameworkElementAutomationPeer,
        IInvokeProvider, IScrollItemProvider, ITransformProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridColumnHeaderAutomationPeer"/> class.
        /// </summary>
        /// <param name="owner">DataGridColumnHeader</param>
        public DataGridColumnHeaderAutomationPeer(DataGridColumnHeader owner)
            : base(owner)
        {
        }

        private DataGridColumnHeader OwningHeader => Owner as DataGridColumnHeader;

        /// <summary>
        /// Gets the control type for the element that is associated with the UI Automation peer.
        /// </summary>
        /// <returns>The control type.</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.HeaderItem;
        }

        /// <summary>
        /// Called by GetClassName that gets a human readable name that, in addition to AutomationControlType,
        /// differentiates the control represented by this AutomationPeer.
        /// </summary>
        /// <returns>The string that contains the name.</returns>
        protected override string GetClassNameCore()
        {
            string classNameCore = Owner.GetType().Name;
#if DEBUG_AUTOMATION
            System.Diagnostics.Debug.WriteLine("DataGridColumnHeaderAutomationPeer.GetClassNameCore returns " + classNameCore);
#endif
            return classNameCore;
        }

        /// <summary>
        /// Gets the string that describes the functionality of the control that is associated with the automation peer.
        /// </summary>
        /// <returns>The string that contains the help text.</returns>
        protected override string GetHelpTextCore()
        {
            if (OwningHeader.OwningColumn != null && OwningHeader.OwningColumn.SortDirection.HasValue)
            {
                if (OwningHeader.OwningColumn.SortDirection.Value == DataGridSortDirection.Ascending)
                {
                    return "Ascending";
                }

                return "Descending";
            }

            return base.GetHelpTextCore();
        }

        /// <summary>
        /// Gets the name of the element.
        /// </summary>
        /// <returns>The string that contains the name.</returns>
        protected override string GetNameCore()
        {
            if (OwningHeader.Content is string header)
            {
                return header;
            }

            return base.GetNameCore();
        }

        /// <summary>
        /// Gets the control pattern that is associated with the specified Windows.UI.Xaml.Automation.Peers.PatternInterface.
        /// </summary>
        /// <param name="patternInterface">A value from the Windows.UI.Xaml.Automation.Peers.PatternInterface enumeration.</param>
        /// <returns>The object that supports the specified pattern, or null if unsupported.</returns>
        protected override object GetPatternCore(PatternInterface patternInterface)
        {
            if (OwningHeader.OwningGrid != null)
            {
                switch (patternInterface)
                {
                    case PatternInterface.Invoke:
                        // this.OwningHeader.OwningGrid.DataConnection.AllowSort property is ignored because of the DataGrid.Sorting custom sorting capability.
                        if (OwningHeader.OwningGrid.CanUserSortColumns &&
                            OwningHeader.OwningColumn.CanUserSort)
                        {
                            return this;
                        }

                        break;

                    case PatternInterface.ScrollItem:
                        if (OwningHeader.OwningGrid.HorizontalScrollBar != null &&
                            OwningHeader.OwningGrid.HorizontalScrollBar.Maximum > 0)
                        {
                            return this;
                        }

                        break;

                    case PatternInterface.Transform:
                        if (OwningHeader.OwningColumn != null &&
                            OwningHeader.OwningColumn.ActualCanUserResize)
                        {
                            return this;
                        }

                        break;
                }
            }

            return base.GetPatternCore(patternInterface);
        }

        /// <summary>
        /// Gets a value that specifies whether the element is a content element.
        /// </summary>
        /// <returns>True if the element is a content element; otherwise false</returns>
        protected override bool IsContentElementCore()
        {
            return false;
        }

        void IInvokeProvider.Invoke()
        {
            OwningHeader.InvokeProcessSort();
        }

        void IScrollItemProvider.ScrollIntoView()
        {
            OwningHeader.OwningGrid.ScrollIntoView(null, OwningHeader.OwningColumn);
        }

        bool ITransformProvider.CanMove => false;

        bool ITransformProvider.CanResize => OwningHeader.OwningColumn != null && OwningHeader.OwningColumn.ActualCanUserResize;

        bool ITransformProvider.CanRotate => false;

        void ITransformProvider.Move(double x, double y)
        {
            throw DataGridError.DataGridAutomationPeer.OperationCannotBePerformed();
        }

        void ITransformProvider.Resize(double width, double height)
        {
            if (OwningHeader.OwningColumn != null &&
                OwningHeader.OwningColumn.ActualCanUserResize)
            {
                OwningHeader.OwningColumn.Width = new DataGridLength(width);
            }
        }

        void ITransformProvider.Rotate(double degrees)
        {
            throw DataGridError.DataGridAutomationPeer.OperationCannotBePerformed();
        }
    }
}
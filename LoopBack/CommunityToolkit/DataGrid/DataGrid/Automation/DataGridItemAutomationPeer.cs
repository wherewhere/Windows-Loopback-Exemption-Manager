// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.WinUI.Controls;
using CommunityToolkit.WinUI.Controls.DataGridInternals;
using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;

namespace CommunityToolkit.WinUI.Automation.Peers
{
    /// <summary>
    /// AutomationPeer for an item in a DataGrid
    /// </summary>
    public partial class DataGridItemAutomationPeer : FrameworkElementAutomationPeer,
        IInvokeProvider, IScrollItemProvider, ISelectionItemProvider, ISelectionProvider
    {
        private readonly object _item;
        private readonly AutomationPeer _dataGridAutomationPeer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridItemAutomationPeer"/> class.
        /// </summary>
        public DataGridItemAutomationPeer(object item, DataGrid dataGrid)
            : base(dataGrid)
        {
            ArgumentNullException.ThrowIfNull(item);
            ArgumentNullException.ThrowIfNull(dataGrid);

            _item = item;
            _dataGridAutomationPeer = CreatePeerForElement(dataGrid);
        }

        private DataGrid OwningDataGrid
        {
            get
            {
                DataGridAutomationPeer gridPeer = _dataGridAutomationPeer as DataGridAutomationPeer;
                return gridPeer.Owner as DataGrid;
            }
        }

        private DataGridRow OwningRow
        {
            get
            {
                int index = OwningDataGrid.DataConnection.IndexOf(_item);
                int slot = OwningDataGrid.SlotFromRowIndex(index);

                if (OwningDataGrid.IsSlotVisible(slot))
                {
                    return OwningDataGrid.DisplayData.GetDisplayedElement(slot) as DataGridRow;
                }

                return null;
            }
        }

        internal DataGridRowAutomationPeer OwningRowPeer
        {
            get
            {
                DataGridRowAutomationPeer rowPeer = null;
                DataGridRow row = OwningRow;
                if (row != null)
                {
                    rowPeer = CreatePeerForElement(row) as DataGridRowAutomationPeer;
                }

                return rowPeer;
            }
        }

        /// <summary>
        /// Returns the accelerator key for the UIElement that is associated with this DataGridItemAutomationPeer.
        /// </summary>
        /// <returns>The accelerator key for the UIElement that is associated with this DataGridItemAutomationPeer.</returns>
        protected override string GetAcceleratorKeyCore()
        {
            return OwningRowPeer != null ? OwningRowPeer.GetAcceleratorKey() : string.Empty;
        }

        /// <summary>
        /// Returns the access key for the UIElement that is associated with this DataGridItemAutomationPeer.
        /// </summary>
        /// <returns>The access key for the UIElement that is associated with this DataGridItemAutomationPeer.</returns>
        protected override string GetAccessKeyCore()
        {
            return OwningRowPeer != null ? OwningRowPeer.GetAccessKey() : string.Empty;
        }

        /// <summary>
        /// Returns the control type for the UIElement that is associated with this DataGridItemAutomationPeer.
        /// </summary>
        /// <returns>The control type for the UIElement that is associated with this DataGridItemAutomationPeer.</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.DataItem;
        }

        /// <summary>
        /// Returns the string that uniquely identifies the FrameworkElement that is associated with this DataGridItemAutomationPeer.
        /// </summary>
        /// <returns>The string that uniquely identifies the FrameworkElement that is associated with this DataGridItemAutomationPeer.</returns>
        protected override string GetAutomationIdCore()
        {
            // The AutomationId should be unset for dynamic content.
            return string.Empty;
        }

        /// <summary>
        /// Returns the Rect that represents the bounding rectangle of the UIElement that is associated with this DataGridItemAutomationPeer.
        /// </summary>
        /// <returns>The Rect that represents the bounding rectangle of the UIElement that is associated with this DataGridItemAutomationPeer.</returns>
        protected override Rect GetBoundingRectangleCore()
        {
            return OwningRowPeer != null ? OwningRowPeer.GetBoundingRectangle() : default;
        }

        /// <summary>
        /// Returns the collection of elements that are represented in the UI Automation tree as immediate
        /// child elements of the automation peer.
        /// </summary>
        /// <returns>The children elements.</returns>
        protected override IList<AutomationPeer> GetChildrenCore()
        {
            if (OwningRowPeer != null)
            {
                OwningRowPeer.InvalidatePeer();
                return OwningRowPeer.GetChildren();
            }

            return new List<AutomationPeer>();
        }

        /// <summary>
        /// Called by GetClassName that gets a human readable name that, in addition to AutomationControlType,
        /// differentiates the control represented by this AutomationPeer.
        /// </summary>
        /// <returns>The string that contains the name.</returns>
        protected override string GetClassNameCore()
        {
            string classNameCore = (OwningRowPeer != null) ? OwningRowPeer.GetClassName() : string.Empty;
#if DEBUG_AUTOMATION
            System.Diagnostics.Debug.WriteLine("DataGridItemAutomationPeer.GetClassNameCore returns " + classNameCore);
#endif
            return classNameCore;
        }

        /// <summary>
        /// Returns a Point that represents the clickable space that is on the UIElement that is associated with this DataGridItemAutomationPeer.
        /// </summary>
        /// <returns>A Point that represents the clickable space that is on the UIElement that is associated with this DataGridItemAutomationPeer.</returns>
        protected override Point GetClickablePointCore()
        {
            return OwningRowPeer != null ? OwningRowPeer.GetClickablePoint() : new Point(double.NaN, double.NaN);
        }

        /// <summary>
        /// Returns the string that describes the functionality of the control that is associated with the automation peer.
        /// </summary>
        /// <returns>The string that contains the help text.</returns>
        protected override string GetHelpTextCore()
        {
            return OwningRowPeer != null ? OwningRowPeer.GetHelpText() : string.Empty;
        }

        /// <summary>
        /// Returns a string that communicates the visual status of the UIElement that is associated with this DataGridItemAutomationPeer.
        /// </summary>
        /// <returns>A string that communicates the visual status of the UIElement that is associated with this DataGridItemAutomationPeer.</returns>
        protected override string GetItemStatusCore()
        {
            return OwningRowPeer != null ? OwningRowPeer.GetItemStatus() : string.Empty;
        }

        /// <summary>
        /// Returns a human-readable string that contains the item type that the UIElement for this DataGridItemAutomationPeer represents.
        /// </summary>
        /// <returns>A human-readable string that contains the item type that the UIElement for this DataGridItemAutomationPeer represents.</returns>
        protected override string GetItemTypeCore()
        {
            return OwningRowPeer != null ? OwningRowPeer.GetItemType() : string.Empty;
        }

        /// <summary>
        /// Returns the AutomationPeer for the element that is targeted to the UIElement for this DataGridItemAutomationPeer.
        /// </summary>
        /// <returns>The AutomationPeer for the element that is targeted to the UIElement for this DataGridItemAutomationPeer.</returns>
        protected override AutomationPeer GetLabeledByCore()
        {
            return OwningRowPeer?.GetLabeledBy();
        }

        /// <summary>
        /// Returns a localized human readable string for this control type.
        /// </summary>
        /// <returns>A localized human readable string for this control type.</returns>
        protected override string GetLocalizedControlTypeCore()
        {
            return OwningRowPeer != null ? OwningRowPeer.GetLocalizedControlType() : string.Empty;
        }

        /// <summary>
        /// Returns the string that describes the functionality of the control that is associated with this DataGridItemAutomationPeer.
        /// </summary>
        /// <returns>The string that contains the help text.</returns>
        protected override string GetNameCore()
        {
            if (OwningRowPeer != null)
            {
                string owningRowPeerName = OwningRowPeer.GetName();
                if (!string.IsNullOrEmpty(owningRowPeerName))
                {
#if DEBUG_AUTOMATION
                    System.Diagnostics.Debug.WriteLine("DataGridItemAutomationPeer.GetNameCore returns " + owningRowPeerName);
#endif
                    return owningRowPeerName;
                }
            }

            string name = Resources.DataGridRowAutomationPeer_ItemType;
#if DEBUG_AUTOMATION
            System.Diagnostics.Debug.WriteLine("DataGridItemAutomationPeer.GetNameCore returns " + name);
#endif
            return name;
        }

        /// <summary>
        /// Returns a value indicating whether the element associated with this DataGridItemAutomationPeer is laid out in a specific direction.
        /// </summary>
        /// <returns>A value indicating whether the element associated with this DataGridItemAutomationPeer is laid out in a specific direction.</returns>
        protected override AutomationOrientation GetOrientationCore()
        {
            return OwningRowPeer != null ? OwningRowPeer.GetOrientation() : AutomationOrientation.None;
        }

        /// <summary>
        /// Returns the control pattern that is associated with the specified Windows.UI.Xaml.Automation.Peers.PatternInterface.
        /// </summary>
        /// <param name="patternInterface">A value from the Windows.UI.Xaml.Automation.Peers.PatternInterface enumeration.</param>
        /// <returns>The object that supports the specified pattern, or null if unsupported.</returns>
        protected override object GetPatternCore(PatternInterface patternInterface)
        {
            switch (patternInterface)
            {
                case PatternInterface.Invoke:
                    if (!OwningDataGrid.IsReadOnly)
                    {
                        return this;
                    }

                    break;
                case PatternInterface.ScrollItem:
                    if (OwningDataGrid.VerticalScrollBar != null &&
                        OwningDataGrid.VerticalScrollBar.Maximum > 0)
                    {
                        return this;
                    }

                    break;
                case PatternInterface.Selection:
                case PatternInterface.SelectionItem:
                    return this;
            }

            return base.GetPatternCore(patternInterface);
        }

        /// <summary>
        /// Returns a value indicating whether the UIElement associated with this DataGridItemAutomationPeer can accept keyboard focus.
        /// </summary>
        /// <returns>True if the element is focusable by the keyboard; otherwise false.</returns>
        protected override bool HasKeyboardFocusCore()
        {
            return OwningRowPeer != null && OwningRowPeer.HasKeyboardFocus();
        }

        /// <summary>
        /// Returns a value indicating whether the element associated with this DataGridItemAutomationPeer is an element that contains data that is presented to the user.
        /// </summary>
        /// <returns>True if the element contains data for the user to read; otherwise, false.</returns>
        protected override bool IsContentElementCore()
        {
            return OwningRowPeer == null || OwningRowPeer.IsContentElement();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the UIElement associated with this DataGridItemAutomationPeer
        /// is understood by the end user as interactive.
        /// </summary>
        /// <returns>True if the UIElement associated with this DataGridItemAutomationPeer
        /// is understood by the end user as interactive.</returns>
        protected override bool IsControlElementCore()
        {
            return OwningRowPeer == null || OwningRowPeer.IsControlElement();
        }

        /// <summary>
        /// Gets a value indicating whether this DataGridItemAutomationPeer can receive and send events to the associated element.
        /// </summary>
        /// <returns>True if this DataGridItemAutomationPeer can receive and send events; otherwise, false.</returns>
        protected override bool IsEnabledCore()
        {
            return OwningRowPeer != null && OwningRowPeer.IsEnabled();
        }

        /// <summary>
        /// Gets a value indicating whether the UIElement associated with this DataGridItemAutomationPeer can accept keyboard focus.
        /// </summary>
        /// <returns>True if the UIElement associated with this DataGridItemAutomationPeer can accept keyboard focus.</returns>
        protected override bool IsKeyboardFocusableCore()
        {
            return OwningRowPeer != null && OwningRowPeer.IsKeyboardFocusable();
        }

        /// <summary>
        /// Gets a value indicating whether the UIElement associated with this DataGridItemAutomationPeer is off the screen.
        /// </summary>
        /// <returns>True if the element is not on the screen; otherwise, false.</returns>
        protected override bool IsOffscreenCore()
        {
            return OwningRowPeer == null || OwningRowPeer.IsOffscreen();
        }

        /// <summary>
        /// Gets a value indicating whether the UIElement associated with this DataGridItemAutomationPeer contains protected content.
        /// </summary>
        /// <returns>True if the UIElement contains protected content.</returns>
        protected override bool IsPasswordCore()
        {
            return OwningRowPeer != null && OwningRowPeer.IsPassword();
        }

        /// <summary>
        /// Gets a value indicating whether the UIElement associated with this DataGridItemAutomationPeer is required to be completed on a form.
        /// </summary>
        /// <returns>True if the UIElement is required to be completed on a form.</returns>
        protected override bool IsRequiredForFormCore()
        {
            return OwningRowPeer != null && OwningRowPeer.IsRequiredForForm();
        }

        /// <summary>
        /// Sets the keyboard input focus on the UIElement associated with this DataGridItemAutomationPeer.
        /// </summary>
        protected override void SetFocusCore()
        {
            OwningRowPeer?.SetFocus();
        }

        void IInvokeProvider.Invoke()
        {
            EnsureEnabled();

            if (OwningRowPeer == null)
            {
                OwningDataGrid.ScrollIntoView(_item, null);
            }

            bool success = false;
            if (OwningRow != null)
            {
                if (OwningDataGrid.WaitForLostFocus(() => { ((IInvokeProvider)this).Invoke(); }))
                {
                    return;
                }

                if (OwningDataGrid.EditingRow == OwningRow)
                {
                    success = OwningDataGrid.CommitEdit(DataGridEditingUnit.Row, true /*exitEditing*/);
                }
                else if (OwningDataGrid.UpdateSelectionAndCurrency(OwningDataGrid.CurrentColumnIndex, OwningRow.Slot, DataGridSelectionAction.SelectCurrent, false))
                {
                    success = OwningDataGrid.BeginEdit();
                }
            }
        }

        void IScrollItemProvider.ScrollIntoView()
        {
            OwningDataGrid.ScrollIntoView(_item, null);
        }

        bool ISelectionItemProvider.IsSelected => OwningDataGrid.SelectedItems.Contains(_item);

        IRawElementProviderSimple ISelectionItemProvider.SelectionContainer => ProviderFromPeer(_dataGridAutomationPeer);

        void ISelectionItemProvider.AddToSelection()
        {
            EnsureEnabled();

            if (OwningDataGrid.SelectionMode == DataGridSelectionMode.Single &&
                OwningDataGrid.SelectedItems.Count > 0 &&
                !OwningDataGrid.SelectedItems.Contains(_item))
            {
                throw DataGridError.DataGridAutomationPeer.OperationCannotBePerformed();
            }

            int index = OwningDataGrid.DataConnection.IndexOf(_item);
            if (index != -1)
            {
                OwningDataGrid.SetRowSelection(OwningDataGrid.SlotFromRowIndex(index), true, false);
                return;
            }

            throw DataGridError.DataGridAutomationPeer.OperationCannotBePerformed();
        }

        void ISelectionItemProvider.RemoveFromSelection()
        {
            EnsureEnabled();

            int index = OwningDataGrid.DataConnection.IndexOf(_item);
            if (index != -1)
            {
                bool success = true;
                if (OwningDataGrid.EditingRow != null && OwningDataGrid.EditingRow.Index == index)
                {
                    if (OwningDataGrid.WaitForLostFocus(() => { ((ISelectionItemProvider)this).RemoveFromSelection(); }))
                    {
                        return;
                    }

                    success = OwningDataGrid.CommitEdit(DataGridEditingUnit.Row, true /*exitEditing*/);
                }

                if (success)
                {
                    OwningDataGrid.SetRowSelection(OwningDataGrid.SlotFromRowIndex(index), false, false);
                    return;
                }

                throw DataGridError.DataGridAutomationPeer.OperationCannotBePerformed();
            }
        }

        void ISelectionItemProvider.Select()
        {
            EnsureEnabled();

            int index = OwningDataGrid.DataConnection.IndexOf(_item);
            if (index != -1)
            {
                bool success = true;
                if (OwningDataGrid.EditingRow != null && OwningDataGrid.EditingRow.Index != index)
                {
                    if (OwningDataGrid.WaitForLostFocus(() => { ((ISelectionItemProvider)this).Select(); }))
                    {
                        return;
                    }

                    success = OwningDataGrid.CommitEdit(DataGridEditingUnit.Row, true /*exitEditing*/);
                }

                if (success)
                {
                    // Clear all the other selected items and select this one
                    int slot = OwningDataGrid.SlotFromRowIndex(index);
                    OwningDataGrid.UpdateSelectionAndCurrency(OwningDataGrid.CurrentColumnIndex, slot, DataGridSelectionAction.SelectCurrent, false);
                    return;
                }

                throw DataGridError.DataGridAutomationPeer.OperationCannotBePerformed();
            }
        }

        bool ISelectionProvider.CanSelectMultiple => false;

        bool ISelectionProvider.IsSelectionRequired => false;

        IRawElementProviderSimple[] ISelectionProvider.GetSelection()
        {
            if (OwningRow != null &&
                OwningDataGrid.IsSlotVisible(OwningRow.Slot) &&
                OwningDataGrid.CurrentSlot == OwningRow.Slot)
            {
                DataGridCell cell = OwningRow.Cells[OwningRow.OwningGrid.CurrentColumnIndex];
                AutomationPeer peer = CreatePeerForElement(cell);
                if (peer != null)
                {
                    return [ProviderFromPeer(peer)];
                }
            }

            return null;
        }

        private void EnsureEnabled()
        {
            if (!_dataGridAutomationPeer.IsEnabled())
            {
                throw new ElementNotEnabledException();
            }
        }
    }
}
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.WinUI.Controls;
using CommunityToolkit.WinUI.Controls.DataGridInternals;
using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;
using Windows.UI.Xaml.Data;
using DiagnosticsDebug = System.Diagnostics.Debug;

namespace CommunityToolkit.WinUI.Automation.Peers
{
    /// <summary>
    /// AutomationPeer for a group of items in a DataGrid
    /// </summary>
    public partial class DataGridGroupItemAutomationPeer : FrameworkElementAutomationPeer,
        IExpandCollapseProvider, IGridProvider, IScrollItemProvider, ISelectionProvider
    {
        private ICollectionViewGroup _group;
        private AutomationPeer _dataGridAutomationPeer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridGroupItemAutomationPeer"/> class.
        /// </summary>
        public DataGridGroupItemAutomationPeer(ICollectionViewGroup group, DataGrid dataGrid)
            : base(dataGrid)
        {
            ArgumentNullException.ThrowIfNull(group);
            ArgumentNullException.ThrowIfNull(dataGrid);

            _group = group;
            _dataGridAutomationPeer = CreatePeerForElement(dataGrid);
        }

        /// <summary>
        /// Gets the owning DataGrid
        /// </summary>
        private DataGrid OwningDataGrid
        {
            get
            {
                DataGridAutomationPeer gridPeer = _dataGridAutomationPeer as DataGridAutomationPeer;
                return gridPeer.Owner as DataGrid;
            }
        }

        /// <summary>
        /// Gets the owning DataGrid's Automation Peer
        /// </summary>
        private DataGridAutomationPeer OwningDataGridPeer => _dataGridAutomationPeer as DataGridAutomationPeer;

        /// <summary>
        /// Gets the owning DataGridRowGroupHeader
        /// </summary>
        private DataGridRowGroupHeader OwningRowGroupHeader
        {
            get
            {
                if (OwningDataGrid != null)
                {
                    DataGridRowGroupInfo groupInfo = OwningDataGrid.RowGroupInfoFromCollectionViewGroup(_group);
                    if (groupInfo != null && OwningDataGrid.IsSlotVisible(groupInfo.Slot))
                    {
                        return OwningDataGrid.DisplayData.GetDisplayedElement(groupInfo.Slot) as DataGridRowGroupHeader;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the owning DataGridRowGroupHeader's Automation Peer
        /// </summary>
        internal DataGridRowGroupHeaderAutomationPeer OwningRowGroupHeaderPeer
        {
            get
            {
                DataGridRowGroupHeaderAutomationPeer rowGroupHeaderPeer = null;
                DataGridRowGroupHeader rowGroupHeader = OwningRowGroupHeader;
                if (rowGroupHeader != null)
                {
                    rowGroupHeaderPeer = FromElement(rowGroupHeader) as DataGridRowGroupHeaderAutomationPeer;
                    if (rowGroupHeaderPeer == null)
                    {
                        rowGroupHeaderPeer = CreatePeerForElement(rowGroupHeader) as DataGridRowGroupHeaderAutomationPeer;
                    }
                }

                return rowGroupHeaderPeer;
            }
        }

        /// <summary>
        /// Returns the accelerator key for the UIElement that is associated with this DataGridGroupItemAutomationPeer.
        /// </summary>
        /// <returns>The accelerator key for the UIElement that is associated with this DataGridGroupItemAutomationPeer.</returns>
        protected override string GetAcceleratorKeyCore()
        {
            return (OwningRowGroupHeaderPeer != null) ? OwningRowGroupHeaderPeer.GetAcceleratorKey() : string.Empty;
        }

        /// <summary>
        /// Returns the access key for the UIElement that is associated with this DataGridGroupItemAutomationPeer.
        /// </summary>
        /// <returns>The access key for the UIElement that is associated with this DataGridGroupItemAutomationPeer.</returns>
        protected override string GetAccessKeyCore()
        {
            return (OwningRowGroupHeaderPeer != null) ? OwningRowGroupHeaderPeer.GetAccessKey() : string.Empty;
        }

        /// <summary>
        /// Returns the control type for the UIElement that is associated with this DataGridGroupItemAutomationPeer.
        /// </summary>
        /// <returns>The control type for the UIElement that is associated with this DataGridGroupItemAutomationPeer.</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Group;
        }

        /// <summary>
        /// Returns the string that uniquely identifies the FrameworkElement that is associated with this DataGridGroupItemAutomationPeer.
        /// </summary>
        /// <returns>The string that uniquely identifies the FrameworkElement that is associated with this DataGridGroupItemAutomationPeer.</returns>
        protected override string GetAutomationIdCore()
        {
            // The AutomationId should be unset for dynamic content.
            return string.Empty;
        }

        /// <summary>
        /// Returns the Rect that represents the bounding rectangle of the UIElement that is associated with this DataGridGroupItemAutomationPeer.
        /// </summary>
        /// <returns>The Rect that represents the bounding rectangle of the UIElement that is associated with this DataGridGroupItemAutomationPeer.</returns>
        protected override Rect GetBoundingRectangleCore()
        {
            return OwningRowGroupHeaderPeer != null ? OwningRowGroupHeaderPeer.GetBoundingRectangle() : default(Rect);
        }

        /// <summary>
        /// Returns the collection of elements that are represented in the UI Automation tree as immediate
        /// child elements of the automation peer.
        /// </summary>
        /// <returns>The children elements.</returns>
        protected override IList<AutomationPeer> GetChildrenCore()
        {
            List<AutomationPeer> children = [];
            if (OwningRowGroupHeaderPeer != null)
            {
                OwningRowGroupHeaderPeer.InvalidatePeer();
                children.AddRange(OwningRowGroupHeaderPeer.GetChildren());
            }

#if FEATURE_ICOLLECTIONVIEW_GROUP
            if (_group.IsBottomLevel)
            {
#endif
            foreach (object item in _group.GroupItems /*Items*/)
            {
                children.Add(OwningDataGridPeer.GetOrCreateItemPeer(item));
            }
#if FEATURE_ICOLLECTIONVIEW_GROUP
            }
            else
            {
                foreach (object group in _group.Items)
                {
                    children.Add(this.OwningDataGridPeer.GetOrCreateGroupItemPeer(group));
                }
            }
#endif
            return children;
        }

        /// <summary>
        /// Called by GetClassName that gets a human readable name that, in addition to AutomationControlType,
        /// differentiates the control represented by this AutomationPeer.
        /// </summary>
        /// <returns>The string that contains the name.</returns>
        protected override string GetClassNameCore()
        {
            string classNameCore = OwningRowGroupHeaderPeer != null ? OwningRowGroupHeaderPeer.GetClassName() : string.Empty;
#if DEBUG_AUTOMATION
            Debug.WriteLine("DataGridGroupItemAutomationPeer.GetClassNameCore returns " + classNameCore);
#endif
            return classNameCore;
        }

        /// <summary>
        /// Returns a Point that represents the clickable space that is on the UIElement that is associated with this DataGridGroupItemAutomationPeer.
        /// </summary>
        /// <returns>A Point that represents the clickable space that is on the UIElement that is associated with this DataGridGroupItemAutomationPeer.</returns>
        protected override Point GetClickablePointCore()
        {
            return OwningRowGroupHeaderPeer != null ? OwningRowGroupHeaderPeer.GetClickablePoint() : new Point(double.NaN, double.NaN);
        }

        /// <summary>
        /// Returns the string that describes the functionality of the control that is associated with the automation peer.
        /// </summary>
        /// <returns>The string that contains the help text.</returns>
        protected override string GetHelpTextCore()
        {
            return OwningRowGroupHeaderPeer != null ? OwningRowGroupHeaderPeer.GetHelpText() : string.Empty;
        }

        /// <summary>
        /// Returns a string that communicates the visual status of the UIElement that is associated with this DataGridGroupItemAutomationPeer.
        /// </summary>
        /// <returns>A string that communicates the visual status of the UIElement that is associated with this DataGridGroupItemAutomationPeer.</returns>
        protected override string GetItemStatusCore()
        {
            return OwningRowGroupHeaderPeer != null ? OwningRowGroupHeaderPeer.GetItemStatus() : string.Empty;
        }

        /// <summary>
        /// Returns a human-readable string that contains the item type that the UIElement for this DataGridGroupItemAutomationPeer represents.
        /// </summary>
        /// <returns>A human-readable string that contains the item type that the UIElement for this DataGridGroupItemAutomationPeer represents.</returns>
        protected override string GetItemTypeCore()
        {
            return (OwningRowGroupHeaderPeer != null) ? OwningRowGroupHeaderPeer.GetItemType() : string.Empty;
        }

        /// <summary>
        /// Returns the AutomationPeer for the element that is targeted to the UIElement for this DataGridGroupItemAutomationPeer.
        /// </summary>
        /// <returns>The AutomationPeer for the element that is targeted to the UIElement for this DataGridGroupItemAutomationPeer.</returns>
        protected override AutomationPeer GetLabeledByCore()
        {
            return (OwningRowGroupHeaderPeer != null) ? OwningRowGroupHeaderPeer.GetLabeledBy() : null;
        }

        /// <summary>
        /// Returns a localized human readable string for this control type.
        /// </summary>
        /// <returns>A localized human readable string for this control type.</returns>
        protected override string GetLocalizedControlTypeCore()
        {
            return (OwningRowGroupHeaderPeer != null) ? OwningRowGroupHeaderPeer.GetLocalizedControlType() : string.Empty;
        }

        /// <summary>
        /// Returns the string that describes the functionality of the control that is associated with this DataGridGroupItemAutomationPeer.
        /// </summary>
        /// <returns>The string that contains the help text.</returns>
        protected override string GetNameCore()
        {
#if FEATURE_ICOLLECTIONVIEW_GROUP
            if (_group.Name != null)
            {
                string name = _group.Name.ToString();
                if (!string.IsNullOrEmpty(name))
                {
                    return name;
                }
            }
#endif
            return base.GetNameCore();
        }

        /// <summary>
        /// Returns a value indicating whether the element associated with this DataGridGroupItemAutomationPeer is laid out in a specific direction.
        /// </summary>
        /// <returns>A value indicating whether the element associated with this DataGridGroupItemAutomationPeer is laid out in a specific direction.</returns>
        protected override AutomationOrientation GetOrientationCore()
        {
            return (OwningRowGroupHeaderPeer != null) ? OwningRowGroupHeaderPeer.GetOrientation() : AutomationOrientation.None;
        }

        /// <summary>
        /// Gets the control pattern that is associated with the specified Windows.UI.Xaml.Automation.Peers.PatternInterface.
        /// </summary>
        /// <param name="patternInterface">A value from the Windows.UI.Xaml.Automation.Peers.PatternInterface enumeration.</param>
        /// <returns>The object that supports the specified pattern, or null if unsupported.</returns>
        protected override object GetPatternCore(PatternInterface patternInterface)
        {
            switch (patternInterface)
            {
                case PatternInterface.ExpandCollapse:
                case PatternInterface.Grid:
                case PatternInterface.Selection:
                case PatternInterface.Table:
                    return this;
                case PatternInterface.ScrollItem:
                    {
                        if (OwningDataGrid.VerticalScrollBar != null &&
                            OwningDataGrid.VerticalScrollBar.Maximum > 0)
                        {
                            return this;
                        }

                        break;
                    }
            }

            return base.GetPatternCore(patternInterface);
        }

        /// <summary>
        /// Returns a value indicating whether the UIElement associated with this DataGridGroupItemAutomationPeer can accept keyboard focus.
        /// </summary>
        /// <returns>True if the element is focusable by the keyboard; otherwise false.</returns>
        protected override bool HasKeyboardFocusCore()
        {
            return OwningRowGroupHeaderPeer != null ? OwningRowGroupHeaderPeer.HasKeyboardFocus() : false;
        }

        /// <summary>
        /// Returns a value indicating whether the element associated with this DataGridGroupItemAutomationPeer is an element that contains data that is presented to the user.
        /// </summary>
        /// <returns>True if the element contains data for the user to read; otherwise, false.</returns>
        protected override bool IsContentElementCore()
        {
            return OwningRowGroupHeaderPeer != null ? OwningRowGroupHeaderPeer.IsContentElement() : true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the UIElement associated with this DataGridGroupItemAutomationPeer
        /// is understood by the end user as interactive.
        /// </summary>
        /// <returns>True if the UIElement associated with this DataGridGroupItemAutomationPeer
        /// is understood by the end user as interactive.</returns>
        protected override bool IsControlElementCore()
        {
            return OwningRowGroupHeaderPeer != null ? OwningRowGroupHeaderPeer.IsControlElement() : true;
        }

        /// <summary>
        /// Gets a value indicating whether this DataGridGroupItemAutomationPeer can receive and send events to the associated element.
        /// </summary>
        /// <returns>True if this DataGridGroupItemAutomationPeer can receive and send events; otherwise, false.</returns>
        protected override bool IsEnabledCore()
        {
            return OwningRowGroupHeaderPeer != null ? OwningRowGroupHeaderPeer.IsEnabled() : false;
        }

        /// <summary>
        /// Gets a value indicating whether the UIElement associated with this DataGridGroupItemAutomationPeer can accept keyboard focus.
        /// </summary>
        /// <returns>True if the UIElement associated with this DataGridGroupItemAutomationPeer can accept keyboard focus.</returns>
        protected override bool IsKeyboardFocusableCore()
        {
            return OwningRowGroupHeaderPeer != null ? OwningRowGroupHeaderPeer.IsKeyboardFocusable() : false;
        }

        /// <summary>
        /// Gets a value indicating whether the UIElement associated with this DataGridGroupItemAutomationPeer is off the screen.
        /// </summary>
        /// <returns>True if the element is not on the screen; otherwise, false.</returns>
        protected override bool IsOffscreenCore()
        {
            return OwningRowGroupHeaderPeer != null ? OwningRowGroupHeaderPeer.IsOffscreen() : true;
        }

        /// <summary>
        /// Gets a value indicating whether the UIElement associated with this DataGridGroupItemAutomationPeer contains protected content.
        /// </summary>
        /// <returns>True if the UIElement contains protected content.</returns>
        protected override bool IsPasswordCore()
        {
            return OwningRowGroupHeaderPeer != null ? OwningRowGroupHeaderPeer.IsPassword() : false;
        }

        /// <summary>
        /// Gets a value indicating whether the UIElement associated with this DataGridGroupItemAutomationPeer is required to be completed on a form.
        /// </summary>
        /// <returns>True if the UIElement is required to be completed on a form.</returns>
        protected override bool IsRequiredForFormCore()
        {
            return OwningRowGroupHeaderPeer != null && OwningRowGroupHeaderPeer.IsRequiredForForm();
        }

        /// <summary>
        /// Sets the keyboard input focus on the UIElement associated with this DataGridGroupItemAutomationPeer.
        /// </summary>
        protected override void SetFocusCore()
        {
            OwningRowGroupHeaderPeer?.SetFocus();
        }

        void IExpandCollapseProvider.Collapse()
        {
            EnsureEnabled();

            OwningDataGrid?.CollapseRowGroup(_group, false /*collapseAllSubgroups*/);
        }

        void IExpandCollapseProvider.Expand()
        {
            EnsureEnabled();

            OwningDataGrid?.ExpandRowGroup(_group, false /*expandAllSubgroups*/);
        }

        ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
        {
            get
            {
                if (OwningDataGrid != null)
                {
                    DataGridRowGroupInfo groupInfo = OwningDataGrid.RowGroupInfoFromCollectionViewGroup(_group);
                    if (groupInfo != null && groupInfo.Visibility == Visibility.Visible)
                    {
                        return ExpandCollapseState.Expanded;
                    }
                }

                return ExpandCollapseState.Collapsed;
            }
        }

        int IGridProvider.ColumnCount
        {
            get
            {
                if (OwningDataGrid != null)
                {
                    return OwningDataGrid.Columns.Count;
                }

                return 0;
            }
        }

        IRawElementProviderSimple IGridProvider.GetItem(int row, int column)
        {
            EnsureEnabled();

            if (OwningDataGrid != null &&
                OwningDataGrid.DataConnection != null &&
                row >= 0 && row < _group.GroupItems.Count /*ItemCount*/ &&
                column >= 0 && column < OwningDataGrid.Columns.Count)
            {
                DataGridRowGroupInfo groupInfo = OwningDataGrid.RowGroupInfoFromCollectionViewGroup(_group);
                if (groupInfo != null)
                {
                    // Adjust the row index to be relative to the DataGrid instead of the group
                    row = groupInfo.Slot - OwningDataGrid.RowGroupHeadersTable.GetIndexCount(0, groupInfo.Slot) + row + 1;
                    DiagnosticsDebug.Assert(row >= 0, "Expected positive row.");
                    DiagnosticsDebug.Assert(row < OwningDataGrid.DataConnection.Count, "Expected row smaller than this.OwningDataGrid.DataConnection.Count.");
                    int slot = OwningDataGrid.SlotFromRowIndex(row);

                    if (!OwningDataGrid.IsSlotVisible(slot))
                    {
                        object item = OwningDataGrid.DataConnection.GetDataItem(row);
                        OwningDataGrid.ScrollIntoView(item, OwningDataGrid.Columns[column]);
                    }

                    DiagnosticsDebug.Assert(OwningDataGrid.IsSlotVisible(slot), "Expected OwningDataGrid.IsSlotVisible(slot) is true.");

                    DataGridRow dgr = OwningDataGrid.DisplayData.GetDisplayedElement(slot) as DataGridRow;

                    // the first cell is always the indentation filler cell if grouping is enabled, so skip it
                    DiagnosticsDebug.Assert(column + 1 < dgr.Cells.Count, "Expected column + 1 smaller than dgr.Cells.Count.");
                    DataGridCell cell = dgr.Cells[column + 1];
                    AutomationPeer peer = CreatePeerForElement(cell);
                    if (peer != null)
                    {
                        return ProviderFromPeer(peer);
                    }
                }
            }

            return null;
        }

        int IGridProvider.RowCount => _group.GroupItems.Count /*ItemCount*/;

        void IScrollItemProvider.ScrollIntoView()
        {
            EnsureEnabled();

            if (OwningDataGrid != null)
            {
                DataGridRowGroupInfo groupInfo = OwningDataGrid.RowGroupInfoFromCollectionViewGroup(_group);
                if (groupInfo != null)
                {
                    OwningDataGrid.ScrollIntoView(groupInfo.CollectionViewGroup, null);
                }
            }
        }

        IRawElementProviderSimple[] ISelectionProvider.GetSelection()
        {
            EnsureEnabled();

            if (OwningDataGrid != null &&
                OwningDataGridPeer != null &&
                OwningDataGrid.SelectedItems != null &&
                _group.GroupItems.Count /*ItemCount*/ > 0)
            {
                DataGridRowGroupInfo groupInfo = OwningDataGrid.RowGroupInfoFromCollectionViewGroup(_group);
                if (groupInfo != null)
                {
                    // See which of the selected items are contained within this group
                    List<IRawElementProviderSimple> selectedProviders = [];
                    int startRowIndex = groupInfo.Slot - OwningDataGrid.RowGroupHeadersTable.GetIndexCount(0, groupInfo.Slot) + 1;
                    foreach (object item in OwningDataGrid.GetSelectionInclusive(startRowIndex, startRowIndex + _group.GroupItems.Count /*ItemCount*/ - 1))
                    {
                        DataGridItemAutomationPeer peer = OwningDataGridPeer.GetOrCreateItemPeer(item);
                        if (peer != null)
                        {
                            selectedProviders.Add(ProviderFromPeer(peer));
                        }
                    }

                    return [.. selectedProviders];
                }
            }

            return null;
        }

        bool ISelectionProvider.CanSelectMultiple => OwningDataGrid != null && OwningDataGrid.SelectionMode == DataGridSelectionMode.Extended;

        bool ISelectionProvider.IsSelectionRequired => false;

        private void EnsureEnabled()
        {
            if (!_dataGridAutomationPeer.IsEnabled())
            {
                throw new ElementNotEnabledException();
            }
        }
    }
}
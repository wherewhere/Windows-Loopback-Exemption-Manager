// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.WinUI.Controls;
using CommunityToolkit.WinUI.Controls.DataGridInternals;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;
using Windows.UI.Xaml.Controls;

namespace CommunityToolkit.WinUI.Automation.Peers
{
    /// <summary>
    /// AutomationPeer for DataGridCell
    /// </summary>
    public partial class DataGridCellAutomationPeer : FrameworkElementAutomationPeer,
        IGridItemProvider, IInvokeProvider, IScrollItemProvider, ISelectionItemProvider, ITableItemProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridCellAutomationPeer"/> class.
        /// </summary>
        /// <param name="owner">DataGridCell</param>
        public DataGridCellAutomationPeer(DataGridCell owner)
            : base(owner)
        {
        }

        private IRawElementProviderSimple ContainingGrid
        {
            get
            {
                AutomationPeer peer = CreatePeerForElement(this.OwningGrid);
                if (peer != null)
                {
                    return ProviderFromPeer(peer);
                }

                return null;
            }
        }

        private DataGridCell OwningCell => Owner as DataGridCell;

        private DataGridColumn OwningColumn => OwningCell.OwningColumn;

        private DataGrid OwningGrid => OwningCell.OwningGrid;

        private DataGridRow OwningRow => OwningCell.OwningRow;

        /// <summary>
        /// Gets the control type for the element that is associated with the UI Automation peer.
        /// </summary>
        /// <returns>The control type.</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return OwningColumn switch
            {
                DataGridCheckBoxColumn => AutomationControlType.CheckBox,
                DataGridTextColumn => AutomationControlType.Text,
                DataGridComboBoxColumn => AutomationControlType.ComboBox,
                _ => AutomationControlType.Custom,
            };
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
            System.Diagnostics.Debug.WriteLine("DataGridCellAutomationPeer.GetClassNameCore returns " + classNameCore);
#endif
            return classNameCore;
        }

        /// <summary>
        /// Gets the name of the element.
        /// </summary>
        /// <returns>The string that contains the name.</returns>
        protected override string GetNameCore()
        {
            TextBlock textBlock = OwningCell.Content as TextBlock;
            if (textBlock != null)
            {
                return textBlock.Text;
            }

            TextBox textBox = OwningCell.Content as TextBox;
            if (textBox != null)
            {
                return textBox.Text;
            }

            if (OwningColumn != null && OwningRow != null)
            {
                object cellContent = null;
                DataGridBoundColumn boundColumn = OwningColumn as DataGridBoundColumn;
                if (boundColumn != null && boundColumn.Binding != null)
                {
                    cellContent = boundColumn.GetCellValue(OwningRow.DataContext, boundColumn.Binding);
                }

                if (cellContent == null && OwningColumn.ClipboardContentBinding != null)
                {
                    cellContent = OwningColumn.GetCellValue(OwningRow.DataContext, OwningColumn.ClipboardContentBinding);
                }

                if (cellContent != null)
                {
                    string cellName = cellContent.ToString();
                    if (!string.IsNullOrEmpty(cellName))
                    {
                        return cellName;
                    }
                }
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
            if (OwningGrid != null)
            {
                switch (patternInterface)
                {
                    case PatternInterface.Invoke:
                        {
                            if (!OwningGrid.IsReadOnly &&
                                OwningColumn != null &&
                                !OwningColumn.IsReadOnly)
                            {
                                return this;
                            }

                            break;
                        }

                    case PatternInterface.ScrollItem:
                        {
                            if (OwningGrid.HorizontalScrollBar != null &&
                                OwningGrid.HorizontalScrollBar.Maximum > 0)
                            {
                                return this;
                            }

                            break;
                        }

                    case PatternInterface.GridItem:
                    case PatternInterface.SelectionItem:
                    case PatternInterface.TableItem:
                        return this;
                }
            }

            return base.GetPatternCore(patternInterface);
        }

        /// <summary>
        /// Gets a value that indicates whether the element can accept keyboard focus.
        /// </summary>
        /// <returns>true if the element can accept keyboard focus; otherwise, false</returns>
        protected override bool IsKeyboardFocusableCore()
        {
            return true;
        }

        int IGridItemProvider.Column
        {
            get
            {
                int column = OwningCell.ColumnIndex;
                if (column >= 0 && OwningGrid != null && OwningGrid.ColumnsInternal.RowGroupSpacerColumn.IsRepresented)
                {
                    column--;
                }

                return column;
            }
        }

        int IGridItemProvider.ColumnSpan => 1;

        IRawElementProviderSimple IGridItemProvider.ContainingGrid => this.ContainingGrid;

        int IGridItemProvider.Row => OwningCell.RowIndex;

        int IGridItemProvider.RowSpan => 1;

        void IInvokeProvider.Invoke()
        {
            EnsureEnabled();

            if (OwningGrid != null)
            {
                if (OwningGrid.WaitForLostFocus(() => { ((IInvokeProvider)this).Invoke(); }))
                {
                    return;
                }

                if (OwningGrid.EditingRow == OwningRow && OwningGrid.EditingColumnIndex == OwningCell.ColumnIndex)
                {
                    OwningGrid.CommitEdit(DataGridEditingUnit.Cell, true /*exitEditingMode*/);
                }
                else if (OwningGrid.UpdateSelectionAndCurrency(OwningCell.ColumnIndex, OwningRow.Slot, DataGridSelectionAction.SelectCurrent, true))
                {
                    OwningGrid.BeginEdit();
                }
            }
        }

        void IScrollItemProvider.ScrollIntoView()
        {
            if (OwningGrid != null)
            {
                OwningGrid.ScrollIntoView(OwningCell.DataContext, OwningColumn);
            }
            else
            {
                throw DataGridError.DataGridAutomationPeer.OperationCannotBePerformed();
            }
        }

        bool ISelectionItemProvider.IsSelected
        {
            get
            {
                if (OwningGrid != null && OwningRow != null)
                {
                    return OwningRow.IsSelected;
                }

                throw DataGridError.DataGridAutomationPeer.OperationCannotBePerformed();
            }
        }

        IRawElementProviderSimple ISelectionItemProvider.SelectionContainer
        {
            get
            {
                AutomationPeer peer = CreatePeerForElement(OwningRow);
                if (peer != null)
                {
                    return ProviderFromPeer(peer);
                }

                return null;
            }
        }

        void ISelectionItemProvider.AddToSelection()
        {
            EnsureEnabled();
            if (OwningCell.OwningGrid == null ||
                OwningCell.OwningGrid.CurrentSlot != OwningCell.RowIndex ||
                OwningCell.OwningGrid.CurrentColumnIndex != OwningCell.ColumnIndex)
            {
                throw DataGridError.DataGridAutomationPeer.OperationCannotBePerformed();
            }
        }

        void ISelectionItemProvider.RemoveFromSelection()
        {
            EnsureEnabled();
            if (OwningCell.OwningGrid == null ||
                (OwningCell.OwningGrid.CurrentSlot == OwningCell.RowIndex &&
                 OwningCell.OwningGrid.CurrentColumnIndex == OwningCell.ColumnIndex))
            {
                throw DataGridError.DataGridAutomationPeer.OperationCannotBePerformed();
            }
        }

        void ISelectionItemProvider.Select()
        {
            EnsureEnabled();

            if (OwningGrid != null)
            {
                if (OwningGrid.WaitForLostFocus(() => { ((ISelectionItemProvider)this).Select(); }))
                {
                    return;
                }

                OwningGrid.UpdateSelectionAndCurrency(OwningCell.ColumnIndex, OwningRow.Slot, DataGridSelectionAction.SelectCurrent, false);
            }
        }

        IRawElementProviderSimple[] ITableItemProvider.GetColumnHeaderItems()
        {
            if (OwningGrid != null &&
                OwningGrid.AreColumnHeadersVisible &&
                OwningColumn.HeaderCell != null)
            {
                AutomationPeer peer = CreatePeerForElement(OwningColumn.HeaderCell);
                if (peer != null)
                {
                    return [ProviderFromPeer(peer)];
                }
            }

            return null;
        }

        IRawElementProviderSimple[] ITableItemProvider.GetRowHeaderItems()
        {
            if (OwningGrid != null &&
                OwningGrid.AreRowHeadersVisible &&
                OwningRow.HeaderCell != null)
            {
                AutomationPeer peer = CreatePeerForElement(OwningRow.HeaderCell);
                if (peer != null)
                {
                    return [ProviderFromPeer(peer)];
                }
            }

            return null;
        }

        private void EnsureEnabled()
        {
            if (!IsEnabled())
            {
                throw new ElementNotEnabledException();
            }
        }
    }
}
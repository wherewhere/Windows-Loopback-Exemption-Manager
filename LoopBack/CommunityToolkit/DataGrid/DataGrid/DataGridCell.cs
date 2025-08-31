// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.WinUI.Automation.Peers;
using CommunityToolkit.WinUI.Controls.DataGridInternals;
using CommunityToolkit.WinUI.Controls.Utilities;
using CommunityToolkit.WinUI.Utilities;
using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Shapes;

using DiagnosticsDebug = System.Diagnostics.Debug;

namespace CommunityToolkit.WinUI.Controls
{
    /// <summary>
    /// Represents an individual <see cref="DataGrid"/> cell.
    /// </summary>
    [TemplatePart(Name = DATAGRIDCELL_elementRightGridLine, Type = typeof(Rectangle))]

    [TemplateVisualState(Name = VisualStates.StateNormal, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = VisualStates.StatePointerOver, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = VisualStates.StateUnselected, GroupName = VisualStates.GroupSelection)]
    [TemplateVisualState(Name = VisualStates.StateSelected, GroupName = VisualStates.GroupSelection)]
    [TemplateVisualState(Name = VisualStates.StateRegular, GroupName = VisualStates.GroupCurrent)]
    [TemplateVisualState(Name = VisualStates.StateCurrent, GroupName = VisualStates.GroupCurrent)]
    [TemplateVisualState(Name = VisualStates.StateCurrentWithFocus, GroupName = VisualStates.GroupCurrent)]
    [TemplateVisualState(Name = VisualStates.StateDisplay, GroupName = VisualStates.GroupInteraction)]
    [TemplateVisualState(Name = VisualStates.StateEditing, GroupName = VisualStates.GroupInteraction)]
    [TemplateVisualState(Name = VisualStates.StateInvalid, GroupName = VisualStates.GroupValidation)]
    [TemplateVisualState(Name = VisualStates.StateValid, GroupName = VisualStates.GroupValidation)]
    public sealed partial class DataGridCell : ContentControl
    {
        private const string DATAGRIDCELL_elementRightGridLine = "RightGridLine";

        private Rectangle _rightGridLine;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridCell"/> class.
        /// </summary>
        public DataGridCell()
        {
            IsTapEnabled = true;
            AddHandler(TappedEvent, new TappedEventHandler(DataGridCell_PointerTapped), true /*handledEventsToo*/);

            PointerCanceled += new PointerEventHandler(DataGridCell_PointerCanceled);
            PointerCaptureLost += new PointerEventHandler(DataGridCell_PointerCaptureLost);
            PointerPressed += new PointerEventHandler(DataGridCell_PointerPressed);
            PointerReleased += new PointerEventHandler(DataGridCell_PointerReleased);
            PointerEntered += new PointerEventHandler(DataGridCell_PointerEntered);
            PointerExited += new PointerEventHandler(DataGridCell_PointerExited);
            PointerMoved += new PointerEventHandler(DataGridCell_PointerMoved);

            DefaultStyleKey = typeof(DataGridCell);
        }

        /// <summary>
        /// Gets a value indicating whether the data in a cell is valid.
        /// </summary>
        public bool IsValid
        {
            get => (bool)GetValue(IsValidProperty);
            internal set => this.SetValueNoCallback(IsValidProperty, value);
        }

        /// <summary>
        /// Identifies the IsValid dependency property.
        /// </summary>
        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register(
                "IsValid",
                typeof(bool),
                typeof(DataGridCell),
                new PropertyMetadata(true, OnIsValidPropertyChanged));

        /// <summary>
        /// IsValidProperty property changed handler.
        /// </summary>
        /// <param name="d">DataGridCell that changed its IsValid.</param>
        /// <param name="e">DependencyPropertyChangedEventArgs.</param>
        private static void OnIsValidPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataGridCell dataGridCell = d as DataGridCell;
            if (!dataGridCell.IsHandlerSuspended(e.Property))
            {
                dataGridCell.SetValueNoCallback(DataGridCell.IsValidProperty, e.OldValue);
                throw DataGridError.DataGrid.UnderlyingPropertyIsReadOnly("IsValid");
            }
        }

        internal double ActualRightGridLineWidth
        {
            get
            {
                if (_rightGridLine != null)
                {
                    return _rightGridLine.ActualWidth;
                }

                return 0;
            }
        }

        internal int ColumnIndex
        {
            get
            {
                if (OwningColumn == null)
                {
                    return -1;
                }

                return OwningColumn.Index;
            }
        }

        internal bool IsCurrent
        {
            get
            {
                DiagnosticsDebug.Assert(OwningGrid != null && OwningColumn != null && OwningRow != null, "Expected non-null owning DataGrid, DataGridColumn and DataGridRow.");

                return OwningGrid.CurrentColumnIndex == OwningColumn.Index &&
                       OwningGrid.CurrentSlot == OwningRow.Slot;
            }
        }

        internal bool IsPointerOver
        {
            get => InteractionInfo != null && InteractionInfo.IsPointerOver;

            set
            {
                if (value && InteractionInfo == null)
                {
                    InteractionInfo = new DataGridInteractionInfo();
                }

                if (InteractionInfo != null)
                {
                    InteractionInfo.IsPointerOver = value;
                }

                ApplyCellState(true /*animate*/);
            }
        }

        internal DataGridColumn OwningColumn { get; set; }

        internal DataGrid OwningGrid
        {
            get
            {
                if (OwningRow != null && OwningRow.OwningGrid != null)
                {
                    return OwningRow.OwningGrid;
                }

                if (OwningColumn != null)
                {
                    return OwningColumn.OwningGrid;
                }

                return null;
            }
        }

        internal DataGridRow OwningRow { get; set; }

        internal int RowIndex
        {
            get
            {
                if (OwningRow == null)
                {
                    return -1;
                }

                return OwningRow.Index;
            }
        }

        private DataGridInteractionInfo InteractionInfo { get; set; }

        private bool IsEdited
        {
            get
            {
                DiagnosticsDebug.Assert(OwningGrid != null, "Expected non-null owning DataGrid.");

                return OwningGrid.EditingRow == OwningRow &&
                       OwningGrid.EditingColumnIndex == ColumnIndex;
            }
        }

        /// <summary>
        /// Builds the visual tree for the row header when a new template is applied.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ApplyCellState(false /*animate*/);

            _rightGridLine = GetTemplateChild(DATAGRIDCELL_elementRightGridLine) as Rectangle;
            if (_rightGridLine != null && OwningColumn == null)
            {
                // Turn off the right GridLine for filler cells
                _rightGridLine.Visibility = Visibility.Collapsed;
            }
            else
            {
                EnsureGridLine(null);
            }
        }

        /// <summary>
        /// Creates AutomationPeer (<see cref="UIElement.OnCreateAutomationPeer"/>)
        /// </summary>
        /// <returns>An automation peer for this <see cref="DataGridCell"/>.</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            if (OwningGrid != null &&
                OwningColumn != null &&
                OwningColumn != OwningGrid.ColumnsInternal.FillerColumn)
            {
                return new DataGridCellAutomationPeer(this);
            }

            return base.OnCreateAutomationPeer();
        }

        internal void ApplyCellState(bool animate)
        {
            if (OwningGrid == null || OwningColumn == null || OwningRow == null || OwningRow.Visibility == Visibility.Collapsed || OwningRow.Slot == -1)
            {
                return;
            }

            // CommonStates
            if (IsPointerOver)
            {
                VisualStates.GoToState(this, animate, VisualStates.StatePointerOver, VisualStates.StateNormal);
            }
            else
            {
                VisualStates.GoToState(this, animate, VisualStates.StateNormal);
            }

            // SelectionStates
            if (OwningRow.IsSelected)
            {
                VisualStates.GoToState(this, animate, VisualStates.StateSelected, VisualStates.StateUnselected);
            }
            else
            {
                VisualStates.GoToState(this, animate, VisualStates.StateUnselected);
            }

            // CurrentStates
            if (IsCurrent && !OwningGrid.ColumnHeaderHasFocus)
            {
                if (OwningGrid.ContainsFocus)
                {
                    VisualStates.GoToState(this, animate, VisualStates.StateCurrentWithFocus, VisualStates.StateCurrent, VisualStates.StateRegular);
                }
                else
                {
                    VisualStates.GoToState(this, animate, VisualStates.StateCurrent, VisualStates.StateRegular);
                }
            }
            else
            {
                VisualStates.GoToState(this, animate, VisualStates.StateRegular);
            }

            // Interaction states
            if (IsEdited)
            {
                VisualStates.GoToState(this, animate, VisualStates.StateEditing, VisualStates.StateDisplay);
            }
            else
            {
                VisualStates.GoToState(this, animate, VisualStates.StateDisplay);
            }

            // Validation states
            if (IsValid)
            {
                VisualStates.GoToState(this, animate, VisualStates.StateValid);
            }
            else
            {
                VisualStates.GoToState(this, animate, VisualStates.StateInvalid, VisualStates.StateValid);
            }
        }

        // Makes sure the right gridline has the proper stroke and visibility. If lastVisibleColumn is specified, the
        // right gridline will be collapsed if this cell belongs to the lastVisibleColumn and there is no filler column
        internal void EnsureGridLine(DataGridColumn lastVisibleColumn)
        {
            if (OwningGrid != null && _rightGridLine != null)
            {
                if (OwningColumn is not DataGridFillerColumn && OwningGrid.VerticalGridLinesBrush != null && OwningGrid.VerticalGridLinesBrush != _rightGridLine.Fill)
                {
                    _rightGridLine.Fill = OwningGrid.VerticalGridLinesBrush;
                }

                Visibility newVisibility =
                    (OwningGrid.GridLinesVisibility == DataGridGridLinesVisibility.Vertical || OwningGrid.GridLinesVisibility == DataGridGridLinesVisibility.All) &&
                    (OwningGrid.ColumnsInternal.FillerColumn.IsActive || OwningColumn != lastVisibleColumn)
                    ? Visibility.Visible : Visibility.Collapsed;

                if (newVisibility != _rightGridLine.Visibility)
                {
                    _rightGridLine.Visibility = newVisibility;
                }
            }
        }

        /// <summary>
        /// Ensures that the correct Style is applied to this object.
        /// </summary>
        /// <param name="previousStyle">Caller's previous associated Style</param>
        internal void EnsureStyle(Style previousStyle)
        {
            if (Style != null &&
                (OwningColumn == null || Style != OwningColumn.CellStyle) &&
                (OwningGrid == null || Style != OwningGrid.CellStyle) &&
                Style != previousStyle)
            {
                return;
            }

            Style style = null;
            if (OwningColumn != null)
            {
                style = OwningColumn.CellStyle;
            }

            if (style == null && OwningGrid != null)
            {
                style = OwningGrid.CellStyle;
            }

            this.SetStyleWithType(style);
        }

        internal void Recycle()
        {
            InteractionInfo = null;
        }

        private void CancelPointer(PointerRoutedEventArgs e)
        {
            if (InteractionInfo != null && InteractionInfo.CapturedPointerId == e.Pointer.PointerId)
            {
                InteractionInfo.CapturedPointerId = 0u;
            }

            IsPointerOver = false;
        }

        private void DataGridCell_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            CancelPointer(e);
        }

        private void DataGridCell_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            CancelPointer(e);
        }

        private void DataGridCell_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Touch &&
                OwningGrid != null &&
                OwningGrid.AllowsManipulation &&
                (InteractionInfo == null || InteractionInfo.CapturedPointerId == 0u) &&
                CapturePointer(e.Pointer))
            {
                InteractionInfo ??= new DataGridInteractionInfo();

                InteractionInfo.CapturedPointerId = e.Pointer.PointerId;
            }
        }

        private void DataGridCell_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (InteractionInfo != null && InteractionInfo.CapturedPointerId == e.Pointer.PointerId)
            {
                ReleasePointerCapture(e.Pointer);
            }
        }

        private void DataGridCell_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            UpdateIsPointerOver(true);
        }

        private void DataGridCell_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            UpdateIsPointerOver(false);
        }

        private void DataGridCell_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            UpdateIsPointerOver(true);
        }

        private void DataGridCell_PointerTapped(object sender, TappedRoutedEventArgs e)
        {
            // OwningGrid is null for TopLeftHeaderCell and TopRightHeaderCell because they have no OwningRow
            if (OwningGrid != null && !OwningGrid.HasColumnUserInteraction)
            {
                if (!e.Handled && OwningGrid.IsTabStop)
                {
                    bool success = OwningGrid.Focus(FocusState.Programmatic);
                    DiagnosticsDebug.Assert(success, "Expected successful focus change.");
                }

                if (OwningRow != null)
                {
                    DiagnosticsDebug.Assert(sender is DataGridCell, "Expected sender is DataGridCell.");
                    DiagnosticsDebug.Assert(sender as DataGridCell == this, "Expected sender is ");
                    e.Handled = OwningGrid.UpdateStateOnTapped(e, ColumnIndex, OwningRow.Slot, !e.Handled /*allowEdit*/);
                    OwningGrid.UpdatedStateOnTapped = true;
                }
            }
        }

        private void UpdateIsPointerOver(bool isPointerOver)
        {
            if (InteractionInfo != null && InteractionInfo.CapturedPointerId != 0u)
            {
                return;
            }

            IsPointerOver = isPointerOver;
        }
    }
}
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.WinUI.Automation.Peers;
using CommunityToolkit.WinUI.Controls.DataGridInternals;
using CommunityToolkit.WinUI.Controls.Utilities;
using CommunityToolkit.WinUI.Utilities;
using System;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using DiagnosticsDebug = System.Diagnostics.Debug;

namespace CommunityToolkit.WinUI.Controls.Primitives
{
    /// <summary>
    /// Represents an individual <see cref="DataGrid"/> column header.
    /// </summary>
    [TemplateVisualState(Name = VisualStates.StateNormal, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = VisualStates.StatePointerOver, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = VisualStates.StatePressed, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = VisualStates.StateFocused, GroupName = VisualStates.GroupFocus)]
    [TemplateVisualState(Name = VisualStates.StateUnfocused, GroupName = VisualStates.GroupFocus)]
    [TemplateVisualState(Name = VisualStates.StateUnsorted, GroupName = VisualStates.GroupSort)]
    [TemplateVisualState(Name = VisualStates.StateSortAscending, GroupName = VisualStates.GroupSort)]
    [TemplateVisualState(Name = VisualStates.StateSortDescending, GroupName = VisualStates.GroupSort)]
    public partial class DataGridColumnHeader : ContentControl
    {
        internal enum DragMode
        {
            None = 0,
            PointerPressed = 1,
            Drag = 2,
            Resize = 3,
            Reorder = 4
        }

        private const int DATAGRIDCOLUMNHEADER_dragThreshold = 2;
        private const int DATAGRIDCOLUMNHEADER_resizeRegionWidthStrict = 5;
        private const int DATAGRIDCOLUMNHEADER_resizeRegionWidthLoose = 9;
        private const double DATAGRIDCOLUMNHEADER_separatorThickness = 1;

        private Visibility _desiredSeparatorVisibility;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridColumnHeader"/> class.
        /// </summary>
        public DataGridColumnHeader()
        {
            PointerCanceled += new PointerEventHandler(DataGridColumnHeader_PointerCanceled);
            PointerCaptureLost += new PointerEventHandler(DataGridColumnHeader_PointerCaptureLost);
            PointerPressed += new PointerEventHandler(DataGridColumnHeader_PointerPressed);
            PointerReleased += new PointerEventHandler(DataGridColumnHeader_PointerReleased);
            PointerMoved += new PointerEventHandler(DataGridColumnHeader_PointerMoved);
            PointerEntered += new PointerEventHandler(DataGridColumnHeader_PointerEntered);
            PointerExited += new PointerEventHandler(DataGridColumnHeader_PointerExited);
            IsEnabledChanged += new DependencyPropertyChangedEventHandler(DataGridColumnHeader_IsEnabledChanged);

            DefaultStyleKey = typeof(DataGridColumnHeader);
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> used to paint the column header separator lines.
        /// </summary>
        public Brush SeparatorBrush
        {
            get => GetValue(SeparatorBrushProperty) as Brush;
            set => SetValue(SeparatorBrushProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="SeparatorBrush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SeparatorBrushProperty =
            DependencyProperty.Register(
                "SeparatorBrush",
                typeof(Brush),
                typeof(DataGridColumnHeader),
                null);

        /// <summary>
        /// Gets or sets a value indicating whether the column header separator lines are visible.
        /// </summary>
        public Visibility SeparatorVisibility
        {
            get => (Visibility)GetValue(SeparatorVisibilityProperty);
            set => SetValue(SeparatorVisibilityProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="SeparatorVisibility"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SeparatorVisibilityProperty =
            DependencyProperty.Register(
                "SeparatorVisibility",
                typeof(Visibility),
                typeof(DataGridColumnHeader),
                new PropertyMetadata(Visibility.Visible, OnSeparatorVisibilityPropertyChanged));

        private static void OnSeparatorVisibilityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataGridColumnHeader columnHeader = d as DataGridColumnHeader;

            if (!columnHeader.IsHandlerSuspended(e.Property))
            {
                columnHeader._desiredSeparatorVisibility = (Visibility)e.NewValue;
                if (columnHeader.OwningGrid != null)
                {
                    columnHeader.UpdateSeparatorVisibility(columnHeader.OwningGrid.ColumnsInternal.LastVisibleColumn);
                }
                else
                {
                    columnHeader.UpdateSeparatorVisibility(null);
                }
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

#if FEATURE_ICOLLECTIONVIEW_SORT
        internal ListSortDirection? CurrentSortingState
        {
            get;
            private set;
        }
#endif

        internal DataGrid OwningGrid
        {
            get
            {
                if (OwningColumn != null && OwningColumn.OwningGrid != null)
                {
                    return OwningColumn.OwningGrid;
                }

                return null;
            }
        }

        internal DataGridColumn OwningColumn { get; set; }

        private bool HasFocus
        {
            get
            {
                return OwningGrid != null &&
                    OwningColumn == OwningGrid.FocusedColumn &&
                    OwningGrid.ColumnHeaderHasFocus;
            }
        }

        private bool IsPointerOver { get; set; }

        private bool IsPressed { get; set; }

        /// <summary>
        /// Builds the visual tree for the column header when a new template is applied.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ApplyState(false /*useTransitions*/);
        }

        /// <summary>
        /// Called when the value of the <see cref="ContentControl.Content"/> property changes.
        /// </summary>
        /// <param name="oldContent">The old value of the <see cref="ContentControl.Content"/> property.</param>
        /// <param name="newContent">The new value of the <see cref="ContentControl.Content"/> property.</param>
        /// <exception cref="T:System.NotSupportedException">
        /// <paramref name="newContent"/> is not a UIElement.
        /// </exception>
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (newContent is UIElement)
            {
                throw DataGridError.DataGridColumnHeader.ContentDoesNotSupportUIElements();
            }

            base.OnContentChanged(oldContent, newContent);
        }

        /// <summary>
        /// Creates AutomationPeer (<see cref="UIElement.OnCreateAutomationPeer"/>)
        /// </summary>
        /// <returns>An automation peer for this <see cref="DataGridColumnHeader"/>.</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            if (OwningGrid != null && OwningColumn != OwningGrid.ColumnsInternal.FillerColumn)
            {
                return new DataGridColumnHeaderAutomationPeer(this);
            }

            return base.OnCreateAutomationPeer();
        }

        internal void ApplyState(bool useTransitions)
        {
            DragMode dragMode = OwningGrid == null ? DragMode.None : OwningGrid.ColumnHeaderInteractionInfo.DragMode;

            // Common States
            if (IsPressed && dragMode != DragMode.Resize)
            {
                VisualStates.GoToState(this, useTransitions, VisualStates.StatePressed, VisualStates.StatePointerOver, VisualStates.StateNormal);
            }
            else if (IsPointerOver && dragMode != DragMode.Resize)
            {
                VisualStates.GoToState(this, useTransitions, VisualStates.StatePointerOver, VisualStates.StateNormal);
            }
            else
            {
                VisualStates.GoToState(this, useTransitions, VisualStates.StateNormal);
            }

            // Focus States
            if (HasFocus)
            {
                VisualStates.GoToState(this, useTransitions, VisualStates.StateFocused, VisualStates.StateRegular);
            }
            else
            {
                VisualStates.GoToState(this, useTransitions, VisualStates.StateUnfocused);
            }

            // Sort States
            if (OwningColumn != null)
            {
                switch (OwningColumn.SortDirection)
                {
                    case null:
                        VisualStates.GoToState(this, useTransitions, VisualStates.StateUnsorted);
                        break;
                    case DataGridSortDirection.Ascending:
                        VisualStates.GoToState(this, useTransitions, VisualStates.StateSortAscending, VisualStates.StateUnsorted);
                        break;
                    case DataGridSortDirection.Descending:
                        VisualStates.GoToState(this, useTransitions, VisualStates.StateSortDescending, VisualStates.StateUnsorted);
                        break;
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
                Style != previousStyle &&
                (OwningColumn == null || Style != OwningColumn.HeaderStyle) &&
                (OwningGrid == null || Style != OwningGrid.ColumnHeaderStyle))
            {
                return;
            }

            Style style = null;
            if (OwningColumn != null)
            {
                style = OwningColumn.HeaderStyle;
            }

            if (style == null && OwningGrid != null)
            {
                style = OwningGrid.ColumnHeaderStyle;
            }

            this.SetStyleWithType(style);
        }

        internal void InvokeProcessSort()
        {
            DiagnosticsDebug.Assert(OwningGrid != null, "Expected non-null owning DataGrid.");

            if (OwningGrid.WaitForLostFocus(() => { InvokeProcessSort(); }))
            {
                return;
            }

            if (OwningGrid.CommitEdit(DataGridEditingUnit.Row, true /*exitEditingMode*/))
            {
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { ProcessSort(); }).AsTask();
            }
        }

        private void ProcessSort()
        {
            if (OwningColumn != null &&
                OwningGrid != null &&
                OwningGrid.EditingRow == null &&
                OwningColumn != OwningGrid.ColumnsInternal.FillerColumn &&
                OwningGrid.CanUserSortColumns &&
                OwningColumn.CanUserSort)
            {
                DataGridColumnEventArgs ea = new(OwningColumn);
                OwningGrid.OnColumnSorting(ea);

#if FEATURE_ICOLLECTIONVIEW_SORT
                if (!ea.Handled && OwningGrid.DataConnection.AllowSort && OwningGrid.DataConnection.SortDescriptions != null)
                {
                    // - DataConnection.AllowSort is true, and
                    // - SortDescriptionsCollection exists, and
                    // - the column's data type is comparable
                    DataGrid owningGrid = OwningGrid;
                    ListSortDirection newSortDirection;
                    SortDescription newSort;

                    bool ctrl;
                    bool shift;

                    KeyboardHelper.GetMetaKeyState(out ctrl, out shift);

                    SortDescription? sort = OwningColumn.GetSortDescription();
                    ICollectionView collectionView = owningGrid.DataConnection.CollectionView;
                    DiagnosticsDebug.Assert(collectionView != null);
                    try
                    {
                        owningGrid.OnUserSorting();
                        using (collectionView.DeferRefresh())
                        {
                            // If shift is held down, we multi-sort, therefore if it isn't, we'll clear the sorts beforehand
                            if (!shift || owningGrid.DataConnection.SortDescriptions.Count == 0)
                            {
                                if (collectionView.CanGroup && collectionView.GroupDescriptions != null)
                                {
                                    // Make sure we sort by the GroupDescriptions first
                                    for (int i = 0; i < collectionView.GroupDescriptions.Count; i++)
                                    {
                                        PropertyGroupDescription groupDescription = collectionView.GroupDescriptions[i] as PropertyGroupDescription;
                                        if (groupDescription != null && collectionView.SortDescriptions.Count <= i || collectionView.SortDescriptions[i].PropertyName != groupDescription.PropertyName)
                                        {
                                            collectionView.SortDescriptions.Insert(Math.Min(i, collectionView.SortDescriptions.Count), new SortDescription(groupDescription.PropertyName, ListSortDirection.Ascending));
                                        }
                                    }
                                    while (collectionView.SortDescriptions.Count > collectionView.GroupDescriptions.Count)
                                    {
                                        collectionView.SortDescriptions.RemoveAt(collectionView.GroupDescriptions.Count);
                                    }
                                }
                                else if (!shift)
                                {
                                    owningGrid.DataConnection.SortDescriptions.Clear();
                                }
                            }

                            if (sort.HasValue)
                            {
                                // swap direction
                                switch (sort.Value.Direction)
                                {
                                    case ListSortDirection.Ascending:
                                        newSortDirection = ListSortDirection.Descending;
                                        break;
                                    default:
                                        newSortDirection = ListSortDirection.Ascending;
                                        break;
                                }

                                newSort = new SortDescription(sort.Value.PropertyName, newSortDirection);

                                // changing direction should not affect sort order, so we replace this column's
                                // sort description instead of just adding it to the end of the collection
                                int oldIndex = owningGrid.DataConnection.SortDescriptions.IndexOf(sort.Value);
                                if (oldIndex >= 0)
                                {
                                    owningGrid.DataConnection.SortDescriptions.Remove(sort.Value);
                                    owningGrid.DataConnection.SortDescriptions.Insert(oldIndex, newSort);
                                }
                                else
                                {
                                    owningGrid.DataConnection.SortDescriptions.Add(newSort);
                                }
                            }
                            else
                            {
                                // start new sort
                                newSortDirection = ListSortDirection.Ascending;

                                string propertyName = OwningColumn.GetSortPropertyName();

                                // no-opt if we couldn't find a property to sort on
                                if (string.IsNullOrEmpty(propertyName))
                                {
                                    return;
                                }

                                newSort = new SortDescription(propertyName, newSortDirection);

                                owningGrid.DataConnection.SortDescriptions.Add(newSort);
                            }
                        }
                    }
                    finally
                    {
                        owningGrid.OnUserSorted();
                    }

                    sortProcessed = true;
                }
#endif

                // Send the Invoked event for the column header's automation peer.
                DataGridAutomationPeer.RaiseAutomationInvokeEvent(this);
            }
        }

        internal void UpdateSeparatorVisibility(DataGridColumn lastVisibleColumn)
        {
            Visibility newVisibility = _desiredSeparatorVisibility;

            // Collapse separator for the last column if there is no filler column
            if (OwningColumn != null &&
                OwningGrid != null &&
                _desiredSeparatorVisibility == Visibility.Visible &&
                OwningColumn == lastVisibleColumn &&
                !OwningGrid.ColumnsInternal.FillerColumn.IsActive)
            {
                newVisibility = Visibility.Collapsed;
            }

            // Update the public property if it has changed
            if (SeparatorVisibility != newVisibility)
            {
                this.SetValueNoCallback(SeparatorVisibilityProperty, newVisibility);
            }
        }

        /// <summary>
        /// Determines whether a column can be resized by dragging the border of its header.  If star sizing
        /// is being used, there are special conditions that can prevent a column from being resized:
        /// 1. The column is the last visible column.
        /// 2. All columns are constrained by either their maximum or minimum values.
        /// </summary>
        /// <param name="column">Column to check.</param>
        /// <returns>Whether or not the column can be resized by dragging its header.</returns>
        private static bool CanResizeColumn(DataGridColumn column)
        {
            if (column.OwningGrid != null && column.OwningGrid.ColumnsInternal != null && column.OwningGrid.UsesStarSizing &&
                (column.OwningGrid.ColumnsInternal.LastVisibleColumn == column || !DoubleUtil.AreClose(column.OwningGrid.ColumnsInternal.VisibleEdgedColumnsWidth, column.OwningGrid.CellsWidth)))
            {
                return false;
            }

            return column.ActualCanUserResize;
        }

        private bool TrySetResizeColumn(uint pointerId, DataGridColumn column)
        {
            // If Datagrid.CanUserResizeColumns == false, then the column can still override it
            if (OwningGrid != null && CanResizeColumn(column))
            {
                DataGridColumnHeaderInteractionInfo interactionInfo = OwningGrid.ColumnHeaderInteractionInfo;

                DiagnosticsDebug.Assert(interactionInfo.DragMode != DragMode.None, "Expected _dragMode other than None.");

                interactionInfo.DragColumn = column;
                interactionInfo.DragMode = DragMode.Resize;
                interactionInfo.DragPointerId = pointerId;

                return true;
            }

            return false;
        }

        private bool CanReorderColumn(DataGridColumn column)
        {
            return OwningGrid.CanUserReorderColumns &&
                column is not DataGridFillerColumn &&
                ((column.CanUserReorderInternal.HasValue && column.CanUserReorderInternal.Value) || !column.CanUserReorderInternal.HasValue);
        }

        private void DataGridColumnHeader_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            CancelPointer(e);
        }

        private void DataGridColumnHeader_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            CancelPointer(e);
        }

        private void CancelPointer(PointerRoutedEventArgs e)
        {
            // When the user stops interacting with the column headers, the drag mode needs to be reset and any open popups closed.
            if (OwningGrid != null)
            {
                IsPressed = false;
                IsPointerOver = false;

                DataGridColumnHeaderInteractionInfo interactionInfo = OwningGrid.ColumnHeaderInteractionInfo;
                bool setResizeCursor = false;

                if (OwningGrid.ColumnHeaders != null)
                {
                    Point pointerPositionHeaders = e.GetCurrentPoint(OwningGrid.ColumnHeaders).Position;
                    setResizeCursor = interactionInfo.DragMode == DragMode.Resize && pointerPositionHeaders.X > 0 && pointerPositionHeaders.X < OwningGrid.ActualWidth;
                }

                if (!setResizeCursor)
                {
                    SetOriginalCursor();
                }

                if (interactionInfo.DragPointerId == e.Pointer.PointerId)
                {
                    OwningGrid.ResetColumnHeaderInteractionInfo();
                }

                if (setResizeCursor)
                {
                    SetResizeCursor(e.Pointer, e.GetCurrentPoint(this).Position);
                }

                ApplyState(false /*useTransitions*/);
            }
        }

        private void DataGridColumnHeader_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (OwningGrid != null && !(bool)e.NewValue)
            {
                IsPressed = false;
                IsPointerOver = false;

                DataGridColumnHeaderInteractionInfo interactionInfo = OwningGrid.ColumnHeaderInteractionInfo;

                if (interactionInfo.CapturedPointer != null)
                {
                    ReleasePointerCapture(interactionInfo.CapturedPointer);
                }

                ApplyState(false /*useTransitions*/);
            }
        }

        private void DataGridColumnHeader_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (!IsEnabled || OwningGrid == null)
            {
                return;
            }

            IsPointerOver = true;

            SetResizeCursor(e.Pointer, e.GetCurrentPoint(this).Position);

            ApplyState(true /*useTransitions*/);
        }

        private void DataGridColumnHeader_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (!IsEnabled || OwningGrid == null)
            {
                return;
            }

            IsPointerOver = false;

            DataGridColumnHeaderInteractionInfo interactionInfo = OwningGrid.ColumnHeaderInteractionInfo;

            if (interactionInfo.DragMode == DragMode.None && interactionInfo.ResizePointerId == e.Pointer.PointerId)
            {
                SetOriginalCursor();
            }

            ApplyState(true /*useTransitions*/);
        }

        private void DataGridColumnHeader_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (OwningGrid == null || OwningColumn == null || e.Handled || !IsEnabled || OwningGrid.ColumnHeaderInteractionInfo.DragMode != DragMode.None)
            {
                return;
            }

            PointerPoint pointerPoint = e.GetCurrentPoint(this);
            DataGridColumnHeaderInteractionInfo interactionInfo = OwningGrid.ColumnHeaderInteractionInfo;

            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse && !pointerPoint.Properties.IsLeftButtonPressed)
            {
                return;
            }

            DiagnosticsDebug.Assert(interactionInfo.DragPointerId == 0, "Expected _dragPointerId is 0.");

            bool handled = e.Handled;

            IsPressed = true;

            if (OwningGrid.ColumnHeaders != null)
            {
                Point pointerPosition = pointerPoint.Position;

                if (CapturePointer(e.Pointer))
                {
                    interactionInfo.CapturedPointer = e.Pointer;
                }
                else
                {
                    interactionInfo.CapturedPointer = null;
                }

                DiagnosticsDebug.Assert(interactionInfo.DragMode == DragMode.None, "Expected _dragMode equals None.");
                DiagnosticsDebug.Assert(interactionInfo.DragColumn == null, "Expected _dragColumn is null.");
                interactionInfo.DragMode = DragMode.PointerPressed;
                interactionInfo.DragPointerId = e.Pointer.PointerId;
                interactionInfo.FrozenColumnsWidth = OwningGrid.ColumnsInternal.GetVisibleFrozenEdgedColumnsWidth();
                interactionInfo.PressedPointerPositionHeaders = interactionInfo.LastPointerPositionHeaders = this.Translate(OwningGrid.ColumnHeaders, pointerPosition);

                double distanceFromLeft = pointerPosition.X;
                double distanceFromRight = ActualWidth - distanceFromLeft;
                DataGridColumn currentColumn = OwningColumn;
                DataGridColumn previousColumn = null;
                if (OwningColumn is not DataGridFillerColumn)
                {
                    previousColumn = OwningGrid.ColumnsInternal.GetPreviousVisibleNonFillerColumn(currentColumn);
                }

                int resizeRegionWidth = e.Pointer.PointerDeviceType == PointerDeviceType.Touch ? DATAGRIDCOLUMNHEADER_resizeRegionWidthLoose : DATAGRIDCOLUMNHEADER_resizeRegionWidthStrict;

                if (distanceFromRight <= resizeRegionWidth)
                {
                    handled = TrySetResizeColumn(e.Pointer.PointerId, currentColumn);
                }
                else if (distanceFromLeft <= resizeRegionWidth && previousColumn != null)
                {
                    handled = TrySetResizeColumn(e.Pointer.PointerId, previousColumn);
                }

                if (interactionInfo.DragMode == DragMode.Resize && interactionInfo.DragColumn != null)
                {
                    interactionInfo.DragStart = interactionInfo.LastPointerPositionHeaders;
                    interactionInfo.OriginalWidth = interactionInfo.DragColumn.ActualWidth;
                    interactionInfo.OriginalHorizontalOffset = OwningGrid.HorizontalOffset;

                    handled = true;
                }
            }

            e.Handled = handled;

            ApplyState(true /*useTransitions*/);
        }

        private void DataGridColumnHeader_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (OwningGrid == null || OwningColumn == null || e.Handled || !IsEnabled)
            {
                return;
            }

            PointerPoint pointerPoint = e.GetCurrentPoint(this);

            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse && pointerPoint.Properties.IsLeftButtonPressed)
            {
                return;
            }

            DataGridColumnHeaderInteractionInfo interactionInfo = OwningGrid.ColumnHeaderInteractionInfo;

            if (interactionInfo.DragPointerId != 0 && interactionInfo.DragPointerId != e.Pointer.PointerId)
            {
                return;
            }

            Point pointerPosition = pointerPoint.Position;
            Point pointerPositionHeaders = e.GetCurrentPoint(OwningGrid.ColumnHeaders).Position;
            bool handled = e.Handled;

            IsPressed = false;

            if (OwningGrid.ColumnHeaders != null)
            {
                switch (interactionInfo.DragMode)
                {
                    case DragMode.PointerPressed:
                        {
                            // Completed a click or tap without dragging, so raise the DataGrid.Sorting event.
                            InvokeProcessSort();
                            break;
                        }

                    case DragMode.Reorder:
                        {
                            // Find header hovered over
                            int targetIndex = GetReorderingTargetDisplayIndex(pointerPositionHeaders);

                            if ((!OwningColumn.IsFrozen && targetIndex >= OwningGrid.FrozenColumnCount) ||
                                (OwningColumn.IsFrozen && targetIndex < OwningGrid.FrozenColumnCount))
                            {
                                OwningColumn.DisplayIndex = targetIndex;

                                DataGridColumnEventArgs ea = new(OwningColumn);
                                OwningGrid.OnColumnReordered(ea);
                            }

                            DragCompletedEventArgs dragCompletedEventArgs = new(pointerPosition.X - interactionInfo.DragStart.Value.X, pointerPosition.Y - interactionInfo.DragStart.Value.Y, false);
                            OwningGrid.OnColumnHeaderDragCompleted(dragCompletedEventArgs);
                            break;
                        }

                    case DragMode.Drag:
                        {
                            DragCompletedEventArgs dragCompletedEventArgs = new(0, 0, false);
                            OwningGrid.OnColumnHeaderDragCompleted(dragCompletedEventArgs);
                            break;
                        }
                }

                SetResizeCursor(e.Pointer, pointerPosition);

                // Variables that track drag mode states get reset in DataGridColumnHeader_LostPointerCapture
                if (interactionInfo.CapturedPointer != null)
                {
                    ReleasePointerCapture(interactionInfo.CapturedPointer);
                }

                OwningGrid.ResetColumnHeaderInteractionInfo();
                handled = true;
            }

            e.Handled = handled;

            ApplyState(true /*useTransitions*/);
        }

        private void DataGridColumnHeader_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (OwningColumn == null || OwningGrid == null || OwningGrid.ColumnHeaders == null || !IsEnabled)
            {
                return;
            }

            PointerPoint pointerPoint = e.GetCurrentPoint(this);
            Point pointerPosition = pointerPoint.Position;
            DataGridColumnHeaderInteractionInfo interactionInfo = OwningGrid.ColumnHeaderInteractionInfo;

            if (pointerPoint.IsInContact && (interactionInfo.DragPointerId == 0 || interactionInfo.DragPointerId == e.Pointer.PointerId))
            {
                Point pointerPositionHeaders = e.GetCurrentPoint(OwningGrid.ColumnHeaders).Position;
                bool handled = false;

                DiagnosticsDebug.Assert(OwningGrid.Parent is UIElement, "Expected owning DataGrid's parent to be a UIElement.");

                double distanceFromLeft = pointerPosition.X;
                double distanceFromRight = ActualWidth - distanceFromLeft;

                OnPointerMove_Resize(ref handled, pointerPositionHeaders);
                OnPointerMove_Reorder(ref handled, e.Pointer, pointerPosition, pointerPositionHeaders, distanceFromLeft, distanceFromRight);

                // If nothing was done about moving the pointer while the pointer is down, remember the dragging, but do not
                // claim the event was actually handled.
                if (interactionInfo.DragMode == DragMode.PointerPressed &&
                    interactionInfo.PressedPointerPositionHeaders.HasValue &&
                    Math.Abs(interactionInfo.PressedPointerPositionHeaders.Value.X - pointerPositionHeaders.X) + Math.Abs(interactionInfo.PressedPointerPositionHeaders.Value.Y - pointerPositionHeaders.Y) > DATAGRIDCOLUMNHEADER_dragThreshold)
                {
                    interactionInfo.DragMode = DragMode.Drag;
                    interactionInfo.DragPointerId = e.Pointer.PointerId;
                }

                if (interactionInfo.DragMode == DragMode.Drag)
                {
                    DragDeltaEventArgs dragDeltaEventArgs = new(pointerPositionHeaders.X - interactionInfo.LastPointerPositionHeaders.Value.X, pointerPositionHeaders.Y - interactionInfo.LastPointerPositionHeaders.Value.Y);
                    OwningGrid.OnColumnHeaderDragDelta(dragDeltaEventArgs);
                }

                interactionInfo.LastPointerPositionHeaders = pointerPositionHeaders;
            }

            SetResizeCursor(e.Pointer, pointerPosition);

            if (!IsPointerOver)
            {
                IsPointerOver = true;
                ApplyState(true /*useTransitions*/);
            }
        }

        /// <summary>
        /// Returns the column against whose top-left the reordering caret should be positioned
        /// </summary>
        /// <param name="pointerPositionHeaders">Pointer position within the ColumnHeadersPresenter</param>
        /// <param name="scroll">Whether or not to scroll horizontally when a column is dragged out of bounds</param>
        /// <param name="scrollAmount">If scroll is true, returns the horizontal amount that was scrolled</param>
        /// <returns>The column against whose top-left the reordering caret should be positioned.</returns>
        private DataGridColumn GetReorderingTargetColumn(Point pointerPositionHeaders, bool scroll, out double scrollAmount)
        {
            DiagnosticsDebug.Assert(OwningGrid != null, "Expected non-null OwningGrid.");

            scrollAmount = 0;
            double leftEdge = 0;

            if (OwningGrid.ColumnsInternal.RowGroupSpacerColumn.IsRepresented)
            {
                leftEdge = OwningGrid.ColumnsInternal.RowGroupSpacerColumn.ActualWidth;
            }

            DataGridColumnHeaderInteractionInfo interactionInfo = OwningGrid.ColumnHeaderInteractionInfo;
            double rightEdge = OwningGrid.CellsWidth;
            if (OwningColumn.IsFrozen)
            {
                rightEdge = Math.Min(rightEdge, interactionInfo.FrozenColumnsWidth);
            }
            else if (OwningGrid.FrozenColumnCount > 0)
            {
                leftEdge = interactionInfo.FrozenColumnsWidth;
            }

            if (pointerPositionHeaders.X < leftEdge)
            {
                if (scroll &&
                    OwningGrid.HorizontalScrollBar != null &&
                    OwningGrid.HorizontalScrollBar.Visibility == Visibility.Visible &&
                    OwningGrid.HorizontalScrollBar.Value > 0)
                {
                    double newVal = pointerPositionHeaders.X - leftEdge;
                    scrollAmount = Math.Min(newVal, OwningGrid.HorizontalScrollBar.Value);
                    OwningGrid.UpdateHorizontalOffset(scrollAmount + OwningGrid.HorizontalScrollBar.Value);
                }

                pointerPositionHeaders.X = leftEdge;
            }
            else if (pointerPositionHeaders.X >= rightEdge)
            {
                if (scroll &&
                    OwningGrid.HorizontalScrollBar != null &&
                    OwningGrid.HorizontalScrollBar.Visibility == Visibility.Visible &&
                    OwningGrid.HorizontalScrollBar.Value < OwningGrid.HorizontalScrollBar.Maximum)
                {
                    double newVal = pointerPositionHeaders.X - rightEdge;
                    scrollAmount = Math.Min(newVal, OwningGrid.HorizontalScrollBar.Maximum - OwningGrid.HorizontalScrollBar.Value);
                    OwningGrid.UpdateHorizontalOffset(scrollAmount + OwningGrid.HorizontalScrollBar.Value);
                }

                pointerPositionHeaders.X = rightEdge - 1;
            }

            foreach (DataGridColumn column in OwningGrid.ColumnsInternal.GetDisplayedColumns())
            {
                Point pointerPosition = OwningGrid.ColumnHeaders.Translate(column.HeaderCell, pointerPositionHeaders);
                double columnMiddle = column.HeaderCell.ActualWidth / 2;
                if (pointerPosition.X >= 0 && pointerPosition.X <= columnMiddle)
                {
                    return column;
                }
                else if (pointerPosition.X > columnMiddle && pointerPosition.X < column.HeaderCell.ActualWidth)
                {
                    return OwningGrid.ColumnsInternal.GetNextVisibleColumn(column);
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the display index to set the column to
        /// </summary>
        /// <param name="pointerPositionHeaders">Pointer position relative to the column headers presenter</param>
        /// <returns>The display index to set the column to.</returns>
        private int GetReorderingTargetDisplayIndex(Point pointerPositionHeaders)
        {
            DiagnosticsDebug.Assert(OwningGrid != null, "Expected non-null OwningGrid.");

            DataGridColumn targetColumn = GetReorderingTargetColumn(pointerPositionHeaders, false /*scroll*/, out _);
            if (targetColumn != null)
            {
                return targetColumn.DisplayIndex > OwningColumn.DisplayIndex ? targetColumn.DisplayIndex - 1 : targetColumn.DisplayIndex;
            }
            else
            {
                return OwningGrid.Columns.Count - 1;
            }
        }

        private void OnPointerMove_BeginReorder(uint pointerId, Point pointerPosition)
        {
            DiagnosticsDebug.Assert(OwningGrid != null, "Expected non-null OwningGrid.");

            DataGridColumnHeader dragIndicator = new()
            {
                OwningColumn = OwningColumn,
                IsEnabled = false,
                Content = Content,
                ContentTemplate = ContentTemplate
            };

            Control dropLocationIndicator = new ContentControl();
            dropLocationIndicator.SetStyleWithType(OwningGrid.DropLocationIndicatorStyle);

            if (OwningColumn.DragIndicatorStyle != null)
            {
                dragIndicator.SetStyleWithType(OwningColumn.DragIndicatorStyle);
            }
            else if (OwningGrid.DragIndicatorStyle != null)
            {
                dragIndicator.SetStyleWithType(OwningGrid.DragIndicatorStyle);
            }

            // If the user didn't style the dragIndicator's Width, default it to the column header's width.
            if (double.IsNaN(dragIndicator.Width))
            {
                dragIndicator.Width = ActualWidth;
            }

            // If the user didn't style the dropLocationIndicator's Height, default to the column header's height.
            if (double.IsNaN(dropLocationIndicator.Height))
            {
                dropLocationIndicator.Height = ActualHeight;
            }

            // pass the caret's data template to the user for modification.
            DataGridColumnReorderingEventArgs columnReorderingEventArgs = new(OwningColumn)
            {
                DropLocationIndicator = dropLocationIndicator,
                DragIndicator = dragIndicator
            };
            OwningGrid.OnColumnReordering(columnReorderingEventArgs);
            if (columnReorderingEventArgs.Cancel)
            {
                return;
            }

            DataGridColumnHeaderInteractionInfo interactionInfo = OwningGrid.ColumnHeaderInteractionInfo;

            // The app didn't cancel, so prepare for the reorder.
            interactionInfo.DragColumn = OwningColumn;
            DiagnosticsDebug.Assert(interactionInfo.DragMode != DragMode.None, "Expected _dragMode other than None.");
            interactionInfo.DragMode = DragMode.Reorder;
            interactionInfo.DragPointerId = pointerId;
            interactionInfo.DragStart = pointerPosition;

            // Display the reordering thumb.
            OwningGrid.ColumnHeaders.DragColumn = OwningColumn;
            OwningGrid.ColumnHeaders.DragIndicator = columnReorderingEventArgs.DragIndicator;
            OwningGrid.ColumnHeaders.DropLocationIndicator = columnReorderingEventArgs.DropLocationIndicator;
        }

        private void OnPointerMove_Reorder(ref bool handled, Pointer pointer, Point pointerPosition, Point pointerPositionHeaders, double distanceFromLeft, double distanceFromRight)
        {
            DiagnosticsDebug.Assert(OwningGrid != null, "Expected non-null OwningGrid.");

            if (handled)
            {
                return;
            }

            DataGridColumnHeaderInteractionInfo interactionInfo = OwningGrid.ColumnHeaderInteractionInfo;
            int resizeRegionWidth = pointer.PointerDeviceType == PointerDeviceType.Touch ? DATAGRIDCOLUMNHEADER_resizeRegionWidthLoose : DATAGRIDCOLUMNHEADER_resizeRegionWidthStrict;

            // Handle entry into reorder mode
            if (interactionInfo.DragMode == DragMode.PointerPressed &&
                interactionInfo.DragColumn == null &&
                distanceFromRight > resizeRegionWidth &&
                distanceFromLeft > resizeRegionWidth &&
                interactionInfo.PressedPointerPositionHeaders.HasValue &&
                Math.Abs(interactionInfo.PressedPointerPositionHeaders.Value.X - pointerPositionHeaders.X) + Math.Abs(interactionInfo.PressedPointerPositionHeaders.Value.Y - pointerPositionHeaders.Y) > DATAGRIDCOLUMNHEADER_dragThreshold)
            {
                DragStartedEventArgs dragStartedEventArgs =
                    new(pointerPositionHeaders.X - interactionInfo.LastPointerPositionHeaders.Value.X, pointerPositionHeaders.Y - interactionInfo.LastPointerPositionHeaders.Value.Y);
                OwningGrid.OnColumnHeaderDragStarted(dragStartedEventArgs);

                handled = CanReorderColumn(OwningColumn);

                if (handled)
                {
                    OnPointerMove_BeginReorder(pointer.PointerId, pointerPosition);
                }
            }

            // Handle reorder mode (eg, positioning of the popup)
            if (interactionInfo.DragMode == DragMode.Reorder && OwningGrid.ColumnHeaders.DragIndicator != null)
            {
                DragDeltaEventArgs dragDeltaEventArgs = new(pointerPositionHeaders.X - interactionInfo.LastPointerPositionHeaders.Value.X, pointerPositionHeaders.Y - interactionInfo.LastPointerPositionHeaders.Value.Y);
                OwningGrid.OnColumnHeaderDragDelta(dragDeltaEventArgs);

                // Find header we're hovering over
                DataGridColumn targetColumn = GetReorderingTargetColumn(pointerPositionHeaders, !OwningColumn.IsFrozen /*scroll*/, out var scrollAmount);

                OwningGrid.ColumnHeaders.DragIndicatorOffset = pointerPosition.X - interactionInfo.DragStart.Value.X + scrollAmount;
                OwningGrid.ColumnHeaders.InvalidateArrange();

                if (OwningGrid.ColumnHeaders.DropLocationIndicator != null)
                {
                    Point targetPosition = new(0, 0);
                    if (targetColumn == null || targetColumn == OwningGrid.ColumnsInternal.FillerColumn || targetColumn.IsFrozen != OwningColumn.IsFrozen)
                    {
                        targetColumn = OwningGrid.ColumnsInternal.GetLastColumn(true /*isVisible*/, OwningColumn.IsFrozen /*isFrozen*/, null /*isReadOnly*/);
                        targetPosition = targetColumn.HeaderCell.Translate(OwningGrid.ColumnHeaders, targetPosition);
                        targetPosition.X += targetColumn.ActualWidth;
                    }
                    else
                    {
                        targetPosition = targetColumn.HeaderCell.Translate(OwningGrid.ColumnHeaders, targetPosition);
                    }

                    OwningGrid.ColumnHeaders.DropLocationIndicatorOffset = targetPosition.X - scrollAmount;
                }

                handled = true;
            }
        }

        private void OnPointerMove_Resize(ref bool handled, Point pointerPositionHeaders)
        {
            DiagnosticsDebug.Assert(OwningGrid != null, "Expected non-null OwningGrid.");

            DataGridColumnHeaderInteractionInfo interactionInfo = OwningGrid.ColumnHeaderInteractionInfo;

            if (!handled && interactionInfo.DragMode == DragMode.Resize && interactionInfo.DragColumn != null && interactionInfo.DragStart.HasValue)
            {
                DiagnosticsDebug.Assert(interactionInfo.ResizePointerId != 0, "Expected interactionInfo.ResizePointerId other than 0.");

                // Resize column
                double pointerDelta = pointerPositionHeaders.X - interactionInfo.DragStart.Value.X;
                double desiredWidth = interactionInfo.OriginalWidth + pointerDelta;

                desiredWidth = Math.Max(interactionInfo.DragColumn.ActualMinWidth, Math.Min(interactionInfo.DragColumn.ActualMaxWidth, desiredWidth));
                interactionInfo.DragColumn.Resize(interactionInfo.DragColumn.Width.Value, interactionInfo.DragColumn.Width.UnitType, interactionInfo.DragColumn.Width.DesiredValue, desiredWidth, true);

                OwningGrid.UpdateHorizontalOffset(interactionInfo.OriginalHorizontalOffset);

                handled = true;
            }
        }

        private void SetOriginalCursor()
        {
            DiagnosticsDebug.Assert(OwningGrid != null, "Expected non-null OwningGrid.");

            DataGridColumnHeaderInteractionInfo interactionInfo = OwningGrid.ColumnHeaderInteractionInfo;

            if (interactionInfo.ResizePointerId != 0)
            {
                DiagnosticsDebug.Assert(interactionInfo.OriginalCursor != null, "Expected non-null interactionInfo.OriginalCursor.");

                Window.Current.CoreWindow.PointerCursor = interactionInfo.OriginalCursor;
                interactionInfo.ResizePointerId = 0;
            }
        }

        private void SetResizeCursor(Pointer pointer, Point pointerPosition)
        {
            DiagnosticsDebug.Assert(OwningGrid != null, "Expected non-null OwningGrid.");

            DataGridColumnHeaderInteractionInfo interactionInfo = OwningGrid.ColumnHeaderInteractionInfo;

            if (interactionInfo.DragMode != DragMode.None || OwningGrid == null || OwningColumn == null)
            {
                return;
            }

            // Set mouse cursor if the column can be resized.
            double distanceFromLeft = pointerPosition.X;
            double distanceFromTop = pointerPosition.Y;
            double distanceFromRight = ActualWidth - distanceFromLeft;
            DataGridColumn currentColumn = OwningColumn;
            DataGridColumn previousColumn = null;

            if (OwningColumn is not DataGridFillerColumn)
            {
                previousColumn = OwningGrid.ColumnsInternal.GetPreviousVisibleNonFillerColumn(currentColumn);
            }

            int resizeRegionWidth = pointer.PointerDeviceType == PointerDeviceType.Touch ? DATAGRIDCOLUMNHEADER_resizeRegionWidthLoose : DATAGRIDCOLUMNHEADER_resizeRegionWidthStrict;
            bool nearCurrentResizableColumnRightEdge = distanceFromRight <= resizeRegionWidth && currentColumn != null && CanResizeColumn(currentColumn) && distanceFromTop < ActualHeight;
            bool nearPreviousResizableColumnLeftEdge = distanceFromLeft <= resizeRegionWidth && previousColumn != null && CanResizeColumn(previousColumn) && distanceFromTop < ActualHeight;

            if (OwningGrid.IsEnabled && (nearCurrentResizableColumnRightEdge || nearPreviousResizableColumnLeftEdge))
            {
                if (Window.Current.CoreWindow.PointerCursor != null && Window.Current.CoreWindow.PointerCursor.Type != CoreCursorType.SizeWestEast)
                {
                    interactionInfo.OriginalCursor = Window.Current.CoreWindow.PointerCursor;
                    interactionInfo.ResizePointerId = pointer.PointerId;
                    Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeWestEast, 0);
                }
            }
            else if (interactionInfo.ResizePointerId == pointer.PointerId)
            {
                SetOriginalCursor();
            }
        }
    }
}
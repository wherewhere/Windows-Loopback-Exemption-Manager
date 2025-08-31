// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.WinUI.Controls.DataGridInternals;
using CommunityToolkit.WinUI.Controls.Primitives;
using CommunityToolkit.WinUI.Data.Utilities;
using CommunityToolkit.WinUI.Utilities;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using DiagnosticsDebug = System.Diagnostics.Debug;

namespace CommunityToolkit.WinUI.Controls
{
    /// <summary>
    /// Represents a <see cref="DataGrid"/> column.
    /// </summary>
    [StyleTypedProperty(Property = "CellStyle", StyleTargetType = typeof(DataGridCell))]
    [StyleTypedProperty(Property = "DragIndicatorStyle", StyleTargetType = typeof(ContentControl))]
    [StyleTypedProperty(Property = "HeaderStyle", StyleTargetType = typeof(DataGridColumnHeader))]
    public abstract class DataGridColumn : DependencyObject
    {
        private const bool DATAGRIDCOLUMN_defaultCanUserReorder = true;
        private const bool DATAGRIDCOLUMN_defaultCanUserResize = true;
        private const bool DATAGRIDCOLUMN_defaultCanUserSort = true;
        private const bool DATAGRIDCOLUMN_defaultIsReadOnly = false;

        private List<string> _bindingPaths;
        private Style _cellStyle;
        private Binding _clipboardContentBinding;
        private int _displayIndexWithFiller;
        private Style _dragIndicatorStyle;
        private FrameworkElement _editingElement;
        private object _header;
        private DataGridColumnHeader _headerCell;
        private Style _headerStyle;
        private List<BindingInfo> _inputBindings;
        private bool? _isReadOnly;
        private double? _maxWidth;
        private double? _minWidth;
        private bool _settingWidthInternally;
        private DataGridLength? _width; // Null by default, null means inherit the Width from the DataGrid
        private Visibility _visibility;
        private DataGridSortDirection? _sortDirection;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridColumn"/> class.
        /// </summary>
        protected internal DataGridColumn()
        {
            _visibility = Visibility.Visible;
            _displayIndexWithFiller = -1;
            IsInitialDesiredWidthDetermined = false;
            InheritsWidth = true;
        }

        /// <summary>
        /// Gets the actual visible width after Width, MinWidth, and MaxWidth setting at the Column level and DataGrid level
        /// have been taken into account.
        /// </summary>
        public double ActualWidth
        {
            get
            {
                if (OwningGrid == null || double.IsNaN(Width.DisplayValue))
                {
                    return ActualMinWidth;
                }

                return Width.DisplayValue;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user can change the column display position by dragging the column header.
        /// </summary>
        /// <returns>
        /// true if the user can drag the column header to a new position; otherwise, false. The default is the current <see cref="CommunityToolkit.WinUI.Controls.DataGrid.CanUserReorderColumns"/> property value.
        /// </returns>
        public bool CanUserReorder
        {
            get
            {
                if (CanUserReorderInternal.HasValue)
                {
                    return CanUserReorderInternal.Value;
                }
                else if (OwningGrid != null)
                {
                    return OwningGrid.CanUserReorderColumns;
                }
                else
                {
                    return DATAGRIDCOLUMN_defaultCanUserReorder;
                }
            }

            set => CanUserReorderInternal = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user can adjust the column width using the mouse.
        /// </summary>
        /// <returns>
        /// True if the user can resize the column; false if the user cannot resize the column. The default is the current <see cref="CommunityToolkit.WinUI.Controls.DataGrid.CanUserResizeColumns"/> property value.
        /// </returns>
        public bool CanUserResize
        {
            get
            {
                if (CanUserResizeInternal.HasValue)
                {
                    return CanUserResizeInternal.Value;
                }
                else if (OwningGrid != null)
                {
                    return OwningGrid.CanUserResizeColumns;
                }
                else
                {
                    return DATAGRIDCOLUMN_defaultCanUserResize;
                }
            }

            set
            {
                CanUserResizeInternal = value;
                if (OwningGrid != null)
                {
                    OwningGrid.OnColumnCanUserResizeChanged(this);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user can sort the column by clicking the column header.
        /// </summary>
        /// <returns>
        /// True if the user can sort the column; false if the user cannot sort the column. The default is the current <see cref="CommunityToolkit.WinUI.Controls.DataGrid.CanUserSortColumns"/> property value.
        /// </returns>
        public bool CanUserSort
        {
            get
            {
                if (CanUserSortInternal.HasValue)
                {
                    return CanUserSortInternal.Value;
                }
#if FEATURE_ICOLLECTIONVIEW_SORT
                else if (OwningGrid != null)
                {
                    string propertyPath = GetSortPropertyName();
                    Type propertyType = OwningGrid.DataConnection.DataType.GetNestedPropertyType(propertyPath);

                    // if the type is nullable, then we will compare the non-nullable type
                    if (TypeHelper.IsNullableType(propertyType))
                    {
                        propertyType = TypeHelper.GetNonNullableType(propertyType);
                    }

                    // return whether or not the property type can be compared
                    return typeof(IComparable).IsAssignableFrom(propertyType) ? true : false;
                }
#endif
                else
                {
                    return DATAGRIDCOLUMN_defaultCanUserSort;
                }
            }

            set => CanUserSortInternal = value;
        }

        /// <summary>
        /// Gets or sets the style that is used when rendering cells in the column.
        /// </summary>
        /// <returns>
        /// The style that is used when rendering cells in the column. The default is null.
        /// </returns>
        public Style CellStyle
        {
            get => _cellStyle;

            set
            {
                if (_cellStyle != value)
                {
                    Style previousStyle = _cellStyle;
                    _cellStyle = value;
                    OwningGrid?.OnColumnCellStyleChanged(this, previousStyle);
                }
            }
        }

        /// <summary>
        /// Gets or sets the binding that will be used to get or set cell content for the clipboard.
        /// </summary>
        public virtual Binding ClipboardContentBinding
        {
            get => _clipboardContentBinding;
            set => _clipboardContentBinding = value;
        }

        /// <summary>
        /// Gets or sets the display position of the column relative to the other columns in the <see cref="DataGrid"/>.
        /// </summary>
        /// <returns>
        /// The zero-based position of the column as it is displayed in the associated <see cref="DataGrid"/>. The default is the index of the corresponding <see cref="P:System.Collections.ObjectModel.Collection`1.Item(System.Int32)"/> in the <see cref="CommunityToolkit.WinUI.Controls.DataGrid.Columns"/> collection.
        /// </returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// When setting this property, the specified value is less than -1 or equal to <see cref="F:System.Int32.MaxValue"/>.
        ///
        /// -or-
        ///
        /// When setting this property on a column in a <see cref="DataGrid"/>, the specified value is less than zero or greater than or equal to the number of columns in the <see cref="DataGrid"/>.
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// When setting this property, the <see cref="DataGrid"/> is already making <see cref="CommunityToolkit.WinUI.Controls.DataGridColumn.DisplayIndex"/> adjustments. For example, this exception is thrown when you attempt to set <see cref="CommunityToolkit.WinUI.Controls.DataGridColumn.DisplayIndex"/> in a <see cref="E:System.Windows.Controls.DataGrid.ColumnDisplayIndexChanged"/> event handler.
        ///
        /// -or-
        ///
        /// When setting this property, the specified value would result in a frozen column being displayed in the range of unfrozen columns, or an unfrozen column being displayed in the range of frozen columns.
        /// </exception>
        public int DisplayIndex
        {
            get => OwningGrid != null && OwningGrid.ColumnsInternal.RowGroupSpacerColumn.IsRepresented
                    ? _displayIndexWithFiller - 1
                    : _displayIndexWithFiller;

            set
            {
                if (value == int.MaxValue)
                {
                    throw DataGridError.DataGrid.ValueMustBeLessThan(nameof(value), "DisplayIndex", int.MaxValue);
                }

                if (OwningGrid != null)
                {
                    if (OwningGrid.ColumnsInternal.RowGroupSpacerColumn.IsRepresented)
                    {
                        value++;
                    }

                    if (_displayIndexWithFiller != value)
                    {
                        if (value < 0 || value >= OwningGrid.ColumnsItemsInternal.Count)
                        {
                            throw DataGridError.DataGrid.ValueMustBeBetween(nameof(value), "DisplayIndex", 0, true, OwningGrid.Columns.Count, false);
                        }

                        // Will throw an error if a visible frozen column is placed inside a non-frozen area or vice-versa.
                        OwningGrid.OnColumnDisplayIndexChanging(this, value);
                        _displayIndexWithFiller = value;
                        try
                        {
                            OwningGrid.InDisplayIndexAdjustments = true;
                            OwningGrid.OnColumnDisplayIndexChanged(this);
                            OwningGrid.OnColumnDisplayIndexChanged_PostNotification();
                        }
                        finally
                        {
                            OwningGrid.InDisplayIndexAdjustments = false;
                        }
                    }
                }
                else
                {
                    if (value < -1)
                    {
                        throw DataGridError.DataGrid.ValueMustBeGreaterThanOrEqualTo(nameof(value), "DisplayIndex", -1);
                    }

                    _displayIndexWithFiller = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the style for the drag indicator.
        /// </summary>
        public Style DragIndicatorStyle
        {
            get => _dragIndicatorStyle;

            set
            {
                if (_dragIndicatorStyle != value)
                {
                    _dragIndicatorStyle = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the style for the header.
        /// </summary>
        public Style HeaderStyle
        {
            get => _headerStyle;

            set
            {
                if (_headerStyle != value)
                {
                    Style previousStyle = _headerStyle;
                    _headerStyle = value;
                    _headerCell?.EnsureStyle(previousStyle);
                }
            }
        }

        /// <summary>
        /// Gets or sets the header object.
        /// </summary>
        public object Header
        {
            get => _header;

            set
            {
                if (_header != value)
                {
                    _header = value;
                    if (_headerCell != null)
                    {
                        _headerCell.Content = _header;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this column is autoGenerated.
        /// </summary>
        public bool IsAutoGenerated { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether this column is frozen.
        /// </summary>
        public bool IsFrozen { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether this column is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                if (OwningGrid == null)
                {
                    return _isReadOnly ?? DATAGRIDCOLUMN_defaultIsReadOnly;
                }

                if (_isReadOnly != null)
                {
                    return _isReadOnly.Value || OwningGrid.IsReadOnly;
                }

                return OwningGrid.GetColumnReadOnlyState(this, DATAGRIDCOLUMN_defaultIsReadOnly);
            }

            set
            {
                if (value != _isReadOnly)
                {
                    OwningGrid?.OnColumnReadOnlyStateChanging(this, value);

                    _isReadOnly = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the column's maximum width.
        /// </summary>
        public double MaxWidth
        {
            get
            {
                if (_maxWidth.HasValue)
                {
                    return _maxWidth.Value;
                }

                return double.PositiveInfinity;
            }

            set
            {
                if (value < 0)
                {
                    throw DataGridError.DataGrid.ValueMustBeGreaterThanOrEqualTo("value", "MaxWidth", 0);
                }

                if (value < ActualMinWidth)
                {
                    throw DataGridError.DataGrid.ValueMustBeGreaterThanOrEqualTo("value", "MaxWidth", "MinWidth");
                }

                if (!_maxWidth.HasValue || _maxWidth.Value != value)
                {
                    double oldValue = ActualMaxWidth;
                    _maxWidth = value;
                    if (OwningGrid != null && OwningGrid.ColumnsInternal != null)
                    {
                        OwningGrid.OnColumnMaxWidthChanged(this, oldValue);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the column's minimum width.
        /// </summary>
        public double MinWidth
        {
            get
            {
                if (_minWidth.HasValue)
                {
                    return _minWidth.Value;
                }

                return 0;
            }

            set
            {
                if (double.IsNaN(value))
                {
                    throw DataGridError.DataGrid.ValueCannotBeSetToNAN("MinWidth");
                }

                if (value < 0)
                {
                    throw DataGridError.DataGrid.ValueMustBeGreaterThanOrEqualTo("value", "MinWidth", 0);
                }

                if (double.IsPositiveInfinity(value))
                {
                    throw DataGridError.DataGrid.ValueCannotBeSetToInfinity("MinWidth");
                }

                if (value > ActualMaxWidth)
                {
                    throw DataGridError.DataGrid.ValueMustBeLessThanOrEqualTo("value", "MinWidth", "MaxWidth");
                }

                if (!_minWidth.HasValue || _minWidth.Value != value)
                {
                    double oldValue = ActualMinWidth;
                    _minWidth = value;
                    if (OwningGrid != null && OwningGrid.ColumnsInternal != null)
                    {
                        OwningGrid.OnColumnMinWidthChanged(this, oldValue);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the column's sort direction. Null indicates no sorting.
        /// </summary>
        public DataGridSortDirection? SortDirection
        {
            get => _sortDirection;

            set
            {
                if (value != _sortDirection)
                {
                    _sortDirection = value;

                    if (HasHeaderCell)
                    {
                        HeaderCell.ApplyState(true /*useTransitions*/);
                    }
                }
            }
        }

#if FEATURE_ICOLLECTIONVIEW_SORT
        /// <summary>
        /// Gets or sets the name of the member to use for sorting, if not using the default.
        /// </summary>
        public string SortMemberPath { get; set; }
#endif

        /// <summary>
        /// Gets or sets an object associated with this column.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Gets or sets the column's visibility.
        /// </summary>
        public Visibility Visibility
        {
            get => _visibility;

            set
            {
                if (value != Visibility)
                {
                    OwningGrid?.OnColumnVisibleStateChanging(this);

                    _visibility = value;

                    if (_headerCell != null)
                    {
                        _headerCell.Visibility = _visibility;
                    }

                    OwningGrid?.OnColumnVisibleStateChanged(this);
                }
            }
        }

        /// <summary>
        /// Gets or sets the column's width.
        /// </summary>
        public DataGridLength Width
        {
            get
            {
                if (_width.HasValue)
                {
                    return _width.Value;
                }
                else if (OwningGrid != null)
                {
                    return OwningGrid.ColumnWidth;
                }
                else
                {
                    return DataGridLength.Auto;
                }
            }

            set
            {
                if (!_width.HasValue || _width.Value != value)
                {
                    if (!_settingWidthInternally)
                    {
                        InheritsWidth = false;
                    }

                    if (OwningGrid != null)
                    {
                        bool isDesignMode = Windows.ApplicationModel.DesignMode.DesignModeEnabled;
                        DataGridLength width = CoerceWidth(value);
                        if (width.IsStar != Width.IsStar || isDesignMode)
                        {
                            // If a column has changed either from or to a star value, we want to recalculate all
                            // star column widths.  They are recalculated during Measure based off what the value we set here.
                            SetWidthInternalNoCallback(width);
                            IsInitialDesiredWidthDetermined = false;
                            OwningGrid.OnColumnWidthChanged(this);
                        }
                        else
                        {
                            // If a column width's value is simply changing, we resize it (to the right only).
                            Resize(width.Value, width.UnitType, width.DesiredValue, width.DisplayValue, false);
                        }
                    }
                    else
                    {
                        SetWidthInternalNoCallback(value);
                    }
                }
            }
        }

        internal bool ActualCanUserResize
        {
            get
            {
                if (OwningGrid == null || OwningGrid.CanUserResizeColumns == false || this is DataGridFillerColumn)
                {
                    return false;
                }

                return CanUserResizeInternal ?? true;
            }
        }

        // MaxWidth from local setting or DataGrid setting
        internal double ActualMaxWidth => _maxWidth ?? (OwningGrid != null ? OwningGrid.MaxColumnWidth : double.PositiveInfinity);

        // MinWidth from local setting or DataGrid setting
        internal double ActualMinWidth
        {
            get
            {
                double minWidth = _minWidth ?? (OwningGrid != null ? OwningGrid.MinColumnWidth : 0);
                if (Width.IsStar)
                {
                    return Math.Max(DataGrid.DATAGRID_minimumStarColumnWidth, minWidth);
                }

                return minWidth;
            }
        }

        internal List<string> BindingPaths
        {
            get
            {
                if (_bindingPaths != null)
                {
                    return _bindingPaths;
                }

                _bindingPaths = CreateBindingPaths();
                return _bindingPaths;
            }
        }

        internal bool? CanUserReorderInternal { get; set; }

        internal bool? CanUserResizeInternal { get; set; }

        internal bool? CanUserSortInternal { get; set; }

        internal bool DisplayIndexHasChanged { get; set; }

        internal int DisplayIndexWithFiller
        {
            get => _displayIndexWithFiller;

            set
            {
                DiagnosticsDebug.Assert(value >= -1, "Expected value >= -1.");
                DiagnosticsDebug.Assert(value < int.MaxValue, "Expected value < int.MaxValue.");

                _displayIndexWithFiller = value;
            }
        }

        internal bool HasHeaderCell => _headerCell != null;

        internal DataGridColumnHeader HeaderCell
        {
            get
            {
                if (_headerCell == null)
                {
                    _headerCell = CreateHeader();
                }

                return _headerCell;
            }
        }

        internal int Index { get; set; }

        /// <summary>
        /// Gets a value indicating whether or not this column inherits its Width value from the DataGrid.
        /// </summary>
        internal bool InheritsWidth { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column has been fully measured. When a column is initially added,
        /// we won't know its initial desired value until all rows have been measured.
        /// </summary>
        internal bool IsInitialDesiredWidthDetermined { get; set; }

        internal bool IsVisible => Visibility == Visibility.Visible;

        internal double LayoutRoundedWidth { get; private set; }

        internal DataGrid OwningGrid { get; set; }

        /// <summary>
        /// Returns the column which contains the given element
        /// </summary>
        /// <param name="element">element contained in a column</param>
        /// <returns>Column that contains the element, or null if not found</returns>
        public static DataGridColumn GetColumnContainingElement(FrameworkElement element)
        {
            // Walk up the tree to find the DataGridCell or DataGridColumnHeader that contains the element
            DependencyObject parent = element;
            while (parent != null)
            {
                DataGridCell cell = parent as DataGridCell;
                if (cell != null)
                {
                    return cell.OwningColumn;
                }

                DataGridColumnHeader columnHeader = parent as DataGridColumnHeader;
                if (columnHeader != null)
                {
                    return columnHeader.OwningColumn;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }

        /// <summary>
        /// Returns the column's content for the provided row.
        /// </summary>
        /// <param name="dataGridRow">Row to get the content for.</param>
        /// <returns>The column's content for the provided row.</returns>
        public FrameworkElement GetCellContent(DataGridRow dataGridRow)
        {
            ArgumentNullException.ThrowIfNull(dataGridRow);

            if (OwningGrid == null)
            {
                throw DataGridError.DataGrid.NoOwningGrid(GetType());
            }

            if (dataGridRow.OwningGrid == OwningGrid)
            {
                DiagnosticsDebug.Assert(Index >= 0, "Expected positive Index.");
                DiagnosticsDebug.Assert(Index < OwningGrid.ColumnsItemsInternal.Count, "Expected smaller Index.");

                DataGridCell dataGridCell = dataGridRow.Cells[Index];
                if (dataGridCell != null)
                {
                    return dataGridCell.Content as FrameworkElement;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the column's content for the provided row dataItem.
        /// </summary>
        /// <param name="dataItem">Row dataItem to get the content for.</param>
        /// <returns>The column's content for the provided row dataItem.</returns>
        public FrameworkElement GetCellContent(object dataItem)
        {
            ArgumentNullException.ThrowIfNull(dataItem);

            if (OwningGrid == null)
            {
                throw DataGridError.DataGrid.NoOwningGrid(GetType());
            }

            DiagnosticsDebug.Assert(Index >= 0, "Expected positive Index.");
            DiagnosticsDebug.Assert(Index < OwningGrid.ColumnsItemsInternal.Count, "Expected smaller Index.");

            DataGridRow dataGridRow = OwningGrid.GetRowFromItem(dataItem);
            if (dataGridRow == null)
            {
                return null;
            }

            return GetCellContent(dataGridRow);
        }

        /// <summary>
        /// When overridden in a derived class, causes the column cell being edited to revert to the unedited value.
        /// </summary>
        /// <param name="editingElement">
        /// The element that the column displays for a cell in editing mode.
        /// </param>
        /// <param name="uneditedValue">
        /// The previous, unedited value in the cell being edited.
        /// </param>
        protected virtual void CancelCellEdit(FrameworkElement editingElement, object uneditedValue)
        {
        }

        /// <summary>
        /// When overridden in a derived class, gets an editing element that is bound to the column's <see cref="CommunityToolkit.WinUI.Controls.DataGridBoundColumn.Binding"/> property value.
        /// </summary>
        /// <param name="cell">
        /// The cell that will contain the generated element.
        /// </param>
        /// <param name="dataItem">
        /// The data item represented by the row that contains the intended cell.
        /// </param>
        /// <returns>
        /// A new editing element that is bound to the column's <see cref="CommunityToolkit.WinUI.Controls.DataGridBoundColumn.Binding"/> property value.
        /// </returns>
        protected abstract FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem);

        /// <summary>
        /// When overridden in a derived class, gets a read-only element that is bound to the column's
        /// <see cref="CommunityToolkit.WinUI.Controls.DataGridBoundColumn.Binding"/> property value.
        /// </summary>
        /// <param name="cell">
        /// The cell that will contain the generated element.
        /// </param>
        /// <param name="dataItem">
        /// The data item represented by the row that contains the intended cell.
        /// </param>
        /// <returns>
        /// A new, read-only element that is bound to the column's <see cref="CommunityToolkit.WinUI.Controls.DataGridBoundColumn.Binding"/> property value.
        /// </returns>
        protected abstract FrameworkElement GenerateElement(DataGridCell cell, object dataItem);

        /// <summary>
        /// Called by a specific column type when one of its properties changed,
        /// and its current cells need to be updated.
        /// </summary>
        /// <param name="propertyName">Indicates which property changed and caused this call</param>
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (OwningGrid == null)
            {
                return;
            }

            OwningGrid.RefreshColumnElements(this, propertyName);
        }

        /// <summary>
        /// When overridden in a derived class, called when a cell in the column enters editing mode.
        /// </summary>
        /// <param name="editingElement">
        /// The element that the column displays for a cell in editing mode.
        /// </param>
        /// <param name="editingEventArgs">
        /// Information about the user gesture that is causing a cell to enter editing mode.
        /// </param>
        /// <returns>
        /// The unedited value.
        /// </returns>
        protected abstract object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs);

        /// <summary>
        /// Called by the DataGrid control when a column asked for its
        /// elements to be refreshed, typically because one of its properties changed.
        /// </summary>
        /// <param name="element">Indicates the element that needs to be refreshed</param>
        /// <param name="computedRowForeground">Indicates the computed row foreground based on RowForeground and AlternatingRowForeground</param>
        /// <param name="propertyName">Indicates which property changed and caused this call</param>
        protected internal virtual void RefreshCellContent(FrameworkElement element, Brush computedRowForeground, string propertyName)
        {
        }

        /// <summary>
        /// Called when the computed foreground of a row changed.
        /// </summary>
        protected internal virtual void RefreshForeground(FrameworkElement element, Brush computedRowForeground)
        {
        }

        internal void CancelCellEditInternal(FrameworkElement editingElement, object uneditedValue)
        {
            CancelCellEdit(editingElement, uneditedValue);
        }

        /// <summary>
        /// Coerces a DataGridLength to a valid value.  If any value components are double.NaN, this method
        /// coerces them to a proper initial value.  For star columns, the desired width is calculated based
        /// on the rest of the star columns.  For pixel widths, the desired value is based on the pixel value.
        /// For auto widths, the desired value is initialized as the column's minimum width.
        /// </summary>
        /// <param name="width">The DataGridLength to coerce.</param>
        /// <returns>The resultant (coerced) DataGridLength.</returns>
        internal DataGridLength CoerceWidth(DataGridLength width)
        {
            double desiredValue = width.DesiredValue;
            if (double.IsNaN(desiredValue))
            {
                if (width.IsStar && OwningGrid != null && OwningGrid.ColumnsInternal != null)
                {
                    double totalStarValues = 0;
                    double totalStarDesiredValues = 0;
                    double totalNonStarDisplayWidths = 0;
                    foreach (DataGridColumn column in OwningGrid.ColumnsInternal.GetDisplayedColumns(c => c.IsVisible && c != this && !double.IsNaN(c.Width.DesiredValue)))
                    {
                        if (column.Width.IsStar)
                        {
                            totalStarValues += column.Width.Value;
                            totalStarDesiredValues += column.Width.DesiredValue;
                        }
                        else
                        {
                            totalNonStarDisplayWidths += column.ActualWidth;
                        }
                    }

                    if (totalStarValues == 0)
                    {
                        // Compute the new star column's desired value based on the available space if there are no other visible star columns
                        desiredValue = Math.Max(ActualMinWidth, OwningGrid.CellsWidth - totalNonStarDisplayWidths);
                    }
                    else
                    {
                        // Otherwise, compute its desired value based on those of other visible star columns
                        desiredValue = totalStarDesiredValues * width.Value / totalStarValues;
                    }
                }
                else if (width.IsAbsolute)
                {
                    desiredValue = width.Value;
                }
                else
                {
                    desiredValue = ActualMinWidth;
                }
            }

            double displayValue = width.DisplayValue;
            if (double.IsNaN(displayValue))
            {
                displayValue = desiredValue;
            }

            displayValue = Math.Max(ActualMinWidth, Math.Min(ActualMaxWidth, displayValue));

            return new DataGridLength(width.Value, width.UnitType, desiredValue, displayValue);
        }

        /// <summary>
        /// If the DataGrid is using layout rounding, the pixel snapping will force all widths to
        /// whole numbers. Since the column widths aren't visual elements, they don't go through the normal
        /// rounding process, so we need to do it ourselves.  If we don't, then we'll end up with some
        /// pixel gaps and/or overlaps between columns.
        /// </summary>
        internal void ComputeLayoutRoundedWidth(double leftEdge)
        {
            if (OwningGrid != null && OwningGrid.UseLayoutRounding)
            {
                double scale;
                if (TypeHelper.IsXamlRootAvailable && OwningGrid.XamlRoot != null)
                {
                    scale = OwningGrid.XamlRoot.RasterizationScale;
                }
                else
                {
                    scale = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
                }

                double roundedLeftEdge = Math.Floor((scale * leftEdge) + 0.5) / scale;
                double roundedRightEdge = Math.Floor((scale * (leftEdge + ActualWidth)) + 0.5) / scale;
                LayoutRoundedWidth = roundedRightEdge - roundedLeftEdge;
            }
            else
            {
                LayoutRoundedWidth = ActualWidth;
            }
        }

        internal virtual List<string> CreateBindingPaths()
        {
            List<string> bindingPaths = new List<string>();
            List<BindingInfo> bindings = null;
            if (_inputBindings == null && OwningGrid != null)
            {
                DataGridRow row = OwningGrid.EditingRow;
                if (row != null && row.Cells != null && row.Cells.Count > Index)
                {
                    // Finds the input bindings if they don't already exist
                    GenerateEditingElementInternal(row.Cells[Index], row.DataContext);
                }
            }

            if (_inputBindings != null)
            {
                DiagnosticsDebug.Assert(OwningGrid != null, "Expected non-null owning DataGrid.");

                // Use the editing bindings if they've already been created
                bindings = _inputBindings;
            }

            if (bindings != null)
            {
                // We're going to return the path of every active binding
                foreach (BindingInfo binding in bindings)
                {
                    if (binding != null &&
                        binding.BindingExpression != null &&
                        binding.BindingExpression.ParentBinding != null &&
                        binding.BindingExpression.ParentBinding.Path != null)
                    {
                        bindingPaths.Add(binding.BindingExpression.ParentBinding.Path.Path);
                    }
                }
            }

            return bindingPaths;
        }

        internal virtual List<BindingInfo> CreateBindings(FrameworkElement element, object dataItem, bool twoWay)
        {
            return element.GetBindingInfo(dataItem, twoWay, false /*useBlockList*/, true /*searchChildren*/, typeof(DataGrid));
        }

        internal virtual DataGridColumnHeader CreateHeader()
        {
            DataGridColumnHeader result = new();
            result.OwningColumn = this;
            result.Content = _header;
            result.EnsureStyle(null);

            return result;
        }

        /// <summary>
        /// Ensures that this column's width has been coerced to a valid value.
        /// </summary>
        internal void EnsureWidth()
        {
            SetWidthInternalNoCallback(CoerceWidth(Width));
        }

        internal FrameworkElement GenerateEditingElementInternal(DataGridCell cell, object dataItem)
        {
            if (_editingElement == null)
            {
                _editingElement = GenerateEditingElement(cell, dataItem);
            }

            if (_inputBindings == null && _editingElement != null)
            {
                _inputBindings = CreateBindings(_editingElement, dataItem, true /*twoWay*/);

                // Setup all of the active input bindings to support validation
                foreach (BindingInfo bindingData in _inputBindings)
                {
                    if (bindingData.BindingExpression != null &&
                        bindingData.BindingExpression.ParentBinding != null &&
                        bindingData.BindingExpression.ParentBinding.UpdateSourceTrigger != UpdateSourceTrigger.Explicit)
                    {
                        Binding binding = new Binding();
                        binding.Converter = bindingData.BindingExpression.ParentBinding.Converter;
                        binding.ConverterLanguage = bindingData.BindingExpression.ParentBinding.ConverterLanguage;
                        binding.ConverterParameter = bindingData.BindingExpression.ParentBinding.ConverterParameter;
                        binding.ElementName = bindingData.BindingExpression.ParentBinding.ElementName;
                        binding.FallbackValue = bindingData.BindingExpression.ParentBinding.FallbackValue;
                        binding.Mode = bindingData.BindingExpression.ParentBinding.Mode;
                        binding.Path = new PropertyPath(bindingData.BindingExpression.ParentBinding.Path.Path);
                        binding.RelativeSource = bindingData.BindingExpression.ParentBinding.RelativeSource;
                        binding.Source = bindingData.BindingExpression.ParentBinding.Source;
                        binding.TargetNullValue = bindingData.BindingExpression.ParentBinding.TargetNullValue;
                        binding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                        bindingData.Element.SetBinding(bindingData.BindingTarget, binding);
                        bindingData.BindingExpression = bindingData.Element.GetBindingExpression(bindingData.BindingTarget);
                    }
                }
            }

            return _editingElement;
        }

        internal FrameworkElement GenerateElementInternal(DataGridCell cell, object dataItem)
        {
            return GenerateElement(cell, dataItem);
        }

        /// <summary>
        /// Gets the value of a cell according to the specified binding.
        /// </summary>
        /// <param name="item">The item associated with a cell.</param>
        /// <param name="binding">The binding to get the value of.</param>
        /// <returns>The resultant cell value.</returns>
        internal object GetCellValue(object item, Binding binding)
        {
            DiagnosticsDebug.Assert(OwningGrid != null, "Expected non-null owning DataGrid.");

            object content = null;
            if (binding != null)
            {
                OwningGrid.ClipboardContentControl.DataContext = item;
                OwningGrid.ClipboardContentControl.SetBinding(ContentControl.ContentProperty, binding);
                content = OwningGrid.ClipboardContentControl.GetValue(ContentControl.ContentProperty);
            }

            return content;
        }

        internal List<BindingInfo> GetInputBindings(FrameworkElement element, object dataItem)
        {
            if (_inputBindings != null)
            {
                return _inputBindings;
            }

            return CreateBindings(element, dataItem, true /*twoWay*/);
        }

#if FEATURE_ICOLLECTIONVIEW_SORT
        /// <summary>
        /// Gets the sort description from the data source.  We don't worry whether we can modify sort -- perhaps the sort description
        /// describes an unchangeable sort that exists on the data.
        /// </summary>
        internal SortDescription? GetSortDescription()
        {
            if (OwningGrid != null
                && OwningGrid.DataConnection != null
                && OwningGrid.DataConnection.SortDescriptions != null)
            {
                string propertyName = GetSortPropertyName();

                SortDescription sort = (new List<SortDescription>(OwningGrid.DataConnection.SortDescriptions))
                    .FirstOrDefault(s => s.PropertyName == propertyName);

                if (sort.PropertyName != null)
                {
                    return sort;
                }

                return null;
            }

            return null;
        }
#endif

#if FEATURE_ICOLLECTIONVIEW_SORT
        internal virtual string GetSortPropertyName()
        {
            return SortMemberPath;
        }
#endif

        internal object PrepareCellForEditInternal(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            return PrepareCellForEdit(editingElement, editingEventArgs);
        }

        /// <summary>
        /// Clears the cached editing element.
        /// </summary>
        internal void RemoveEditingElement()
        {
            _editingElement = null;
            _inputBindings = null;
            _bindingPaths = null;
        }

        /// <summary>
        /// Attempts to resize the column's width to the desired DisplayValue, but limits the final size
        /// to the column's minimum and maximum values.  If star sizing is being used, then the column
        /// can only decrease in size by the amount that the columns after it can increase in size.
        /// Likewise, the column can only increase in size if other columns can spare the width.
        /// </summary>
        /// <param name="value">The new Value.</param>
        /// <param name="unitType">The new UnitType.</param>
        /// <param name="desiredValue">The new DesiredValue.</param>
        /// <param name="displayValue">The new DisplayValue.</param>
        /// <param name="userInitiated">Whether or not this resize was initiated by a user action.</param>
        internal void Resize(double value, DataGridLengthUnitType unitType, double desiredValue, double displayValue, bool userInitiated)
        {
            DiagnosticsDebug.Assert(OwningGrid != null, "Expected non-null owning DataGrid.");

            double newValue = value;
            double newDesiredValue = desiredValue;
            double newDisplayValue = Math.Max(ActualMinWidth, Math.Min(ActualMaxWidth, displayValue));
            DataGridLengthUnitType newUnitType = unitType;

            int starColumnsCount = 0;
            double totalDisplayWidth = 0;
            foreach (DataGridColumn column in OwningGrid.ColumnsInternal.GetVisibleColumns())
            {
                column.EnsureWidth();
                totalDisplayWidth += column.ActualWidth;
                starColumnsCount += (column != this && column.Width.IsStar) ? 1 : 0;
            }

            bool hasInfiniteAvailableWidth = !OwningGrid.RowsPresenterAvailableSize.HasValue || double.IsPositiveInfinity(OwningGrid.RowsPresenterAvailableSize.Value.Width);

            // If we're using star sizing, we can only resize the column as much as the columns to the
            // right will allow (i.e. until they hit their max or min widths).
            if (!hasInfiniteAvailableWidth && (starColumnsCount > 0 || (unitType == DataGridLengthUnitType.Star && Width.IsStar && userInitiated)))
            {
                double limitedDisplayValue = Width.DisplayValue;
                double availableIncrease = Math.Max(0, OwningGrid.CellsWidth - totalDisplayWidth);
                double desiredChange = newDisplayValue - Width.DisplayValue;
                if (desiredChange > availableIncrease)
                {
                    // The desired change is greater than the amount of available space,
                    // so we need to decrease the widths of columns to the right to make room.
                    desiredChange -= availableIncrease;
                    double actualChange = desiredChange + OwningGrid.DecreaseColumnWidths(DisplayIndex + 1, -desiredChange, userInitiated);
                    limitedDisplayValue += availableIncrease + actualChange;
                }
                else if (desiredChange > 0)
                {
                    // The desired change is positive but less than the amount of available space,
                    // so there's no need to decrease the widths of columns to the right.
                    limitedDisplayValue += desiredChange;
                }
                else
                {
                    // The desired change is negative, so we need to increase the widths of columns to the right.
                    limitedDisplayValue += desiredChange + OwningGrid.IncreaseColumnWidths(DisplayIndex + 1, -desiredChange, userInitiated);
                }

                if (ActualCanUserResize || (Width.IsStar && !userInitiated))
                {
                    newDisplayValue = limitedDisplayValue;
                }
            }

            if (userInitiated)
            {
                newDesiredValue = newDisplayValue;
                if (!Width.IsStar)
                {
                    InheritsWidth = false;
                    newValue = newDisplayValue;
                    newUnitType = DataGridLengthUnitType.Pixel;
                }
                else if (starColumnsCount > 0 && !hasInfiniteAvailableWidth)
                {
                    // Recalculate star weight of this column based on the new desired value
                    InheritsWidth = false;
                    newValue = (Width.Value * newDisplayValue) / ActualWidth;
                }
            }

            DataGridLength oldWidth = Width;
            SetWidthInternalNoCallback(new DataGridLength(Math.Min(double.MaxValue, newValue), newUnitType, newDesiredValue, newDisplayValue));
            if (Width != oldWidth)
            {
                OwningGrid.OnColumnWidthChanged(this);
            }
        }

        /// <summary>
        /// Sets the column's Width to a new DataGridLength with a different DesiredValue.
        /// </summary>
        /// <param name="desiredValue">The new DesiredValue.</param>
        internal void SetWidthDesiredValue(double desiredValue)
        {
            SetWidthInternalNoCallback(new DataGridLength(Width.Value, Width.UnitType, desiredValue, Width.DisplayValue));
        }

        /// <summary>
        /// Sets the column's Width to a new DataGridLength with a different DisplayValue.
        /// </summary>
        /// <param name="displayValue">The new DisplayValue.</param>
        internal void SetWidthDisplayValue(double displayValue)
        {
            SetWidthInternalNoCallback(new DataGridLength(Width.Value, Width.UnitType, Width.DesiredValue, displayValue));
        }

        /// <summary>
        /// Set the column's Width without breaking inheritance.
        /// </summary>
        /// <param name="width">The new Width.</param>
        internal void SetWidthInternal(DataGridLength width)
        {
            bool originalValue = _settingWidthInternally;
            _settingWidthInternally = true;
            try
            {
                Width = width;
            }
            finally
            {
                _settingWidthInternally = originalValue;
            }
        }

        /// <summary>
        /// Sets the column's Width directly, without any callback effects.
        /// </summary>
        /// <param name="width">The new Width.</param>
        internal void SetWidthInternalNoCallback(DataGridLength width)
        {
            _width = width;
        }

        /// <summary>
        /// Set the column's star value.  Whenever the star value changes, width inheritance is broken.
        /// </summary>
        /// <param name="value">The new star value.</param>
        internal void SetWidthStarValue(double value)
        {
            DiagnosticsDebug.Assert(Width.IsStar, "Expected Width.IsStar.");

            InheritsWidth = false;
            SetWidthInternalNoCallback(new DataGridLength(value, Width.UnitType, Width.DesiredValue, Width.DisplayValue));
        }
    }
}
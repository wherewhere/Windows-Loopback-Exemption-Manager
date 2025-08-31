// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.WinUI.Controls.DataGridInternals;
using System;
using System.Collections.Specialized;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace CommunityToolkit.WinUI.Controls
{
    /// <summary>
    /// Represents a <see cref="DataGrid"/> column that hosts
    /// <see cref="T:System.Windows.Controls.CheckBox"/> controls in its cells.
    /// </summary>
    [StyleTypedProperty(Property = "ElementStyle", StyleTargetType = typeof(CheckBox))]
    [StyleTypedProperty(Property = "EditingElementStyle", StyleTargetType = typeof(CheckBox))]
    public class DataGridCheckBoxColumn : DataGridBoundColumn
    {
        private const string DATAGRIDCHECKBOXCOLUMN_isThreeStateName = "IsThreeState";
        private const double DATAGRIDCHECKBOXCOLUMN_leftMargin = 12.0;

        private bool _beganEditWithKeyboard;
        private bool _isThreeState;
        private CheckBox _currentCheckBox;
        private DataGrid _owningGrid;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridCheckBoxColumn"/> class.
        /// </summary>
        public DataGridCheckBoxColumn()
        {
            BindingTarget = Windows.UI.Xaml.Controls.Primitives.ToggleButton.IsCheckedProperty;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the hosted <see cref="T:System.Windows.Controls.CheckBox"/> controls allow three states or two.
        /// </summary>
        /// <returns>
        /// true if the hosted controls support three states; false if they support two states. The default is false.
        /// </returns>
        public bool IsThreeState
        {
            get => _isThreeState;

            set
            {
                if (_isThreeState != value)
                {
                    _isThreeState = value;
                    NotifyPropertyChanged(DATAGRIDCHECKBOXCOLUMN_isThreeStateName);
                }
            }
        }

        /// <summary>
        /// Causes the column cell being edited to revert to the specified value.
        /// </summary>
        /// <param name="editingElement">
        /// The element that the column displays for a cell in editing mode.
        /// </param>
        /// <param name="uneditedValue">
        /// The previous, unedited value in the cell being edited.
        /// </param>
        protected override void CancelCellEdit(FrameworkElement editingElement, object uneditedValue)
        {
            CheckBox editingCheckBox = editingElement as CheckBox;
            if (editingCheckBox != null)
            {
                editingCheckBox.IsChecked = (bool?)uneditedValue;
            }
        }

        /// <summary>
        /// Gets a <see cref="T:System.Windows.Controls.CheckBox"/> control that is bound to the column's <see cref="P:CommunityToolkit.WinUI.Controls.DataGridBoundColumn.Binding"/> property value.
        /// </summary>
        /// <param name="cell">
        /// The cell that will contain the generated element.
        /// </param>
        /// <param name="dataItem">
        /// The data item represented by the row that contains the intended cell.
        /// </param>
        /// <returns>
        /// A new <see cref="T:System.Windows.Controls.CheckBox"/> control that is bound to the column's <see cref="P:CommunityToolkit.WinUI.Controls.DataGridBoundColumn.Binding"/> property value.
        /// </returns>
        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            CheckBox checkBox = new();
            ConfigureCheckBox(checkBox, (cell != null & cell.OwningRow != null) ? cell.OwningRow.ComputedForeground : null);
            return checkBox;
        }

        /// <summary>
        /// Gets a read-only <see cref="T:System.Windows.Controls.CheckBox"/> control that is bound to the column's <see cref="P:CommunityToolkit.WinUI.Controls.DataGridBoundColumn.Binding"/> property value.
        /// </summary>
        /// <param name="cell">
        /// The cell that will contain the generated element.
        /// </param>
        /// <param name="dataItem">
        /// The data item represented by the row that contains the intended cell.
        /// </param>
        /// <returns>
        /// A new, read-only <see cref="T:System.Windows.Controls.CheckBox"/> control that is bound to the column's <see cref="P:CommunityToolkit.WinUI.Controls.DataGridBoundColumn.Binding"/> property value.
        /// </returns>
        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            bool isEnabled = false;
            CheckBox checkBoxElement = new();
            if (EnsureOwningGrid())
            {
                if (cell.RowIndex != -1 && cell.ColumnIndex != -1 &&
                    cell.OwningRow != null &&
                    cell.OwningRow.Slot == OwningGrid.CurrentSlot &&
                    cell.ColumnIndex == OwningGrid.CurrentColumnIndex)
                {
                    isEnabled = true;
                    if (_currentCheckBox != null)
                    {
                        _currentCheckBox.IsEnabled = false;
                    }

                    _currentCheckBox = checkBoxElement;
                }
            }

            checkBoxElement.IsEnabled = isEnabled;
            checkBoxElement.IsHitTestVisible = false;
            checkBoxElement.IsTabStop = false;
            ConfigureCheckBox(checkBoxElement, (cell != null & cell.OwningRow != null) ? cell.OwningRow.ComputedForeground : null);
            return checkBoxElement;
        }

        /// <summary>
        /// Called when a cell in the column enters editing mode.
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
        protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            CheckBox editingCheckBox = editingElement as CheckBox;
            if (editingCheckBox != null)
            {
                bool? uneditedValue = editingCheckBox.IsChecked;

                PointerRoutedEventArgs pointerEventArgs = editingEventArgs as PointerRoutedEventArgs;
                bool editValue = false;
                if (pointerEventArgs != null)
                {
                    // Editing was triggered by a mouse click
                    Point position = pointerEventArgs.GetCurrentPoint(editingCheckBox).Position;
                    Rect rect = new(0, 0, editingCheckBox.ActualWidth, editingCheckBox.ActualHeight);
                    editValue = rect.Contains(position);
                }
                else if (_beganEditWithKeyboard)
                {
                    // Editing began by a user pressing spacebar
                    editValue = true;
                    _beganEditWithKeyboard = false;
                }

                if (editValue)
                {
                    // User clicked the checkbox itself or pressed space, let's toggle the IsChecked value
                    if (editingCheckBox.IsThreeState)
                    {
                        switch (editingCheckBox.IsChecked)
                        {
                            case false:
                                editingCheckBox.IsChecked = true;
                                break;
                            case true:
                                editingCheckBox.IsChecked = null;
                                break;
                            case null:
                                editingCheckBox.IsChecked = false;
                                break;
                        }
                    }
                    else
                    {
                        editingCheckBox.IsChecked = !editingCheckBox.IsChecked;
                    }
                }

                return uneditedValue;
            }

            return false;
        }

        /// <summary>
        /// Called by the DataGrid control when this column asks for its elements to be
        /// updated, because its CheckBoxContent or IsThreeState property changed.
        /// </summary>
        protected internal override void RefreshCellContent(FrameworkElement element, Brush computedRowForeground, string propertyName)
        {
            ArgumentNullException.ThrowIfNull(element);

            CheckBox checkBox = element as CheckBox ?? throw DataGridError.DataGrid.ValueIsNotAnInstanceOf("element", typeof(CheckBox));
            if (propertyName == DATAGRIDCHECKBOXCOLUMN_isThreeStateName)
            {
                checkBox.IsThreeState = IsThreeState;
            }
            else
            {
                checkBox.IsThreeState = IsThreeState;
                checkBox.Foreground = computedRowForeground;
            }
        }

        /// <summary>
        /// Called when the computed foreground of a row changed.
        /// </summary>
        protected internal override void RefreshForeground(FrameworkElement element, Brush computedRowForeground)
        {
            CheckBox checkBox = element as CheckBox;
            if (checkBox != null)
            {
                checkBox.Foreground = computedRowForeground;
            }
        }

        private void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (OwningGrid == null && _owningGrid != null)
            {
                _owningGrid.Columns.CollectionChanged -= new NotifyCollectionChangedEventHandler(Columns_CollectionChanged);
                _owningGrid.CurrentCellChanged -= new EventHandler<EventArgs>(OwningGrid_CurrentCellChanged);
                _owningGrid.KeyDown -= new KeyEventHandler(OwningGrid_KeyDown);
                _owningGrid.LoadingRow -= new EventHandler<DataGridRowEventArgs>(OwningGrid_LoadingRow);
                _owningGrid = null;
            }
        }

        private void ConfigureCheckBox(CheckBox checkBox, Brush computedRowForeground)
        {
            checkBox.Margin = new Thickness(DATAGRIDCHECKBOXCOLUMN_leftMargin, 0.0, 0.0, 0.0);
            checkBox.HorizontalAlignment = HorizontalAlignment.Left;
            checkBox.VerticalAlignment = VerticalAlignment.Center;
            checkBox.IsThreeState = IsThreeState;
            checkBox.UseSystemFocusVisuals = false;
            checkBox.Foreground = computedRowForeground;
            if (Binding != null)
            {
                checkBox.SetBinding(BindingTarget, Binding);
            }
        }

        private bool EnsureOwningGrid()
        {
            if (OwningGrid != null)
            {
                if (OwningGrid != _owningGrid)
                {
                    _owningGrid = OwningGrid;
                    _owningGrid.Columns.CollectionChanged += new NotifyCollectionChangedEventHandler(Columns_CollectionChanged);
                    _owningGrid.CurrentCellChanged += new EventHandler<EventArgs>(OwningGrid_CurrentCellChanged);
                    _owningGrid.KeyDown += new KeyEventHandler(OwningGrid_KeyDown);
                    _owningGrid.LoadingRow += new EventHandler<DataGridRowEventArgs>(OwningGrid_LoadingRow);
                }

                return true;
            }

            return false;
        }

        private void OwningGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            if (_currentCheckBox != null)
            {
                _currentCheckBox.IsEnabled = false;
            }

            if (OwningGrid != null && OwningGrid.CurrentColumn == this &&
                OwningGrid.IsSlotVisible(OwningGrid.CurrentSlot))
            {
                DataGridRow row = OwningGrid.DisplayData.GetDisplayedElement(OwningGrid.CurrentSlot) as DataGridRow;
                if (row != null)
                {
                    CheckBox checkBox = GetCellContent(row) as CheckBox;
                    if (checkBox != null)
                    {
                        checkBox.IsEnabled = true;
                    }

                    _currentCheckBox = checkBox;
                }
            }
        }

        private void OwningGrid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Space &&
                OwningGrid != null &&
                OwningGrid.CurrentColumn == this)
            {
                DataGridRow row = OwningGrid.DisplayData.GetDisplayedElement(OwningGrid.CurrentSlot) as DataGridRow;
                if (row != null)
                {
                    CheckBox checkBox = GetCellContent(row) as CheckBox;
                    if (checkBox == _currentCheckBox)
                    {
                        _beganEditWithKeyboard = true;
                        OwningGrid.BeginEdit();
                        return;
                    }
                }
            }

            _beganEditWithKeyboard = false;
        }

        private void OwningGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (OwningGrid != null)
            {
                CheckBox checkBox = GetCellContent(e.Row) as CheckBox;
                if (checkBox != null)
                {
                    if (OwningGrid.CurrentColumnIndex == Index && OwningGrid.CurrentSlot == e.Row.Slot)
                    {
                        if (_currentCheckBox != null)
                        {
                            _currentCheckBox.IsEnabled = false;
                        }

                        checkBox.IsEnabled = true;
                        _currentCheckBox = checkBox;
                    }
                    else
                    {
                        checkBox.IsEnabled = false;
                    }
                }
            }
        }
    }
}
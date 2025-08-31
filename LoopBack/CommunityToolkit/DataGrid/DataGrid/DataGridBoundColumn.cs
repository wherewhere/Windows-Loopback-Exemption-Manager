// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.WinUI.Controls.DataGridInternals;
using CommunityToolkit.WinUI.Data.Utilities;
using CommunityToolkit.WinUI.Utilities;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace CommunityToolkit.WinUI.Controls
{
    /// <summary>
    /// Represents a <see cref="DataGrid"/> column that can
    /// bind to a property in the grid's data source.
    /// </summary>
    [StyleTypedProperty(Property = "ElementStyle", StyleTargetType = typeof(FrameworkElement))]
    [StyleTypedProperty(Property = "EditingElementStyle", StyleTargetType = typeof(FrameworkElement))]
    public abstract class DataGridBoundColumn : DataGridColumn
    {
        private Binding _binding;
        private Style _elementStyle;
        private Style _editingElementStyle;

        /// <summary>
        /// Gets or sets the binding that associates the column with a property in the data source.
        /// </summary>
        public virtual Binding Binding
        {
            get => _binding;

            set
            {
                if (_binding != value)
                {
                    if (OwningGrid != null && !OwningGrid.CommitEdit(DataGridEditingUnit.Row, true /*exitEditing*/))
                    {
                        // Edited value couldn't be committed, so we force a CancelEdit
                        _ = OwningGrid.CancelEdit(DataGridEditingUnit.Row, false /*raiseEvents*/);
                    }

                    _binding = value;

                    if (_binding != null)
                    {
                        // Force the TwoWay binding mode if there is a Path present.  TwoWay binding requires a Path.
                        if (_binding.Path != null && !string.IsNullOrEmpty(_binding.Path.Path))
                        {
                            _binding.Mode = BindingMode.TwoWay;
                        }

                        _binding.Converter ??= new DataGridValueConverter();

                        _binding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;

                        // Apply the new Binding to existing rows in the DataGrid
                        // TODO: We want to clear the Bindings if Binding is set to null
                        // but there's no way to do that right now.  Revisit this if UWP
                        // implements the equivalent of BindingOperations.ClearBinding.
                        OwningGrid?.OnColumnBindingChanged(this);
                    }

                    RemoveEditingElement();
                }
            }
        }

        /// <summary>
        /// Gets or sets the binding that will be used to get or set cell content for the clipboard.
        /// If the base ClipboardContentBinding is not explicitly set, this will return the value of Binding.
        /// </summary>
        public override Binding ClipboardContentBinding
        {
            get => base.ClipboardContentBinding ?? Binding;
            set => base.ClipboardContentBinding = value;
        }

        /// <summary>
        /// Gets or sets the style that is used when rendering the element that the column displays for a cell in editing mode.
        /// </summary>
        public Style EditingElementStyle
        {
            get => _editingElementStyle;
            set
            {
                if (_editingElementStyle != value)
                {
                    _editingElementStyle = value;

                    // We choose not to update the elements already editing in the Grid here.
                    // They will get the EditingElementStyle next time they go into edit mode.
                }
            }
        }

        /// <summary>
        /// Gets or sets the style that is used when rendering the element that the column displays for a cell that is not in editing mode.
        /// </summary>
        public Style ElementStyle
        {
            get => _elementStyle;
            set
            {
                if (_elementStyle != value)
                {
                    _elementStyle = value;
                    OwningGrid?.OnColumnElementStyleChanged(this);
                }
            }
        }

        internal DependencyProperty BindingTarget { get; set; }

        internal override List<string> CreateBindingPaths()
        {
            if (Binding != null && Binding.Path != null)
            {
                return [Binding.Path.Path];
            }

            return base.CreateBindingPaths();
        }

        internal override List<BindingInfo> CreateBindings(FrameworkElement element, object dataItem, bool twoWay)
        {
            BindingInfo bindingData = new();
            if (twoWay && BindingTarget != null)
            {
                bindingData.BindingExpression = element.GetBindingExpression(BindingTarget);
                if (bindingData.BindingExpression != null)
                {
                    bindingData.BindingTarget = BindingTarget;
                    bindingData.Element = element;
                    return [bindingData];
                }
            }

            foreach (DependencyProperty bindingTarget in element.GetDependencyProperties(false))
            {
                bindingData.BindingExpression = element.GetBindingExpression(bindingTarget);
                if (bindingData.BindingExpression != null
                    && bindingData.BindingExpression.ParentBinding == Binding)
                {
                    BindingTarget = bindingTarget;
                    bindingData.BindingTarget = BindingTarget;
                    bindingData.Element = element;
                    return [bindingData];
                }
            }

            return base.CreateBindings(element, dataItem, twoWay);
        }

#if FEATURE_ICOLLECTIONVIEW_SORT
        internal override string GetSortPropertyName()
        {
            if (string.IsNullOrEmpty(SortMemberPath) && Binding != null && Binding.Path != null)
            {
                return Binding.Path.Path;
            }

            return SortMemberPath;
        }
#endif

        internal void SetHeaderFromBinding()
        {
            if (OwningGrid != null && OwningGrid.DataConnection.DataType != null &&
                Header == null && Binding != null && Binding.Path != null)
            {
                string header = OwningGrid.DataConnection.DataType.GetDisplayName(Binding.Path.Path);
                if (header != null)
                {
                    Header = header;
                }
            }
        }
    }
}
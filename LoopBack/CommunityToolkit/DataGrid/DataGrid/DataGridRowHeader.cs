// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.WinUI.Automation.Peers;
using CommunityToolkit.WinUI.Controls.Utilities;
using CommunityToolkit.WinUI.Utilities;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

using DiagnosticsDebug = System.Diagnostics.Debug;

namespace CommunityToolkit.WinUI.Controls.Primitives
{
    /// <summary>
    /// Represents an individual <see cref="DataGrid"/> row header.
    /// </summary>
    [TemplatePart(Name = DATAGRIDROWHEADER_elementRootName, Type = typeof(FrameworkElement))]

    [TemplateVisualState(Name = DATAGRIDROWHEADER_stateNormal, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = DATAGRIDROWHEADER_stateNormalCurrentRow, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = DATAGRIDROWHEADER_stateNormalEditingRow, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = DATAGRIDROWHEADER_stateNormalEditingRowFocused, GroupName = VisualStates.GroupCommon)]

    [TemplateVisualState(Name = DATAGRIDROWHEADER_statePointerOver, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = DATAGRIDROWHEADER_statePointerOverCurrentRow, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = DATAGRIDROWHEADER_statePointerOverEditingRow, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = DATAGRIDROWHEADER_statePointerOverEditingRowFocused, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = DATAGRIDROWHEADER_statePointerOverSelected, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = DATAGRIDROWHEADER_statePointerOverSelectedFocused, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = DATAGRIDROWHEADER_statePointerOverSelectedCurrentRow, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = DATAGRIDROWHEADER_statePointerOverSelectedCurrentRowFocused, GroupName = VisualStates.GroupCommon)]

    [TemplateVisualState(Name = DATAGRIDROWHEADER_stateSelected, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = DATAGRIDROWHEADER_stateSelectedCurrentRow, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = DATAGRIDROWHEADER_stateSelectedCurrentRowFocused, GroupName = VisualStates.GroupCommon)]
    [TemplateVisualState(Name = DATAGRIDROWHEADER_stateSelectedFocused, GroupName = VisualStates.GroupCommon)]

    [TemplateVisualState(Name = VisualStates.StateRowInvalid, GroupName = VisualStates.GroupValidation)]
    [TemplateVisualState(Name = VisualStates.StateRowValid, GroupName = VisualStates.GroupValidation)]
    public partial class DataGridRowHeader : ContentControl
    {
        private const string DATAGRIDROWHEADER_elementRootName = "RowHeaderRoot";
        private const double DATAGRIDROWHEADER_separatorThickness = 1;

        private const string DATAGRIDROWHEADER_statePointerOver = "PointerOver";
        private const string DATAGRIDROWHEADER_statePointerOverCurrentRow = "PointerOverCurrentRow";
        private const string DATAGRIDROWHEADER_statePointerOverEditingRow = "PointerOverUnfocusedEditingRow";
        private const string DATAGRIDROWHEADER_statePointerOverEditingRowFocused = "PointerOverEditingRow";
        private const string DATAGRIDROWHEADER_statePointerOverSelected = "PointerOverUnfocusedSelected";
        private const string DATAGRIDROWHEADER_statePointerOverSelectedCurrentRow = "PointerOverUnfocusedCurrentRowSelected";
        private const string DATAGRIDROWHEADER_statePointerOverSelectedCurrentRowFocused = "PointerOverCurrentRowSelected";
        private const string DATAGRIDROWHEADER_statePointerOverSelectedFocused = "PointerOverSelected";
        private const string DATAGRIDROWHEADER_stateNormal = "Normal";
        private const string DATAGRIDROWHEADER_stateNormalCurrentRow = "NormalCurrentRow";
        private const string DATAGRIDROWHEADER_stateNormalEditingRow = "UnfocusedEditingRow";
        private const string DATAGRIDROWHEADER_stateNormalEditingRowFocused = "NormalEditingRow";
        private const string DATAGRIDROWHEADER_stateSelected = "UnfocusedSelected";
        private const string DATAGRIDROWHEADER_stateSelectedCurrentRow = "UnfocusedCurrentRowSelected";
        private const string DATAGRIDROWHEADER_stateSelectedCurrentRowFocused = "NormalCurrentRowSelected";
        private const string DATAGRIDROWHEADER_stateSelectedFocused = "NormalSelected";

        private const byte DATAGRIDROWHEADER_statePointerOverCode = 0;
        private const byte DATAGRIDROWHEADER_statePointerOverCurrentRowCode = 1;
        private const byte DATAGRIDROWHEADER_statePointerOverEditingRowCode = 2;
        private const byte DATAGRIDROWHEADER_statePointerOverEditingRowFocusedCode = 3;
        private const byte DATAGRIDROWHEADER_statePointerOverSelectedCode = 4;
        private const byte DATAGRIDROWHEADER_statePointerOverSelectedCurrentRowCode = 5;
        private const byte DATAGRIDROWHEADER_statePointerOverSelectedCurrentRowFocusedCode = 6;
        private const byte DATAGRIDROWHEADER_statePointerOverSelectedFocusedCode = 7;
        private const byte DATAGRIDROWHEADER_stateNormalCode = 8;
        private const byte DATAGRIDROWHEADER_stateNormalCurrentRowCode = 9;
        private const byte DATAGRIDROWHEADER_stateNormalEditingRowCode = 10;
        private const byte DATAGRIDROWHEADER_stateNormalEditingRowFocusedCode = 11;
        private const byte DATAGRIDROWHEADER_stateSelectedCode = 12;
        private const byte DATAGRIDROWHEADER_stateSelectedCurrentRowCode = 13;
        private const byte DATAGRIDROWHEADER_stateSelectedCurrentRowFocusedCode = 14;
        private const byte DATAGRIDROWHEADER_stateSelectedFocusedCode = 15;
        private const byte DATAGRIDROWHEADER_stateNullCode = 255;

        private static readonly byte[] _fallbackStateMapping =
        [
            DATAGRIDROWHEADER_stateNormalCode,
            DATAGRIDROWHEADER_stateNormalCurrentRowCode,
            DATAGRIDROWHEADER_statePointerOverEditingRowFocusedCode,
            DATAGRIDROWHEADER_stateNormalEditingRowFocusedCode,
            DATAGRIDROWHEADER_statePointerOverSelectedFocusedCode,
            DATAGRIDROWHEADER_statePointerOverSelectedCurrentRowFocusedCode,
            DATAGRIDROWHEADER_stateSelectedFocusedCode,
            DATAGRIDROWHEADER_stateSelectedFocusedCode,
            DATAGRIDROWHEADER_stateNullCode,
            DATAGRIDROWHEADER_stateNormalCode,
            DATAGRIDROWHEADER_stateNormalEditingRowFocusedCode,
            DATAGRIDROWHEADER_stateSelectedCurrentRowFocusedCode,
            DATAGRIDROWHEADER_stateSelectedFocusedCode,
            DATAGRIDROWHEADER_stateSelectedCurrentRowFocusedCode,
            DATAGRIDROWHEADER_stateNormalCurrentRowCode,
            DATAGRIDROWHEADER_stateNormalCode,
        ];

        private static readonly byte[] _idealStateMapping =
        [
            DATAGRIDROWHEADER_stateNormalCode,
            DATAGRIDROWHEADER_stateNormalCode,
            DATAGRIDROWHEADER_statePointerOverCode,
            DATAGRIDROWHEADER_statePointerOverCode,
            DATAGRIDROWHEADER_stateNullCode,
            DATAGRIDROWHEADER_stateNullCode,
            DATAGRIDROWHEADER_stateNullCode,
            DATAGRIDROWHEADER_stateNullCode,
            DATAGRIDROWHEADER_stateSelectedCode,
            DATAGRIDROWHEADER_stateSelectedFocusedCode,
            DATAGRIDROWHEADER_statePointerOverSelectedCode,
            DATAGRIDROWHEADER_statePointerOverSelectedFocusedCode,
            DATAGRIDROWHEADER_stateNormalEditingRowCode,
            DATAGRIDROWHEADER_stateNormalEditingRowFocusedCode,
            DATAGRIDROWHEADER_statePointerOverEditingRowCode,
            DATAGRIDROWHEADER_statePointerOverEditingRowFocusedCode,
            DATAGRIDROWHEADER_stateNormalCurrentRowCode,
            DATAGRIDROWHEADER_stateNormalCurrentRowCode,
            DATAGRIDROWHEADER_statePointerOverCurrentRowCode,
            DATAGRIDROWHEADER_statePointerOverCurrentRowCode,
            DATAGRIDROWHEADER_stateNullCode,
            DATAGRIDROWHEADER_stateNullCode,
            DATAGRIDROWHEADER_stateNullCode,
            DATAGRIDROWHEADER_stateNullCode,
            DATAGRIDROWHEADER_stateSelectedCurrentRowCode,
            DATAGRIDROWHEADER_stateSelectedCurrentRowFocusedCode,
            DATAGRIDROWHEADER_statePointerOverSelectedCurrentRowCode,
            DATAGRIDROWHEADER_statePointerOverSelectedCurrentRowFocusedCode,
            DATAGRIDROWHEADER_stateNormalEditingRowCode,
            DATAGRIDROWHEADER_stateNormalEditingRowFocusedCode,
            DATAGRIDROWHEADER_statePointerOverEditingRowCode,
            DATAGRIDROWHEADER_statePointerOverEditingRowFocusedCode
        ];

        private static readonly string[] _stateNames =
        [
            DATAGRIDROWHEADER_statePointerOver,
            DATAGRIDROWHEADER_statePointerOverCurrentRow,
            DATAGRIDROWHEADER_statePointerOverEditingRow,
            DATAGRIDROWHEADER_statePointerOverEditingRowFocused,
            DATAGRIDROWHEADER_statePointerOverSelected,
            DATAGRIDROWHEADER_statePointerOverSelectedCurrentRow,
            DATAGRIDROWHEADER_statePointerOverSelectedCurrentRowFocused,
            DATAGRIDROWHEADER_statePointerOverSelectedFocused,
            DATAGRIDROWHEADER_stateNormal,
            DATAGRIDROWHEADER_stateNormalCurrentRow,
            DATAGRIDROWHEADER_stateNormalEditingRow,
            DATAGRIDROWHEADER_stateNormalEditingRowFocused,
            DATAGRIDROWHEADER_stateSelected,
            DATAGRIDROWHEADER_stateSelectedCurrentRow,
            DATAGRIDROWHEADER_stateSelectedCurrentRowFocused,
            DATAGRIDROWHEADER_stateSelectedFocused
        ];

        private FrameworkElement _rootElement;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridRowHeader"/> class.
        /// </summary>
        public DataGridRowHeader()
        {
            IsTapEnabled = true;

            AddHandler(TappedEvent, new TappedEventHandler(DataGridRowHeader_Tapped), true /*handledEventsToo*/);

            DefaultStyleKey = typeof(DataGridRowHeader);
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> used to paint the row header separator lines.
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
                typeof(DataGridRowHeader),
                null);

        /// <summary>
        /// Gets or sets a value indicating whether the row header separator lines are visible.
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
                typeof(DataGridRowHeader),
                new PropertyMetadata(Visibility.Visible));

        private DataGrid OwningGrid
        {
            get
            {
                if (OwningRow != null)
                {
                    return OwningRow.OwningGrid;
                }
                else if (OwningRowGroupHeader != null)
                {
                    return OwningRowGroupHeader.OwningGrid;
                }

                return null;
            }
        }

        private DataGridRow OwningRow => Owner as DataGridRow;

        private DataGridRowGroupHeader OwningRowGroupHeader => Owner as DataGridRowGroupHeader;

        internal Control Owner { get; set; }

        private int Slot
        {
            get
            {
                if (OwningRow != null)
                {
                    return OwningRow.Slot;
                }
                else if (OwningRowGroupHeader != null)
                {
                    return OwningRowGroupHeader.RowGroupInfo.Slot;
                }

                return -1;
            }
        }

        /// <summary>
        /// Builds the visual tree for the row header when a new template is applied.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _rootElement = GetTemplateChild(DATAGRIDROWHEADER_elementRootName) as FrameworkElement;
            if (_rootElement != null)
            {
                ApplyOwnerState(false /*animate*/);
            }
        }

        /// <summary>
        /// Measures the children of a <see cref="DataGridRowHeader"/> to prepare for arranging them during the <see cref="M:System.Windows.FrameworkElement.ArrangeOverride(System.Windows.Size)"/> pass.
        /// </summary>
        /// <param name="availableSize">
        /// The available size that this element can give to child elements. Indicates an upper limit that child elements should not exceed.
        /// </param>
        /// <returns>
        /// The size that the <see cref="DataGridRowHeader"/> determines it needs during layout, based on its calculations of child object allocated sizes.
        /// </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (OwningRow == null || OwningGrid == null)
            {
                return base.MeasureOverride(availableSize);
            }

            double measureHeight = double.IsNaN(OwningGrid.RowHeight) ? availableSize.Height : OwningGrid.RowHeight;
            double measureWidth = double.IsNaN(OwningGrid.RowHeaderWidth) ? availableSize.Width : OwningGrid.RowHeaderWidth;
            Size measuredSize = base.MeasureOverride(new Size(measureWidth, measureHeight));

            // Auto grow the row header or force it to a fixed width based on the DataGrid's setting
            if (!double.IsNaN(OwningGrid.RowHeaderWidth) || measuredSize.Width < OwningGrid.ActualRowHeaderWidth)
            {
                return new Size(OwningGrid.ActualRowHeaderWidth, measuredSize.Height);
            }

            return measuredSize;
        }

        /// <summary>
        /// Creates AutomationPeer (<see cref="UIElement.OnCreateAutomationPeer"/>)
        /// </summary>
        /// <returns>An automation peer for this <see cref="DataGridRowHeader"/>.</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new DataGridRowHeaderAutomationPeer(this);
        }

        internal void ApplyOwnerState(bool animate)
        {
            if (_rootElement != null && Owner != null && Owner.Visibility == Visibility.Visible)
            {
                byte idealStateMappingIndex = 0;

                if (OwningRow != null)
                {
                    if (OwningRow.IsValid)
                    {
                        VisualStates.GoToState(this, true, VisualStates.StateRowValid);
                    }
                    else
                    {
                        VisualStates.GoToState(this, true, VisualStates.StateRowInvalid, VisualStates.StateRowValid);
                    }

                    if (OwningGrid != null)
                    {
                        if (OwningGrid.CurrentSlot == OwningRow.Slot)
                        {
                            idealStateMappingIndex += 16;
                        }

                        if (OwningGrid.ContainsFocus)
                        {
                            idealStateMappingIndex += 1;
                        }
                    }

                    if (OwningRow.IsSelected || OwningRow.IsEditing)
                    {
                        idealStateMappingIndex += 8;
                    }

                    if (OwningRow.IsEditing)
                    {
                        idealStateMappingIndex += 4;
                    }

                    if (OwningRow.IsPointerOver)
                    {
                        idealStateMappingIndex += 2;
                    }
                }
                else if (OwningRowGroupHeader != null && OwningGrid != null && OwningGrid.CurrentSlot == OwningRowGroupHeader.RowGroupInfo.Slot)
                {
                    idealStateMappingIndex += 16;
                }

                byte stateCode = _idealStateMapping[idealStateMappingIndex];
                DiagnosticsDebug.Assert(stateCode != DATAGRIDROWHEADER_stateNullCode, "Expected stateCode other than DATAGRIDROWHEADER_stateNullCode.");

                string storyboardName;
                while (stateCode != DATAGRIDROWHEADER_stateNullCode)
                {
                    storyboardName = _stateNames[stateCode];
                    if (VisualStateManager.GoToState(this, storyboardName, animate))
                    {
                        break;
                    }
                    else
                    {
                        // The state wasn't implemented so fall back to the next one
                        stateCode = _fallbackStateMapping[stateCode];
                    }
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
                OwningRow != null &&
                Style != OwningRow.HeaderStyle &&
                OwningRowGroupHeader != null &&
                Style != OwningRowGroupHeader.HeaderStyle &&
                OwningGrid != null &&
                Style != OwningGrid.RowHeaderStyle &&
                Style != previousStyle)
            {
                return;
            }

            Style style = null;
            if (OwningRow != null)
            {
                style = OwningRow.HeaderStyle;
            }

            if (style == null && OwningGrid != null)
            {
                style = OwningGrid.RowHeaderStyle;
            }

            this.SetStyleWithType(style);
        }

        private void DataGridRowHeader_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (OwningGrid != null && !OwningGrid.HasColumnUserInteraction)
            {
                if (!e.Handled && OwningGrid.IsTabStop)
                {
                    bool success = OwningGrid.Focus(FocusState.Programmatic);
                    DiagnosticsDebug.Assert(success, "Expected successful focus change.");
                }

                if (OwningRow != null)
                {
                    DiagnosticsDebug.Assert(sender is DataGridRowHeader, "Expected sender is DataGridRowHeader.");
                    DiagnosticsDebug.Assert(sender as DataGridRowHeader == this, "Expected sender is ");

                    e.Handled = OwningGrid.UpdateStateOnTapped(e, -1, Slot, false /*allowEdit*/);
                    OwningGrid.UpdatedStateOnTapped = true;
                }
            }
        }
    }
}
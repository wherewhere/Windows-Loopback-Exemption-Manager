// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;

namespace CommunityToolkit.WinUI.Controls.DataGridInternals
{
    internal class DataGridCellCoordinates(int columnIndex, int slot)
    {
        public DataGridCellCoordinates(DataGridCellCoordinates dataGridCellCoordinates)
            : this(dataGridCellCoordinates.ColumnIndex, dataGridCellCoordinates.Slot)
        {
        }

        public int ColumnIndex { get; set; } = columnIndex;

        public int Slot { get; set; } = slot;

        public override bool Equals(object o)
        {
            if (o is DataGridCellCoordinates dataGridCellCoordinates)
            {
                return dataGridCellCoordinates.ColumnIndex == ColumnIndex && dataGridCellCoordinates.Slot == Slot;
            }

            return false;
        }

        // Avoiding build warning CS0659: 'DataGridCellCoordinates' overrides Object.Equals(object o) but does not override Object.GetHashCode()
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

#if DEBUG
        public override string ToString()
        {
            return "DataGridCellCoordinates {ColumnIndex = " + ColumnIndex.ToString(CultureInfo.CurrentCulture) +
                   ", Slot = " + Slot.ToString(CultureInfo.CurrentCulture) + "}";
        }
#endif
    }
}
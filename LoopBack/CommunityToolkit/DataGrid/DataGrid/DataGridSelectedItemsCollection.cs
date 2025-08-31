// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.WinUI.Controls.DataGridInternals;
using CommunityToolkit.WinUI.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using DiagnosticsDebug = System.Diagnostics.Debug;

namespace CommunityToolkit.WinUI.Controls
{
    internal partial class DataGridSelectedItemsCollection(DataGrid owningGrid) : IList
    {
        private List<object> _oldSelectedItemsCache = [];
        private IndexToValueTable<bool> _oldSelectedSlotsTable = new();
        private List<object> _selectedItemsCache = [];
        private readonly IndexToValueTable<bool> _selectedSlotsTable = new();

        public object this[int index]
        {
            get
            {
                if (index < 0 || index >= _selectedSlotsTable.IndexCount)
                {
                    throw DataGridError.DataGrid.ValueMustBeBetween(nameof(index), "Index", 0, true, _selectedSlotsTable.IndexCount, false);
                }

                int slot = _selectedSlotsTable.GetNthIndex(index);
                DiagnosticsDebug.Assert(slot >= 0, "Expected positive slot.");
                return OwningGrid.DataConnection.GetDataItem(OwningGrid.RowIndexFromSlot(slot));
            }

            set => throw new NotSupportedException();
        }

        public bool IsFixedSize => false;

        public bool IsReadOnly => false;

        public int Add(object dataItem)
        {
            if (OwningGrid.SelectionMode == DataGridSelectionMode.Single)
            {
                throw DataGridError.DataGridSelectedItemsCollection.CannotChangeSelectedItemsCollectionInSingleMode();
            }

            int itemIndex = OwningGrid.DataConnection.IndexOf(dataItem);
            if (itemIndex == -1)
            {
                throw DataGridError.DataGrid.ItemIsNotContainedInTheItemsSource(nameof(dataItem));
            }

            DiagnosticsDebug.Assert(itemIndex >= 0, "Expected positive itemIndex.");

            int slot = OwningGrid.SlotFromRowIndex(itemIndex);
            if (_selectedSlotsTable.RangeCount == 0)
            {
                OwningGrid.SelectedItem = dataItem;
            }
            else
            {
                OwningGrid.SetRowSelection(slot, true /*isSelected*/, false /*setAnchorSlot*/);
            }

            return _selectedSlotsTable.IndexOf(slot);
        }

        public void Clear()
        {
            if (OwningGrid.SelectionMode == DataGridSelectionMode.Single)
            {
                throw DataGridError.DataGridSelectedItemsCollection.CannotChangeSelectedItemsCollectionInSingleMode();
            }

            if (_selectedSlotsTable.RangeCount > 0)
            {
                // Clearing the selection does not reset the potential current cell.
                if (!OwningGrid.CommitEdit(DataGridEditingUnit.Row, true /*exitEditing*/))
                {
                    // Edited value couldn't be committed or aborted
                    return;
                }

                OwningGrid.ClearRowSelection(true /*resetAnchorSlot*/);
            }
        }

        public bool Contains(object dataItem)
        {
            int itemIndex = OwningGrid.DataConnection.IndexOf(dataItem);
            if (itemIndex == -1)
            {
                return false;
            }

            DiagnosticsDebug.Assert(itemIndex >= 0, "Expected positive itemIndex.");

            return ContainsSlot(OwningGrid.SlotFromRowIndex(itemIndex));
        }

        public int IndexOf(object dataItem)
        {
            int itemIndex = OwningGrid.DataConnection.IndexOf(dataItem);
            if (itemIndex == -1)
            {
                return -1;
            }

            DiagnosticsDebug.Assert(itemIndex >= 0, "Expected positive itemIndex.");

            int slot = OwningGrid.SlotFromRowIndex(itemIndex);
            return _selectedSlotsTable.IndexOf(slot);
        }

        public void Insert(int index, object dataItem)
        {
            throw new NotSupportedException();
        }

        public void Remove(object dataItem)
        {
            if (OwningGrid.SelectionMode == DataGridSelectionMode.Single)
            {
                throw DataGridError.DataGridSelectedItemsCollection.CannotChangeSelectedItemsCollectionInSingleMode();
            }

            int itemIndex = OwningGrid.DataConnection.IndexOf(dataItem);
            if (itemIndex == -1)
            {
                return;
            }

            DiagnosticsDebug.Assert(itemIndex >= 0, "Expected positive itemIndex.");

            if (itemIndex == OwningGrid.CurrentSlot &&
                !OwningGrid.CommitEdit(DataGridEditingUnit.Row, true /*exitEditing*/))
            {
                // Edited value couldn't be committed or aborted
                return;
            }

            OwningGrid.SetRowSelection(itemIndex, false /*isSelected*/, false /*setAnchorSlot*/);
        }

        public void RemoveAt(int index)
        {
            if (OwningGrid.SelectionMode == DataGridSelectionMode.Single)
            {
                throw DataGridError.DataGridSelectedItemsCollection.CannotChangeSelectedItemsCollectionInSingleMode();
            }

            if (index < 0 || index >= _selectedSlotsTable.IndexCount)
            {
                throw DataGridError.DataGrid.ValueMustBeBetween(nameof(index), "Index", 0, true, _selectedSlotsTable.IndexCount, false);
            }

            int rowIndex = _selectedSlotsTable.GetNthIndex(index);
            DiagnosticsDebug.Assert(rowIndex > -1, "Expected positive itemIndex.");

            if (rowIndex == OwningGrid.CurrentSlot &&
                !OwningGrid.CommitEdit(DataGridEditingUnit.Row, true /*exitEditing*/))
            {
                // Edited value couldn't be committed or aborted
                return;
            }

            OwningGrid.SetRowSelection(rowIndex, false /*isSelected*/, false /*setAnchorSlot*/);
        }

        public int Count => _selectedSlotsTable.IndexCount;

        public bool IsSynchronized => false;

        public object SyncRoot => this;

        public void CopyTo(Array array, int index)
        {
            // TODO: Not supported yet.
            throw new NotImplementedException();
        }

        public IEnumerator GetEnumerator()
        {
            DiagnosticsDebug.Assert(OwningGrid != null, "Expected non-null owning DataGrid.");
            DiagnosticsDebug.Assert(OwningGrid.DataConnection != null, "Expected non-null owning DataGrid.DataConnection.");
            DiagnosticsDebug.Assert(_selectedSlotsTable != null, "Expected non-null _selectedSlotsTable.");

            foreach (int slot in _selectedSlotsTable.GetIndexes())
            {
                int rowIndex = OwningGrid.RowIndexFromSlot(slot);
                DiagnosticsDebug.Assert(rowIndex > -1, "Expected positive rowIndex.");
                yield return OwningGrid.DataConnection.GetDataItem(rowIndex);
            }
        }

        internal DataGrid OwningGrid { get; private set; } = owningGrid;

        internal List<object> SelectedItemsCache
        {
            get => _selectedItemsCache;

            set
            {
                _selectedItemsCache = value;
                UpdateIndexes();
            }
        }

        internal void ClearRows()
        {
            _selectedSlotsTable.Clear();
            _selectedItemsCache.Clear();
        }

        internal bool ContainsSlot(int slot)
        {
            return _selectedSlotsTable.Contains(slot);
        }

        internal bool ContainsAll(int startSlot, int endSlot)
        {
            int itemSlot = OwningGrid.RowGroupHeadersTable.GetNextGap(startSlot - 1);
            while (itemSlot <= endSlot)
            {
                // Skip over the RowGroupHeaderSlots
                int nextRowGroupHeaderSlot = OwningGrid.RowGroupHeadersTable.GetNextIndex(itemSlot);
                int lastItemSlot = nextRowGroupHeaderSlot == -1 ? endSlot : Math.Min(endSlot, nextRowGroupHeaderSlot - 1);
                if (!_selectedSlotsTable.ContainsAll(itemSlot, lastItemSlot))
                {
                    return false;
                }

                itemSlot = OwningGrid.RowGroupHeadersTable.GetNextGap(lastItemSlot);
            }

            return true;
        }

        // Called when an item is deleted from the ItemsSource as opposed to just being unselected
        internal void Delete(int slot, object item)
        {
            if (_oldSelectedSlotsTable.Contains(slot))
            {
                OwningGrid.SelectionHasChanged = true;
            }

            DeleteSlot(slot);
            _selectedItemsCache.Remove(item);
        }

        internal void DeleteSlot(int slot)
        {
            _selectedSlotsTable.RemoveIndex(slot);
            _oldSelectedSlotsTable.RemoveIndex(slot);
        }

        // Returns the inclusive index count between lowerBound and upperBound of all indexes with the given value
        internal int GetIndexCount(int lowerBound, int upperBound)
        {
            return _selectedSlotsTable.GetIndexCount(lowerBound, upperBound, true);
        }

        internal IEnumerable<int> GetIndexes()
        {
            return _selectedSlotsTable.GetIndexes();
        }

        internal IEnumerable<int> GetSlots(int startSlot)
        {
            return _selectedSlotsTable.GetIndexes(startSlot);
        }

        internal SelectionChangedEventArgs GetSelectionChangedEventArgs()
        {
            List<object> addedSelectedItems = [];
            List<object> removedSelectedItems = [];

            // Compare the old selected indexes with the current selection to determine which items
            // have been added and removed since the last time this method was called
            foreach (int newSlot in _selectedSlotsTable.GetIndexes())
            {
                object newItem = OwningGrid.DataConnection.GetDataItem(OwningGrid.RowIndexFromSlot(newSlot));
                if (_oldSelectedSlotsTable.Contains(newSlot))
                {
                    _oldSelectedSlotsTable.RemoveValue(newSlot);
                    _oldSelectedItemsCache.Remove(newItem);
                }
                else
                {
                    addedSelectedItems.Add(newItem);
                }
            }

            foreach (object oldItem in _oldSelectedItemsCache)
            {
                removedSelectedItems.Add(oldItem);
            }

            // The current selection becomes the old selection
            _oldSelectedSlotsTable = _selectedSlotsTable.Copy();
            _oldSelectedItemsCache = [.. _selectedItemsCache];

            return new SelectionChangedEventArgs(new ListReader(removedSelectedItems), new ListReader(addedSelectedItems));
        }

        private partial class ListReader(IList<object> list) : IList<object>
        {
            public object this[int index]
            {
                get => list[index];
                set => list[index] = value;
            }

            public int Count => list.Count;

            public bool IsReadOnly => list.IsReadOnly;

            public void Add(object item) => list.Add(item);

            public void Clear() => list.Clear();

            public bool Contains(object item) => list.Contains(item);

            public void CopyTo(object[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);

            public IEnumerator<object> GetEnumerator() => list.GetEnumerator();

            public int IndexOf(object item) => list.IndexOf(item);

            public void Insert(int index, object item) => list.Insert(index, item);

            public bool Remove(object item) => list.Remove(item);

            public void RemoveAt(int index) => list.RemoveAt(index);

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)list).GetEnumerator();
        }

        internal void InsertIndex(int slot)
        {
            _selectedSlotsTable.InsertIndex(slot);
            _oldSelectedSlotsTable.InsertIndex(slot);

            // It's possible that we're inserting an item that was just removed.  If that's the case,
            // and the re-inserted item used to be selected, we want to update the _oldSelectedSlotsTable
            // to include the item's new index within the collection.
            int rowIndex = OwningGrid.RowIndexFromSlot(slot);
            if (rowIndex != -1)
            {
                object insertedItem = OwningGrid.DataConnection.GetDataItem(rowIndex);
                if (insertedItem != null && _oldSelectedItemsCache.Contains(insertedItem))
                {
                    _oldSelectedSlotsTable.AddValue(slot, true);
                }
            }
        }

        internal void SelectSlot(int slot, bool select)
        {
            if (OwningGrid.RowGroupHeadersTable.Contains(slot))
            {
                return;
            }

            if (select)
            {
                if (!_selectedSlotsTable.Contains(slot))
                {
                    _selectedItemsCache.Add(OwningGrid.DataConnection.GetDataItem(OwningGrid.RowIndexFromSlot(slot)));
                }

                _selectedSlotsTable.AddValue(slot, true);
            }
            else
            {
                if (_selectedSlotsTable.Contains(slot))
                {
                    _selectedItemsCache.Remove(OwningGrid.DataConnection.GetDataItem(OwningGrid.RowIndexFromSlot(slot)));
                }

                _selectedSlotsTable.RemoveValue(slot);
            }
        }

        internal void SelectSlots(int startSlot, int endSlot, bool select)
        {
            int itemSlot = OwningGrid.RowGroupHeadersTable.GetNextGap(startSlot - 1);
            int endItemSlot = OwningGrid.RowGroupHeadersTable.GetPreviousGap(endSlot + 1);

            if (select)
            {
                while (itemSlot <= endItemSlot)
                {
                    // Add the newly selected item slots by skipping over the RowGroupHeaderSlots
                    int nextRowGroupHeaderSlot = OwningGrid.RowGroupHeadersTable.GetNextIndex(itemSlot);
                    int lastItemSlot = nextRowGroupHeaderSlot == -1 ? endItemSlot : Math.Min(endItemSlot, nextRowGroupHeaderSlot - 1);

                    for (int slot = itemSlot; slot <= lastItemSlot; slot++)
                    {
                        if (!_selectedSlotsTable.Contains(slot))
                        {
                            _selectedItemsCache.Add(OwningGrid.DataConnection.GetDataItem(OwningGrid.RowIndexFromSlot(slot)));
                        }
                    }

                    _selectedSlotsTable.AddValues(itemSlot, lastItemSlot - itemSlot + 1, true);
                    itemSlot = OwningGrid.RowGroupHeadersTable.GetNextGap(lastItemSlot);
                }
            }
            else
            {
                while (itemSlot <= endItemSlot)
                {
                    // Remove the unselected item slots by skipping over the RowGroupHeaderSlots
                    int nextRowGroupHeaderSlot = OwningGrid.RowGroupHeadersTable.GetNextIndex(itemSlot);
                    int lastItemSlot = nextRowGroupHeaderSlot == -1 ? endItemSlot : Math.Min(endItemSlot, nextRowGroupHeaderSlot - 1);

                    for (int slot = itemSlot; slot <= lastItemSlot; slot++)
                    {
                        if (_selectedSlotsTable.Contains(slot))
                        {
                            _selectedItemsCache.Remove(OwningGrid.DataConnection.GetDataItem(OwningGrid.RowIndexFromSlot(slot)));
                        }
                    }

                    _selectedSlotsTable.RemoveValues(itemSlot, lastItemSlot - itemSlot + 1);
                    itemSlot = OwningGrid.RowGroupHeadersTable.GetNextGap(lastItemSlot);
                }
            }
        }

        internal void UpdateIndexes()
        {
            _oldSelectedSlotsTable.Clear();
            _selectedSlotsTable.Clear();

            if (OwningGrid.DataConnection.DataSource == null)
            {
                if (SelectedItemsCache.Count > 0)
                {
                    OwningGrid.SelectionHasChanged = true;
                    SelectedItemsCache.Clear();
                }
            }
            else
            {
                List<object> tempSelectedItemsCache = [];
                foreach (object item in _selectedItemsCache)
                {
                    int index = OwningGrid.DataConnection.IndexOf(item);
                    if (index != -1)
                    {
                        tempSelectedItemsCache.Add(item);
                        _selectedSlotsTable.AddValue(OwningGrid.SlotFromRowIndex(index), true);
                    }
                }

                foreach (object item in _oldSelectedItemsCache)
                {
                    int index = OwningGrid.DataConnection.IndexOf(item);
                    if (index == -1)
                    {
                        OwningGrid.SelectionHasChanged = true;
                    }
                    else
                    {
                        _oldSelectedSlotsTable.AddValue(OwningGrid.SlotFromRowIndex(index), true);
                    }
                }

                _selectedItemsCache = tempSelectedItemsCache;
            }
        }
    }
}
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.WinUI.Data.Utilities;
using CommunityToolkit.WinUI.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using DiagnosticsDebug = System.Diagnostics.Debug;

namespace CommunityToolkit.WinUI.Controls.DataGridInternals
{
    internal class DataGridDataConnection(DataGrid owner)
    {
        private int _backupSlotForCurrentChanged;
        private int _columnForCurrentChanged;
        private PropertyInfo[] _dataProperties;
        private IEnumerable _dataSource;
        private Type _dataType;
        private bool _expectingCurrentChanged;
        private ISupportIncrementalLoading _incrementalItemsSource;
        private object _itemToSelectOnCurrentChanged;
        private IAsyncOperation<LoadMoreItemsResult> _loadingOperation;
        private bool _scrollForCurrentChanged;
        private DataGridSelectionAction _selectionActionForCurrentChanged;
        private WeakEventListener<DataGridDataConnection, object, NotifyCollectionChangedEventArgs> _weakCollectionChangedListener;
        private WeakEventListener<DataGridDataConnection, object, IVectorChangedEventArgs> _weakVectorChangedListener;
        private WeakEventListener<DataGridDataConnection, object, CurrentChangingEventArgs> _weakCurrentChangingListener;
        private WeakEventListener<DataGridDataConnection, object, object> _weakCurrentChangedListener;
        private WeakEventListener<DataGridDataConnection, object, PropertyChangedEventArgs> _weakIncrementalItemsSourcePropertyChangedListener;

#if FEATURE_ICOLLECTIONVIEW_SORT
        private WeakEventListener<DataGridDataConnection, object, NotifyCollectionChangedEventArgs> _weakSortDescriptionsCollectionChangedListener;
#endif

        public bool AllowEdit => List == null || !List.IsReadOnly;

        /// <summary>
        /// Gets a value indicating whether the collection view says it can sort.
        /// </summary>
        public bool AllowSort
        {
            get
            {
                if (CollectionView == null)
                {
                    return false;
                }

#if FEATURE_IEDITABLECOLLECTIONVIEW
                if (EditableCollectionView != null && (EditableCollectionView.IsAddingNew || EditableCollectionView.IsEditingItem))
                {
                    return false;
                }
#endif

#if FEATURE_ICOLLECTIONVIEW_SORT
                return CollectionView.CanSort;
#else
                return false;
#endif
            }
        }

        public bool CanCancelEdit
        {
            get
            {
#if FEATURE_IEDITABLECOLLECTIONVIEW
                return EditableCollectionView != null && EditableCollectionView.CanCancelEdit;
#else
                return false;
#endif
            }
        }

        public ICollectionView CollectionView => DataSource as ICollectionView;

        public int Count
        {
            get
            {
                IList list = List;
                if (list != null)
                {
                    return list.Count;
                }

#if FEATURE_PAGEDCOLLECTIONVIEW
                PagedCollectionView collectionView = DataSource as PagedCollectionView;
                if (collectionView != null)
                {
                    return collectionView.Count;
                }
#endif

                int count = 0;
                IEnumerable enumerable = DataSource;
                if (enumerable != null)
                {
                    IEnumerator enumerator = enumerable.GetEnumerator();
                    if (enumerator != null)
                    {
                        while (enumerator.MoveNext())
                        {
                            count++;
                        }
                    }
                }

                return count;
            }
        }

        public bool DataIsPrimitive => DataTypeIsPrimitive(DataType);

        public PropertyInfo[] DataProperties
        {
            get
            {
                if (_dataProperties == null)
                {
                    UpdateDataProperties();
                }

                return _dataProperties;
            }
        }

        public IEnumerable DataSource
        {
            get => _dataSource;

            set
            {
                _dataSource = value;

                // Because the DataSource is changing, we need to reset our cached values for DataType and DataProperties,
                // which are dependent on the current DataSource
                _dataType = null;
                UpdateDataProperties();
                UpdateIncrementalItemsSource();
            }
        }

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicNestedTypes | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicEvents)]
        public Type DataType
        {
            [SuppressMessage("Trimming", "IL2078:Target method return value does not satisfy 'DynamicallyAccessedMembersAttribute' requirements. The source field does not have matching annotations.", Justification = "<¹ÒÆð>")]
            get
            {
                // We need to use the raw ItemsSource as opposed to DataSource because DataSource
                // may be the ItemsSource wrapped in a collection view, in which case we wouldn't
                // be able to take T to be the type if we're given IEnumerable<T>
                if (_dataType == null && owner.ItemsSource != null)
                {
                    _dataType = owner.ItemsSource.GetItemType();
                }

                return _dataType;
            }
        }

        public bool HasMoreItems => IsDataSourceIncremental && _incrementalItemsSource.HasMoreItems;

        public bool IsDataSourceIncremental => _incrementalItemsSource != null;

        public bool IsLoadingMoreItems => _loadingOperation != null;

#if FEATURE_IEDITABLECOLLECTIONVIEW
        public IEditableCollectionView EditableCollectionView
        {
            get
            {
                return DataSource as IEditableCollectionView;
            }
        }
#endif

        public bool EndingEdit { get; private set; }

        public bool EventsWired { get; private set; }

        public bool IsAddingNew
        {
            get
            {
#if FEATURE_IEDITABLECOLLECTIONVIEW
                return EditableCollectionView != null && EditableCollectionView.IsAddingNew;
#else
                return false;
#endif
            }
        }

        private bool IsGrouping
        {
            get
            {
                return CollectionView != null &&
#if FEATURE_ICOLLECTIONVIEW_GROUP
                    CollectionView.CanGroup &&
#endif
                    CollectionView.CollectionGroups != null &&
                    CollectionView.CollectionGroups.Count > 0;
            }
        }

        public IList List => DataSource as IList;

        public int NewItemPlaceholderIndex
        {
            get
            {
#if FEATURE_IEDITABLECOLLECTIONVIEW
                if (EditableCollectionView != null && EditableCollectionView.NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtEnd)
                {
                    return Count - 1;
                }
#endif

                return -1;
            }
        }

#if FEATURE_IEDITABLECOLLECTIONVIEW
        public NewItemPlaceholderPosition NewItemPlaceholderPosition
        {
            get
            {
                if (EditableCollectionView != null)
                {
                    return EditableCollectionView.NewItemPlaceholderPosition;
                }

                return NewItemPlaceholderPosition.None;
            }
        }
#endif

        public bool ShouldAutoGenerateColumns => owner.AutoGenerateColumns
                    && (owner.ColumnsInternal.AutogeneratedColumnCount == 0)
                    && ((DataProperties != null && DataProperties.Length > 0) || DataIsPrimitive);

#if FEATURE_ICOLLECTIONVIEW_SORT
        public SortDescriptionCollection SortDescriptions
        {
            get
            {
                if (CollectionView != null && CollectionView.CanSort)
                {
                    return CollectionView.SortDescriptions;
                }
                else
                {
                    return (SortDescriptionCollection)null;
                }
            }
        }
#endif

        public static bool CanEdit(Type type)
        {
            DiagnosticsDebug.Assert(type != null, "Expected non-null type.");

            type = type.GetNonNullableType();

            return
                type.GetTypeInfo().IsEnum
                || type == typeof(string)
                || type == typeof(char)
                || type == typeof(bool)
                || type == typeof(byte)
                || type == typeof(sbyte)
                || type == typeof(float)
                || type == typeof(double)
                || type == typeof(decimal)
                || type == typeof(short)
                || type == typeof(int)
                || type == typeof(long)
                || type == typeof(ushort)
                || type == typeof(uint)
                || type == typeof(ulong)
                || type == typeof(DateTime);
        }

        /// <summary>
        /// Puts the entity into editing mode if possible
        /// </summary>
        /// <param name="dataItem">The entity to edit</param>
        /// <returns>True if editing was started</returns>
        public bool BeginEdit(object dataItem)
        {
            if (dataItem == null)
            {
                return false;
            }

#if FEATURE_IEDITABLECOLLECTIONVIEW
            IEditableCollectionView editableCollectionView = EditableCollectionView;
            if (editableCollectionView != null)
            {
                if ((editableCollectionView.IsEditingItem && (dataItem == editableCollectionView.CurrentEditItem)) ||
                    (editableCollectionView.IsAddingNew && (dataItem == editableCollectionView.CurrentAddItem)))
                {
                    return true;
                }
                else
                {
                    editableCollectionView.EditItem(dataItem);
                    return editableCollectionView.IsEditingItem;
                }
            }
#endif

            if (dataItem is IEditableObject editableDataItem)
            {
                editableDataItem.BeginEdit();
                return true;
            }

            return true;
        }

        /// <summary>
        /// Cancels the current entity editing and exits the editing mode.
        /// </summary>
        /// <param name="dataItem">The entity being edited</param>
        /// <returns>True if a cancellation operation was invoked.</returns>
        public bool CancelEdit(object dataItem)
        {
#if FEATURE_IEDITABLECOLLECTIONVIEW
            IEditableCollectionView editableCollectionView = EditableCollectionView;
            if (editableCollectionView != null)
            {
                _owner.NoCurrentCellChangeCount++;
                EndingEdit = true;
                try
                {
                    if (editableCollectionView.IsAddingNew && dataItem == editableCollectionView.CurrentAddItem)
                    {
                        editableCollectionView.CancelNew();
                        return true;
                    }
                    else if (editableCollectionView.CanCancelEdit)
                    {
                        editableCollectionView.CancelEdit();
                        return true;
                    }
                }
                finally
                {
                    _owner.NoCurrentCellChangeCount--;
                    EndingEdit = false;
                }

                return false;
            }
#endif

            IEditableObject editableDataItem = dataItem as IEditableObject;
            if (editableDataItem != null)
            {
                editableDataItem.CancelEdit();
                return true;
            }

            return true;
        }

        /// <summary>
        /// Commits the current entity editing and exits the editing mode.
        /// </summary>
        /// <param name="dataItem">The entity being edited</param>
        /// <returns>True if a commit operation was invoked.</returns>
        public bool EndEdit(object dataItem)
        {
#if FEATURE_IEDITABLECOLLECTIONVIEW
            IEditableCollectionView editableCollectionView = EditableCollectionView;
            if (editableCollectionView != null)
            {
                // IEditableCollectionView.CommitEdit can potentially change currency. If it does,
                // we don't want to attempt a second commit inside our CurrentChanging event handler.
                _owner.NoCurrentCellChangeCount++;
                EndingEdit = true;
                try
                {
                    if (editableCollectionView.IsAddingNew && dataItem == editableCollectionView.CurrentAddItem)
                    {
                        editableCollectionView.CommitNew();
                    }
                    else
                    {
                        editableCollectionView.CommitEdit();
                    }
                }
                finally
                {
                    _owner.NoCurrentCellChangeCount--;
                    EndingEdit = false;
                }

                return true;
            }
#endif

            if (dataItem is IEditableObject editableDataItem)
            {
                editableDataItem.EndEdit();
            }

            return true;
        }

        // Assumes index >= 0, returns null if index >= Count
        public object GetDataItem(int index)
        {
            DiagnosticsDebug.Assert(index >= 0, "Expected positive index.");

            IList list = List;
            if (list != null)
            {
                return (index < list.Count) ? list[index] : null;
            }

#if FEATURE_PAGEDCOLLECTIONVIEW
            PagedCollectionView collectionView = DataSource as PagedCollectionView;
            if (collectionView != null)
            {
                return (index < collectionView.Count) ? collectionView.GetItemAt(index) : null;
            }
#endif

            IEnumerable enumerable = DataSource;
            if (enumerable != null)
            {
                IEnumerator enumerator = enumerable.GetEnumerator();
                int i = -1;
                while (enumerator.MoveNext() && i < index)
                {
                    i++;
                    if (i == index)
                    {
                        return enumerator.Current;
                    }
                }
            }

            return null;
        }

        [SuppressMessage("Trimming", "IL2072:Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.", Justification = "<¹ÒÆð>")]
        public bool GetPropertyIsReadOnly(string propertyName)
        {
            if (DataType != null)
            {
                if (!string.IsNullOrEmpty(propertyName))
                {
                    Type propertyType = DataType;
                    PropertyInfo propertyInfo = null;
                    List<string> propertyNames = TypeHelper.SplitPropertyPath(propertyName);
                    for (int i = 0; i < propertyNames.Count; i++)
                    {
                        if (propertyType.GetTypeInfo().GetIsReadOnly())
                        {
                            return true;
                        }

                        propertyInfo = propertyType.GetPropertyOrIndexer(propertyNames[i], out _);
                        if (propertyInfo == null || propertyInfo.GetIsReadOnly())
                        {
                            // Either the property doesn't exist or it does exist but is read-only.
                            return true;
                        }

                        // Check if EditableAttribute is defined on the property and if it indicates uneditable
                        var editableAttribute = propertyInfo.GetCustomAttributes().OfType<EditableAttribute>().FirstOrDefault();
                        if (editableAttribute != null && !editableAttribute.AllowEdit)
                        {
                            return true;
                        }

                        propertyType = propertyInfo.PropertyType.GetNonNullableType();
                    }

                    return propertyInfo == null || !propertyInfo.CanWrite || !AllowEdit || !CanEdit(propertyType);
                }
                else
                {
                    if (DataType.GetTypeInfo().GetIsReadOnly())
                    {
                        return true;
                    }
                }
            }

            return !AllowEdit;
        }

        public int IndexOf(object dataItem)
        {
            IList list = List;
            if (list != null)
            {
                return list.IndexOf(dataItem);
            }

#if FEATURE_PAGEDCOLLECTIONVIEW
            PagedCollectionView cv = DataSource as PagedCollectionView;
            if (cv != null)
            {
                return cv.IndexOf(dataItem);
            }
#endif

            IEnumerable enumerable = DataSource;
            if (enumerable != null && dataItem != null)
            {
                int index = 0;
                foreach (object dataItemTmp in enumerable)
                {
                    if ((dataItem == null && dataItemTmp == null) ||
                        dataItem.Equals(dataItemTmp))
                    {
                        return index;
                    }

                    index++;
                }
            }

            return -1;
        }

        public void LoadMoreItems(uint count)
        {
            DiagnosticsDebug.Assert(_loadingOperation == null, "Expected _loadingOperation == null.");

            _loadingOperation = _incrementalItemsSource.LoadMoreItemsAsync(count);

            if (_loadingOperation != null)
            {
                _loadingOperation.Completed = OnLoadingOperationCompleted;
            }
        }

#if FEATURE_PAGEDCOLLECTIONVIEW
        /// <summary>
        /// Creates a collection view around the DataGrid's source. ICollectionViewFactory is
        /// used if the source implements it. Otherwise a PagedCollectionView is returned.
        /// </summary>
        /// <param name="source">Enumerable source for which to create a view</param>
        /// <returns>ICollectionView view over the provided source</returns>
#else
        /// <summary>
        /// Creates a collection view around the DataGrid's source. ICollectionViewFactory is
        /// used if the source implements it.
        /// </summary>
        /// <param name="source">Enumerable source for which to create a view</param>
        /// <returns>ICollectionView view over the provided source</returns>
#endif
        internal static ICollectionView CreateView(IEnumerable source)
        {
            DiagnosticsDebug.Assert(source != null, "source unexpectedly null");
            DiagnosticsDebug.Assert(source is not ICollectionView, "source is an ICollectionView");

            ICollectionView collectionView = null;

            if (source is ICollectionViewFactory collectionViewFactory)
            {
                // If the source is a collection view factory, give it a chance to produce a custom collection view.
                collectionView = collectionViewFactory.CreateView();

                // Intentionally not catching potential exception thrown by ICollectionViewFactory.CreateView().
            }

#if FEATURE_PAGEDCOLLECTIONVIEW
            if (collectionView == null)
            {
                collectionView = new PagedCollectionView(source);
            }
#endif

            collectionView ??= source is IList sourceAsList ? new ListCollectionView(sourceAsList) : new EnumerableCollectionView(source);

            return collectionView;
        }

        internal static bool DataTypeIsPrimitive(Type dataType)
        {
            if (dataType != null)
            {
                Type type = TypeHelper.GetNonNullableType(dataType);  // no-opt if dataType isn't nullable
                return
                    type.GetTypeInfo().IsPrimitive ||
                    type == typeof(string) ||
                    type == typeof(decimal) ||
                    type == typeof(DateTime);
            }
            else
            {
                return false;
            }
        }

        internal void ClearDataProperties()
        {
            _dataProperties = null;
        }

        internal void MoveCurrentTo(object item, int backupSlot, int columnIndex, DataGridSelectionAction action, bool scrollIntoView)
        {
            if (CollectionView != null)
            {
                _expectingCurrentChanged = true;
                _columnForCurrentChanged = columnIndex;
                _itemToSelectOnCurrentChanged = item;
                _selectionActionForCurrentChanged = action;
                _scrollForCurrentChanged = scrollIntoView;
                _backupSlotForCurrentChanged = backupSlot;

                var itemIsCollectionViewGroup = item is ICollectionViewGroup;
                CollectionView.MoveCurrentTo((itemIsCollectionViewGroup || IndexOf(item) == NewItemPlaceholderIndex) ? null : item);

                _expectingCurrentChanged = false;
            }
        }

        internal void UnWireEvents(IEnumerable value)
        {
            if (value is INotifyCollectionChanged notifyingDataSource1 && _weakCollectionChangedListener != null)
            {
                _weakCollectionChangedListener.Detach();
                _weakCollectionChangedListener = null;
            }

            if (value is IObservableVector<object> notifyingDataSource2 && _weakVectorChangedListener != null)
            {
                _weakVectorChangedListener.Detach();
                _weakVectorChangedListener = null;
            }

#if FEATURE_ICOLLECTIONVIEW_SORT
            if (SortDescriptions != null && _weakSortDescriptionsCollectionChangedListener != null)
            {
                _weakSortDescriptionsCollectionChangedListener.Detach();
                _weakSortDescriptionsCollectionChangedListener = null;
            }
#endif

            if (CollectionView != null)
            {
                if (_weakCurrentChangedListener != null)
                {
                    _weakCurrentChangedListener.Detach();
                    _weakCurrentChangedListener = null;
                }

                if (_weakCurrentChangingListener != null)
                {
                    _weakCurrentChangingListener.Detach();
                    _weakCurrentChangingListener = null;
                }
            }

            EventsWired = false;
        }

        internal void WireEvents(IEnumerable value)
        {
            if (value is INotifyCollectionChanged notifyingDataSource1)
            {
                _weakCollectionChangedListener = new WeakEventListener<DataGridDataConnection, object, NotifyCollectionChangedEventArgs>(this)
                {
                    OnEventAction = (instance, source, eventArgs) => instance.NotifyingDataSource_CollectionChanged(source, eventArgs),
                    OnDetachAction = (weakEventListener) => notifyingDataSource1.CollectionChanged -= weakEventListener.OnEvent
                };
                notifyingDataSource1.CollectionChanged += _weakCollectionChangedListener.OnEvent;
            }
            else
            {
                if (value is IObservableVector<object> notifyingDataSource2)
                {
                    _weakVectorChangedListener = new WeakEventListener<DataGridDataConnection, object, IVectorChangedEventArgs>(this)
                    {
                        OnEventAction = (instance, source, eventArgs) => instance.NotifyingDataSource_VectorChanged(source as IObservableVector<object>, eventArgs)
                    };
                    _weakVectorChangedListener.OnDetachAction = (weakEventListener) => notifyingDataSource2.VectorChanged -= _weakVectorChangedListener.OnEvent;
                    notifyingDataSource2.VectorChanged += _weakVectorChangedListener.OnEvent;
                }
            }

#if FEATURE_ICOLLECTIONVIEW_SORT
            if (SortDescriptions != null)
            {
                INotifyCollectionChanged sortDescriptionsINCC = (INotifyCollectionChanged)SortDescriptions;
                _weakSortDescriptionsCollectionChangedListener = new WeakEventListener<DataGridDataConnection, object, NotifyCollectionChangedEventArgs>(this);
                _weakSortDescriptionsCollectionChangedListener.OnEventAction = (instance, source, eventArgs) => instance.CollectionView_SortDescriptions_CollectionChanged(source, eventArgs);
                _weakSortDescriptionsCollectionChangedListener.OnDetachAction = (weakEventListener) => sortDescriptionsINCC.CollectionChanged -= weakEventListener.OnEvent;
                sortDescriptionsINCC.CollectionChanged += _weakSortDescriptionsCollectionChangedListener.OnEvent;
            }
#endif

            if (CollectionView != null)
            {
                // A local variable must be used in the lambda expression or the CollectionView will leak
                ICollectionView collectionView = CollectionView;

                _weakCurrentChangedListener = new WeakEventListener<DataGridDataConnection, object, object>(this)
                {
                    OnEventAction = (instance, source, eventArgs) => instance.CollectionView_CurrentChanged(source, null),
                    OnDetachAction = (weakEventListener) => collectionView.CurrentChanged -= weakEventListener.OnEvent
                };
                CollectionView.CurrentChanged += _weakCurrentChangedListener.OnEvent;

                _weakCurrentChangingListener = new WeakEventListener<DataGridDataConnection, object, CurrentChangingEventArgs>(this)
                {
                    OnEventAction = (instance, source, eventArgs) => instance.CollectionView_CurrentChanging(source, eventArgs),
                    OnDetachAction = (weakEventListener) => collectionView.CurrentChanging -= weakEventListener.OnEvent
                };
                CollectionView.CurrentChanging += _weakCurrentChangingListener.OnEvent;
            }

            EventsWired = true;
        }

        private void CollectionView_CurrentChanged(object sender, object e)
        {
            if (_expectingCurrentChanged)
            {
                // Committing Edit could cause our item to move to a group that no longer exists.  In
                // this case, we need to update the item.
                if (_itemToSelectOnCurrentChanged is ICollectionViewGroup collectionViewGroup)
                {
                    DataGridRowGroupInfo groupInfo = owner.RowGroupInfoFromCollectionViewGroup(collectionViewGroup);
                    if (groupInfo == null)
                    {
                        // Move to the next slot if the target slot isn't visible
                        if (!owner.IsSlotVisible(_backupSlotForCurrentChanged))
                        {
                            _backupSlotForCurrentChanged = owner.GetNextVisibleSlot(_backupSlotForCurrentChanged);
                        }

                        // Move to the next best slot if we've moved past all the slots.  This could happen if multiple
                        // groups were removed.
                        if (_backupSlotForCurrentChanged >= owner.SlotCount)
                        {
                            _backupSlotForCurrentChanged = owner.GetPreviousVisibleSlot(owner.SlotCount);
                        }

                        // Update the itemToSelect
                        int newCurrentPosition = -1;
                        _itemToSelectOnCurrentChanged = owner.ItemFromSlot(_backupSlotForCurrentChanged, ref newCurrentPosition);
                    }
                }

                owner.ProcessSelectionAndCurrency(
                    _columnForCurrentChanged,
                    _itemToSelectOnCurrentChanged,
                    _backupSlotForCurrentChanged,
                    _selectionActionForCurrentChanged,
                    _scrollForCurrentChanged);
            }
            else if (CollectionView != null)
            {
                owner.UpdateStateOnCurrentChanged(CollectionView.CurrentItem, CollectionView.CurrentPosition);
            }
        }

        private void CollectionView_CurrentChanging(object sender, CurrentChangingEventArgs e)
        {
            if (owner.NoCurrentCellChangeCount == 0 &&
                !_expectingCurrentChanged &&
                !EndingEdit &&
                !owner.CommitEdit())
            {
                // If CommitEdit failed, then the user has most likely input invalid data.
                // Cancel the current change if possible, otherwise abort the edit.
                if (e.IsCancelable)
                {
                    e.Cancel = true;
                }
                else
                {
                    owner.CancelEdit(DataGridEditingUnit.Row, false);
                }
            }
        }

#if FEATURE_ICOLLECTIONVIEW_SORT
        private void CollectionView_SortDescriptions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_owner.ColumnsItemsInternal.Count == 0)
            {
                return;
            }

            // Refresh sort description
            foreach (DataGridColumn column in _owner.ColumnsItemsInternal)
            {
                column.HeaderCell.ApplyState(true);
            }
        }
#endif

        private void NotifyingDataSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (owner.LoadingOrUnloadingRow)
            {
                throw DataGridError.DataGrid.CannotChangeItemsWhenLoadingRows();
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    DiagnosticsDebug.Assert(e.NewItems != null, "Unexpected NotifyCollectionChangedAction.Add notification");
                    DiagnosticsDebug.Assert(ShouldAutoGenerateColumns || IsGrouping || e.NewItems.Count == 1, "Expected NewItems.Count equals 1.");
                    NotifyingDataSource_Add(e.NewStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    IList removedItems = e.OldItems;
                    if (removedItems == null || e.OldStartingIndex < 0)
                    {
                        DiagnosticsDebug.Assert(false, "Unexpected NotifyCollectionChangedAction.Remove notification");
                        return;
                    }

                    if (!IsGrouping)
                    {
                        // If we're grouping then we handle this through the CollectionViewGroup notifications.
                        // Remove is a single item operation.
                        foreach (object item in removedItems)
                        {
                            DiagnosticsDebug.Assert(item != null, "Expected non-null item.");
                            owner.RemoveRowAt(e.OldStartingIndex, item);
                        }
                    }

                    break;

                case NotifyCollectionChangedAction.Replace:
                    throw new NotSupportedException();

                case NotifyCollectionChangedAction.Reset:
                    NotifyingDataSource_Reset();
                    break;
            }
        }

        private void NotifyingDataSource_VectorChanged(IObservableVector<object> sender, IVectorChangedEventArgs e)
        {
            if (owner.LoadingOrUnloadingRow)
            {
                throw DataGridError.DataGrid.CannotChangeItemsWhenLoadingRows();
            }

            int index = (int)e.Index;

            switch (e.CollectionChange)
            {
                case CollectionChange.ItemChanged:
                    throw new NotSupportedException();

                case CollectionChange.ItemInserted:
                    NotifyingDataSource_Add(index);
                    break;

                case CollectionChange.ItemRemoved:
                    if (!IsGrouping)
                    {
                        // If we're grouping then we handle this through the CollectionViewGroup notifications.
                        // Remove is a single item operation.
                        owner.RemoveRowAt(index, sender[index]);
                    }

                    break;

                case CollectionChange.Reset:
                    NotifyingDataSource_Reset();
                    break;
            }
        }

        private void NotifyingDataSource_Add(int index)
        {
            if (ShouldAutoGenerateColumns)
            {
                // The columns are also affected (not just rows) in this case, so reset everything.
                owner.InitializeElements(false /*recycleRows*/);
            }
            else if (!IsGrouping)
            {
                // If we're grouping then we handle this through the CollectionViewGroup notifications.
                // Add is a single item operation.
                owner.InsertRowAt(index);
            }
        }

        private void NotifyingDataSource_Reset()
        {
            // Did the data type change during the reset?  If not, we can recycle
            // the existing rows instead of having to clear them all.  We still need to clear our cached
            // values for DataType and DataProperties, though, because the collection has been reset.
            Type previousDataType = _dataType;
            _dataType = null;
            if (previousDataType != DataType)
            {
                ClearDataProperties();
                owner.InitializeElements(false /*recycleRows*/);
            }
            else
            {
                owner.InitializeElements(!ShouldAutoGenerateColumns /*recycleRows*/);
            }
        }

        private void NotifyingIncrementalItemsSource(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(HasMoreItems))
            {
                owner.LoadMoreDataFromIncrementalItemsSource();
            }
        }

        private void OnLoadingOperationCompleted(object info, AsyncStatus status)
        {
            if (status != AsyncStatus.Started)
            {
                _loadingOperation = null;
            }
        }

        private void UpdateDataProperties()
        {
            Type dataType = DataType;

            if (DataSource != null && dataType != null && !DataTypeIsPrimitive(dataType))
            {
                _dataProperties = dataType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                DiagnosticsDebug.Assert(_dataProperties != null, "Expected non-null _dataProperties.");
            }
            else
            {
                _dataProperties = null;
            }
        }

        private void UpdateIncrementalItemsSource()
        {
            if (_weakIncrementalItemsSourcePropertyChangedListener != null)
            {
                _weakIncrementalItemsSourcePropertyChangedListener.Detach();
                _weakIncrementalItemsSourcePropertyChangedListener = null;
            }

            // Determine if incremental loading should be used
            if (_dataSource is ISupportIncrementalLoading incrementalDataSource)
            {
                _incrementalItemsSource = incrementalDataSource;
            }
            else if (owner.ItemsSource is ISupportIncrementalLoading incrementalItemsSource)
            {
                _incrementalItemsSource = incrementalItemsSource;
            }
            else
            {
                _incrementalItemsSource = default;
            }

            if (_incrementalItemsSource != null && _incrementalItemsSource is INotifyPropertyChanged inpc)
            {
                _weakIncrementalItemsSourcePropertyChangedListener = new WeakEventListener<DataGridDataConnection, object, PropertyChangedEventArgs>(this)
                {
                    OnEventAction = (instance, source, eventArgs) => instance.NotifyingIncrementalItemsSource(source, eventArgs),
                    OnDetachAction = (weakEventListener) => inpc.PropertyChanged -= weakEventListener.OnEvent
                };
                inpc.PropertyChanged += _weakIncrementalItemsSourcePropertyChangedListener.OnEvent;
            }

            if (_loadingOperation != null)
            {
                _loadingOperation.Cancel();
                _loadingOperation = null;
            }
        }
    }
}
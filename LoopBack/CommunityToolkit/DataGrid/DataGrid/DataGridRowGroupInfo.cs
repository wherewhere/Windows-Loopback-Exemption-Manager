// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace CommunityToolkit.WinUI.Controls.DataGridInternals
{
    internal class DataGridRowGroupInfo(
        ICollectionViewGroup collectionViewGroup,
        Visibility visibility,
        int level,
        int slot,
        int lastSubItemSlot)
    {
        public ICollectionViewGroup CollectionViewGroup { get; private set; } = collectionViewGroup;

        public int LastSubItemSlot { get; set; } = lastSubItemSlot;

        public int Level { get; private set; } = level;

        public int Slot { get; set; } = slot;

        public Visibility Visibility { get; set; } = visibility;
    }
}
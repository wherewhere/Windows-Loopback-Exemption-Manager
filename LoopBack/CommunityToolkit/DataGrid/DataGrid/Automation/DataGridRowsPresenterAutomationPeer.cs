// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.WinUI.Controls.Primitives;
using System.Collections.Generic;
using Windows.UI.Xaml.Automation.Peers;

namespace CommunityToolkit.WinUI.Automation.Peers
{
    /// <summary>
    /// AutomationPeer for the <see cref="DataGridRowsPresenter"/> class.
    /// </summary>
    public partial class DataGridRowsPresenterAutomationPeer : FrameworkElementAutomationPeer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridRowsPresenterAutomationPeer"/> class.
        /// </summary>
        /// <param name="owner">Owning DataGridRowsPresenter</param>
        public DataGridRowsPresenterAutomationPeer(DataGridRowsPresenter owner)
            : base(owner)
        {
        }

        private DataGridAutomationPeer GridPeer
        {
            get
            {
                if (OwningRowsPresenter.OwningGrid != null)
                {
                    return CreatePeerForElement(OwningRowsPresenter.OwningGrid) as DataGridAutomationPeer;
                }

                return null;
            }
        }

        private DataGridRowsPresenter OwningRowsPresenter => Owner as DataGridRowsPresenter;

        /// <summary>
        /// Gets the control type for the element that is associated with the UI Automation peer.
        /// </summary>
        /// <returns>The control type.</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Custom;
        }

        /// <summary>
        /// Gets the collection of elements that are represented in the UI Automation tree as immediate
        /// child elements of the automation peer.
        /// </summary>
        /// <returns>The children elements.</returns>
        protected override IList<AutomationPeer> GetChildrenCore()
        {
            if (OwningRowsPresenter.OwningGrid == null)
            {
                return new List<AutomationPeer>();
            }

            return this.GridPeer.GetChildPeers();
        }

        /// <summary>
        /// Called by GetClassName that gets a human readable name that, in addition to AutomationControlType,
        /// differentiates the control represented by this AutomationPeer.
        /// </summary>
        /// <returns>The string that contains the name.</returns>
        protected override string GetClassNameCore()
        {
            string classNameCore = Owner.GetType().Name;
#if DEBUG_AUTOMATION
            System.Diagnostics.Debug.WriteLine("DataGridRowsPresenterAutomationPeer.GetClassNameCore returns " + classNameCore);
#endif
            return classNameCore;
        }

        /// <summary>
        /// Gets a value that specifies whether the element is a content element.
        /// </summary>
        /// <returns>True if the element is a content element; otherwise false</returns>
        protected override bool IsContentElementCore()
        {
            return false;
        }
    }
}
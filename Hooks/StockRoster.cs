using KSP;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace KerbalSorter.Hooks {
    /// <summary>
    /// Wrapper object for the stock kerbal roster for use with SortBar.
    /// </summary>
    class StockRoster : Roster<IUIListObject> {
        private UIScrollList crew;

        public StockRoster(UIScrollList crew) {
            this.crew = crew;
        }

        /// <summary>
        /// Number of kerbals in the list.
        /// </summary>
        public override int Count {
            get { return crew.Count; }
        }

        /// <summary>
        /// Gets the IUIListObject representing the kerbal at the given index.
        /// </summary>
        /// Use GetKerbal() to retrieve the ProtoCrewMember in the returned item.
        /// <param name="index"></param>
        /// <returns></returns>
        public override IUIListObject GetItem(int index) {
            return crew.GetItem(index);
        }

        /// <summary>
        /// Removes the kerbal at the given index.
        /// </summary>
        /// <param name="index"></param>
        public override void RemoveItem(int index) {
            crew.RemoveItem(index, false);
        }

        /// <summary>
        /// Inserts a kerbal at the given index.
        /// </summary>
        /// <param name="item">The IUIListObject representing the kerbal</param>
        /// <param name="index"></param>
        public override void InsertItem(IUIListObject item, int index) {
            crew.InsertItem(item, index);
        }

        /// <summary>
        /// Retrieves a kerbal from its IUIListObject wrapper.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override ProtoCrewMember GetKerbal(IUIListObject item) {
            return item.gameObject.GetComponent<CrewItemContainer>().GetCrewRef();
        }
    }
}

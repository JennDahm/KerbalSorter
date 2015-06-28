using KSP;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace KerbalSorter.Hooks {
    class StockRoster : Roster<IUIListObject> {
        private UIScrollList crew;

        public StockRoster(UIScrollList crew) {
            this.crew = crew;
        }

        public int Count {
            get { return crew.Count; }
        }

        public IUIListObject GetItem(int index) {
            return crew.GetItem(index);
        }

        public void RemoveItem(int index) {
            crew.RemoveItem(index, false);
        }

        public void InsertItem(IUIListObject item, int index) {
            crew.InsertItem(item, index);
        }

        public ProtoCrewMember GetKerbal(IUIListObject item) {
            return item.gameObject.GetComponent<CrewItemContainer>().GetCrewRef();
        }
    }
}

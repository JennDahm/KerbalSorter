using KSP;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace KerbalSorter.Hooks {
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class AstronautComplexHook : MonoBehaviour {
        CMAstronautComplex complex;
        SortingButtons sortBar;
        StockRoster available;
        StockRoster assigned;
        StockRoster killed;
        CrewPanel curPanel;

        protected void Start() {
            try {
                // Set up hooks:
                GameEvents.onGUIAstronautComplexSpawn.Add(OnACSpawn);
                GameEvents.onGUIAstronautComplexDespawn.Add(OnACDespawn);
                GameEvents.OnCrewmemberHired.Add(OnHire);
                //GameEvents.OnCrewmemberSacked.Add(OnFire);

                // Get rosters:
                complex = UIManager.instance.gameObject.GetComponentsInChildren<CMAstronautComplex>(true).FirstOrDefault();
                if( complex == null ) throw new Exception("Could not find astronaut complex");
                UIScrollList availableCrew = complex.transform.Find("CrewPanels/panel_enlisted/panelManager/panel_available/scrolllist_available").GetComponent<UIScrollList>();
                UIScrollList assignedCrew = complex.transform.Find("CrewPanels/panel_enlisted/panelManager/panel_assigned/scrolllist_assigned").GetComponent<UIScrollList>();
                UIScrollList killedCrew = complex.transform.Find("CrewPanels/panel_enlisted/panelManager/panel_kia/scrolllist_kia").GetComponent<UIScrollList>();
                available = new StockRoster(availableCrew);
                assigned = new StockRoster(assignedCrew);
                killed = new StockRoster(killedCrew);

                // Set up button list:
                SortButtonDef[] buttons = new SortButtonDef[]{ StandardButtonDefs.ByName,
                    StandardButtonDefs.ByClass, StandardButtonDefs.ByLevel, StandardButtonDefs.ByGender
                };

                // Initialize the sort bar:
                sortBar = gameObject.AddComponent<SortingButtons>();
                sortBar.SetRoster(available);
                sortBar.SetButtons(buttons);
                sortBar.SetDefaultOrdering(StandardKerbalComparers.DefaultAvailable);
                sortBar.enabled = false;
                curPanel = CrewPanel.Available;


                // Assign enable listeners to the rosters:
                EnableListener listener = availableCrew.gameObject.AddComponent<EnableListener>();
                listener.Panel = CrewPanel.Available;
                listener.Callback = OnTabSwitch;
                listener.SkipFirstTime = true;

                listener = assignedCrew.gameObject.AddComponent<EnableListener>();
                listener.Panel = CrewPanel.Assigned;
                listener.Callback = OnTabSwitch;
                listener.SkipFirstTime = true;

                listener = killedCrew.gameObject.AddComponent<EnableListener>();
                listener.Panel = CrewPanel.Killed;
                listener.Callback = OnTabSwitch;
                listener.SkipFirstTime = true;
            }
            catch( Exception e ) {
                Debug.Log("KerbalSorter: Unexpected error in AstronautComplex Hook: " + e);
            }
        }


        protected void OnACSpawn() {
            // Set position:
            Transform targetTabTrans = complex.transform.Find("CrewPanels/panel_enlisted/tabs/tab_kia");
            BTPanelTab targetTab = targetTabTrans.GetComponent<BTPanelTab>();
            Vector3 screenPos = Utilities.GetPosition(targetTabTrans);
            float x = screenPos.x + targetTab.width + 5;
            float y = screenPos.y - 1;
            sortBar.SetPos(x, y);

            sortBar.enabled = true;
        }

        protected void OnACDespawn() {
            sortBar.enabled = false;
        }

        protected void OnHire(ProtoCrewMember kerbal, int numActiveKerbals) {
            sortBar.SortRoster(true);
        }

        /*protected void OnFire(ProtoCrewMember kerbal, int numActiveKerbals) {
            Debug.Log("KerbalSorter: OnFire called: "+ kerbal.name + " - " + something);
        }*/

        protected void OnTabSwitch(CrewPanel panel) {
            if( this.curPanel == panel ) {
                return;
            }
            this.curPanel = panel;

            StockRoster roster = null;
            KerbalComparer defaultOrder = null;
            switch( panel ) {
                case CrewPanel.Available:
                    roster = this.available;
                    defaultOrder = StandardKerbalComparers.DefaultAvailable;
                    break;
                case CrewPanel.Assigned:
                    roster = this.assigned;
                    defaultOrder = StandardKerbalComparers.DefaultAssigned;
                    break;
                case CrewPanel.Killed:
                    roster = this.killed;
                    defaultOrder = StandardKerbalComparers.DefaultKilled;
                    break;
            }
            sortBar.SetRoster(roster);
            sortBar.SetDefaultOrdering(defaultOrder);
        }

        protected void OnDestroy() {
            GameEvents.onGUIAstronautComplexSpawn.Remove(OnACSpawn);
            GameEvents.onGUIAstronautComplexDespawn.Remove(OnACDespawn);
            GameEvents.OnCrewmemberHired.Remove(OnHire);
            //GameEvents.OnCrewmemberSacked.Remove(OnFire);
        }


        protected enum CrewPanel {
            Available,
            Assigned,
            Killed
        }

        // Use: Assign as component to component you want to listen to.
        protected class EnableListener : MonoBehaviour {
            public CrewPanel Panel;
            public Action<CrewPanel> Callback;
            public bool SkipFirstTime = false;
            private bool _doneFirstTime = false;
            protected void OnEnable() {
                if( SkipFirstTime && !_doneFirstTime ) {
                    _doneFirstTime = true;
                    return;
                }
                Callback(Panel);
            }
        }
    }

    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class AstronautComplexHook_EditorFix : AstronautComplexHook {
    }
}

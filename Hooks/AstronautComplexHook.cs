using KSP;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace KerbalSorter.Hooks {
    /// <summary>
    /// The main hook for the Astronaut Complex. Started up whenever the Space Centre is loaded.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class AstronautComplexHook : MonoBehaviour {
        CMAstronautComplex complex;
        SortBar sortBar;
        StockRoster available;
        StockRoster assigned;
        StockRoster killed;
        CrewPanel curPanel;

        /// <summary>
        /// Set up the SortBar for the Astronaut Complex. (Callback)
        /// </summary>
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
                sortBar = gameObject.AddComponent<SortBar>();
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
                Debug.LogError("KerbalSorter: Unexpected error in AstronautComplexHook: " + e);
            }
        }


        /// <summary>
        /// Set the Sort Bar position and enable it on Astronaut Complex spawn. (Callback)
        /// </summary>
        protected void OnACSpawn() {
            try {
                // Set position:
                Transform targetTabTrans = complex.transform.Find("CrewPanels/panel_enlisted/tabs/tab_kia");
                BTPanelTab targetTab = targetTabTrans.GetComponent<BTPanelTab>();
                Vector3 screenPos = Utilities.GetPosition(targetTabTrans);
                float x = screenPos.x + targetTab.width + 5;
                float y = screenPos.y - 1;
                sortBar.SetPos(x, y);

                sortBar.enabled = true;
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in AstronautComplexHook: " + e);
            }
        }

        /// <summary>
        /// Disable the Sort Bar on Astronaut Complex despawn. (Callback)
        /// </summary>
        protected void OnACDespawn() {
            sortBar.enabled = false;
        }

        /// <summary>
        /// Resort the lists when a new kerbal is hired. (Callback)
        /// </summary>
        /// <param name="kerbal">The kerbal just hired</param>
        /// <param name="numActiveKerbals">The new number of active kerbals</param>
        protected void OnHire(ProtoCrewMember kerbal, int numActiveKerbals) {
            try {
                sortBar.SortRoster(true);
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in AstronautComplexHook: " + e);
            }
        }

        /*protected void OnFire(ProtoCrewMember kerbal, int numActiveKerbals) {
            Debug.Log("KerbalSorter: OnFire called: "+ kerbal.name + " - " + something);
        }*/

        /// <summary>
        /// Switch the list that the Sort Bar operates with on tab change. (Callback)
        /// </summary>
        /// <param name="panel">The new panel</param>
        protected void OnTabSwitch(CrewPanel panel) {
            try {
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
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in AstronautComplexHook: " + e);
            }
        }

        /// <summary>
        /// Remove GameEvent hooks when this hook is unloaded. (Callback)
        /// </summary>
        protected void OnDestroy() {
            try {
                GameEvents.onGUIAstronautComplexSpawn.Remove(OnACSpawn);
                GameEvents.onGUIAstronautComplexDespawn.Remove(OnACDespawn);
                GameEvents.OnCrewmemberHired.Remove(OnHire);
                //GameEvents.OnCrewmemberSacked.Remove(OnFire);
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in AstronautComplexHook: " + e);
            }
        }


        /// <summary>
        /// The panels in the Astronaut Complex's tab control.
        /// </summary>
        protected enum CrewPanel {
            Available,
            Assigned,
            Killed
        }


        /// <summary>
        /// A hook into any component's OnEnable event.
        /// </summary>
        /// Use: Assign, as a component, to the component you want to listen to.
        protected class EnableListener : MonoBehaviour {
            public CrewPanel Panel;
            public Action<CrewPanel> Callback;
            public bool SkipFirstTime = false;
            private bool _doneFirstTime = false;
            protected void OnEnable() {
                try {
                    if( SkipFirstTime && !_doneFirstTime ) {
                        _doneFirstTime = true;
                        return;
                    }
                    Callback(Panel);
                }
                catch( Exception e ) {
                    Debug.LogError("KerbalSorter: Unexpected error in AstronautComplexHook: " + e);
                }
            }
        }
    }

    /// <summary>
    /// A hook for the Astronaut Complex accessed through the editors.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class AstronautComplexHook_EditorFix : AstronautComplexHook {
    }
}

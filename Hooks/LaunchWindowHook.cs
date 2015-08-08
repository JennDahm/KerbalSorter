using KSP;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

using StockList = KerbalSorter.Hooks.Utilities.StockList;

namespace KerbalSorter.Hooks {
    /// <summary>
    /// A hook for the Launch Windows. Started up whenever the Space Centre is loaded.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class LaunchWindowHook : MonoBehaviour {
        SortBar sortBar;
        UIScrollList availableCrew;
        UIScrollList vesselCrew;
        bool launchScreenUp = false;
        bool sortBarDisabled = false;

        bool onUpdate_ReSort = false;
        bool onUpdate_ApplyInputListeners = false;

        /// <summary>
        /// Set up the Sort Bar for the Launch Windows. (Callback)
        /// </summary>
        protected void Start() {
            try {
                GameEvents.onGUILaunchScreenSpawn.Add(LaunchScreenSpawn);
                GameEvents.onGUILaunchScreenDespawn.Add(LaunchScreenDespawn);
                GameEvents.onGUILaunchScreenVesselSelected.Add(VesselSelect);
                // We actually do need these:
                GameEvents.onGUIAstronautComplexSpawn.Add(OnACSpawn);
                GameEvents.onGUIAstronautComplexDespawn.Add(OnACDespawn);

                // Get the Roster and the vessel crew list:
                VesselSpawnDialog window = UIManager.instance.gameObject.GetComponentsInChildren<VesselSpawnDialog>(true).FirstOrDefault();
                if( window == null ) {
                    throw new Exception("Could not find Launch Window!");
                }
                UIScrollList[] lists = window.GetComponentsInChildren<UIScrollList>(true);
                availableCrew = null;
                vesselCrew = null;
                foreach( UIScrollList list in lists ) {
                    if( list.name == "scrolllist_avail" ) {
                        availableCrew = list;
                        if( vesselCrew != null ) {
                            break;
                        }
                    }
                    else if( list.name == "scrolllist_crew" ) {
                        vesselCrew = list;
                        if( availableCrew != null ) {
                            break;
                        }
                    }
                }
                if( availableCrew == null ) {
                    throw new Exception("Could not find Available Crew List!");
                }
                if( vesselCrew == null ) {
                    throw new Exception("Could not find Vessel Crew List!");
                }
                StockRoster available = new StockRoster(availableCrew);

                // Get sort bar definition:
                SortBarDef bar = ButtonAndBarLoader.SortBarDefs[Utilities.GetListName(StockList.CrewAssign)];

                // Initialize the sort bar:
                sortBar = gameObject.AddComponent<SortBar>();
                sortBar.SetDefinition(bar);
                sortBar.SetSortDelegate(available.Sort);
                sortBar.StateChanged += SortBarStateChanged;
                sortBar.enabled = false;
                sortBarDisabled = available == null
                               || bar.buttons == null
                               || bar.buttons.Length == 0;


                // Set up some hooks to detect when the list is changing:
                availableCrew.AddValueChangedDelegate(OnAvailListValueChanged);
                Transform anchorButtons = window.transform.FindChild("anchor/vesselInfo/crewAssignment/crewAssignmentSpawnpoint/anchorButtons");
                BTButton btn = anchorButtons.FindChild("button_reset").GetComponent<BTButton>();
                btn.AddValueChangedDelegate(OnResetBtn);
                btn = anchorButtons.FindChild("button_clear").GetComponent<BTButton>();
                btn.AddValueChangedDelegate(OnClearBtn);
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in LaunchWindowHook: " + e);
            }
        }

        /// <summary>
        /// Remove GameEvent hooks when this hook is unloaded. (Callback)
        /// </summary>
        protected void OnDestroy() {
            try {
                GameEvents.onGUILaunchScreenSpawn.Remove(LaunchScreenSpawn);
                GameEvents.onGUILaunchScreenDespawn.Remove(LaunchScreenDespawn);
                GameEvents.onGUILaunchScreenVesselSelected.Remove(VesselSelect);
                GameEvents.onGUIAstronautComplexSpawn.Remove(OnACSpawn);
                GameEvents.onGUIAstronautComplexDespawn.Remove(OnACDespawn);
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in LaunchWindowHook: " + e);
            }
        }

        /// <summary>
        /// Save the Sort Bar's state whenever it changes. (Callback)
        /// </summary>
        /// <param name="bar">The Sort Bar</param>
        /// <param name="newState">The new state of the Sort Bar</param>
        protected void SortBarStateChanged(SortBar bar, SortBarState newState) {
            try {
                KerbalSorterStates.SetSortBarState(Utilities.GetListName(StockList.CrewAssign), newState);
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in LaunchWindowHook: " + e);
            }
        }


        // ====================================================================
        //  Game Event Hooks
        // ====================================================================

        /// <summary>
        /// Set the Sort Bar position and enable it on Launch Window spawn. (Callback)
        /// </summary>
        /// <param name="blah">?</param>
        protected void LaunchScreenSpawn(GameEvents.VesselSpawnInfo blah) {
            try {
                Transform tab_crewavail = availableCrew.transform.parent.Find("tab_crewavail");
                BTButton tab = tab_crewavail.GetComponent<BTButton>();

                // Set position:
                Vector3 tabPos = Utilities.GetPosition(tab_crewavail);
                float x = tabPos.x + tab.width + 5;
                float y = tabPos.y - 1;
                sortBar.SetPos(x, y);

                string name = Utilities.GetListName(StockList.CrewAssign);
                if( KerbalSorterStates.IsSortBarStateStored(name) ) {
                    sortBar.SetState(KerbalSorterStates.GetSortBarState(name));
                }

                sortBar.enabled = !sortBarDisabled;
                launchScreenUp = true;
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in LaunchWindowHook: " + e);
            }
        }

        /// <summary>
        /// Disable the Sort Bar on Astronaut Complex despawn. (Callback)
        /// </summary>
        protected void LaunchScreenDespawn() {
            sortBar.enabled = false;
            launchScreenUp = false;
        }

        /// <summary>
        /// Fix the default vessel crew listing when a vessel is selected. (Callback)
        /// </summary>
        /// This is called every time a vessel is selected, including when the
        /// Launch Window opens.
        /// This is called after the list has been entirely rewritten, and
        /// kerbals have already been (temporarily) assigned to the ship.
        /// <param name="ship">The vessel selected</param>
        protected void VesselSelect(ShipTemplate ship) {
            try {
                if( !sortBarDisabled ) {
                    Utilities.AddInputDelegateToKerbals(vesselCrew, OnKerbalMouseInput);
                    Utilities.AddInputDelegateToKerbals(availableCrew, OnKerbalMouseInput);
                    sortBar.SortRoster();
                }
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in LaunchWindowHook: " + e);
            }
        }

        /// <summary>
        /// Disable the Sort Bar when we jump to the Astronaut Complex. (Callback)
        /// </summary>
        protected void OnACSpawn() {
            sortBar.enabled = false;
        }

        /// <summary>
        /// Re-enable the Sort Bar when we leave the Astronaut Complex. (Callback)
        /// </summary>
        protected void OnACDespawn() {
            // When we come out of the AC, we may or may not be on the launch screen.
            if( launchScreenUp && !sortBarDisabled ) {
                sortBar.enabled = true;
                // We need to reapply the input listeners, but we can't do it here.
                onUpdate_ApplyInputListeners = true;
            }
        }


        // ====================================================================
        //  Other UI Hooks
        // ====================================================================

        /// <summary>
        /// Fix the default vessel crew listing when the Reset button is pressed. (Callback)
        /// </summary>
        /// This is called after the list has been entirely rewritten, and
        /// kerbals have already been (temporarily) assigned to the ship. Here,
        /// we assign input delegates to each of the entries so that we can
        /// detect dragging.
        /// <param name="btn">The reset button</param>
        protected void OnResetBtn(IUIObject btn) {
            try {
                if( !sortBarDisabled ) {
                    Utilities.AddInputDelegateToKerbals(vesselCrew, OnKerbalMouseInput);
                    Utilities.AddInputDelegateToKerbals(availableCrew, OnKerbalMouseInput);
                    sortBar.SortRoster();
                }
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in LaunchWindowHook: " + e);
            }
        }

        /// <summary>
        /// Re-sort the available list when we clear the vessel crew. (Callback)
        /// </summary>
        /// This is called after the vessel crew list has been emptied after
        /// pressing the Clear button.
        /// <param name="btn"></param>
        protected void OnClearBtn(IUIObject btn) {
            try {
                if( !sortBarDisabled ) {
                    sortBar.SortRoster();
                }
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in LaunchWindowHook: " + e);
            }
        }

        /// <summary>
        /// Re-sort the list whenever it changes. (Callback)
        /// </summary>
        /// This is called when we click on a kerbal in the list, or when
        /// the red X next to a kerbal in the vessel crew is clicked.
        /// It is, unfortunately, not called when a kerbal is dragged into,
        /// out of, or within the list. We need other detection methods for that.
        /// <param name="obj"></param>
        protected void OnAvailListValueChanged(IUIObject obj) {
            try {
                if( !sortBarDisabled ) {
                    sortBar.SortRoster();
                }
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in LaunchWindowHook: " + e);
            }
        }


        /// <summary>
        /// Catch when we finish dragging a kerbal's entry in a list.
        /// </summary>
        /// We have no other way of detecting dragging, so we have to do this,
        /// in combination with a later re-sort in the Update hook since the
        /// mouse input comes before the kerbal is placed.
        /// <param name="ptr">Information about the mouse input</param>
        protected void OnKerbalMouseInput(ref POINTER_INFO ptr) {
            // Catch when the mouse finishes clicking/dragging.
            if( ptr.evt == POINTER_INFO.INPUT_EVENT.RELEASE_OFF ) {
                // We can't re-sort here, so we have to signal a change for later.
                onUpdate_ReSort = true;
            }
        }

        /// <summary>
        /// Re-sort the list if we just finished dragging a kerbal.
        /// </summary>
        /// This complements the OnKerbalMouseInput hook, allowing us to re-sort
        /// the list of available kerbals whenever we drag them. I haven't found
        /// a better way to do that, so here we are.
        protected void Update() {
            try {
                if( onUpdate_ReSort ) {
                    onUpdate_ReSort = false;
                    sortBar.SortRoster();
                }
                if( onUpdate_ApplyInputListeners ) {
                    onUpdate_ApplyInputListeners = false;
                    Utilities.AddInputDelegateToKerbals(vesselCrew, OnKerbalMouseInput);
                    Utilities.AddInputDelegateToKerbals(availableCrew, OnKerbalMouseInput);
                }
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in LaunchWindowHook: " + e);
            }
        }
    }
}

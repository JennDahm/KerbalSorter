using KSP;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

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

                // Set up button list:
                SortButtonDef[] buttons = new SortButtonDef[]{
                    StandardButtonDefs.ByName, StandardButtonDefs.ByClass,
                    StandardButtonDefs.ByLevel, StandardButtonDefs.ByGender,
                    StandardButtonDefs.ByNumFlights
                };

                // Initialize the sort bar:
                sortBar = gameObject.AddComponent<SortBar>();
                sortBar.SetRoster(available);
                sortBar.SetButtons(buttons);
                sortBar.SetDefaultOrdering(StandardKerbalComparers.DefaultAvailable);
                sortBar.enabled = false;


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

                sortBar.enabled = true;
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
        /// This is called after the list has been entirely rewritten, and
        /// kerbals have already been (temporarily) assigned to the ship.
        /// <param name="ship">The vessel selected</param>
        protected void VesselSelect(ShipTemplate ship) {
            try {
                Utilities.FixDefaultVesselCrew(vesselCrew, availableCrew, sortBar);
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
            sortBar.enabled = launchScreenUp;
        }


        // ====================================================================
        //  Other UI Hooks
        // ====================================================================

        /// <summary>
        /// Fix the default vessel crew listing when the Reset button is pressed. (Callback)
        /// </summary>
        /// This is called after the list has been entirely rewritten, and
        /// kerbals have already been (temporarily) assigned to the ship.
        /// <param name="btn"></param>
        protected void OnResetBtn(IUIObject btn) {
            try {
                Utilities.FixDefaultVesselCrew(vesselCrew, availableCrew, sortBar);
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
                sortBar.SortRoster();
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
        /// out of, or within the list. The only way to detect that is to
        /// put an InputListener on each of those items, and that doesn't
        /// seem to give us a hook *after* the kerbal has been placed into
        /// the list, which means ATM we're SOL on really detecting drags.
        /// <param name="obj"></param>
        protected void OnAvailListValueChanged(IUIObject obj) {
            try {
                sortBar.SortRoster();
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in LaunchWindowHook: " + e);
            }
        }


        // Experimental:
        /*POINTER_INFO.INPUT_EVENT lastInputEvt = POINTER_INFO.INPUT_EVENT.NO_CHANGE;
        protected void OnInput(ref POINTER_INFO ptr) {
            if( ptr.evt != lastInputEvt ){
                Debug.Log("KerbalSorter: OnInput -- " + ptr.evt);
                lastInputEvt = ptr.evt;
            }
            // Catch when the mouse finishes clicking/dragging.
            if( ptr.evt == POINTER_INFO.INPUT_EVENT.RELEASE_OFF ){
                Debug.Log("KerbalSorter: Logged drag.");
                sortBar.SortRoster();
            }
        }*/
    }
}

using KSP;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace KerbalSorter.Hooks {
    /// <summary>
    /// A hook for the Editors. Started up whenever an Editor is loaded.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class EditorHook : MonoBehaviour {
        SortBar sortBar;
        UIScrollList availableCrew;
        UIScrollList vesselCrew;
        bool fixDefaultAssignment = false;
        bool loadBtnPressed = false;
        bool noParts = true;
        bool sortBarDisabled = false;
        bool resortAfterDrag = false;

        /// <summary>
        /// Set up the SortBar for the Editors' crew assignment panel. (Callback)
        /// </summary>
        protected void Start() {
            try {
                // Game Event Hooks
                GameEvents.onEditorScreenChange.Add(OnEditorScreenChange);
                GameEvents.onEditorLoad.Add(OnEditorLoad);
                GameEvents.onEditorRestart.Add(OnEditorRestart);
                GameEvents.onEditorShipModified.Add(OnEditorShipModified);
                // We actually do need these:
                GameEvents.onGUIAstronautComplexSpawn.Add(OnACSpawn);
                GameEvents.onGUIAstronautComplexDespawn.Add(OnACDespawn);


                // Get Roster:
                UIScrollList[] lists = UIManager.instance.gameObject.GetComponentsInChildren<UIScrollList>(true);
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


                // Get position: (This is probably the one time we actually want to do this here)
                Transform tab_crewavail = availableCrew.transform.parent.Find("tab_crewavail");
                BTButton tab = tab_crewavail.GetComponent<BTButton>();
                Vector3 tabPos = Utilities.GetPosition(tab_crewavail);
                float x = tabPos.x + tab.width + 5;
                float y = tabPos.y - 1;

                // Set up button list:
                SortButtonDef[] buttons = ButtonAndBarLoader.SortBarDefs["CrewAssign"];

                // Initialize the sort bar:
                sortBar = gameObject.AddComponent<SortBar>();
                sortBar.SetRoster(available);
                sortBar.SetButtons(buttons);
                sortBar.SetDefaultOrdering(StandardKerbalComparers.DefaultAvailable);
                sortBar.SetPos(x, y);
                sortBar.enabled = false;
                sortBarDisabled = available == null
                               || buttons == null
                               || buttons.Length == 0;

                // Create a fly-in animation for the sort bar.
                baseX = x;
                baseY = y;
                float animBeginTime = 0.2f;
                animEndTime = 0.5f;
                anim = AnimationCurve.Linear(animBeginTime, -575f, animEndTime, 0f);

                // This is what I would have *liked* to have done, but Unity decided this should do absolutely nothing.
                /*AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 10f, 100f);
                AnimationClip clip = new AnimationClip();
                clip.SetCurve("", typeof(Transform), "position.x", curve);
                sortBar.gameObject.AddComponent<Animation>().AddClip(clip, "flyin");*/


                // Add some extra hooks:
                availableCrew.AddValueChangedDelegate(OnAvailListValueChanged);
                Transform trans = UIManager.instance.transform.FindChild("panel_CrewAssignmentInEditor");
                foreach( BTButton btn in trans.GetComponentsInChildren<BTButton>() ) {
                    if( btn.name == "button_reset" ) {
                        btn.AddValueChangedDelegate(OnResetBtn);
                    }
                    else if( btn.name == "button_clear" ) {
                        btn.AddValueChangedDelegate(OnClearBtn);
                    }
                }
                trans = UIManager.instance.transform.FindChild("TopRightAnchor");
                foreach( UIButton btn in trans.GetComponentsInChildren<UIButton>() ) {
                    if( btn.name == "ButtonLoad" ) {
                        btn.AddValueChangedDelegate(OnLoadBtn);
                    }
                }

                fixDefaultAssignment = false;
                loadBtnPressed = false;
                noParts = true;
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in EditorHook: " + e);
            }
        }

        /// <summary>
        /// Remove GameEvent hooks when this hook is unloaded. (Callback)
        /// </summary>
        protected void OnDestroy() {
            try {
                GameEvents.onEditorScreenChange.Remove(OnEditorScreenChange);
                GameEvents.onEditorLoad.Remove(OnEditorLoad);
                GameEvents.onEditorRestart.Remove(OnEditorRestart);
                GameEvents.onEditorShipModified.Remove(OnEditorShipModified);
                GameEvents.onGUIAstronautComplexSpawn.Remove(OnACSpawn);
                GameEvents.onGUIAstronautComplexDespawn.Remove(OnACDespawn);
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in EditorHook: " + e);
            }
        }


        // ====================================================================
        //  Game Event Hooks
        // ====================================================================
        
        /// <summary>
        /// Enable the sort bar when we switch to the Crew AssignmentPanel; disable it when we switch out. (Callback)
        /// </summary>
        /// <param name="screen">The screen we've just switched to</param>
        protected void OnEditorScreenChange(EditorScreen screen) {
            try {
                if( screen == EditorScreen.Crew && !sortBarDisabled ) {
                    sortBar.enabled = true;
                    playAnimation = true;
                    animProgress = 0f;
                    // To ensure a smooth transition:
                    sortBar.SetPos(baseX + anim.Evaluate(0f), baseY);

                    //sortBar.gameObject.GetComponent<Animation>().Play("flyin");

                    if( fixDefaultAssignment ) {
                        Utilities.AddInputDelegateToKerbals(vesselCrew, OnKerbalMouseInput);
                        Utilities.AddInputDelegateToKerbals(availableCrew, OnKerbalMouseInput);
                        sortBar.SortRoster();
                        fixDefaultAssignment = false;
                    }
                }
                else {
                    sortBar.enabled = false;
                }
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in EditorHook: " + e);
            }
        }

        /// <summary>
        /// Set up to fix default crew assignment when user loads ship. (Callback)
        /// </summary>
        /// <param name="ship">The ship just loaded</param>
        /// <param name="type">How the ship was inserted</param>
        protected void OnEditorLoad(ShipConstruct ship, CraftBrowser.LoadType type) {
            try {
                if( type == CraftBrowser.LoadType.Normal && loadBtnPressed ) {
                    // Unfortunately, the crew roster isn't set up yet here, so we
                    // have to delay fixing the default assignment until either the
                    // user opens the crew assignment tab, or they launch the craft.
                    fixDefaultAssignment = true;
                    loadBtnPressed = false;
                }
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in EditorHook: " + e);
            }
        }

        /// <summary>
        /// Note when the editor restarts. (Callback)
        /// </summary>
        protected void OnEditorRestart() {
            noParts = true;
        }

        /// <summary>
        /// Note whenever we place a first part/remove the last part. (Callback)
        /// </summary>
        /// <param name="ship">The ship that was modified</param>
        protected void OnEditorShipModified(ShipConstruct ship) {
            if( ship == null ) {
                Debug.LogError("KerbalSorter: Expected non-null ShipConstruct in OnEditorShipModified.");
                return;
            }
            if( ship.Count == 0 ) {
                noParts = true;
            }
            else if( noParts ) {
                // If they didn't have parts before, they might have placed a
                // command capsule, which would be auto-filled. We'll need to
                // correct this.
                noParts = false;
                fixDefaultAssignment = true;
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
            // When we come out of the AC, we'll be in the Crew Select screen.
            sortBar.enabled = !sortBarDisabled;
        }


        // ====================================================================
        //  Other UI Hooks
        // ====================================================================

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
                Debug.LogError("KerbalSorter: Unexpected error in EditorHook: " + e);
            }
        }

        /// <summary>
        /// Note whenever the Load button is pressed. (Callback)
        /// </summary>
        /// We need this to detect when a craft is being explicitly loaded
        /// by the user, rather than automatically loaded on entrance.
        /// <param name="btn">The Load button</param>
        protected void OnLoadBtn(IUIObject btn) {
            try {
                loadBtnPressed = true;
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in EditorHook: " + e);
            }
        }

        /// <summary>
        /// Fix the default vessel crew listing when the Reset button is pressed. (Callback)
        /// </summary>
        /// This is called after the list has been entirely rewritten, and
        /// kerbals have already been (temporarily) assigned to the ship.
        /// <param name="btn"></param>
        protected void OnResetBtn(IUIObject btn) {
            try {
                if( !sortBarDisabled ) {
                    Utilities.AddInputDelegateToKerbals(vesselCrew, OnKerbalMouseInput);
                    Utilities.AddInputDelegateToKerbals(availableCrew, OnKerbalMouseInput);
                    sortBar.SortRoster(); 
                }
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in EditorHook: " + e);
            }
        }

        /// <summary>
        /// Re-sort the list whenever it changes. (Callback)
        /// </summary>
        /// This is called when we click on a kerbal in the list, or when
        /// the red X next to a kerbal in the vessel crew is clicked.
        /// It is, unfortunately, not called when a kerbal is dragged into,
        /// out of, or within the list. We need other detection methods for that.
        /// <param name="obj">?</param>
        protected void OnAvailListValueChanged(IUIObject obj) {
            try {
                if( !sortBarDisabled ) {
                    sortBar.SortRoster(); 
                }
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in EditorHook: " + e);
            }
        }

        /// <summary>
        /// Catch when we finish dragging a kerbal's entry in a list. (Callback)
        /// </summary>
        /// We have no other way of detecting dragging, so we have to do this,
        /// in combination with a later re-sort in the Update hook since the
        /// mouse input comes before the kerbal is placed.
        /// <param name="ptr">Information about the mouse input</param>
        protected void OnKerbalMouseInput(ref POINTER_INFO ptr) {
            // Catch when the mouse finishes clicking/dragging.
            if( ptr.evt == POINTER_INFO.INPUT_EVENT.RELEASE_OFF ) {
                // We can't re-sort here, so we have to signal a change for later.
                resortAfterDrag = true;
            }
        }

        /// <summary>
        /// Re-sort the list if we just finished dragging a kerbal. Animate if necessary. (Callback)
        /// </summary>
        /// This complements the OnKerbalMouseInput hook, allowing us to re-sort
        /// the list of available kerbals whenever we drag them. I haven't found
        /// a better way to do that, so here we are.
        /// This also animates the sort bar as it flies in with the crew select panel.
        protected void Update() {
            try {
                Animate();
                if( resortAfterDrag ) {
                    resortAfterDrag = false;
                    sortBar.SortRoster();
                }
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in EditorHook: " + e);
            }
        }


        // ====================================================================
        //  Animation handling
        // ====================================================================
        // Unity doesn't want to work with us on
        // animation, so we have to do it manually.
        AnimationCurve anim;
        bool playAnimation = false;
        float animProgress = 0f;
        float animEndTime;
        float baseX;
        float baseY;

        /// <summary>
        /// Update the animation if it's playing.
        /// </summary>
        protected void Animate() {
            try {
                if( playAnimation ) {
                    if( animProgress > animEndTime ) {
                        playAnimation = false;
                    }
                    float x = baseX + anim.Evaluate(animProgress);
                    sortBar.SetPos(x, baseY);
                    animProgress += Time.deltaTime;
                }
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in EditorHook: " + e);
            }
        }
    }
}

using KSP;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace KerbalSorter.Hooks {
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class EditorHook : MonoBehaviour {
        SortBar sortBar;
        UIScrollList availableCrew;
        UIScrollList vesselCrew;
        bool fixDefaultAssignment = false;
        bool loadBtnPressed = false;
        bool noParts = true;

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
                SortButtonDef[] buttons = new SortButtonDef[]{ StandardButtonDefs.ByName,
                    StandardButtonDefs.ByClass, StandardButtonDefs.ByLevel, StandardButtonDefs.ByGender
                };

                // Initialize the sort bar:
                sortBar = gameObject.AddComponent<SortBar>();
                sortBar.SetRoster(available);
                sortBar.SetButtons(buttons);
                sortBar.SetDefaultOrdering(StandardKerbalComparers.DefaultAvailable);
                sortBar.SetPos(x, y);
                sortBar.enabled = false;

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
                Debug.LogError("KerbalSorter: Unexpected error in Editor Hook! " + e);
            }
        }

        protected void OnDestroy() {
            GameEvents.onEditorScreenChange.Remove(OnEditorScreenChange);
            GameEvents.onEditorLoad.Remove(OnEditorLoad);
            GameEvents.onEditorRestart.Remove(OnEditorRestart);
            GameEvents.onEditorShipModified.Remove(OnEditorShipModified);
            GameEvents.onGUIAstronautComplexSpawn.Remove(OnACSpawn);
            GameEvents.onGUIAstronautComplexDespawn.Remove(OnACDespawn);
        }


        // Game Event Hooks:
        protected void OnEditorScreenChange(EditorScreen screen) {
            if( screen == EditorScreen.Crew ) {
                sortBar.enabled = true;
                playAnimation = true;
                animProgress = 0f;
                // To ensure a smooth transition:
                sortBar.SetPos(baseX + anim.Evaluate(0f), baseY);

                //sortBar.gameObject.GetComponent<Animation>().Play("flyin");

                if( fixDefaultAssignment ) {
                    Utilities.FixDefaultVesselCrew(vesselCrew, availableCrew, sortBar);
                    fixDefaultAssignment = false;
                }
            }
            else {
                sortBar.enabled = false;
            }
        }

        protected void OnEditorLoad(ShipConstruct ship, CraftBrowser.LoadType type) {
            if( type == CraftBrowser.LoadType.Normal && loadBtnPressed ) {
                // Unfortunately, the crew roster isn't set up yet here, so we
                // have to delay fixing the default assignment until either the
                // user opens the crew assignment tab, or they launch the craft.
                fixDefaultAssignment = true;
                loadBtnPressed = false;
            }
        }

        protected void OnEditorRestart() {
            noParts = true;
        }

        protected void OnEditorShipModified(ShipConstruct ship) {
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

        protected void OnACSpawn() {
            sortBar.enabled = false;
        }

        protected void OnACDespawn() {
            // When we come out of the AC, we'll be in the Crew Select screen.
            sortBar.enabled = true;
        }


        // Other UI Hooks:
        protected void OnClearBtn(IUIObject btn) {
            // At this point, the vessel crew list has been emptied back into
            // the list of available crew.
            sortBar.SortRoster();
        }

        protected void OnLoadBtn(IUIObject btn) {
            // We need this to detect when a craft is being explicitly loaded
            // by the user, rather than automatically loaded on entrance.
            loadBtnPressed = true;
        }

        protected void OnResetBtn(IUIObject btn) {
            // At this point, the list has been entirely rewritten, and kerbals
            // have already been (temporarily) assigned to the ship.
            Utilities.FixDefaultVesselCrew(vesselCrew, availableCrew, sortBar);
        }

        protected void OnAvailListValueChanged(IUIObject obj) {
            // This is called when we click on a kerbal in the list, or when
            // the red X next to a kerbal in the vessel crew is clicked.
            // It is, unfortunately, not called when a kerbal is dragged into,
            // out of, or within the list. The only way to detect that is to
            // put an InputListener on each of those items, and that doesn't
            // seem to give us a hook *after* the kerbal has been placed into
            // the list, which means ATM we're SOL on really detecting drags.
            sortBar.SortRoster();
        }



        // ------- Animation handling -------
        // Unity doesn't want to work with us on
        // animation, so we have to do it manually.
        AnimationCurve anim;
        bool playAnimation = false;
        float animProgress = 0f;
        float animEndTime;
        float baseX;
        float baseY;

        protected void Update() {
            if( playAnimation ) {
                if( animProgress > animEndTime ) {
                    playAnimation = false;
                }
                float x = baseX + anim.Evaluate(animProgress);
                sortBar.SetPos(x, baseY);
                animProgress += Time.deltaTime;
            }
        }
    }
}

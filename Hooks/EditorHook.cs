using KSP;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace KerbalSorter.Hooks
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class EditorHook : MonoBehaviour {
        SortingButtons sortBar;
        
        protected void Start() {
            try{
                GameEvents.onEditorScreenChange.Add(OnEditorScreenChange);
                // We actually do need these:
                GameEvents.onGUIAstronautComplexSpawn.Add(OnACSpawn);
                GameEvents.onGUIAstronautComplexDespawn.Add(OnACDespawn);

                // Get Roster:
                UIScrollList[] lists = UIManager.instance.gameObject.GetComponentsInChildren<UIScrollList>(true);
                UIScrollList availableCrew = null;
                foreach( UIScrollList list in lists ){
                    if (list.name == "scrolllist_avail")
                    {
                        availableCrew = list;
                        break;
                    }
                }
                if( availableCrew == null ){
                    throw new Exception("Could not find Available Crew List!");
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
                sortBar = gameObject.AddComponent<SortingButtons>();
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
            }
            catch (Exception e){
                Debug.LogError("KerbalSorter: Unexpected error in Editor Hook! " + e);
            }
        }

        protected void OnDestroy() {
            GameEvents.onEditorScreenChange.Remove(OnEditorScreenChange);
            GameEvents.onGUIAstronautComplexSpawn.Remove(OnACSpawn);
            GameEvents.onGUIAstronautComplexDespawn.Remove(OnACDespawn);
        }

        protected void OnEditorScreenChange(EditorScreen screen) {
            if( screen == EditorScreen.Crew ){
                sortBar.enabled = true;
                playAnimation = true;
                animProgress = 0f;
                // To ensure a smooth transition:
                sortBar.SetPos(baseX + anim.Evaluate(0f), baseY);

                //sortBar.gameObject.GetComponent<Animation>().Play("flyin");
            } else {
                sortBar.enabled = false;
            }
        }

        protected void OnACSpawn(){
            sortBar.enabled = false;
        }
        protected void OnACDespawn()
        {
            // When we come out of the AC, we'll be in the Crew Select screen.
            sortBar.enabled = true;
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
            if( playAnimation ){
                if( animProgress > animEndTime ){
                    playAnimation = false;
                }
                float x = baseX + anim.Evaluate(animProgress);
                sortBar.SetPos(x, baseY);
                animProgress += Time.deltaTime;
            }
        }
    }
}

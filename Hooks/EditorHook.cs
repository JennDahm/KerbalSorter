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
                sortBar.SetPos(x, y);
                sortBar.enabled = false;
            }
            catch (Exception e){
                Debug.LogError("KerbalSorter: Unexpected error in Editor Hook! " + e);
            }
        }

        protected void OnDestroy() {
            GameEvents.onEditorScreenChange.Remove(OnEditorScreenChange);
        }

        protected void OnEditorScreenChange(EditorScreen screen) {
            if( screen == EditorScreen.Crew ){
                sortBar.enabled = true;
            } else {
                sortBar.enabled = false;
            }
        }
    }
}

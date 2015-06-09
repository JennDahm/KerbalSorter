using KSP;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace KerbalSorter.Hooks
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class AstronautComplexHook : MonoBehaviour {
        CMAstronautComplex complex;
        SortingButtons sortBar;
        Roster<IUIListObject> available;
        Roster<IUIListObject> assigned;
        Roster<IUIListObject> killed;

        protected void Start() {
            try{
                // Set up hooks:
                GameEvents.onGUIAstronautComplexSpawn.Add(OnACSpawn);
                GameEvents.onGUIAstronautComplexDespawn.Add(OnACDespawn);

                // Get rosters:
                complex = UIManager.instance.gameObject.GetComponentsInChildren<CMAstronautComplex>(true).FirstOrDefault();
                if (complex == null) throw new Exception("Could not find astronaut complex");
                //UIScrollList availableCrew = complex.transform.Find("CrewPanels/panel_enlisted/panelManager/panel_available/scrolllist_available").GetComponent<UIScrollList>();
                //UIScrollList assignedCrew = complex.transform.Find("CrewPanels/panel_enlisted/panelManager/panel_assigned/scrolllist_assigned").GetComponent<UIScrollList>();
                //UIScrollList killedCrew = complex.transform.Find("CrewPanels/panel_enlisted/panelManager/panel_kia/scrolllist_kia").GetComponent<UIScrollList>();

                // Set up button list:
                SortButtonDef[] buttons = new SortButtonDef[]{ StandardButtonDefs.ByName,
                    StandardButtonDefs.ByClass, StandardButtonDefs.ByLevel, StandardButtonDefs.ByGender
                };

                // Initialize the sort bar:
                sortBar = gameObject.AddComponent<SortingButtons>();
                //sortBar.SetRoster(available);
                sortBar.SetButtons(buttons);
                sortBar.enabled = false;
            } catch( Exception e ){
                Debug.Log("KerbalSorter: Unexpected error in AstronautComplex Hook: " + e);
            }
        }


        protected void OnACSpawn() {
            // Set position:
            Transform targetTabTrans = complex.transform.Find("CrewPanels/panel_enlisted/tabs/tab_kia");
            BTPanelTab targetTab = targetTabTrans.GetComponent<BTPanelTab>();
            Vector3 screenPos = Utilities.GetPosition(targetTabTrans);
            float x = screenPos.x+targetTab.width+5;
            float y = screenPos.y - 1;
            sortBar.SetPos(x,y);

            sortBar.enabled = true;
        }

        protected void OnACDespawn() {
            sortBar.enabled = false;
        }

        protected void OnDestroy() {
            GameEvents.onGUIAstronautComplexSpawn.Remove(OnACSpawn);
            GameEvents.onGUIAstronautComplexDespawn.Remove(OnACDespawn);
        }
    }

    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class AstronautComplexHook_EditorFix : AstronautComplexHook {
    }
}

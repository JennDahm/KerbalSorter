using KSP;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace KerbalSorter.Hooks
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class LaunchWindowHook : MonoBehaviour {
        SortingButtons sortBar;
        UIScrollList availableCrew;
        protected void Start() {
            try{
                GameEvents.onGUILaunchScreenSpawn.Add(LaunchScreenSpawn);
                GameEvents.onGUILaunchScreenDespawn.Add(LaunchScreenDespawn);
                //GameEvents.onGUILaunchScreenVesselSelected.Add(VesselSelect);

                // Get the Roster:
                VesselSpawnDialog window = UIManager.instance.gameObject.GetComponentsInChildren<VesselSpawnDialog>(true).FirstOrDefault();
                if( window == null ){
                    throw new Exception("Could not find Launch Window!");
                }
                UIScrollList[] lists = window.GetComponentsInChildren<UIScrollList>(true);
                availableCrew = null;
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
            } catch( Exception e ){
                Debug.LogError("KerbalSorter: Unexpected error in LaunchWindow Hook! " + e);
            }
        }

        protected void OnDestroy() {
            GameEvents.onGUILaunchScreenSpawn.Remove(LaunchScreenSpawn);
            GameEvents.onGUILaunchScreenDespawn.Remove(LaunchScreenDespawn);
            //GameEvents.onGUILaunchScreenVesselSelected.Remove(VesselSelect);
        }


        protected void LaunchScreenSpawn(GameEvents.VesselSpawnInfo blah) {
            Transform tab_crewavail = availableCrew.transform.parent.Find("tab_crewavail");
            BTButton tab = tab_crewavail.GetComponent<BTButton>();
            
            // Set position:
            Vector3 tabPos = Utilities.GetPosition(tab_crewavail);
            float x = tabPos.x + tab.width + 5;
            float y = tabPos.y - 1;
            sortBar.SetPos(x, y);

            sortBar.enabled = true;
        }

        protected void LaunchScreenDespawn() {
            //Debug.Log("KerbalSorter: Launch Screen Despawned!");
            sortBar.enabled = false;
        }
    }
}

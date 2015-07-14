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
        SortBar sortBarCrew;
        SortBar sortBarApplicants;
        StockRoster available;
        StockRoster assigned;
        StockRoster killed;
        StockRoster applicants;
        CrewPanel curPanel;

        bool KSILoaded = false;

        /// <summary>
        /// Set up the SortBars for the Astronaut Complex. (Callback)
        /// </summary>
        protected void Start() {
            try {
                // Set up hooks:
                GameEvents.onGUIAstronautComplexSpawn.Add(OnACSpawn);
                GameEvents.onGUIAstronautComplexDespawn.Add(OnACDespawn);
                GameEvents.OnCrewmemberHired.Add(OnHire);
                GameEvents.OnCrewmemberSacked.Add(OnFire);

                // Get rosters:
                complex = UIManager.instance.gameObject.GetComponentsInChildren<CMAstronautComplex>(true).FirstOrDefault();
                if( complex == null ) throw new Exception("Could not find astronaut complex");
                UIScrollList availableList = complex.transform.Find("CrewPanels/panel_enlisted/panelManager/panel_available/scrolllist_available").GetComponent<UIScrollList>();
                UIScrollList assignedList = complex.transform.Find("CrewPanels/panel_enlisted/panelManager/panel_assigned/scrolllist_assigned").GetComponent<UIScrollList>();
                UIScrollList killedList = complex.transform.Find("CrewPanels/panel_enlisted/panelManager/panel_kia/scrolllist_kia").GetComponent<UIScrollList>();
                UIScrollList applicantList = complex.transform.Find("CrewPanels/panel_applicants/scrolllist_applicants").GetComponent<UIScrollList>();
                available = new StockRoster(availableList);
                assigned = new StockRoster(assignedList);
                killed = new StockRoster(killedList);
                applicants = new StockRoster(applicantList);

                // Set up button list:
                SortButtonDef[] buttonsCrew = new SortButtonDef[]{ StandardButtonDefs.ByName,
                    StandardButtonDefs.ByClass, StandardButtonDefs.ByLevel, StandardButtonDefs.ByGender
                };
                SortButtonDef[] buttonsApplicants = new SortButtonDef[]{
                    StandardButtonDefs.ByName, StandardButtonDefs.ByClass, StandardButtonDefs.ByGender
                };

                // Initialize the crew sort bar:
                sortBarCrew = gameObject.AddComponent<SortBar>();
                sortBarCrew.SetRoster(available);
                sortBarCrew.SetButtons(buttonsCrew);
                sortBarCrew.SetDefaultOrdering(StandardKerbalComparers.DefaultAvailable);
                sortBarCrew.enabled = false;
                curPanel = CrewPanel.Available;

                /// Initialize the applicant sort bar:
                sortBarApplicants = gameObject.AddComponent<SortBar>();
                sortBarApplicants.SetRoster(applicants);
                sortBarApplicants.SetButtons(buttonsApplicants);
                sortBarApplicants.SetDefaultOrdering(StandardKerbalComparers.DefaultApplicant);
                sortBarApplicants.enabled = false;


                // Assign enable listeners to the rosters:
                Utilities.AddOnEnableListener(availableList.gameObject, OnTabAvailable, true);
                Utilities.AddOnEnableListener(assignedList.gameObject, OnTabAssigned, true);
                Utilities.AddOnEnableListener(killedList.gameObject, OnTabKilled, true);

                // There's no other way to detect KSI's presence, unfortunately. :/
                foreach( AssemblyLoader.LoadedAssembly asm in AssemblyLoader.loadedAssemblies ){
                    if( asm.dllName == "KSI" ){
                        KSILoaded = true;
                    }
                }
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in AstronautComplexHook: " + e);
            }
        }


        /// <summary>
        /// Set the SortBars' position and enable them on Astronaut Complex spawn. (Callback)
        /// </summary>
        protected void OnACSpawn() {
            try {
                // Set position:
                Transform targetTabTrans = complex.transform.Find("CrewPanels/panel_enlisted/tabs/tab_kia");
                BTPanelTab targetTab = targetTabTrans.GetComponent<BTPanelTab>();
                Vector3 screenPos = Utilities.GetPosition(targetTabTrans);
                float x = screenPos.x + targetTab.width + 5;
                float y = screenPos.y - 1;
                sortBarCrew.SetPos(x, y);
                sortBarCrew.enabled = true;

                if( !KSILoaded ) {
                    targetTabTrans = complex.transform.Find("CrewPanels/panel_applicants/tab_crew");
                    BTButton targetTab2 = targetTabTrans.GetComponent<BTButton>(); // Because consistancy is not their strong suit.
                    screenPos = Utilities.GetPosition(targetTabTrans);
                    x = screenPos.x + targetTab2.width + 5;
                    y = screenPos.y - 1;
                    sortBarApplicants.SetPos(x, y);
                    sortBarApplicants.enabled = true; 
                }
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in AstronautComplexHook: " + e);
            }
        }

        /// <summary>
        /// Disable the SortBars on Astronaut Complex despawn. (Callback)
        /// </summary>
        protected void OnACDespawn() {
            sortBarCrew.enabled = false;
            sortBarApplicants.enabled = false;
        }

        /// <summary>
        /// Re-sort the crew lists when a new kerbal is hired. (Callback)
        /// </summary>
        /// <param name="kerbal">The kerbal just hired</param>
        /// <param name="numActiveKerbals">The new number of active kerbals</param>
        protected void OnHire(ProtoCrewMember kerbal, int numActiveKerbals) {
            try {
                sortBarCrew.SortRoster(true);
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in AstronautComplexHook: " + e);
            }
        }

        /// <summary>
        /// Re-sort the applicant list when a kerbal is fired.
        /// </summary>
        /// <param name="kerbal">The kerbal just fired</param>
        /// <param name="numActiveKerbals">The new number of active kerbals</param>
        protected void OnFire(ProtoCrewMember kerbal, int numActiveKerbals) {
            try {
                sortBarApplicants.SortRoster(true);
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in AstronautComplexHook: " + e);
            }
        }

        /// <summary>
        /// Switch the list that the crew Sort Bar operates with on tab change. (Callback)
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
                sortBarCrew.SetRoster(roster);
                sortBarCrew.SetDefaultOrdering(defaultOrder);
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in AstronautComplexHook: " + e);
            }
        }

        /// <summary>
        /// Switch the crew Sort Bar to operate on the Available list. (Callback)
        /// </summary>
        protected void OnTabAvailable() {
            OnTabSwitch(CrewPanel.Available);
        }

        /// <summary>
        /// Switch the crew Sort Bar to operate on the Assigned list. (Callback)
        /// </summary>
        protected void OnTabAssigned() {
            OnTabSwitch(CrewPanel.Assigned);
        }

        /// <summary>
        /// Switch the crew Sort Bar to operate on the Killed list. (Callback)
        /// </summary>
        protected void OnTabKilled() {
            OnTabSwitch(CrewPanel.Killed);
        }

        /// <summary>
        /// Remove GameEvent hooks when this hook is unloaded. (Callback)
        /// </summary>
        protected void OnDestroy() {
            try {
                GameEvents.onGUIAstronautComplexSpawn.Remove(OnACSpawn);
                GameEvents.onGUIAstronautComplexDespawn.Remove(OnACDespawn);
                GameEvents.OnCrewmemberHired.Remove(OnHire);
                GameEvents.OnCrewmemberSacked.Remove(OnFire);
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
    }

    /// <summary>
    /// A hook for the Astronaut Complex accessed through the editors.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class AstronautComplexHook_EditorFix : AstronautComplexHook {
    }
}

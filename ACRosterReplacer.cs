using System;
using KSP;
using UnityEngine;
using System.Collections.Generic;

namespace KerbalSorter
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class SpaceCentreHook : MonoBehaviour
    {
        private void Start() { gameObject.AddComponent<ACRosterReplacer>(); }
    }

    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class EditorHook : SpaceCentreHook
    {
    }

    //[KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class ACRosterReplacer : MonoBehaviour
    {
        static CMAstronautComplex complex;
        bool sorted = false;

        private void Start()
        {
            try
            {
                complex = UIManager.instance.gameObject.GetComponentsInChildren<CMAstronautComplex>(true)[0];
                if (complex == null) throw new Exception("Could not find astronaut complex");
                GameEvents.onGUIAstronautComplexSpawn.Add(OnACSpawn);
                GameEvents.onGUIAstronautComplexDespawn.Add(OnACDespawn);
                enabled = false;
                //Debug.Log("KerbalSorter: Start method finished");
            }
            catch (Exception e)
            {
                Debug.LogError("KerbalSorter: Encountered unhandled exception: " + e);
                Destroy(this);
            }
        }

        void OnDestroy()
        {
            GameEvents.onGUIAstronautComplexSpawn.Remove(OnACSpawn);
            GameEvents.onGUIAstronautComplexDespawn.Remove(OnACDespawn);
        }

        private void OnACSpawn(){
            enabled = true;
            sorted = false;
        }
        private void OnACDespawn(){
            enabled = false;
            sorted = false;
        }

        private void OnGUI(){
            if( !sorted ){
                try{
                    UIScrollList availableCrew = complex.transform.Find("CrewPanels/panel_enlisted/panelManager/panel_available/scrolllist_available").GetComponent<UIScrollList>();
                    KerbalComparer[] comparisons = new KerbalComparer[2];
                    comparisons[0] = StandardKerbalComparers.NameAscending;
                    comparisons[1] = StandardKerbalComparers.ProfessionAscending;
                    CrewSorter sorter = new CrewSorter(comparisons);
                    sorter.SortRoster(availableCrew);
                    sorted = true;
                    /*Debug.Log("KerbalSorter: Sorted Available Crew");
                    Debug.Log("New Order:");
                    for(int i = 0; i < availableCrew.Count; i++ ){
                        IUIListObject entry = availableCrew.GetItem(i);
                        if( entry != null ){
                            ProtoCrewMember kerbal = availableCrew.GetItem(i).gameObject.GetComponent<CrewItemContainer>().GetCrewRef();
                            if( kerbal != null ){
                                Debug.Log((i+1) + ". " + kerbal.name + ": " + kerbal.experienceTrait.Title);
                            } else {
                                Debug.Log((i+1) + ". NULL");
                            }
                        } else {
                            Debug.Log((i+1) + ". NULL");
                        }
                    }*/
                } catch( Exception e ){
                    Debug.LogError("KerbalSorter: Unhandled exception occured while sorting: " + e);
                    sorted = true;
                }
            }
        }
    }
    
    public class CrewSorter{
        private List<KerbalComparer> comparisons;

        public CrewSorter(){
            this.comparisons = new List<KerbalComparer>();
        }
        public CrewSorter(KerbalComparer[] comparisons){
            this.comparisons = new List<KerbalComparer>(comparisons);
        }

        public void Reset(){
            comparisons.Clear();
        }

        public void AppendComparison(KerbalComparer comparison){
            comparisons.Add(comparison);
        }

        public void SortRoster(UIScrollList roster){
            //Retrieve and temporarily store the list of kerbals
            IUIListObject[] sortedRoster = new IUIListObject[roster.Count];
            for( int i = 0; i < roster.Count; i++ ){
                sortedRoster[i] = roster.GetItem(i);
            }
            //Debug.Log("KerbalSorter: Finished copying from old roster.");

            //Run through each comparison:
            for( int a = 0; a < comparisons.Count; a++ ){
                var compare = comparisons[a];

                //Insertion sort, since it's stable and we don't have a large roster:
                for( int i = 1; i < sortedRoster.Length; i++ ){
                    IUIListObject kerbal = sortedRoster[i];
                    int k = i;
                    while( 0 < k  &&  compare(GetKerbal(kerbal),GetKerbal(sortedRoster[k-1])) < 0 ){
                        sortedRoster[k] = sortedRoster[k-1];
                        k--;
                    }
                    sortedRoster[k] = kerbal;
                }

                //Debug.Log("KerbalSorter: Finished Comparison " + (a+1));
            }

            //Apply the new order to the roster:
            roster.ClearList(false);
            //Debug.Log("KerbalSorter: Finished removing from old roster.");
            for( int i = 0; i < sortedRoster.Length; i++ ){
                if( sortedRoster[i] == null ){
                    throw new NullReferenceException("Crew Member " + (i+1) + " went missing during sorting!");
                }
                roster.InsertItem(sortedRoster[i], i);
                //Debug.Log("Added Crew Member " + GetKerbal(sortedRoster[i]).name + ": " + GetKerbal(sortedRoster[i]).experienceTrait.Title);
            }
            //Debug.Log("KerbalSorter: Finished creating new roster.");
        }

        private ProtoCrewMember GetKerbal(IUIListObject entry){
            return entry.gameObject.GetComponent<CrewItemContainer>().GetCrewRef();
        }
    }

    public delegate int KerbalComparer(ProtoCrewMember a, ProtoCrewMember b);
    public class StandardKerbalComparers{
        static public int NameAscending(ProtoCrewMember a, ProtoCrewMember b){
            return a.name.CompareTo(b.name);
        }
        static public int NameDescending(ProtoCrewMember a, ProtoCrewMember b){
            return -NameAscending(a,b);
        }

        static public int ProfessionAscending(ProtoCrewMember a, ProtoCrewMember b){
            return a.experienceTrait.Title.CompareTo(b.experienceTrait.Title);
        }
        static public int ProfessionDescending(ProtoCrewMember a, ProtoCrewMember b){
            return -ProfessionAscending(a,b);
        }

        static public int LevelAscending(ProtoCrewMember a, ProtoCrewMember b){
            return a.experienceLevel - b.experienceLevel;
        }
        static public int LevelDescending(ProtoCrewMember a, ProtoCrewMember b){
            return -LevelAscending(a,b);
        }

        static public int GenderAscending(ProtoCrewMember a, ProtoCrewMember b){
            return a.gender - b.gender;
        }
        static public int GenderDescending(ProtoCrewMember a, ProtoCrewMember b){
            return -GenderAscending(a,b);
        }

        static public int NumFlightsAscending(ProtoCrewMember a, ProtoCrewMember b){
            return a.flightLog.Entries.Count - b.flightLog.Entries.Count;
        }
        static public int NumFlightsDescending(ProtoCrewMember a, ProtoCrewMember b){
            return -NumFlightsAscending(a,b);
        }
    }
}

using KSP;
using System;
using UnityEngine;
using System.Linq;
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
        bool guiSetup = false;
        GUIStyle buttonStyle;

        private void Start()
        {
            try
            {
                complex = UIManager.instance.gameObject.GetComponentsInChildren<CMAstronautComplex>(true).FirstOrDefault();
                if (complex == null) throw new Exception("Could not find astronaut complex");
                GameEvents.onGUIAstronautComplexSpawn.Add(OnACSpawn);
                GameEvents.onGUIAstronautComplexDespawn.Add(OnACDespawn);
                enabled = false;

                if( !guiSetup ){
                    buttonStyle = new GUIStyle(HighLogic.Skin.button);
                    buttonStyle.padding = new RectOffset(4,4,4,4);
                    guiSetup = true;
                }
                //Debug.Log("KerbalSorter: Start method finished");
            }
            catch (Exception e)
            {
                Debug.LogError("KerbalSorter: Encountered unhandled exception: " + e);
                Destroy(this);
            }
        }

        void OnDestroy(){
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
            /*if( !sorted ){
                try{
                    UIScrollList availableCrew = complex.transform.Find("CrewPanels/panel_enlisted/panelManager/panel_available/scrolllist_available").GetComponent<UIScrollList>();
                    KerbalComparer[] comparisons = new KerbalComparer[2];
                    comparisons[0] = StandardKerbalComparers.NameAscending;
                    comparisons[1] = StandardKerbalComparers.ProfessionAscending;
                    CrewSorterOld sorter = new CrewSorterOld(comparisons);
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
                    }*//*
                } catch( Exception e ){
                    Debug.LogError("KerbalSorter: Unhandled exception occured while sorting: " + e);
                    sorted = true;
                }
            }

            Transform targetTabTrans = complex.transform.Find("CrewPanels/panel_enlisted/tabs/tab_kia");
            BTPanelTab targetTab = targetTabTrans.GetComponent<BTPanelTab>();
            var uiCams = UIManager.instance.uiCameras;
            EZCameraSettings uiCam = null;
            for( int i = 0; i < uiCams.Length; i++ ){
                if( (uiCams[i].mask & (1 << targetTabTrans.gameObject.layer)) != 0 ){
                    uiCam = uiCams[i];
                    break;
                }
            }
            Vector3 screenPos = uiCam.camera.WorldToScreenPoint(targetTabTrans.position);
            //GUI.Button(new Rect(1284,195,25,25),
            GUI.Button(new Rect(screenPos.x+targetTab.width+5, Screen.height-screenPos.y-1,   25,25),
                    (Texture)GameDatabase.Instance.GetTexture("KerbalSorter/Images/SortBtnOut", false),
                    buttonStyle);*/
        }
    }

    public class CrewSorterOld
    {
        private List<KerbalComparer> comparisons;

        public CrewSorterOld()
        {
            this.comparisons = new List<KerbalComparer>();
        }
        public CrewSorterOld(KerbalComparer[] comparisons)
        {
            this.comparisons = new List<KerbalComparer>(comparisons);
        }

        public void Reset()
        {
            comparisons.Clear();
        }

        public void AppendComparison(KerbalComparer comparison)
        {
            comparisons.Add(comparison);
        }

        public void SortRoster(UIScrollList roster)
        {
            //Retrieve and temporarily store the list of kerbals
            IUIListObject[] sortedRoster = new IUIListObject[roster.Count];
            for (int i = 0; i < roster.Count; i++)
            {
                sortedRoster[i] = roster.GetItem(i);
            }
            //Debug.Log("KerbalSorter: Finished copying from old roster.");

            //Run through each comparison:
            for (int a = 0; a < comparisons.Count; a++)
            {
                var compare = comparisons[a];

                //Insertion sort, since it's stable and we don't have a large roster:
                for (int i = 1; i < sortedRoster.Length; i++)
                {
                    IUIListObject kerbal = sortedRoster[i];
                    int k = i;
                    while (0 < k && compare(GetKerbal(kerbal), GetKerbal(sortedRoster[k - 1])) < 0)
                    {
                        sortedRoster[k] = sortedRoster[k - 1];
                        k--;
                    }
                    sortedRoster[k] = kerbal;
                }

                //Debug.Log("KerbalSorter: Finished Comparison " + (a+1));
            }

            //Apply the new order to the roster:
            roster.ClearList(false);
            //Debug.Log("KerbalSorter: Finished removing from old roster.");
            for (int i = 0; i < sortedRoster.Length; i++)
            {
                if (sortedRoster[i] == null)
                {
                    throw new NullReferenceException("Crew Member " + (i + 1) + " went missing during sorting!");
                }
                roster.InsertItem(sortedRoster[i], i);
                //Debug.Log("Added Crew Member " + GetKerbal(sortedRoster[i]).name + ": " + GetKerbal(sortedRoster[i]).experienceTrait.Title);
            }
            //Debug.Log("KerbalSorter: Finished creating new roster.");
        }

        private ProtoCrewMember GetKerbal(IUIListObject entry)
        {
            return entry.gameObject.GetComponent<CrewItemContainer>().GetCrewRef();
        }
    }
}

using KSP;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace KerbalSorter
{
    // Unity apparently doesn't like generics, which would be very useful here. Way to go, Unity.
    public class SortingButtons : MonoBehaviour {
        private Roster<IUIListObject> roster = null;
        private SortButtonDef[] buttons = new SortButtonDef[0];
        private int[] buttonStates = new int[0];
        float x = 0;
        float y = 0;

        GUIStyle buttonStyle;
        bool skinSetup = false;
        bool expanded = false;

        List<int> buttonSelectOrder = new List<int>();
        bool sorted = false;

        KerbalComparer defaultOrder = null;

        // Apparently we can't use constructors in Unity, so we'll have to deal with it this way:
        public void SetRoster(Roster<IUIListObject> roster) {
            this.roster = roster;
            this.sorted = false;
        }
        public void SetButtons(SortButtonDef[] buttons) {
            this.buttons = buttons;
            this.buttonStates = new int[buttons.Length];
            this.buttonSelectOrder.Clear();
        }
        public void SetPos(float x, float y) {
            this.x = x;
            this.y = y;
        }
        public void SetDefaultOrdering(KerbalComparer comp){
            defaultOrder = comp;
        }


        protected void OnEnable() {
            sorted = false;
        }
        protected void OnDisable() {
            expanded = false;
        }


        protected void OnGUI() {
            if( !skinSetup ){
                buttonStyle = new GUIStyle(HighLogic.Skin.button);
                buttonStyle.padding = new RectOffset(4,4,4,4);
                skinSetup = true;
            }

            Texture buttonIcon = GameDatabase.Instance.GetTexture("KerbalSorter/Images/" + (expanded?"SortBtnIn":"SortBtnOut"), false);
            string hoverText = "Sorting Options";
            bool masterPressed = GUI.Button(new Rect(x,y , 25,25), new GUIContent(buttonIcon, hoverText), buttonStyle);

            // Draw the sorting buttons.
            if( expanded ){
                float nextX = x+25;

                for( int i = 0; i < buttons.Length; i++ ){
                    buttonIcon = GameDatabase.Instance.GetTexture(buttons[i].iconLocs[buttonStates[i]], false);
                    hoverText = buttons[i].hoverText[buttonStates[i]];
                    bool pressed = GUI.Button(new Rect(nextX,y , 25,25), new GUIContent(buttonIcon,hoverText), buttonStyle);
                    if( pressed ){
                        buttonStates[i] = (buttonStates[i]+1) % buttons[i].numStates;
                        buttonSelectOrder.Remove(i);
                        if( buttonStates[i] != 0 ){
                            buttonSelectOrder.Add(i);
                        }
                        sorted = false;
                    }
                    nextX += 25;
                }
            }

            if( masterPressed ){
                expanded = !expanded;
            }

            // Do sorting:
            SortRoster(false);
        }

        public void SortRoster(bool force = true) {
            if( roster != null && (!sorted || force) ){
                int count = buttonSelectOrder.Count;
                int off = 0;
                if( defaultOrder != null ) {
                    count++;
                    off++;
                }
                KerbalComparer[] comparisons = new KerbalComparer[count];
                if( defaultOrder != null ){
                    comparisons[0] = defaultOrder;
                }
                for( int i = 0; i < buttonSelectOrder.Count; i++ ){
                    int bIdx = buttonSelectOrder[i];
                    comparisons[i+off] = buttons[bIdx].comparers[buttonStates[bIdx]];
                }
                CrewSorter<IUIListObject>.SortRoster(roster, comparisons);
                sorted = true;
            }
        }
    }

    public static class CrewSorter<T> {
        public static void SortRoster(Roster<T> roster, KerbalComparer[] comparisons) {
            //Retrieve and temporarily store the list of kerbals
            T[] sortedRoster = new T[roster.Count];
            for (int i = 0; i < roster.Count; i++) {
                sortedRoster[i] = roster.GetItem(i);
            }

            //Run through each comparison:
            for (int a = 0; a < comparisons.Length; a++) {
                var compare = comparisons[a];

                //Insertion sort, since it's stable and we don't have a large roster:
                for (int i = 1; i < sortedRoster.Length; i++) {
                    T kerbal = sortedRoster[i];
                    int k = i;
                    while (0 < k && compare(roster.GetKerbal(kerbal), roster.GetKerbal(sortedRoster[k - 1])) < 0) {
                        sortedRoster[k] = sortedRoster[k - 1];
                        k--;
                    }
                    sortedRoster[k] = kerbal;
                }
            }

            //Apply the new order to the roster:
            while( roster.Count > 0 ){
                roster.RemoveItem(0);
            }
            for (int i = 0; i < sortedRoster.Length; i++) {
                roster.InsertItem(sortedRoster[i], i);
            }
        }
    }
}

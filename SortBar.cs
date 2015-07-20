using KSP;
using System;
using UnityEngine;
using System.Collections.Generic;

// Consider using Tooltips.TooltipController
namespace KerbalSorter {
    // Unity apparently doesn't like generics, which would be very useful here. Way to go, Unity.

    /// <summary>
    /// Class representing the KerbalSorter Sort Bar.
    /// </summary>
    public class SortBar : MonoBehaviour {
        /// <summary>
        /// The Roster that we're sorting.
        /// </summary>
        private Roster<IUIListObject> roster = null;
        /// <summary>
        /// The buttons to display.
        /// </summary>
        private SortButtonDef[] buttons = new SortButtonDef[0];
        /// <summary>
        /// Each of the buttons' states.
        /// </summary>
        private int[] buttonStates = new int[0];
        /// <summary>
        /// X position on the screen.
        /// </summary>
        float x = 0;
        /// <summary>
        /// Y position on the screen.
        /// </summary>
        float y = 0;

        /// <summary>
        /// The style of the displayed buttons.
        /// </summary>
        GUIStyle buttonStyle;
        /// <summary>
        /// The style of the displayed tooltip.
        /// </summary>
        GUIStyle tooltipStyle;
        /// <summary>
        /// Is the style set up yet?
        /// </summary>
        bool skinSetup = false;
        /// <summary>
        /// Is the bar expanded?
        /// </summary>
        bool expanded = false;

        /// <summary>
        /// The order in which the user selected the buttons.
        /// </summary>
        List<int> buttonSelectOrder = new List<int>();
        /// <summary>
        /// Have we already sorted the roster?
        /// </summary>
        bool sorted = false;

        /// <summary>
        /// The base order of the roster.
        /// </summary>
        /// If null, no base order is set.
        KerbalComparer defaultOrder = null;

        // Apparently we can't use constructors in Unity, so we'll have to deal with it this way:
        /// <summary>
        /// Sets the roster that this Sort Bar will sort.
        /// </summary>
        /// <param name="roster">The Roster to sort</param>
        public void SetRoster(Roster<IUIListObject> roster) {
            this.roster = roster;
            this.sorted = false;
        }
        /// <summary>
        /// Sets the buttons to display in this Sort Bar.
        /// </summary>
        /// <param name="buttons">The list of button definitions to use</param>
        public void SetButtons(SortButtonDef[] buttons) {
            this.buttons = buttons;
            this.buttonStates = new int[buttons.Length];
            this.buttonSelectOrder.Clear();
            this.sorted = false;
        }
        /// <summary>
        /// Sets the position of the Sort Bar on the screen.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetPos(float x, float y) {
            this.x = x;
            this.y = y;
        }
        /// <summary>
        /// Sets the base ordering of the Roster.
        /// </summary>
        /// <param name="comp">The function defining the base ordering</param>
        public void SetDefaultOrdering(KerbalComparer comp) {
            defaultOrder = comp;
            this.sorted = false;
        }


        /// <summary>
        /// Called when the enable property is set to true. DO NOT CALL DIRECTLY.
        /// </summary>
        protected void OnEnable() {
            sorted = false;
        }
        /// <summary>
        /// Called when the enable property is set to false. DO NOT CALL DIRECTLY.
        /// </summary>
        protected void OnDisable() {
            expanded = false;
        }


        /// <summary>
        /// Called to update the GUI. DO NOT CALL DIRECTLY.
        /// </summary>
        protected void OnGUI() {
            if( !skinSetup ) {
                buttonStyle = new GUIStyle(HighLogic.Skin.button);
                buttonStyle.padding = new RectOffset(4, 4, 4, 4);
                skinSetup = true;
                tooltipStyle = new GUIStyle(GUI.skin.textField);
                tooltipStyle.padding.top += 2;
                tooltipStyle.padding.bottom += 2;
                tooltipStyle.padding.left += 2;
                tooltipStyle.padding.right += 2;
            }

            Texture buttonIcon = GameDatabase.Instance.GetTexture("KerbalSorter/Images/" + (expanded ? "SortBtnIn" : "SortBtnOut"), false);
            string hoverText = "Sorting Options";
            bool masterPressed = GUI.Button(new Rect(x, y, 25, 25), new GUIContent(buttonIcon, hoverText), buttonStyle);

            // Draw the sorting buttons.
            if( expanded ) {
                float nextX = x + 25;

                for( int i = 0; i < buttons.Length; i++ ) {
                    buttonIcon = GameDatabase.Instance.GetTexture(buttons[i].iconLocs[buttonStates[i]], false);
                    hoverText = buttons[i].hoverText[buttonStates[i]];
                    bool pressed = GUI.Button(new Rect(nextX, y, 25, 25), new GUIContent(buttonIcon, hoverText), buttonStyle);
                    if( pressed ) {
                        buttonStates[i] = (buttonStates[i] + 1) % buttons[i].numStates;
                        buttonSelectOrder.Remove(i);
                        if( buttonStates[i] != 0 ) {
                            buttonSelectOrder.Add(i);
                        }
                        sorted = false;
                    }
                    nextX += 25;
                }
            }

            if( masterPressed ) {
                expanded = !expanded;
            }

            if( GUI.tooltip != null && GUI.tooltip != "" ){
                Vector2 size = tooltipStyle.CalcSize(new GUIContent(GUI.tooltip));
                GUI.TextField(new Rect(x, y - size.y, size.x, size.y), GUI.tooltip, tooltipStyle);
            }

            // Do sorting:
            SortRoster(false);
        }

        /// <summary>
        /// Sorts the Roster.
        /// </summary>
        /// <param name="force">Force re-sorting? (Default: true)</param>
        public void SortRoster(bool force = true) {
            if( roster != null && (!sorted || force) ) {
                int count = buttonSelectOrder.Count;
                int off = 0;
                if( defaultOrder != null ) {
                    count++;
                    off++;
                }
                KerbalComparer[] comparisons = new KerbalComparer[count];
                if( defaultOrder != null ) {
                    comparisons[0] = defaultOrder;
                }
                for( int i = 0; i < buttonSelectOrder.Count; i++ ) {
                    int bIdx = buttonSelectOrder[i];
                    comparisons[i + off] = buttons[bIdx].comparers[buttonStates[bIdx]];
                }
                CrewSorter<IUIListObject>.SortRoster(roster, comparisons);
                sorted = true;
            }
        }
    }

    /// <summary>
    /// Static class that actually does the sorting.
    /// </summary>
    /// <typeparam name="T">The type held in the Roster.</typeparam>
    public static class CrewSorter<T> {
        /// <summary>
        /// Sorts the given Roster using the given comparison functions.
        /// </summary>
        /// <param name="roster">The Roster to sort</param>
        /// <param name="comparisons">The Comparisons to use, in order of application</param>
        public static void SortRoster(Roster<T> roster, KerbalComparer[] comparisons) {
            //Retrieve and temporarily store the list of kerbals
            T[] sortedRoster = new T[roster.Count];
            for( int i = 0; i < roster.Count; i++ ) {
                sortedRoster[i] = roster.GetItem(i);
            }

            //Run through each comparison:
            for( int a = 0; a < comparisons.Length; a++ ) {
                var compare = comparisons[a];

                //Insertion sort, since it's stable and we don't have a large roster:
                for( int i = 1; i < sortedRoster.Length; i++ ) {
                    T kerbal = sortedRoster[i];
                    int k = i;
                    while( 0 < k && compare(roster.GetKerbal(kerbal), roster.GetKerbal(sortedRoster[k - 1])) < 0 ) {
                        sortedRoster[k] = sortedRoster[k - 1];
                        k--;
                    }
                    sortedRoster[k] = kerbal;
                }
            }

            //Apply the new order to the roster:
            while( roster.Count > 0 ) {
                roster.RemoveItem(0);
            }
            for( int i = 0; i < sortedRoster.Length; i++ ) {
                roster.InsertItem(sortedRoster[i], i);
            }
        }
    }
}

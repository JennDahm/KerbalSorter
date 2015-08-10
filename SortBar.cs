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
        // =====================================================================
        //  Interface
        // =====================================================================

        /// <summary>
        /// Handles when a SortBar changes its state.
        /// </summary>
        /// <param name="bar">The bar whose state changed</param>
        /// <param name="newState">A copy of the new state</param>
        public delegate void StateChangedHandler(SortBar bar, SortBarState newState);

        /// <summary>
        /// Event fired whenever this SortBar's state changes.
        /// </summary>
        /// Don't fire this directly -- call FireStateChanged().
        public event StateChangedHandler StateChanged;

        /// <summary>
        /// Handles a sorting event.
        /// </summary>
        /// <param name="criteria">The criteria to sort by</param>
        public delegate void SortDelegate(KerbalComparer[] criteria);

        // Apparently we can't use constructors in Unity, so we'll have to deal with it this way:

        /// <summary>
        /// Sets the sort delegate used to sort kerbals.
        /// </summary>
        /// <param name="sorter">The delegate to call in order to sort</param>
        public void SetSortDelegate(SortDelegate sorter) {
            this.sorter = sorter;
            this.sorted = false;
        }

        /// <summary>
        /// Sets the buttons to display in this Sort Bar.
        /// </summary>
        /// <param name="buttons">The list of button definitions to use</param>
        public void SetButtons(SortButtonDef[] buttons) {
            this.def.buttons = buttons;
            this.Reset(false);
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
            this.def.defaultComparison = comp;
            this.sorted = false;
        }

        /// <summary>
        /// Gets this SortBar's definition.
        /// </summary>
        /// <returns></returns>
        public SortBarDef GetDefinition() {
            return this.def;
        }

        /// <summary>
        /// Sets this SortBar's definition, including buttons and default comparison.
        /// </summary>
        /// <param name="def">The SortBarDef object to define ourselves around.</param>
        public void SetDefinition(SortBarDef def) {
            this.def = def;
            this.Reset(false);
        }

        /// <summary>
        /// Gets this SortBar's current state.
        /// </summary>
        /// <returns></returns>
        public SortBarState GetState() {
            return new SortBarState() {
                definitionHash = this.def.GetHashCode(),
                buttonStates = this.buttonStates,
                selectionOrder = this.buttonSelectOrder.ToArray()
            };
        }

        /// <summary>
        /// Sets the SortBar's state. Fires StateChanged event if fireEvent is true.
        /// </summary>
        /// <param name="state">The state to put the SortBar in</param>
        /// <param name="fireEvent">Whether to fire the StateChanged event (default=false)</param>
        /// <exception cref="System.ArgumentException">Thrown if the state isn't compatible with this SortBar's definition.</exception>
        public void SetState(SortBarState state, bool fireEvent = false) {
            if( state.definitionHash != this.def.GetHashCode() ) {
                string error = "The hash the state kept doesn't match this SortBar's definition!";
                throw new ArgumentException(error, "state.definitionHash");
            }
            this.buttonStates = (int[])state.buttonStates.Clone();
            this.buttonSelectOrder = new List<int>(state.selectionOrder);
            this.sorted = false;
            if( fireEvent ) {
                this.FireStateChanged();
            }
        }

        /// <summary>
        /// Resets the button states. Fires StateChanged event if fireEvent is true.
        /// </summary>
        /// <param name="fireEvent">Whether to fire the StateChanged event (default=false)</param>
        public void Reset(bool fireEvent = false) {
            this.buttonStates = new int[this.def.buttons.Length];
            this.buttonSelectOrder.Clear();
            this.sorted = false;
            if( fireEvent ) {
                this.FireStateChanged();
            }
        }

        /// <summary>
        /// Sorts the Roster.
        /// </summary>
        /// <param name="force">Force re-sorting? (Default: true)</param>
        public void SortRoster(bool force = true) {
            if( sorter != null && (!sorted || force) ) {
                int count = buttonSelectOrder.Count;
                int off = 0;
                if( def.defaultComparison != null ) {
                    count++;
                    off++;
                }
                KerbalComparer[] comparisons = new KerbalComparer[count];
                if( def.defaultComparison != null ) {
                    comparisons[0] = def.defaultComparison;
                }
                for( int i = 0; i < buttonSelectOrder.Count; i++ ) {
                    int bIdx = buttonSelectOrder[i];
                    comparisons[i + off] = def.buttons[bIdx].comparers[buttonStates[bIdx]];
                }

                sorter(comparisons);
                sorted = true;
            }
        }


        // =====================================================================
        //  Internals
        // =====================================================================

        /// <summary>
        /// The delegate to call in order to sort.
        /// </summary>
        protected SortDelegate sorter = null;
        /// <summary>
        /// The defining values of the SortBar; its buttons and default order.
        /// </summary>
        protected SortBarDef def;
        /// <summary>
        /// Each of the buttons' states.
        /// </summary>
        protected int[] buttonStates = new int[0];
        /// <summary>
        /// The order in which the user selected the buttons.
        /// </summary>
        protected List<int> buttonSelectOrder = new List<int>();

        /// <summary>
        /// X position on the screen.
        /// </summary>
        protected float x = 0;
        /// <summary>
        /// Y position on the screen.
        /// </summary>
        protected float y = 0;
        /// <summary>
        /// Is the style set up yet?
        /// </summary>
        protected bool skinSetup = false;
        /// <summary>
        /// Is the bar expanded?
        /// </summary>
        protected bool expanded = false;
        /// <summary>
        /// Have we already sorted the roster?
        /// </summary>
        protected bool sorted = false;

        /// <summary>
        /// The style of the displayed buttons.
        /// </summary>
        GUIStyle buttonStyle;
        /// <summary>
        /// The style of the displayed tooltip.
        /// </summary>
        GUIStyle tooltipStyle;


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
        /// Call this to fire the StateChanged event with the appropriate parameters.
        /// </summary>
        virtual protected void FireStateChanged() {
            if( StateChanged != null ) {
                StateChanged(this, this.GetState());
            }
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
                tooltipStyle.padding.top    += 2;
                tooltipStyle.padding.bottom += 2;
                tooltipStyle.padding.left   += 2;
                tooltipStyle.padding.right  += 2;
            }

            Texture buttonIcon = GameDatabase.Instance.GetTexture("KerbalSorter/Images/" + (expanded ? "SortBtnIn" : "SortBtnOut"), false);
            string hoverText = "Sorting Options";
            bool masterPressed = GUI.Button(new Rect(x, y, 25, 25), new GUIContent(buttonIcon, hoverText), buttonStyle);
            bool stateChanged = false;

            // Draw the sorting buttons.
            if( expanded ) {
                float nextX = x + 25;

                for( int i = 0; i < def.buttons.Length; i++ ) {
                    buttonIcon = GameDatabase.Instance.GetTexture(def.buttons[i].iconLocs[buttonStates[i]], false);
                    hoverText = def.buttons[i].hoverText[buttonStates[i]];
                    bool pressed = GUI.Button(new Rect(nextX, y, 25, 25), new GUIContent(buttonIcon, hoverText), buttonStyle);
                    if( pressed ) {
                        buttonStates[i] = (buttonStates[i] + 1) % def.buttons[i].numStates;
                        buttonSelectOrder.Remove(i);
                        if( buttonStates[i] != 0 ) {
                            buttonSelectOrder.Add(i);
                        }
                        sorted = false;
                        stateChanged = true;
                    }
                    nextX += 25;
                }
            }

            if( masterPressed ) {
                expanded = !expanded;
            }

            if( GUI.tooltip != null && GUI.tooltip != "" ) {
                Vector2 size = tooltipStyle.CalcSize(new GUIContent(GUI.tooltip));
                GUI.TextField(new Rect(x, y - size.y, size.x, size.y), GUI.tooltip, tooltipStyle);
            }

            // Do sorting:
            SortRoster(false);

            if( stateChanged ) {
                FireStateChanged();
            }
        }
    }
}

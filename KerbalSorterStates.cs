using KSP;
using System;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace KerbalSorter {

    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.SPACECENTER, GameScenes.TRACKSTATION)]
    class KerbalSorterStates : ScenarioModule {
        // =====================================================================
        //  Interface
        // =====================================================================

        /// <summary>
        /// Handles when a Sort Bar's state is set within this class.
        /// </summary>
        /// <param name="name">The name of the Sort Bar whose state was set</param>
        /// <param name="newState">The new state of the Sort Bar</param>
        public delegate void SortBarStateSetHandler(string name, SortBarState newState);

        /// <summary>
        /// Fires whenever a Sort Bar's state is set within this class.
        /// </summary>
        public static event SortBarStateSetHandler SortBarStateSet;

        /// <summary>
        /// Checks whether a state is stored with the given name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Whether a state exists with the given name</returns>
        public static bool IsSortBarStateStored(string name) {
            return SortBarStates.ContainsKey(name);
        }

        /// <summary>
        /// Retrieves the saved Sort Bar state with the given name.
        /// </summary>
        /// <param name="name">The name of the Sort Bar</param>
        /// <returns></returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if no state exists for a SortBar with the given name.</exception>
        public static SortBarState GetSortBarState(string name) {
            return SortBarStates[name];
        }

        /// <summary>
        /// Stores a Sort Bar's state under the given name.
        /// </summary>
        /// <param name="name">The name of the Sort Bar</param>
        /// <param name="state">The state of the Sort Bar</param>
        public static void SetSortBarState(string name, SortBarState state) {
            SortBarStates[name] = state;
            if( SortBarStateSet != null ) {
                SortBarStateSet(name, state);
            }
        }


        // =====================================================================
        //  Internals
        // =====================================================================

        /// <summary>
        /// All Sort Bar states.
        /// </summary>
        protected static Dictionary<string, SortBarState> SortBarStates;

        /// <summary>
        /// Save all states under the given ConfigNode. (Callback)
        /// </summary>
        /// <param name="node">The ConfigNode to save to</param>
        public override void OnSave(ConfigNode node) {
            foreach( string name in SortBarStates.Keys ) {
                node.AddNode("SORTBAR_STATE", NodifySortState(SortBarStates[name])).AddValue("name", name);
            }
        }

        /// <summary>
        /// Load all states from the given ConfigNode (Callback)
        /// </summary>
        /// <param name="node">The ConfigNode to load from</param>
        public override void OnLoad(ConfigNode node) {
            SortBarStates = new Dictionary<string, SortBarState>();
            foreach( ConfigNode stateNode in node.GetNodes("SORTBAR_STATE") ) {
                string name = stateNode.GetValue("name");
                SortBarStates[name] = ParseSortStateNode(stateNode);
            }
        }

        /// <summary>
        /// Converts a SortBar state to a ConfigNode.
        /// </summary>
        /// <param name="state">The state to convert</param>
        /// <returns>The converted state; null if something went wrong</returns>
        protected static ConfigNode NodifySortState(SortBarState state) {
            try {
                ConfigNode node = new ConfigNode();
                node.AddValue("hash", state.definitionHash);

                string buttonStates = "";
                for( int i = 0; i < state.buttonStates.Length; i++ ) {
                    buttonStates += state.buttonStates[i] + " ";
                }
                node.AddValue("states", buttonStates.Trim());

                string selectOrder = "";
                for( int i = 0; i < state.selectionOrder.Length; i++ ) {
                    selectOrder += state.selectionOrder[i] + " ";
                }
                node.AddValue("order", selectOrder.Trim());

                return node;
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Error in KerbalSorterStates.NodifySortState()");
                Debug.LogException(e);
            }
            return null;
        }

        /// <summary>
        /// Parses a SortBar state from a ConfigNode.
        /// </summary>
        /// <param name="node">The ConfigNode to parse</param>
        /// <returns>The parsed SortBar state</returns>
        protected static SortBarState ParseSortStateNode(ConfigNode node) {
            SortBarState state = new SortBarState();

            state.definitionHash = int.Parse(node.GetValue("hash"));

            string[] buttonStates = node.GetValue("states").Split(' ');
            state.buttonStates = new int[buttonStates.Length];
            for( int i = 0; i < buttonStates.Length; i++ ) {
                state.buttonStates[i] = int.Parse(buttonStates[i]);
            }

            string[] selectOrder = node.GetValue("order").Split(' ');
            state.selectionOrder = new int[selectOrder.Length];
            for( int i = 0; i < selectOrder.Length; i++ ) {
                state.selectionOrder[i] = int.Parse(selectOrder[i]);
            }

            return state;
        }
    }
}

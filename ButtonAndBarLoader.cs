using KSP;
using System;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace KerbalSorter {
    /// <summary>
    /// This class loads the buttons and bars from the ConfigNode files.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class ButtonAndBarLoader : MonoBehaviour {
        // =====================================================================
        //  Interface
        // =====================================================================

        /// <summary>
        /// All loaded sort buttons.
        /// </summary>
        public static Dictionary<string, SortButtonDef> SortButtons { get; protected set; }

        /// <summary>
        /// All loaded sort bar definitions.
        /// </summary>
        public static Dictionary<string, SortBarDef> SortBarDefs { get; protected set; }


        // =====================================================================
        //  Internals
        // =====================================================================

        /// <summary>
        /// Load up all definitions on awakening.
        /// </summary>
        protected void Awake() {
            try {
                SortButtons = new Dictionary<string, SortButtonDef>();
                SortBarDefs = new Dictionary<string, SortBarDef>();

                // Load all buttons
                Debug.Log("KerbalSorter: Loading sort buttons from configuration...");
                ParseSortButtons();

                // Load all bars
                Debug.Log("KerbalSorter: Loading sort bar definitions...");
                ParseSortBars();
            }
            catch( Exception e ) {
                Debug.LogError("KerbalSorter: Unexpected error in ButtonAndBarLoader. ");
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Parses all SORTBUTTON ConfigNodes and saves them in this.SortButtons.
        /// </summary>
        /// All ConfigNodes are accessed after ModuleManager has modified them.
        protected void ParseSortButtons() {
            ConfigNode[] buttonNodes = GameDatabase.Instance.GetConfigNodes("SORTBUTTON");
            foreach( ConfigNode buttonNode in buttonNodes ) {
                string buttonName = null;
                try {
                    // Get the button's name.
                    buttonName = buttonNode.GetValue("name");
                    if( buttonName == null || buttonName.Trim().Equals("") ) {
                        buttonName = null;
                        throw new ArgumentNullException("name", "The name field of a SORTBUTTON exist and must be non-empty.");
                    }
                    // Get the buttons's states.
                    ConfigNode[] stateNodes = buttonNode.GetNodes("STATE");
                    if( stateNodes == null || stateNodes.Length == 0 ) {
                        throw new ArgumentNullException("STATE", "Each SORTBUTTON must have at least one STATE node.");
                    }

                    // Set up the button definition object
                    SortButtonDef buttonDef = new SortButtonDef();
                    buttonDef.numStates = stateNodes.Length;
                    buttonDef.iconLocs  = new string[buttonDef.numStates];
                    buttonDef.hoverText = new string[buttonDef.numStates];
                    buttonDef.comparers = new KerbalComparer[buttonDef.numStates];

                    // Parse each of the states.
                    for( int i = 0; i < stateNodes.Length; i++ ) {
                        // HoverText can be null/empty.
                        buttonDef.hoverText[i] = stateNodes[i].GetValue("hoverText");
                        if( buttonDef.hoverText[i] == null ) {
                            buttonDef.hoverText[i] = "";
                        }
                        // Icon Locations must not be null/empty.
                        buttonDef.iconLocs[i] = stateNodes[i].GetValue("iconLoc");
                        if( buttonDef.iconLocs[i] == null
                         || buttonDef.iconLocs[i].Trim().Equals("") ) {
                            string param = String.Format("iconLoc[{0}]", i);
                            throw new ArgumentNullException(param, "Each STATE node needs an icon!");
                        }
                        // Similarly, the comparer should not be null/empty.
                        // The handling for that, and for strings that can't be
                        // found, is in GetComparer(). We won't fail, we'll just
                        // replace it with StandardKerbalComparers.None.
                        string comparerRaw = stateNodes[i].GetValue("comparer");
                        buttonDef.comparers[i] = GetComparer(comparerRaw);
                    }

                    SortButtons.Add(buttonName, buttonDef);
                    Debug.Log(String.Format("KerbalSorter: Loaded: \"{0}\" with {1} states.", buttonName, stateNodes.Length));
                }
                catch( Exception e ) {
                    string errorMsg = "KerbalSorter: Error parsing sort button";
                    if( buttonName != null ) {
                        errorMsg += String.Format(" \"{0}\"", buttonName);
                    }
                    errorMsg += ".";
                    Debug.LogError(errorMsg);
                    Debug.LogException(e);
                }
            }
        }

        /// <summary>
        /// Parses all SORTBAR ConfigNodes and saves them in this.SortBarDefs.
        /// </summary>
        /// All ConfigNodes are accessed after ModuleManager has modified them.
        protected void ParseSortBars() {
            ConfigNode[] barNodes = GameDatabase.Instance.GetConfigNodes("SORTBAR");
            foreach( ConfigNode barNode in barNodes ) {
                string barName = null;
                try {
                    barName = barNode.GetValue("name");
                    if( barName == null || barName.Trim().Equals("") ) {
                        barName = null;
                        throw new ArgumentNullException("name", "The name field of a SORTBAR must exist and be non-empty.");
                    }

                    ConfigNode[] buttonNodes = barNode.GetNodes("BUTTON");
                    if( buttonNodes == null || buttonNodes.Length == 0 ) {
                        buttonNodes = new ConfigNode[0];
                        Debug.LogWarning(String.Format("KerbalSorter: SORTBAR \"{0}\" has no BUTTONs.", barName));
                    }


                    // The comparer can be null;
                    // the default is StandardKerbalComparers.None
                    string comparerRaw = barNode.GetValue("defaultComparer");
                    KerbalComparer comparer = StandardKerbalComparers.None;
                    if( comparerRaw != null && !comparerRaw.Trim().Equals("") ) {
                        comparer = GetComparer(comparerRaw);
                    }

                    // Parse each button
                    List<SortButtonDef> buttonDefs = new List<SortButtonDef>();
                    for( int i = 0; i < buttonNodes.Length; i++ ) {
                        string buttonName = buttonNodes[i].GetValue("name");
                        if( buttonName == null || buttonName.Trim().Equals("") ) {
                            Debug.LogWarning(String.Format("KerbalSorter: In SORTBAR \"{0}\": BUTTON {1} has no name. Omitting.", barName, i));
                            continue;
                        }
                        if( !SortButtons.ContainsKey(buttonName) ) {
                            Debug.LogWarning(String.Format("KerbalSorter: In SORTBAR \"{0}\": BUTTON {1} doesn't exist. Omitting.", barName, buttonName));
                            continue;
                        }
                        SortButtonDef buttonDef = SortButtons[buttonName];
                        buttonDefs.Add(buttonDef);
                    }

                    SortBarDef def = new SortBarDef() {
                        buttons = buttonDefs.ToArray(),
                        defaultComparison = comparer
                    };
                    SortBarDefs.Add(barName, def);
                    Debug.Log(String.Format("KerbalSorter: Loaded: \"{0}\" with {1} buttons.", barName, buttonDefs.Count));
                }
                catch( Exception e ) {
                    string errorMsg = "KerbalSorter: Error parsing sort bar";
                    if( barName != null ) {
                        errorMsg += String.Format(" \"{0}\"", barName);
                    }
                    errorMsg += ".";
                    Debug.LogError(errorMsg);
                    Debug.LogException(e);
                }
            }
        }

        /// <summary>
        /// Retrieves the KerbalComparer method given.
        /// </summary>
        /// <param name="name">Fully qualified method name</param>
        /// <returns>The KerbalComparer referenced, or StandardKerbalComparers.None if something went wrong</returns>
        protected static KerbalComparer GetComparer(string name) {
            int separatorIdx = name.LastIndexOf('.');
            if( separatorIdx < 0 ) {
                Debug.LogWarning(String.Format("KerbalSorter: \"{0}\" is not valid. You must provide a fully qualified static method name.", name));
            }
            string className  = name.Substring(0, separatorIdx);
            string methodName = name.Substring(separatorIdx + 1);
            if( className == "" ) {
                Debug.LogWarning(String.Format("KerbalSorter: \"{0}\" is not valid. You must provide a class name.", name));
                return StandardKerbalComparers.None;
            }
            if( methodName == "" ) {
                Debug.LogWarning(String.Format("KerbalSorter: \"{0}\" is not valid. You must provide a method name.", name));
                return StandardKerbalComparers.None;
            }

            // Using reflection, attempt to create a delegate.
            Type type = Type.GetType(className, false);
            // This looks absurd, but the version of .NET that KSP uses doesn't
            // have == defined for Type. Most of us compiling this will have a
            // later version of .NET that does, which then confuses KSP because
            // it encounters a reference to a method it doesn't recognize.
            if( ((object)type) == null ) {
                Debug.LogWarning(String.Format("KerbalSorter: Could not find class \"{0}\".", className));
                return StandardKerbalComparers.None;
            }
            MethodInfo method = type.GetMethod(methodName);
            if( ((object)method) == null ) {
                Debug.LogWarning(String.Format("KerbalSorter: Could not find method \"{0}\" within class \"{1}\". If it exists, ensure that it is static.", methodName, className));
                return StandardKerbalComparers.None;
            }

            // This will return null if the method doesn't match.
            KerbalComparer comparer = (KerbalComparer)Delegate.CreateDelegate(typeof(KerbalComparer), method, false);
            if( comparer == null ) {
                Debug.LogWarning(String.Format("KerbalSorter: Method \"{0}\" does not match KerbalComparer Delegate.", name));
                return StandardKerbalComparers.None;
            }
            return comparer;
        }
    }
}

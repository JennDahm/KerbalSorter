using KSP;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace KerbalSorter.Hooks {
    /// <summary>
    /// A static class with utility functions.
    /// </summary>
    static class Utilities {
        /// <summary>
        /// All SortBars we implement by default.
        /// </summary>
        public enum StockList {
            Applicants,
            Available,
            Assigned,
            Killed,
            CrewAssign
        }

        /// <summary>
        /// Get the name we use to identify each of our Sort Bars.
        /// </summary>
        /// <param name="list">The Sort Bar ID</param>
        /// <returns>The name of the Sort Bar</returns>
        public static string GetListName(StockList list) {
            switch( list ) {
                case StockList.Applicants:
                    return "Applicants";
                case StockList.Available:
                    return "Available";
                case StockList.Assigned:
                    return "Assigned";
                case StockList.Killed:
                    return "Killed";
                case StockList.CrewAssign:
                    return "CrewAssign";
                default:
                    return "";
            }
        }

        /// <summary>
        /// Get the ID of the Sort Bar whose name is given.
        /// </summary>
        /// <param name="listName">The name of the Sort Bar</param>
        /// <returns>The ID of the Sort Bar</returns>
        public static StockList GetListId(string listName) {
            if( listName == "Applicants" ) {
                return StockList.Applicants;
            } else if( listName == "Available" ) {
                return StockList.Available;
            } else if( listName == "Assigned" ) {
                return StockList.Assigned;
            } else if( listName == "Killed" ) {
                return StockList.Killed;
            } else if( listName == "CrewAssign" ) {
                return StockList.CrewAssign;
            } else {
                throw new ArgumentException("Unknown list: " + listName, "listName");
            }
        }

        /// <summary>
        /// Gets the screen position of a particular UI object whose transformation is given.
        /// </summary>
        /// <param name="trans">The transformation of the UI object to locate</param>
        /// <returns>The screen position of the UI object</returns>
        public static Vector3 GetPosition(Transform trans) {
            var uiCams = UIManager.instance.uiCameras;
            EZCameraSettings uiCam = null;
            for( int i = 0; i < uiCams.Length; i++ ) {
                if( (uiCams[i].mask & (1 << trans.gameObject.layer)) != 0 ) {
                    uiCam = uiCams[i];
                    break;
                }
            }
            Vector3 screenPos = uiCam.camera.WorldToScreenPoint(trans.position);
            screenPos.y = Screen.height - screenPos.y;
            return screenPos;
        }


        public static void AddInputDelegateToKerbals(UIScrollList crew, EZInputDelegate callback) {
            for( int i = 0; i < crew.Count; i++ ) {
                IUIListObject obj = crew.GetItem(i);
                if( obj == null ) {
                    continue;
                }
                CrewItemContainer cont = obj.gameObject.GetComponent<CrewItemContainer>();
                if( cont != null ) {
                    obj.AddInputDelegate(callback);
                }
            }
        }

        /// <summary>
        /// Replaces a vessel's default crew with the first few kerbals available according to the given sortbar's settings.
        /// </summary>
        /// Prequisites: The vessel has only one cabin with crew in it.
        /// <param name="vesselCrew">List containing the vessel's crew</param>
        /// <param name="availableCrew">List containing the available crew</param>
        /// <param name="sortBar">The sortbar whose criterion to sort by</param>
        public static void FixDefaultVesselCrew(UIScrollList vesselCrew, UIScrollList availableCrew, SortBar sortBar) {
            // WARNING: Apparently this causes NullReferenceExceptions when used. I have yet to determine exactly why.
            // Until I can fix the NullReferenceExceptions, this will be commented out.

            // Find the one cabin with crew in it:
            /*int numCrew = 0;
            int crewLoc = -1;
            for( int i = 0; i < vesselCrew.Count; i++ ) {
                IUIListObject obj = vesselCrew.GetItem(i);
                CrewItemContainer cont = obj.gameObject.GetComponent<CrewItemContainer>();
                if( cont == null && crewLoc >= 0 ) {
                    // If both the above are true, we've hit the end of the crewed cabin.
                    break;
                }
                else if( cont != null ) {
                    if( crewLoc < 0 ) {
                        crewLoc = i;
                    }
                    string debug = "KerbalSorter: " + cont.GetName() + " found in the vessel's crew.";
                    debug += " In Vessel ";
                    if( cont.GetCrewRef() != null && cont.GetCrewRef().KerbalRef != null && cont.GetCrewRef().KerbalRef.InVessel != null ) {
                        debug += cont.GetCrewRef().KerbalRef.InVessel.name;
                    }
                    else {
                        debug += "???";
                    }
                    debug += " In Part ";
                    if( cont.GetCrewRef() != null && cont.GetCrewRef().KerbalRef != null && cont.GetCrewRef().KerbalRef.InPart != null ) {
                        debug += cont.GetCrewRef().KerbalRef.InPart.name;
                    }
                    else {
                        debug += "???";
                    }
                    debug += " Seat ";
                    if( cont.GetCrewRef() != null && cont.GetCrewRef().seat != null ) {
                        debug += cont.GetCrewRef().seat.name;
                    }
                    else {
                        debug += "???";
                    }
                    debug += " Idx ";
                    if( cont.GetCrewRef() != null ) {
                        debug += cont.GetCrewRef().seatIdx;
                    }
                    else {
                        debug += "?";
                    }
                    Debug.Log(debug);
                    cont.SetButton(CrewItemContainer.ButtonTypes.V);
                    vesselCrew.RemoveItem(i, false);
                    availableCrew.AddItem(obj);
                    numCrew++;
                    i--; // Don't accidentally skip something!
                }
            }*/

            // Re-sort the kerbals
            sortBar.SortRoster();

            // Add input listeners to each of the kerbals so we can tell when they're dragged
            /*for( int i = 0; i < availableCrew.Count; i++ ) {
                availableCrew.GetItem(i).AddInputDelegate(OnInput);
            }*/

            // Place the appropriate number of kerbals back into the crew roster
            /*for( int i = 0; i < numCrew; i++ ) {
                IUIListObject obj = availableCrew.GetItem(0);
                availableCrew.RemoveItem(0, false);
                vesselCrew.InsertItem(obj, crewLoc + i);

                obj.gameObject.GetComponent<CrewItemContainer>().SetButton(CrewItemContainer.ButtonTypes.X);
            }*/
        }

        /// <summary>
        /// Adds a component to given Game Object that listens for it to be enabled.
        /// </summary>
        /// <param name="obj">The Game Object to listen to</param>
        /// <param name="callback">The function to call when enabled</param>
        /// <param name="skipFirstTime">Skip the first enable?</param>
        public static void AddOnEnableListener(GameObject obj, Callback callback, bool skipFirstTime = false) {
            OnEnableListener listener = obj.AddComponent<OnEnableListener>();
            listener.Callback = callback;
            listener.SkipFirstTime = skipFirstTime;
        }

        /// <summary>
        /// Adds a component to given Game Object that listens for it to be disabled.
        /// </summary>
        /// <param name="obj">The Game Object to listen to</param>
        /// <param name="callback">The function to call when disabled</param>
        /// <param name="skipFirstTime">Skip the first disable?</param>
        public static void AddOnDisableListener(GameObject obj, Callback callback, bool skipFirstTime = false) {
            OnDisableListener listener = obj.AddComponent<OnDisableListener>();
            listener.Callback = callback;
            listener.SkipFirstTime = skipFirstTime;
        }

        /// <summary>
        /// A hook into any component's OnEnable event.
        /// </summary>
        /// Use: Assign, as a component, to the component you want to listen to.
        public class OnEnableListener : MonoBehaviour {
            public Callback Callback;
            public bool SkipFirstTime = false;
            private bool _doneFirstTime = false;
            protected void OnEnable() {
                try {
                    if( SkipFirstTime && !_doneFirstTime ) {
                        _doneFirstTime = true;
                        return;
                    }
                    Callback();
                }
                catch( Exception e ) {
                    Debug.LogError("KerbalSorter: Unexpected error in OnEnableListener: " + e);
                }
            }
        }

        /// <summary>
        /// A hook into any component's OnDisable event.
        /// </summary>
        /// Use: Assign, as a component, to the component you want to listen to.
        public class OnDisableListener : MonoBehaviour {
            public Callback Callback;
            public bool SkipFirstTime = false;
            private bool _doneFirstTime = false;
            protected void OnDisable() {
                try {
                    if( SkipFirstTime && !_doneFirstTime ) {
                        _doneFirstTime = true;
                        return;
                    }
                    Callback();
                }
                catch( Exception e ) {
                    Debug.LogError("KerbalSorter: Unexpected error in OnDisableListener: " + e);
                }
            }
        }


        /// <summary>
        /// An implementation of Rabin's fingerprinting algorithm with
        /// 32-bit fingerprints.
        /// </summary>
        public static class Fingerprinter {
            // =====================================================================
            //  Interface
            // =====================================================================

            /// <summary>
            /// Creates a fingerprint for a bar.
            /// </summary>
            /// <param name="message">The bar's important parts, as an array of UInt32s</param>
            /// <returns>The fingerprint</returns>
            public static UInt32 FingerprintBar(UInt32[] message) {
                return Fingerprint(message, barPoly);
            }

            /// <summary>
            /// Creates a fingerprint for a button.
            /// </summary>
            /// <param name="message">The button's important parts, as an array of UInt32s</param>
            /// <returns>The fingerprint</returns>
            public static UInt32 FingerprintButton(UInt32[] message) {
                return Fingerprint(message, buttonPoly);
            }


            // =====================================================================
            //  Internals
            // =====================================================================

            /// <summary>
            /// The Rabin Polynomial to use when fingerprinting buttons.
            /// </summary>
            private readonly static RabinPolynomial buttonPoly;
            /// <summary>
            /// The Rabin Polynomial to use when fingerprinting bars.
            /// </summary>
            private static RabinPolynomial barPoly;

            /// <summary>
            /// Static constructor.
            /// </summary>
            static Fingerprinter() {
                // We need two different polynomials because we shouldn't be
                // using a polynomial to fingerprint fingerprints that were made
                // with the same polynomial, and fingerprinting fingerprints is
                // exactly what we do to fingerprint bars.

                // Represents the irreducible polynomial x^32 + x^31 + x^28 +
                // x^26 + x^25 + x^24 + x^23 + x^21 + x^19 + x^18 + x^17 + x^15
                // + x^14 + x^13 + x^11 + x^10 + x^9 + x^8 + x^5 + x^4 + x^3 +
                // x^1 + x^0
                buttonPoly = new RabinPolynomial(0x97AEEF3B);

                // Represents the irreducible polynomial x^32 + x^29 + x^28 +
                // x^20 + x^19 + x^17 + x^12 + x^11 + x^10 + x^9 + x^7 + x^6 +
                // x^5 + x^1 + x^0
                barPoly = new RabinPolynomial(0x301A1EE3);
            }

            /// <summary>
            /// Calculates a fingerprint 
            /// </summary>
            /// <param name="message"></param>
            /// <param name="poly"></param>
            /// <returns></returns>
            private static UInt32 Fingerprint(UInt32[] message, RabinPolynomial poly) {
                UInt32 print = 0;
                for( int i = 0; i < message.Length; i++ ) {
                    print = message[i] ^ poly.ComputeXORFactor(print);
                }
                return print;
            }


            /// <summary>
            /// A 32-bit Rabin polynomial.
            /// </summary>
            private sealed class RabinPolynomial {
                /// <summary>
                /// Initializes this object using the given irreducible polynomial of degree 32.
                /// </summary>
                /// <param name="poly">An irreducible polynomial of degree 32, leading coefficient not included</param>
                public RabinPolynomial(UInt32 poly) {
                    this.polyLCR = poly;
                    InitTables();
                }

                /// <summary>
                /// The irreducible polynomial we base this on, leading coefficient removed.
                /// </summary>
                private UInt32 polyLCR;

                private UInt32[] tableA = new UInt32[256];
                private UInt32[] tableB = new UInt32[256];
                private UInt32[] tableC = new UInt32[256];
                private UInt32[] tableD = new UInt32[256];

                private void InitTables() {
                    // Compute some polynomials mod P. These will help us
                    // initialize the tables.
                    // Each of these is a polynomial. mods[i] = x^(32 + i) mod P
                    UInt32[] mods = new UInt32[32];
                    //x^32 mod P = P - x^32 = polyLCR
                    mods[0] = polyLCR;
                    for( int i = 1; i < 32; i++ ) {
                        // By property of modulo:
                        // x^j mod P = x*(x^(j-1) mod P)
                        mods[i] = mods[i - 1] << 1;
                        // If the highest bit of mods[i-1] is 1, we need to add
                        // polyLCR (which is done with XOR).
                        if( (mods[i - 1] & 0x80000000) != 0 ) {
                            mods[i] ^= polyLCR;
                        }
                    }

                    // Calculate table values:
                    for( int i = 0; i < 256; i++ ) {
                        int temp = i;
                        // Consider each bit, and add the appropriate polynomial
                        // mod P if the bit is 1.
                        for( int k = 0; k < 8; k++ ){
                            if( (temp & 1) == 1 ) {
                                tableA[i] ^= mods[k + 24];
                                tableB[i] ^= mods[k + 16];
                                tableC[i] ^= mods[k +  8];
                                tableD[i] ^= mods[k +  0];
                            }
                            temp = temp >> 1;
                        }
                    }
                }

                public UInt32 ComputeXORFactor(UInt32 rolling_hash) {
                    return tableA[(rolling_hash >> 24) & 0xFF] ^
                           tableB[(rolling_hash >> 16) & 0xFF] ^
                           tableC[(rolling_hash >>  8) & 0xFF] ^
                           tableD[(rolling_hash >>  0) & 0xFF];
                }
            }
        }


        /// <summary>
        /// Returns a list of all transform objects under the given transform object.
        /// </summary>
        /// This is intended for debug and development purposes only.
        /// <param name="trans">The transform whose descendents to enumerate</param>
        /// <returns>A list of all descendents of the given transform</returns>
        public static List<string> EnumerateTransformDescendents(Transform trans) {
            List<string> children = new List<string>();
            if( trans == null ) {
                return children;
            }

            foreach( Transform child in trans ) {
                string name = child.name;
                children.Add(name);
                List<string> grandchildren = EnumerateTransformDescendents(child);
                foreach( string grandchild in grandchildren ) {
                    children.Add(name + "/" + grandchild);
                }
            }

            return children;
        }
    }
}

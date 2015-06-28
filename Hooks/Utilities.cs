using KSP;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace KerbalSorter.Hooks {
    static class Utilities {
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

        // Prequisites: The vessel has only one cabin with crew in it.
        public static void FixDefaultVesselCrew(UIScrollList vesselCrew, UIScrollList availableCrew, SortingButtons sortBar) {
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

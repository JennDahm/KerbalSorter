using KSP;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace KerbalSorter {
    /// <summary>
    /// An interface to a generic roster of Kerbals.
    /// </summary>
    /// <typeparam name="T">The type contained in the Roster</typeparam>
    public abstract class Roster<T> {
        public abstract int Count { get; }
        public abstract T GetItem(int index);
        public abstract void RemoveItem(int index);
        public abstract void InsertItem(T item, int index);

        public abstract ProtoCrewMember GetKerbal(T item);

        /// <summary>
        /// Sorts this Roster using the given comparison functions.
        /// </summary>
        /// <param name="comparisons">The Comparisons to use, in order of application</param>
        public virtual void Sort(KerbalComparer[] comparisons) {
            //Retrieve and temporarily store the list of kerbals
            T[] sortedRoster = new T[this.Count];
            for( int i = 0; i < this.Count; i++ ) {
                sortedRoster[i] = this.GetItem(i);
            }

            //Run through each comparison:
            for( int a = 0; a < comparisons.Length; a++ ) {
                var compare = comparisons[a];

                //Insertion sort, since it's stable and we don't have a large roster:
                for( int i = 1; i < sortedRoster.Length; i++ ) {
                    T kerbal = sortedRoster[i];
                    int k = i;
                    while( 0 < k && compare(this.GetKerbal(kerbal), this.GetKerbal(sortedRoster[k - 1])) < 0 ) {
                        sortedRoster[k] = sortedRoster[k - 1];
                        k--;
                    }
                    sortedRoster[k] = kerbal;
                }
            }

            //Apply the new order to the roster:
            while( this.Count > 0 ) {
                this.RemoveItem(0);
            }
            for( int i = 0; i < sortedRoster.Length; i++ ) {
                this.InsertItem(sortedRoster[i], i);
            }
        }
    }

    /// <summary>
    /// A definition of a button used by the SortBar class.
    /// </summary>
    public struct SortButtonDef {
        /// <summary>
        /// The number of states this button has. Must be greater than 0. State 0 is the default state.
        /// </summary>
        public int numStates;
        /// <summary>
        /// The locations of the icons for each state of the button within the GameData directory.
        /// </summary>
        public string[] iconLocs;
        /// <summary>
        /// The hover text for each state of the button.
        /// </summary>
        public string[] hoverText;
        /// <summary>
        /// The Comparer to use for each state of the button.
        /// </summary>
        public KerbalComparer[] comparers;

        /// <summary>
        /// Generates a fingerprint hash uniquely identifying this sort button definition.
        /// </summary>
        public override int GetHashCode() {
            // TODO: Write a hashing function
            // Must be based on # states,
            // Should also be based on the comparers implemented,
            // Should not be based on the hover text or icon locations.

            // return numStates; // basic, though shitty, implementation.
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// A definition of a sort bar; used to set up the SortBar class.
    /// </summary>
    public struct SortBarDef {
        /// <summary>
        /// The KerbalComparer to base the list order on.
        /// </summary>
        public KerbalComparer defaultComparison;

        /// <summary>
        /// The buttons the sort bar should use.
        /// </summary>
        public SortButtonDef[] buttons;

        /// <summary>
        /// Generates a fingerprint hash uniquely identifying this sort bar definition.
        /// </summary>
        public override int GetHashCode() {
            // TODO: Write a hashing function
            // Must be based on each button
            // Should not be based on default comparison.
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// The state of a sort bar; used to save/restore SortBar state.
    /// </summary>
    public struct SortBarState {
        /// <summary>
        /// The hash of the SortBarDef used to set up the SortBar whose state
        /// we're saving.
        /// </summary>
        public int definitionHash;

        /// <summary>
        /// The state of each button on the bar.
        /// </summary>
        public int[] buttonStates;

        /// <summary>
        /// The order in which the user selected the buttons on the bar.
        /// </summary>
        /// Each entry in the array is an index into the list of buttons. The
        /// order in which they appear in this array is the order in which their
        /// comparisons should be made.
        public int[] selectionOrder;
    }

    /// <summary>
    /// Compares two kerbals. For use in sorting.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>Negative if a comes before b,
    ///          Zero if a and b are equivalent,
    ///          Positive if a comes after b.</returns>
    public delegate int KerbalComparer(ProtoCrewMember a, ProtoCrewMember b);

    /// <summary>
    /// A static class defining the standard comparers.
    /// </summary>
    public static class StandardKerbalComparers {
        /// <summary>
        /// Sorts by kerbal name alphabetically; <see cref="KerbalSorter.KerbalComparer(ProtoCrewMember,ProtoCrewMember)"/>.
        /// </summary>
        static public int NameAscending(ProtoCrewMember a, ProtoCrewMember b) {
            return a.name.CompareTo(b.name);
        }
        /// <summary>
        /// Sorts by kerbal name in reverse alphabetical order; <see cref="KerbalSorter.KerbalComparer(ProtoCrewMember,ProtoCrewMember)"/>.
        /// </summary>
        static public int NameDescending(ProtoCrewMember a, ProtoCrewMember b) {
            return -NameAscending(a, b);
        }

        /// <summary>
        /// Sorts Engineers first, then by class name alphabetically; <see cref="KerbalSorter.KerbalComparer(ProtoCrewMember,ProtoCrewMember)"/>.
        /// </summary>
        static public int ClassEngineersFirst(ProtoCrewMember a, ProtoCrewMember b) {
            bool aIsEng = a.experienceTrait.Title == "Engineer";
            bool bIsEng = b.experienceTrait.Title == "Engineer";
            if( aIsEng && bIsEng ) {
                return 0;
            } else if( aIsEng ) {
                return -1;
            } else if( bIsEng ) {
                return 1;
            } else {
                return a.experienceTrait.Title.CompareTo(b.experienceTrait.Title);
            }
        }
        /// <summary>
        /// Sorts Pilots first, then by class name alphabetically; <see cref="KerbalSorter.KerbalComparer(ProtoCrewMember,ProtoCrewMember)"/>.
        /// </summary>
        static public int ClassPilotsFirst(ProtoCrewMember a, ProtoCrewMember b) {
            bool aIsPil = a.experienceTrait.Title == "Pilot";
            bool bIsPil = b.experienceTrait.Title == "Pilot";
            if( aIsPil && bIsPil ) {
                return 0;
            } else if( aIsPil ) {
                return -1;
            } else if( bIsPil ) {
                return 1;
            } else {
                return a.experienceTrait.Title.CompareTo(b.experienceTrait.Title);
            }
        }
        /// <summary>
        /// Sorts Scientists first, then by class name alphabetically; <see cref="KerbalSorter.KerbalComparer(ProtoCrewMember,ProtoCrewMember)"/>.
        /// </summary>
        static public int ClassScientistsFirst(ProtoCrewMember a, ProtoCrewMember b) {
            bool aIsSci = a.experienceTrait.Title == "Scientist";
            bool bIsSci = b.experienceTrait.Title == "Scientist";
            if( aIsSci && bIsSci ) {
                return 0;
            } else if( aIsSci ) {
                return -1;
            } else if( bIsSci ) {
                return 1;
            } else {
                return a.experienceTrait.Title.CompareTo(b.experienceTrait.Title);
            }
        }

        /// <summary>
        /// Sorts by level, lower levels first; <see cref="KerbalSorter.KerbalComparer(ProtoCrewMember,ProtoCrewMember)"/>.
        /// </summary>
        static public int LevelAscending(ProtoCrewMember a, ProtoCrewMember b) {
            return a.experienceLevel - b.experienceLevel;
        }
        /// <summary>
        /// Sorts by level, higher levels first; <see cref="KerbalSorter.KerbalComparer(ProtoCrewMember,ProtoCrewMember)"/>.
        /// </summary>
        static public int LevelDescending(ProtoCrewMember a, ProtoCrewMember b) {
            return -LevelAscending(a, b);
        }

        /// <summary>
        /// Sorts by gender, male first; <see cref="KerbalSorter.KerbalComparer(ProtoCrewMember,ProtoCrewMember)"/>.
        /// </summary>
        static public int GenderAscending(ProtoCrewMember a, ProtoCrewMember b) {
            return a.gender - b.gender;
        }
        /// <summary>
        /// Sorts by gender, female first; <see cref="KerbalSorter.KerbalComparer(ProtoCrewMember,ProtoCrewMember)"/>.
        /// </summary>
        static public int GenderDescending(ProtoCrewMember a, ProtoCrewMember b) {
            return -GenderAscending(a, b);
        }

        /// <summary>
        /// Sorts by number of completed flights, lower numbers first; <see cref="KerbalSorter.KerbalComparer(ProtoCrewMember,ProtoCrewMember)"/>.
        /// </summary>
        static public int NumFlightsAscending(ProtoCrewMember a, ProtoCrewMember b) {
            return a.careerLog.Entries.Count - b.careerLog.Entries.Count;
        }
        /// <summary>
        /// Sorts by number of completed flights, higher numbers first; <see cref="KerbalSorter.KerbalComparer(ProtoCrewMember,ProtoCrewMember)"/>.
        /// </summary>
        static public int NumFlightsDescending(ProtoCrewMember a, ProtoCrewMember b) {
            return -NumFlightsAscending(a, b);
        }

        /// <summary>
        /// No sorting; <see cref="KerbalSorter.KerbalComparer(ProtoCrewMember,ProtoCrewMember)"/>.
        /// </summary>
        /// <returns>0</returns>
        static public int None(ProtoCrewMember a, ProtoCrewMember b) {
            return 0;
        }

        /// <summary>
        /// Sorts by the default order kerbals appear in the Available lists; <see cref="KerbalSorter.KerbalComparer(ProtoCrewMember,ProtoCrewMember)"/>.
        /// </summary>
        /// The default order that kerbals appear in is the order in which the
        /// kerbals appear in the save file. Under normal circumstances, the
        /// order in which the kerbals appear in the save file is the order in
        /// which they were hired.
        static public int DefaultAvailable(ProtoCrewMember a, ProtoCrewMember b) {
            if( a == b ) {
                return 0;
            }
            KerbalRoster roster = HighLogic.CurrentGame.CrewRoster;
            foreach( ProtoCrewMember kerbal in roster.Crew ) {
                if( kerbal == a ) return -1;
                if( kerbal == b ) return 1;
            }
            // Uuuuum, neither are in the roster?
            return 0;
        }

        /// <summary>
        /// Sorts by the default order kerbals appear in the Applicant list; <see cref="KerbalSorter.KerbalComparer(ProtoCrewMember,ProtoCrewMember)"/>.
        /// </summary>
        /// The default order that kerbals appear in is the order in which the
        /// kerbals appear in the save file. Under normal circumstances, the
        /// order in which the kerbals appear in the save file is the order in
        /// which they were spawned.
        static public int DefaultApplicant(ProtoCrewMember a, ProtoCrewMember b) {
            if( a == b ) {
                return 0;
            }
            KerbalRoster roster = HighLogic.CurrentGame.CrewRoster;
            foreach( ProtoCrewMember kerbal in roster.Applicants ) {
                if( kerbal == a ) return -1;
                if( kerbal == b ) return 1;
            }
            // Uuuuum, neither are applicants?
            return 0;
        }

        /// <summary>
        /// Sorts by the default order kerbals appear in the Assigned list; <see cref="KerbalSorter.KerbalComparer(ProtoCrewMember,ProtoCrewMember)"/>.
        /// </summary>
        /// The default order that kerbals appear in seems to be the order in
        /// which the vessels appear in the save file. Under normal
        /// circumstances, this is the order in which the vessels popped into
        /// the universe, either by launching them or by undocking/decoupling
        /// them from another vessel. Then, kerbals are ordered by the order
        /// in which they appear within the vessel. Generally this is based on
        /// their distance to the root part of the vessel, and their seat.
        static public int DefaultAssigned(ProtoCrewMember a, ProtoCrewMember b) {
            if( a == b ) {
                return 0;
            }
            foreach( ProtoVessel vessel in HighLogic.CurrentGame.flightState.protoVessels ) {
                foreach( ProtoCrewMember kerbal in vessel.GetVesselCrew() ) {
                    if( kerbal == a ) return -1;
                    if( kerbal == b ) return 1;
                }
            }
            // Uuuuum, neither are assigned?
            return 0;
        }

        /// <summary>
        /// Sorts by the default order kerbals appear in the Killed list; <see cref="KerbalSorter.KerbalComparer(ProtoCrewMember,ProtoCrewMember)"/>.
        /// </summary>
        /// The default order that kerbals appear in is the order in which the
        /// kerbals appear in the save file. Under normal circumstances, the
        /// order in which the kerbals appear in the save file is the order in
        /// which they were hired.
        static public int DefaultKilled(ProtoCrewMember a, ProtoCrewMember b) {
            // Apparently Killed kerbals appear in the same order they would be if they were available.
            return DefaultAvailable(a, b);
        }
    }
}

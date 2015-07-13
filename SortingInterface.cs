using KSP;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace KerbalSorter {
    /// <summary>
    /// An interface to a generic roster of Kerbals.
    /// </summary>
    /// <typeparam name="T">The type contained in the Roster</typeparam>
    public interface Roster<T> {
        int Count { get; }
        T GetItem(int index);
        void RemoveItem(int index);
        void InsertItem(T item, int index);

        ProtoCrewMember GetKerbal(T item);
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
    }

    /// <summary>
    /// A static class defining the standard buttons.
    /// </summary>
    public static class StandardButtonDefs {
        static public string BaseDir = "KerbalSorter/Images/";
        static public SortButtonDef ByName = new SortButtonDef{
            numStates = 3,
            iconLocs = new string[]{ BaseDir+"SortName",
                                     BaseDir+"SortNameAsc",
                                     BaseDir+"SortNameDes"},
            hoverText = new string[]{ "Sort by Name",
                                      "Sort by Name, Ascending",
                                      "Sort by Name, Descending"},
            comparers = new KerbalComparer[]{StandardKerbalComparers.None,
                                             StandardKerbalComparers.NameAscending,
                                             StandardKerbalComparers.NameDescending}
        };
        static public SortButtonDef ByClass = new SortButtonDef{
            numStates = 4,
            iconLocs = new string[]{ BaseDir+"SortClass",
                                     BaseDir+"SortClassEng",
                                     BaseDir+"SortClassPil",
                                     BaseDir+"SortClassSci"},
            hoverText = new string[]{ "Sort by Class",
                                      "Sort by Class, Engineers First",
                                      "Sort by Class, Pilots First",
                                      "Sort by Class, Scientists First"},
            comparers = new KerbalComparer[]{StandardKerbalComparers.None,
                                             StandardKerbalComparers.ClassEngineersFirst,
                                             StandardKerbalComparers.ClassPilotsFirst,
                                             StandardKerbalComparers.ClassScientistsFirst}
        };
        static public SortButtonDef ByLevel = new SortButtonDef{
            numStates = 3,
            iconLocs = new string[]{ BaseDir+"SortLevel",
                                     BaseDir+"SortLevelAsc",
                                     BaseDir+"SortLevelDes"},
            hoverText = new string[]{ "Sort by Level",
                                      "Sort by Level, Ascending",
                                      "Sort by Level, Descending"},
            comparers = new KerbalComparer[]{StandardKerbalComparers.None,
                                             StandardKerbalComparers.LevelAscending,
                                             StandardKerbalComparers.LevelDescending}
        };
        static public SortButtonDef ByGender = new SortButtonDef{
            numStates = 3,
            iconLocs = new string[]{ BaseDir+"SortGender",
                                     BaseDir+"SortGenderMale",
                                     BaseDir+"SortGenderFemale"},
            hoverText = new string[]{ "Sort by Gender",
                                      "Sort by Gender, Male First",
                                      "Sort by Gender, Female First"},
            comparers = new KerbalComparer[]{StandardKerbalComparers.None,
                                             StandardKerbalComparers.GenderAscending,
                                             StandardKerbalComparers.GenderDescending}
        };
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
            return a.flightLog.Entries.Count - b.flightLog.Entries.Count;
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
        /// Sorts by the default order kerbals appear in the Assigned lists; <see cref="KerbalSorter.KerbalComparer(ProtoCrewMember,ProtoCrewMember)"/>.
        /// </summary>
        /// Currently I don't know exactly how this is ordered, so this just
        /// returns 0 for now.
        static public int DefaultAssigned(ProtoCrewMember a, ProtoCrewMember b) {
            return 0;
        }

        /// <summary>
        /// Sorts by the default order kerbals appear in the Killed lists; <see cref="KerbalSorter.KerbalComparer(ProtoCrewMember,ProtoCrewMember)"/>.
        /// </summary>
        /// The default order that kerbals appear in is the order in which the
        /// kerbals appear in the save file. Under normal circumstances, the
        /// order in which the kerbals appear in the save file is the order in
        /// which they were hired.
        static public int DefaultKilled(ProtoCrewMember a, ProtoCrewMember b) {
            // Apparently Killed kerbals appear in the same order they would be if they were available.
            return DefaultAvailable(a,b);
        }
    }
}

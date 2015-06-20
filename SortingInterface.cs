using KSP;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace KerbalSorter
{
    public interface Roster<T> {
        int Count{ get; }
        T GetItem(int index);
        void RemoveItem(int index);
        void InsertItem(T item, int index);

        ProtoCrewMember GetKerbal(T item);
    }

    public struct SortButtonDef {
        public int numStates; //Must be >= 1. State 0 should be the default.
        public string[] iconLocs;
        public string[] hoverText;
        public KerbalComparer[] comparers;
    }

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

    public delegate int KerbalComparer(ProtoCrewMember a, ProtoCrewMember b);
    public static class StandardKerbalComparers {
        static public int NameAscending(ProtoCrewMember a, ProtoCrewMember b) {
            return a.name.CompareTo(b.name);
        }
        static public int NameDescending(ProtoCrewMember a, ProtoCrewMember b) {
            return -NameAscending(a, b);
        }

        static public int ClassEngineersFirst(ProtoCrewMember a, ProtoCrewMember b) {
            bool aIsEng = a.experienceTrait.Title == "Engineer";
            bool bIsEng = b.experienceTrait.Title == "Engineer";
            if( aIsEng && bIsEng ){
                return 0;
            } else if( aIsEng ){
                return -1;
            } else if( bIsEng ){
                return 1;
            } else {
                return a.experienceTrait.Title.CompareTo(b.experienceTrait.Title);
            }
        }
        static public int ClassPilotsFirst(ProtoCrewMember a, ProtoCrewMember b) {
            bool aIsPil = a.experienceTrait.Title == "Pilot";
            bool bIsPil = b.experienceTrait.Title == "Pilot";
            if( aIsPil && bIsPil ){
                return 0;
            } else if( aIsPil ){
                return -1;
            } else if( bIsPil ){
                return 1;
            } else {
                return a.experienceTrait.Title.CompareTo(b.experienceTrait.Title);
            }
        }
        static public int ClassScientistsFirst(ProtoCrewMember a, ProtoCrewMember b) {
            bool aIsSci = a.experienceTrait.Title == "Scientist";
            bool bIsSci = b.experienceTrait.Title == "Scientist";
            if( aIsSci && bIsSci ){
                return 0;
            } else if( aIsSci ){
                return -1;
            } else if( bIsSci ){
                return 1;
            } else {
                return a.experienceTrait.Title.CompareTo(b.experienceTrait.Title);
            }
        }

        static public int LevelAscending(ProtoCrewMember a, ProtoCrewMember b) {
            return a.experienceLevel - b.experienceLevel;
        }
        static public int LevelDescending(ProtoCrewMember a, ProtoCrewMember b) {
            return -LevelAscending(a, b);
        }

        static public int GenderAscending(ProtoCrewMember a, ProtoCrewMember b) {
            return a.gender - b.gender;
        }
        static public int GenderDescending(ProtoCrewMember a, ProtoCrewMember b) {
            return -GenderAscending(a, b);
        }

        static public int NumFlightsAscending(ProtoCrewMember a, ProtoCrewMember b) {
            return a.flightLog.Entries.Count - b.flightLog.Entries.Count;
        }
        static public int NumFlightsDescending(ProtoCrewMember a, ProtoCrewMember b) {
            return -NumFlightsAscending(a, b);
        }

        static public int None(ProtoCrewMember a, ProtoCrewMember b) {
            return 0;
        }

        static public int DefaultAvailable(ProtoCrewMember a, ProtoCrewMember b) {
            if( a == b ){
                return 0;
            }
            KerbalRoster roster = HighLogic.CurrentGame.CrewRoster;
            foreach( ProtoCrewMember kerbal in roster.Crew ){
                if( kerbal == a ) return -1;
                if( kerbal == b ) return 1;
            }
            // Uuuuum, neither are in the roster?
            return 0;
        }

        static public int DefaultAssigned(ProtoCrewMember a, ProtoCrewMember b) {
            return 0;
        }

        static public int DefaultKilled(ProtoCrewMember a, ProtoCrewMember b) {
            // Apparently Killed kerbals appear in the same order they would be if they were available.
            return DefaultAvailable(a,b);
        }
    }
}

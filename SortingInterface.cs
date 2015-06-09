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

        static public int ClassAscending(ProtoCrewMember a, ProtoCrewMember b) {
            return a.experienceTrait.Title.CompareTo(b.experienceTrait.Title);
        }
        static public int ClassDescending(ProtoCrewMember a, ProtoCrewMember b) {
            return -ClassAscending(a, b);
        }

        static public int ClassEngineersFirst(ProtoCrewMember a, ProtoCrewMember b){
            return 0;
        }
        static public int ClassPilotsFirst(ProtoCrewMember a, ProtoCrewMember b){
            return 0;
        }
        static public int ClassScientistsFirst(ProtoCrewMember a, ProtoCrewMember b){
            return 0;
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
    }
}

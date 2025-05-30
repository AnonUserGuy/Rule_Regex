using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RR
{
    public class RegexSymbolDef : Def
    {
        public List<PawnNameSlotGenderTuple> pawnNames = new List<PawnNameSlotGenderTuple>();

        [MayTranslate]
        [TranslationCanChangeCount]
        public List<RegexSymbolPath> paths = new List<RegexSymbolPath>();

        [MayTranslate]
        [TranslationCanChangeCount]
        public List<RegexSymbolStringSet> stringSets = new List<RegexSymbolStringSet>();

        private bool isInit = false;

        public bool Initialized
        {
            get => isInit;
        }
        public int Count
        {
            get {
                int a = 0;
                foreach (RegexSymbolStringSet stringSet in stringSets)
                {
                    a += stringSet.strings.Count;
                }
                return a;
            }
        }

        public void Init()
        {
            isInit = true;

            foreach (RegexSymbolPath path in paths)
            {
                AddStringsFromFile(path.path, path.priority);
            }

            // of course the vanilla namebanks system doesn't let you just access the list of names,
            // only random members. So yeah, we have to make a whole seperate copy of all the names
            // just for us. great
            foreach (PawnNameSlotGenderTuple pawnName in pawnNames)
            {
                if (pawnName.slot == PawnNameSlot.First)
                {
                    if (pawnName.gender == Gender.Male)
                    {
                        AddStringsFromNames("First_Male", pawnName.priority);
                    }
                    else
                    {
                        AddStringsFromNames("First_Female", pawnName.priority);
                    }
                }
                else if (pawnName.slot == PawnNameSlot.Nick)
                {
                    if (pawnName.gender == Gender.Male)
                    {
                        AddStringsFromNames("Nick_Male", pawnName.priority);
                    }
                    else if (pawnName.gender == Gender.Female)
                    {
                        AddStringsFromNames("Nick_Female", pawnName.priority);
                    }
                    else
                    {
                        AddStringsFromNames("Nick_Unisex", pawnName.priority);
                    }
                }
                else
                {
                    AddStringsFromNames("Last", pawnName.priority);
                }
            }
        }

        private void AddStringsFromFile(string filePath, float priority)
        {
            if (!Translator.TryGetTranslatedStringsForFile(filePath, out var stringList))
            {
                return;
            }
            AddStrings(stringList, priority);
        }

        private void AddStringsFromNames(string fileName, float priority)
        {
            AddStrings(GenFile.LinesFromFile("Names/" + fileName), priority);
        }

        public void AddStrings(IEnumerable<string> stringList, float priority)
        {
            int i = stringSets.FindIndex(x => x.priority == priority);
            if (i == -1)
            {
                stringSets.Add(new RegexSymbolStringSet(priority));
                i = stringSets.Count - 1;
            }
            foreach (string item in stringList)
            {
                stringSets[i].strings.Add(item);
            }
        }
    }
}

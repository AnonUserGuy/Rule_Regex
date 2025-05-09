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
        public List<string> paths = new List<string>();

        [MayTranslate]
        [TranslationCanChangeCount]
        public List<string> strings = new List<string>();

        private bool isInit = false;

        public bool Initialized
        {
            get => isInit;
        }
        public int Count
        {
            get => strings.Count;
        }

        public void Init()
        {
            isInit = true;

            foreach (string path in paths)
            {
                LoadStringsFromFile(path);
            }

            // of course the vanilla namebanks system doesn't let you just access the list of names,
            // only random members. So yeah, we have to make a whole seperate copy off all the names
            // just for us. great
            foreach (PawnNameSlotGenderTuple pawnName in pawnNames)
            {
                if (pawnName.slot == PawnNameSlot.First)
                {
                    if (pawnName.gender == Gender.Male)
                    {
                        AddNamesFromFile("First_Male");
                    }
                    else
                    {
                        AddNamesFromFile("First_Female");
                    }
                }
                else if (pawnName.slot == PawnNameSlot.Nick)
                {
                    if (pawnName.gender == Gender.Male)
                    {
                        AddNamesFromFile("Nick_Male");
                    }
                    else if (pawnName.gender == Gender.Female)
                    {
                        AddNamesFromFile("Nick_Female");
                    }
                    else
                    {
                        AddNamesFromFile("Nick_Unisex");
                    }
                }
                else
                {
                    AddNamesFromFile("Last");
                }
            }
        }

        private void LoadStringsFromFile(string filePath)
        {
            if (!Translator.TryGetTranslatedStringsForFile(filePath, out var stringList))
            {
                return;
            }
            foreach (string item in stringList)
            {
                strings.Add(item);
            }
        }

        private void AddNamesFromFile(string fileName)
        {
            AddNames(GenFile.LinesFromFile("Names/" + fileName));
        }

        public void AddNames(IEnumerable<string> namesToAdd)
        {
            foreach (string item in namesToAdd)
            {
                strings.Add(item);
            }
        }
    }
}

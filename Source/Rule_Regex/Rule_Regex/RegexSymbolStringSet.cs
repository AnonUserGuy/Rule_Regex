using System.Collections.Generic;
using Verse;

namespace RR
{
    public class RegexSymbolStringSet
    {
        [MayTranslate]
        [TranslationCanChangeCount]
        public List<string> strings = new List<string>();

        [MayTranslate]
        public float priority = 1f;

        public RegexSymbolStringSet()
        {
        }

        public RegexSymbolStringSet(float priority)
        {
            this.priority = priority;
        }

        public int Count
        {
            get => strings.Count;
        }
    }
}

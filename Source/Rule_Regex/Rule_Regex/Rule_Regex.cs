using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Verse;
using Verse.Grammar;

namespace RR
{
    public class Rule_Regex : Rule
    {

        private struct setAndStr
        {
            // Contains the index for a string set and a string within that set.
            public setAndStr(int set, int str)
            {
                this.set = set;
                this.str = str;
            }

            public int set;
            public int str;
        }

        private int iterator = 0;
        private bool resolvable = true;

        public string regex;
        public RegexConcatinationMethod concatinate = RegexConcatinationMethod.Cumulative;

        [LoadAlias("regices")]
        public List<RegexEntry> regexes;

        public string scheme;

        private int[] schemeVals;

        public List<RegexSymbolDef> symbols = new List<RegexSymbolDef>();

        private string[] resultStrings;

        public override float BaseSelectionWeight => 1f;

        public override Rule DeepCopy()
        {
            Rule_Regex rule = (Rule_Regex)base.DeepCopy();
            rule.iterator = iterator;
            // TODO: finish later 
            return rule;
        }

        public override string Generate()
        {
            iterator = iterator % schemeVals.Length;
            if (iterator == 0 && resolvable)
            {
                Randomize();
            }
            if (!resolvable)
            {
                return "ERROR";
            }

            return resultStrings[schemeVals[iterator++]];
        }
        
        public override void Init()
        {
            if (scheme == null)
            {
                schemeVals = new int[symbols.Count];
                for (int i = 0; i < symbols.Count; i++)
                {
                    schemeVals[i] = i;
                }
            }
            else
            {
                string[] words = scheme.Split(',');
                schemeVals = new int[words.Length];
                for (int i = 0; i < words.Length; i++)
                {
                    schemeVals[i] = Int32.Parse(words[i]);
                }
            }

            if (!regexes.NullOrEmpty() && regexes.Count == symbols.Count - 1)
            {
                regexes.Prepend(new RegexEntry());
            }

            resultStrings = new string[schemeVals.Length];
            foreach (RegexSymbolDef member in symbols)
            {
                if (!member.Initialized)
                {
                    member.Init();
                }
            }
        }

        private void Randomize()
        {
            setAndStr[] result = new setAndStr[symbols.Count];
            List<setAndStr>[] valids = new List<setAndStr>[symbols.Count];

            for (int i = 0; i < symbols.Count; i++)
            {
                valids[i] = new List<setAndStr>();

                string tester = "";
                if (i != 0)
                {
                    if (!regexes.NullOrEmpty() && regexes[i].concatinate == RegexConcatinationMethod.Cumulative || regexes.NullOrEmpty() && concatinate == RegexConcatinationMethod.Cumulative)
                    {
                        // perform regex test on all previous selected strings + next
                        for (int j = 0; j < i; j++)
                        {
                            tester += symbols[j].stringSets[result[j].set].strings[result[j].str] + "\n";
                        }
                    }
                    else if (!regexes.NullOrEmpty() && regexes[i].concatinate == RegexConcatinationMethod.Previous || regexes.NullOrEmpty() && concatinate == RegexConcatinationMethod.Previous)
                    {
                        // perform regex test on only immediate previously selected string + next
                        tester = symbols[i - 1].stringSets[result[i - 1].set].strings[result[i - 1].str] + "\n";
                    }
                    // if none then leave tester as ""
                }


                string regexLocal;
                if (regexes.NullOrEmpty())
                {
                    if (i == 0 && symbols.Count > 1)
                    {
                        regexLocal = null;
                    }
                    else
                    {
                        regexLocal = regex;
                    }
                }
                else
                {
                    regexLocal = regexes[i].regex;
                }


                if (regexLocal.NullOrEmpty())
                {
                    for (int j = 0; j < symbols[i].stringSets.Count; j++)
                    {
                        for (int k = 0; k < symbols[i].stringSets[j].Count; k++)
                        {
                            valids[i].Add(new setAndStr(j, k));
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < symbols[i].stringSets.Count; j++)
                    {
                        for (int k = 0; k < symbols[i].stringSets[j].Count; k++)
                        {
                            if (Regex.IsMatch(tester + symbols[i].stringSets[j].strings[k], regexLocal))
                            {
                                valids[i].Add(new setAndStr(j, k));
                            }
                        }
                    }
                }
                    

                if (valids[i].Any())
                {
                    // select one of the strings that passed the regex
                    result[i] = valids[i].RandomElementByWeight((setAndStr a) => symbols[i].stringSets[a.set].priority);
                }
                else 
                {
                    // took a bad path, go back at least one to find another string
                    do
                    {
                        if (i == 0)
                        {
                            // all root strings were tested and none worked ever
                            Log.Error("Could not resolve a Rule_GroupRegex.");
                            resolvable = false;
                            return;
                        }

                        i--;
                        valids[i].Remove(result[i]);
                    } 
                    while (!valids[i].Any());

                    // select another string that passed the regex
                    result[i] = valids[i].RandomElement();
                }
            }

            // successfully resolved!
            for (int i = 0; i < result.Length; i++)
            {
                resultStrings[i] = symbols[i].stringSets[result[i].set].strings[result[i].str];
            }
        }

        public override string ToString()
        {
            return keyword + "->(Rule_Regex)";
        }
    }
}

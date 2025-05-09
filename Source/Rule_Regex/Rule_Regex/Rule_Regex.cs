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
        private int iterator = 0;
        private bool resolvable = true;

        public string regex;

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
            int[] result = new int[symbols.Count];
            List<int>[] valids = new List<int>[symbols.Count];

            for (int i = 0; i < symbols.Count; i++)
            {
                valids[i] = new List<int>();

                string tester = "";
                if (i != 0)
                {
                    if (regexes.NullOrEmpty() || regexes[i].concatinate == RegexConcatinationMethod.Cumulative)
                    {
                        // perform regex test on all previous selected strings + next
                        for (int j = 0; j < i; j++)
                        {
                            tester += symbols[j].strings[result[j]] + "\n";
                        }
                    }
                    else if (regexes[i].concatinate == RegexConcatinationMethod.Previous)
                    {
                        // perform regex test on only immediate previously selected string + next
                        tester = symbols[i - 1] + "\n";
                    }
                    // if none then leave tester as ""
                }


                string regexLocal;
                if (regexes.NullOrEmpty())
                {
                    regexLocal = regex;
                }
                else
                {
                    regexLocal = regexes[i].regex;
                }


                if (regexLocal.NullOrEmpty())
                {
                    for (int j = 0; j < symbols[i].Count; j++)
                    {
                        valids[i].Add(j);
                    }
                }
                else
                {
                    for (int j = 0; j < symbols[i].Count; j++)
                    {
                        Log.Message(tester + symbols[i].strings[j]);
                        Log.Message(regexLocal);
                        if (Regex.IsMatch(tester + symbols[i].strings[j], regexLocal))
                        {
                            valids[i].Add(j);
                        }
                    }
                }
                    

                if (valids[i].Any())
                {
                    // select one of the strings that passed the regex
                    result[i] = valids[i].RandomElement();
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
                resultStrings[i] = symbols[i].strings[result[i]];
            }
        }

        public override string ToString()
        {
            return keyword + "->(Rule_Regex)";
        }
    }
}

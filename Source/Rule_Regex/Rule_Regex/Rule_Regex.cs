using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Verse;
using Verse.Grammar;

namespace RR
{
    public class Rule_Regex : Rule
    {
        private int iterator = 0;
        private bool resolvable = true;

        public string regex = "";

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
                if (i == 0)
                {
                    // no regex, just allow all strings
                    // could make do regex, but I assume only valid strings will be supplied to first group
                    for (int j = 0; j < symbols[i].Count; j++)
                    {
                        valids[i].Add(j);
                    }
                }
                else
                {
                    // perform regex test on previous selected strings + all possible next ones
                    string tester = "";
                    for (int j = 0; j < i; j++)
                    {
                        tester += symbols[j].strings[result[j]] + "\n";
                    }
                    for (int j = 0; j < symbols[i].Count; j++)
                    {
                        if (Regex.IsMatch(tester + symbols[i].strings[j], regex))
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

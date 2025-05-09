using RimWorld;
using Verse.Grammar;
using Verse;

namespace RR
{
    public class Rule_NamePersonBetter : Rule
    {
        public PawnNameSlot slot = PawnNameSlot.First;

        public Gender gender = Gender.None;

        public override float BaseSelectionWeight => 1f;

        public override Rule DeepCopy()
        {
            Rule_NamePersonBetter obj = (Rule_NamePersonBetter)base.DeepCopy();
            obj.slot = slot;
            obj.gender = gender;
            return obj;
        }

        public override string Generate()
        {
            NameBank nameBank = PawnNameDatabaseShuffled.BankOf(PawnNameCategory.HumanStandard);
            Gender gender = this.gender;
            if (slot != PawnNameSlot.First && gender == Gender.None)
            {
                gender = ((Rand.Value < 0.5f) ? Gender.Male : Gender.Female);
            }
            return nameBank.GetName(slot, gender, checkIfAlreadyUsed: false);
        }

        public override string ToString()
        {
            return keyword + "->(personname)";
        }
    }
}

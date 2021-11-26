using DifferenceUtility.Net.Base;

namespace Sample.NetConsole
{
    public class TestDiffCallback : BaseDiffCallback<Program.CharWrapper, Program.CharWrapper>
    {
        public override bool AreItemsTheSame(Program.CharWrapper oldItem, Program.CharWrapper newItem)
        {
            return oldItem.C == newItem.C;
        }
    }
}

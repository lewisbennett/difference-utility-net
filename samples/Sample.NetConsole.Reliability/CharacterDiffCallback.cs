using DifferenceUtility.Net.Base;

namespace Sample.NetConsole.Reliability;

public class CharacterDiffCallback : BaseDiffCallback<char, char>
{
    public override bool AreItemsTheSame(char sourceItem, char destinationItem)
    {
        return sourceItem == destinationItem;
    }
}
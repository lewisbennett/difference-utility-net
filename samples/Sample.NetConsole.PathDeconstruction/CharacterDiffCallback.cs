using DifferenceUtility.Net.Base;

namespace Sample.NetConsole.PathDeconstruction;

public class CharacterDiffCallback : BaseDiffCallback<char, char>
{
    public override bool AreItemsTheSame(char sourceItem, char destinationItem)
    {
        return sourceItem == destinationItem;
    }
}
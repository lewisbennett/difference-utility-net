using DifferenceUtility.Net.Base;

namespace DifferenceUtility.Net.Benchmarks;

public class IntDiffCallback : BaseDiffCallback<int, int>
{
    public override bool AreItemsTheSame(int sourceItem, int destinationItem)
    {
        return sourceItem == destinationItem;
    }
}
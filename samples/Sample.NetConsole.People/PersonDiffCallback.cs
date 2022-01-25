using System.Collections.Generic;
using DifferenceUtility.Net.Base;
using Sample.Assets;

namespace Sample.NetConsole.People;

public class PersonDiffCallback : BaseDiffCallback<Person, Person>
{
    public override bool AreContentsTheSame(Person sourceItem, Person destinationItem)
    {
        // Contents are the same if the property values match.
        // This method is only invoked if AreItemsTheSame returns true, so we don't need to compare IDs.
        return EqualityComparer<string>.Default.Equals(sourceItem.FirstName, destinationItem.FirstName)
            && EqualityComparer<string>.Default.Equals(sourceItem.LastName, destinationItem.LastName);
    }

    public override bool AreItemsTheSame(Person sourceItem, Person destinationItem)
    {
        // Items are the same if their persistant IDs are the same.
        return sourceItem.ID == destinationItem.ID;
    }

    public override void UpdateContents(Person item, Person dataSource)
    {
        base.UpdateContents(item, dataSource);

        item.FirstName = dataSource.FirstName;
        item.LastName = dataSource.LastName;
    }
}
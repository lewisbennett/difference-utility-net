using DifferenceUtility.Net.Base;
using Sample.Assets;
using System.Collections.Generic;

namespace Sample.MvvmCross.Core;

public class PersonDiffCallback : BaseDiffCallback<PersonModel, Person>
{
    public override bool AreContentsTheSame(PersonModel sourceItem, Person destinationItem)
    {
        // Contents are the same if the property values match.
        // This method is only invoked if AreItemsTheSame returns true, so we don't need to compare IDs.
        return EqualityComparer<string>.Default.Equals(sourceItem.Person.FirstName, destinationItem.FirstName)
            && EqualityComparer<string>.Default.Equals(sourceItem.Person.LastName, destinationItem.LastName);
    }
        
    public override bool AreItemsTheSame(PersonModel sourceItem, Person destinationItem)
    {
        // Items are the same if their persistant IDs are the same.
        return sourceItem.Person.ID == destinationItem.ID;
    }

    public override PersonModel ConstructFinalItem(Person destinationItem)
    {
        return new PersonModel
        {
            Person = destinationItem
        };
    }

    public override void UpdateContents(PersonModel item, Person dataSource)
    {
        base.UpdateContents(item, dataSource);

        item.Person = dataSource;
    }
}
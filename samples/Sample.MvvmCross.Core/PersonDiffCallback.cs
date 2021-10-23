using DifferenceUtility.Net.Base;
using Sample.Assets;
using System.Collections.Generic;

namespace Sample.MvvmCross.Core
{
    public class PersonDiffCallback : BaseDiffCallback<PersonModel, Person>
    {
        public override bool AreContentsTheSame(PersonModel oldItem, Person newItem)
        {
            // Contents are the same if the property values match.
            // This method is only invoked if AreItemsTheSame returns true, so we don't need to compare IDs.
            return EqualityComparer<string>.Default.Equals(oldItem.Person.FirstName, newItem.FirstName)
                && EqualityComparer<string>.Default.Equals(oldItem.Person.LastName, newItem.LastName);
        }
        
        public override bool AreItemsTheSame(PersonModel oldItem, Person newItem)
        {
            // Items are the same if their persistant IDs are the same.
            return oldItem.Person.ID == newItem.ID;
        }

        public override PersonModel ConstructFinalItem(Person newItem)
        {
            return new PersonModel
            {
                Person = newItem
            };
        }

        public override void UpdateContents(PersonModel item, Person dataSource)
        {
            base.UpdateContents(item, dataSource);

            item.Person = dataSource;
        }
    }
}

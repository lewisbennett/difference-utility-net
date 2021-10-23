using DifferenceUtility.Net.Base;
using Sample.Assets;
using System.Collections.Generic;

namespace Sample.NetConsole
{
    public class PersonDiffCallback : BaseDiffCallback<Person>
    {
        public override bool AreContentsTheSame(Person oldItem, Person newItem)
        {
            // Contents are the same if the property values match.
            // This method is only invoked if AreItemsTheSame returns true, so we don't need to compare IDs.
            return EqualityComparer<string>.Default.Equals(oldItem.FirstName, newItem.FirstName)
                && EqualityComparer<string>.Default.Equals(oldItem.LastName, newItem.LastName);
        }

        public override bool AreItemsTheSame(Person oldItem, Person newItem)
        {
            // Items are the same if their persistant IDs are the same.
            return oldItem.ID == newItem.ID;
        }

        public override void UpdateContents(Person item, Person dataSource)
        {
            base.UpdateContents(item, dataSource);

            item.FirstName = dataSource.FirstName;
            item.LastName = dataSource.LastName;
        }
    }
}

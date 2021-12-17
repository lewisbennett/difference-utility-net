using System;
using System.Collections.Generic;
using DifferenceUtility.Net.Base;
using DifferenceUtility.Net.Benchmarks.Data;

namespace DifferenceUtility.Net.Benchmarks
{
    public class PersonDiffCallback_Guid : BaseDiffCallback<Person<Guid>, Person<Guid>>
    {
        #region Public Methods
        /// <inheritdoc />
        public override bool AreContentsTheSame(Person<Guid> sourceItem, Person<Guid> destinationItem)
        {
            // Contents are the same if the property values match.
            // This method is only invoked if AreItemsTheSame returns true, so we don't need to compare IDs.
            return EqualityComparer<string>.Default.Equals(sourceItem.FirstName, destinationItem.FirstName)
                && EqualityComparer<string>.Default.Equals(sourceItem.LastName, destinationItem.LastName);
        }

        /// <inheritdoc />
        public override bool AreItemsTheSame(Person<Guid> sourceItem, Person<Guid> destinationItem)
        {
            // Items are the same if their persistant IDs are the same.
            return sourceItem.ID == destinationItem.ID;
        }
        #endregion
    }
    
    public class PersonDiffCallback_Int : BaseDiffCallback<Person<int>, Person<int>>
    {
        #region Public Methods
        /// <inheritdoc />
        public override bool AreContentsTheSame(Person<int> sourceItem, Person<int> destinationItem)
        {
            // Contents are the same if the property values match.
            // This method is only invoked if AreItemsTheSame returns true, so we don't need to compare IDs.
            return EqualityComparer<string>.Default.Equals(sourceItem.FirstName, destinationItem.FirstName)
                   && EqualityComparer<string>.Default.Equals(sourceItem.LastName, destinationItem.LastName);
        }

        /// <inheritdoc />
        public override bool AreItemsTheSame(Person<int> sourceItem, Person<int> destinationItem)
        {
            // Items are the same if their persistant IDs are the same.
            return sourceItem.ID == destinationItem.ID;
        }
        #endregion
    }
}

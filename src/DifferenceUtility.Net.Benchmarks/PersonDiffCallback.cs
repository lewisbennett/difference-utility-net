using System;
using System.Collections.Generic;
using DifferenceUtility.Net.Base;
using DifferenceUtility.Net.Benchmarks.Data;

namespace DifferenceUtility.Net.Benchmarks
{
    public class PersonDiffCallback_Gen_Guid : BaseDiffCallback<Person<Guid>>
    {
        #region Public Methods
        /// <inheritdoc />
        public override bool AreContentsTheSame(Person<Guid> oldItem, Person<Guid> newItem)
        {
            // Contents are the same if the property values match.
            // This method is only invoked if AreItemsTheSame returns true, so we don't need to compare IDs.
            return EqualityComparer<string>.Default.Equals(oldItem.FirstName, newItem.FirstName)
                && EqualityComparer<string>.Default.Equals(oldItem.LastName, newItem.LastName);
        }

        /// <inheritdoc />
        public override bool AreItemsTheSame(Person<Guid> oldItem, Person<Guid> newItem)
        {
            // Items are the same if their persistant IDs are the same.
            return oldItem.ID == newItem.ID;
        }
        #endregion
    }

    public class PersonDiffCallback_Gen2_Guid : BaseDiffCallback<Person<Guid>, Person<Guid>>
    {
        #region Public Methods
        /// <inheritdoc />
        public override bool AreContentsTheSame(Person<Guid> oldItem, Person<Guid> newItem)
        {
            // Contents are the same if the property values match.
            // This method is only invoked if AreItemsTheSame returns true, so we don't need to compare IDs.
            return EqualityComparer<string>.Default.Equals(oldItem.FirstName, newItem.FirstName)
                && EqualityComparer<string>.Default.Equals(oldItem.LastName, newItem.LastName);
        }

        /// <inheritdoc />
        public override bool AreItemsTheSame(Person<Guid> oldItem, Person<Guid> newItem)
        {
            // Items are the same if their persistant IDs are the same.
            return oldItem.ID == newItem.ID;
        }
        #endregion
    }
    
    public class PersonDiffCallback_Gen_Int : BaseDiffCallback<Person<int>>
    {
        #region Public Methods
        /// <inheritdoc />
        public override bool AreContentsTheSame(Person<int> oldItem, Person<int> newItem)
        {
            // Contents are the same if the property values match.
            // This method is only invoked if AreItemsTheSame returns true, so we don't need to compare IDs.
            return EqualityComparer<string>.Default.Equals(oldItem.FirstName, newItem.FirstName)
                   && EqualityComparer<string>.Default.Equals(oldItem.LastName, newItem.LastName);
        }

        /// <inheritdoc />
        public override bool AreItemsTheSame(Person<int> oldItem, Person<int> newItem)
        {
            // Items are the same if their persistant IDs are the same.
            return oldItem.ID == newItem.ID;
        }
        #endregion
    }

    public class PersonDiffCallback_Gen2_Int : BaseDiffCallback<Person<int>, Person<int>>
    {
        #region Public Methods
        /// <inheritdoc />
        public override bool AreContentsTheSame(Person<int> oldItem, Person<int> newItem)
        {
            // Contents are the same if the property values match.
            // This method is only invoked if AreItemsTheSame returns true, so we don't need to compare IDs.
            return EqualityComparer<string>.Default.Equals(oldItem.FirstName, newItem.FirstName)
                   && EqualityComparer<string>.Default.Equals(oldItem.LastName, newItem.LastName);
        }

        /// <inheritdoc />
        public override bool AreItemsTheSame(Person<int> oldItem, Person<int> newItem)
        {
            // Items are the same if their persistant IDs are the same.
            return oldItem.ID == newItem.ID;
        }
        #endregion
    }
}

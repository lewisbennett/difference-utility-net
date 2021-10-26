using System.Text.Json.Serialization;

namespace DifferenceUtility.Net.Benchmarks.Data
{
    public abstract class Person
    {
        #region Properties
        /// <summary>
        /// Gets or sets the person's first name.
        /// </summary>
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the person's last name.
        /// </summary>
        [JsonPropertyName("lastName")]
        public string LastName { get; set; }
        #endregion
    }

    public class Person<T> : Person
    {
        #region Properties
        /// <summary>
        /// Gets or sets the person's ID.
        /// </summary>
        [JsonPropertyName("id")]
        public T ID { get; set; }
        #endregion
    }
}

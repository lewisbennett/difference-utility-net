using System.Text.Json.Serialization;

namespace DifferenceUtility.Net.Benchmarks.Data
{
    public class BenchmarkData<T>
    {
        #region Properties
        /// <summary>
        /// Gets or sets the test data for the insertion test.
        /// </summary>
        [JsonPropertyName("insertionTestData")]
        public Person<T>[] InsertionTestData { get; set; }

        /// <summary>
        /// Gets or sets the test data for the moves test.
        /// </summary>
        [JsonPropertyName("movesTestData")]
        public Person<T>[] MovesTestData { get; set; }

        /// <summary>
        /// Gets or sets the original test data.
        /// </summary>
        [JsonPropertyName("originalData")]
        public Person<T>[] OriginalData { get; set; }

        /// <summary>
        /// Gets or sets the test data for the removals test.
        /// </summary>
        [JsonPropertyName("removalsTestData")]
        public Person<T>[] RemovalsTestData { get; set; }

        /// <summary>
        /// Gets or sets the test data for the updates test.
        /// </summary>
        [JsonPropertyName("updatesTestData")]
        public Person<T>[] UpdatesTestData { get; set; }
        #endregion
    }
}

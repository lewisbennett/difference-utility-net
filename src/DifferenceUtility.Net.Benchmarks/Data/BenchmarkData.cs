using System.Text.Json.Serialization;

namespace DifferenceUtility.Net.Benchmarks.Data;

public class BenchmarkData
{
    #region Properties
    /// <summary>
    ///     Gets or sets the test data for the everything test.
    /// </summary>
    [JsonPropertyName("everythingTestData")]
    public int[] EverythingTestData { get; set; }

    /// <summary>
    ///     Gets or sets the test data for the insertion test.
    /// </summary>
    [JsonPropertyName("insertionTestData")]
    public int[] InsertionTestData { get; set; }

    /// <summary>
    ///     Gets or sets the test data for the moves test.
    /// </summary>
    [JsonPropertyName("movesTestData")]
    public int[] MovesTestData { get; set; }

    /// <summary>
    ///     Gets or sets the original test data.
    /// </summary>
    [JsonPropertyName("originalData")]
    public int[] OriginalData { get; set; }

    /// <summary>
    ///     Gets or sets the test data for the removals test.
    /// </summary>
    [JsonPropertyName("removalsTestData")]
    public int[] RemovalsTestData { get; set; }
    #endregion
}
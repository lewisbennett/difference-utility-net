using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BenchmarkDotNet.Running;
using DifferenceUtility.Net.Benchmarks.Data;
#if DEBUG
using BenchmarkDotNet.Configs;
#endif

namespace DifferenceUtility.Net.Benchmarks;

public static class Program
{
    #region Configuration Constants
    /// <summary>
    ///     Change this to the number of entries in the test data to benchmark.
    /// </summary>
    public const int TestCount = 10;
    #endregion

    #region Public Methods
    /// <summary>
    ///     Main application entry point.
    /// </summary>
    public static void Main()
    {
        // GenerateAllBenchmarkData();

#if DEBUG
        BenchmarkRunner.Run<CalculateDiffBenchmarks>(new DebugInProcessConfig());
        BenchmarkRunner.Run<DifferenceUtilityBenchmarks>(new DebugInProcessConfig());
#else
        // BenchmarkRunner.Run<CalculateDiffBenchmarks>();
        BenchmarkRunner.Run<DifferenceUtilityBenchmarks>();
#endif
    }
    #endregion

    #region Private Methods
    /// <summary>
    ///     <para>Generates and saves test data, JSON serialized, to the project directory.</para>
    ///     <para>
    ///         This method will return if test data already exists for all supported ID data types, but will continue, and
    ///         overwrite, any existing test data if any are missing.
    ///     </para>
    /// </summary>
    private static void GenerateAllBenchmarkData()
    {
        // Make sure the count is a multiple of two.
        const int testCount = TestCount - TestCount % 2;

        // Sanity check in case of TestCount value being changed manually.
        if (testCount < 2)
            throw new InvalidOperationException("Invalid test data item count provided.");

        var projectDirectory = Environment.GetEnvironmentVariable("PROJECT_DIRECTORY");

        if (string.IsNullOrWhiteSpace(projectDirectory))
            throw new InvalidOperationException("PROJECT_DIRECTORY environment variable missing.");

        var testDataPath = Path.Combine(Path.Combine(projectDirectory, "TestData"), $"test_data_{testCount}.json");

        // No need to generate new test data if it already exists.
        if (File.Exists(testDataPath))
            return;

        var random = new Random();

        var originalData = new HashSet<int>();

        for (var i = 0; i < testCount; i++)
        {
            int newItem;

            do
            {
                newItem = random.Next(0, testCount);

            } while (originalData.Contains(newItem));

            originalData.Add(newItem);
        }

        // Insertions: +50% dummy data inserted randomly.
        var insertData = originalData.ToList();

        for (var i = 0; i < testCount / 2; i++)
        {
            int newItem;

            do
            {
                newItem = random.Next(testCount, testCount / 2 + testCount + 1);

            } while (insertData.Contains(newItem));

            insertData.Insert(random.Next(0, insertData.Count), newItem);
        }

        // Moves: 50% original data moved randomly.
        var moveData = originalData.ToList();

        for (var i = 0; i < testCount / 2; i++)
        {
            int oldIndex, newIndex;

            do
            {
                oldIndex = random.Next(0, moveData.Count);
                newIndex = random.Next(0, moveData.Count);

            } while (oldIndex == newIndex);

            var item = moveData[oldIndex];

            moveData.RemoveAt(oldIndex);

            if (oldIndex < newIndex)
                moveData.Insert(newIndex - 1, item);

            else
                moveData.Insert(newIndex, item);
        }

        // Removals: -50% original data removed randomly.
        var removeData = originalData.ToList();

        for (var i = 0; i < testCount / 2; i++)
            removeData.RemoveAt(random.Next(0, removeData.Count));

        // Everything: an entirely new collection.
        var everythingData = new HashSet<int>();

        for (var i = 0; i < testCount; i++)
            everythingData.Add(random.Next(0, testCount * 2));

        var benchmarkData = new BenchmarkData
        {
            EverythingTestData = everythingData.ToArray(),
            InsertionTestData = insertData.ToArray(),
            MovesTestData = moveData.ToArray(),
            OriginalData = originalData.ToArray(),
            RemovalsTestData = removeData.ToArray()
        };

        // Serialize and save the tests.
        using var streamWriter = new StreamWriter(testDataPath);

        streamWriter.WriteLine(JsonSerializer.Serialize(benchmarkData));
    }
    #endregion
}
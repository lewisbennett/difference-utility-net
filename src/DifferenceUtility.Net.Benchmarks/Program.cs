using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BenchmarkDotNet.Running;
using DifferenceUtility.Net.Benchmarks.Data;

namespace DifferenceUtility.Net.Benchmarks
{
    public static class Program
    {
        public static BenchmarkData<Guid> BenchmarkData_Guid { get; private set; }

        public static BenchmarkData<int> BenchmarkData_Int { get; private set; }

        /// <summary>
        /// // Main application entry point.
        /// </summary>
        public static void Main()
        {
            var projectDirectory = Environment.GetEnvironmentVariable("PROJECT_DIRECTORY");

            // Change this value to generate/import then benchmark a different dataset.
            const int testCount = 10;

            GenerateOrImportTestData(projectDirectory, testCount, out var benchmarkData_Guid, out var benchmarkData_Int);

            BenchmarkData_Guid = benchmarkData_Guid;
            BenchmarkData_Int = benchmarkData_Int;

#if DEBUG
            BenchmarkRunner.Run<DifferenceUtilityBenchmarks>(new BenchmarkDotNet.Configs.DebugInProcessConfig());
#else
            BenchmarkRunner.Run<DifferenceUtilityBenchmarks>();
#endif
        }
        
        /// <summary>
        /// Generates test data and saves them, JSON serialized, to the project directory.
        /// </summary>
        /// <param name="projectDirectory">This project's directory.</param>
        /// <param name="testCount">The number of entries for each test.</param>
        public static void GenerateOrImportTestData(string projectDirectory, int testCount, out BenchmarkData<Guid> benchmarkData_Guid, out BenchmarkData<int> benchmarkData_Int)
        {
            // Make sure the count is a multiple of two.
            testCount -= testCount % 2;

            if (testCount < 2)
                throw new InvalidOperationException("Invalid test data item count provided.");

            var testDataPath_Guid = Path.Combine(projectDirectory, $"TestData/test_data_guid_{testCount}.json");
            var testDataPath_Int = Path.Combine(projectDirectory, $"TestData/test_data_int_{testCount}.json");

            if (File.Exists(testDataPath_Guid) && File.Exists(testDataPath_Int))
            {
                // Import Guid test.
                using (var streamReader = new StreamReader(testDataPath_Guid))
                    benchmarkData_Guid = JsonSerializer.Deserialize<BenchmarkData<Guid>>(streamReader.ReadToEnd());

                // Import int test.
                using (var streamReader = new StreamReader(testDataPath_Int))
                    benchmarkData_Int = JsonSerializer.Deserialize<BenchmarkData<int>>(streamReader.ReadToEnd());
                
                return;
            }
            
            string[] names;

            // Import the name bank.
            using (var streamReader = new StreamReader(Path.Combine(projectDirectory, "name_bank.json")))
                names = JsonSerializer.Deserialize<string[]>(streamReader.ReadToEnd()).Distinct().ToArray();

            if (testCount > names.Length / 2)
                throw new InvalidOperationException("Test data item count cannot exceed half the provided names.");

            var testData_Guid = new BenchmarkData<Guid>();
            var testData_Int = new BenchmarkData<int>();

            var originalData = names.Take(testCount).ToArray();
            
            for (var i = 0; i < testCount; i++)
            {
                var id_Guid = Guid.NewGuid();
                var id_Int = i;

                #region Original Data

                var nameSplit = originalData[i].Split(' ');

                // Guid person objects.
                (testData_Guid.OriginalData ??= new Person<Guid>[testCount])[i] = new Person<Guid>
                {
                    FirstName = nameSplit[0],
                    LastName = nameSplit[1],
                    ID = id_Guid
                };

                // Int person objects.
                (testData_Int.OriginalData ??= new Person<int>[testCount])[i] = new Person<int>
                {
                    FirstName = nameSplit[0],
                    LastName = nameSplit[1],
                    ID = id_Int
                };

                #endregion
            }

            var indexes = new int[testCount];

            for (var i = 0; i < testCount; i++)
                indexes[i] = i;

            var removedIndexes = indexes.ToList().RemoveRandom(indexes.Length / 2).ToArray();
            var shuffledIndexes = indexes.ToList().Shuffle().ToArray();

            var dummyData = names.Skip(testCount).Take(testCount).ToArray();

            var insertions_Guid = new List<Person<Guid>>();
            var insertions_Int = new List<Person<int>>();

            for (var i = 0; i < testCount; i++)
            {
                var id_Guid = testData_Guid.OriginalData[i].ID;
                var id_Int = testData_Int.OriginalData[i].ID;

                var dummyNameSplit = dummyData[i].Split(' ');
                var removedIndex = Array.IndexOf(removedIndexes, i);

                if (removedIndex != -1)
                {
                    #region Insertions Test Data

                    insertions_Guid.Add(new Person<Guid>
                    {
                        FirstName = dummyNameSplit[0],
                        LastName = dummyNameSplit[1],
                        ID = Guid.NewGuid()
                    });

                    insertions_Int.Add(new Person<int>
                    {
                        FirstName = dummyNameSplit[0],
                        LastName = dummyNameSplit[1],
                        ID = id_Int + testCount
                    });

                    #endregion

                    #region Removals Test Data

                    if (removedIndex != -1)
                    {
                        (testData_Guid.RemovalsTestData ??= new Person<Guid>[removedIndexes.Length])[removedIndex] = testData_Guid.OriginalData[i];

                        (testData_Int.RemovalsTestData ??= new Person<int>[removedIndexes.Length])[removedIndex] = testData_Int.OriginalData[i];
                    }

                    #endregion
                }

                #region Moves Test Data

                var shuffledPerson_Guid = testData_Guid.OriginalData[shuffledIndexes[i]];

                (testData_Guid.MovesTestData ??= new Person<Guid>[testCount])[i] = new Person<Guid>
                {
                    FirstName = shuffledPerson_Guid.FirstName,
                    LastName = shuffledPerson_Guid.LastName,
                    ID = shuffledPerson_Guid.ID
                };

                var shuffledPerson_Int = testData_Int.OriginalData[shuffledIndexes[i]];

                (testData_Int.MovesTestData ??= new Person<int>[testCount])[i] = new Person<int>
                {
                    FirstName = shuffledPerson_Int.FirstName,
                    LastName = shuffledPerson_Int.LastName,
                    ID = shuffledPerson_Int.ID
                };
                #endregion

                #region Updates Test Data

                // Guid person objects.
                (testData_Guid.UpdatesTestData ??= new Person<Guid>[testCount])[i] = new Person<Guid>
                {
                    // New name, same ID.
                    FirstName = dummyNameSplit[0],
                    LastName = dummyNameSplit[1],
                    ID = id_Guid
                };

                // Int person objects.
                (testData_Int.UpdatesTestData ??= new Person<int>[testCount])[i] = new Person<int>
                {
                    // New name, same ID.
                    FirstName = dummyNameSplit[0],
                    LastName = dummyNameSplit[1],
                    ID = id_Int
                };

                #endregion
            }

            // Add calculated insertions to original data.
            testData_Guid.InsertionTestData = testData_Guid.OriginalData.ToList().InsertRandom(insertions_Guid).ToArray();
            testData_Int.InsertionTestData = testData_Int.OriginalData.ToList().InsertRandom(insertions_Int).ToArray();

            // Serialize and save the tests.
            using (var streamWriter = new StreamWriter(testDataPath_Guid))
                streamWriter.WriteLine(JsonSerializer.Serialize(testData_Guid));

            using (var streamWriter = new StreamWriter(testDataPath_Int))
                streamWriter.WriteLine(JsonSerializer.Serialize(testData_Int));

            benchmarkData_Guid = testData_Guid;
            benchmarkData_Int = testData_Int;
        }
    }
}

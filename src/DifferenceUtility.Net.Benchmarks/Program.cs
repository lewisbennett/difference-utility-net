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
        #region Configuration Constants
        /// <summary>
        /// Change this to the number of entries in the test data to benchmark.
        /// </summary>
        public const int TestCount = 10;
        #endregion
        
        #region Public Methods
        /// <summary>
        /// // Main application entry point.
        /// </summary>
        public static void Main()
        {
            GenerateTestData();

#if DEBUG
            BenchmarkRunner.Run<DifferenceUtilityBenchmarks>(new BenchmarkDotNet.Configs.DebugInProcessConfig());
#else
            BenchmarkRunner.Run<DifferenceUtilityBenchmarks>();
#endif
        }
        #endregion
        
        #region Private Methods
        /// <summary>
        /// Generates and saves test data, JSON serialized, to the project directory.
        ///
        /// Test data is generated in bundles. This means that the data will be the same, only the format and value of <see cref="Person{T}.ID" /> will changed.
        /// 
        /// This method will return if test data already exists for all supported ID data types, but will continue, and overwrite, any existing test data if any are missing.
        /// </summary>
        private static void GenerateTestData()
        {
            // Make sure the count is a multiple of two.
            const int testCount = TestCount - TestCount % 2;

            // Sanity check in case of TestCount value being changed manually.
            if (testCount < 2)
                throw new InvalidOperationException("Invalid test data item count provided.");

            var projectDirectory = Environment.GetEnvironmentVariable("PROJECT_DIRECTORY");

            if (string.IsNullOrWhiteSpace(projectDirectory))
                throw new InvalidOperationException("PROJECT_DIRECTORY environment variable missing.");
            
            var testDataPath = Path.Combine(projectDirectory, "TestData");

            var testDataPath_Guid = Path.Combine(testDataPath, $"test_data_guid_{testCount}.json");
            var testDataPath_Int = Path.Combine(testDataPath, $"test_data_int_{testCount}.json");

            // No need to generate new test data if it already exists.
            if (File.Exists(testDataPath_Guid) && File.Exists(testDataPath_Int))
                return;
            
            string[] names;

            // Import the name bank.
            using (var streamReader = new StreamReader(Path.Combine(projectDirectory, "name_bank.json")))
                names = JsonSerializer.Deserialize<string[]>(streamReader.ReadToEnd()).Distinct().ToArray();
            
            // Maximum number of test entries is 50% the length of the provided name bank,
            // as half of the names will be used for original and dummy data.
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
        }
        #endregion
    }
}

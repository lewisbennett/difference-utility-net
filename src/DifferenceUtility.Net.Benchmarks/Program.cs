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
        #region Fields
        private static readonly Random _random = new();
        #endregion
        
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
            GenerateAllBenchmarkData();
            
            // Comment/uncomment required benchmarks.
#if DEBUG
            BenchmarkRunner.Run<DifferenceUtilityBenchmarks_Guid>(new DebugInProcessConfig());
            BenchmarkRunner.Run<DifferenceUtilityBenchmarks_Int>(new DebugInProcessConfig());
#else
            BenchmarkRunner.Run<DifferenceUtilityBenchmarks_Guid>();
            BenchmarkRunner.Run<DifferenceUtilityBenchmarks_Int>();
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

            // Insertions: +50% dummy data inserted randomly.
            // Moves: 50% original data moved randomly
            // Removals: 50% original data removed randomly.
            // Updates: 50% original data names updated with dummy names - IDs remain the same.
            // Everything: TODO
            
            var originalData = names.Take(testCount).Select(n =>
            {
                var nameSplit = n.Split(' ');

                return (FirstName: nameSplit[0], LastName: nameSplit[1]);

            }).ToArray();
            
            var dummyData = names.Take(testCount).Select(n =>
            {
                var nameSplit = n.Split(' ');

                return (FirstName: nameSplit[0], LastName: nameSplit[1]);

            }).ToArray();

            const int halfTestCount = testCount / 2;
            
            var insertionsIndexes = GetRandomIndexes(halfTestCount);
            var (movesFromIndexes, movesToIndexes) = (GetRandomIndexes(halfTestCount), GetRandomIndexes(halfTestCount).ToList());
            var removalsIndexes = GetRandomIndexes(halfTestCount);
            var updatesIndexes = GetRandomIndexes(halfTestCount);

            var insertions = originalData.ToList();
            var moves = originalData.ToList();
            var removals = originalData.ToList();
            var updates = originalData.ToList();
            
            for (var i = 0; i < testCount; i++)
            {
                // Handle insert.
                var insertIndex = Array.IndexOf(insertionsIndexes, i);
                
                if (insertIndex != -1)
                    insertions.Insert(insertIndex, dummyData[i]);

                // Handle move.
                var moveIndex = Array.IndexOf(movesFromIndexes, i);

                if (moveIndex != -1)
                {
                    var nameToMove = moves[moveIndex];
                    var toIndex = movesToIndexes.First();
                    
                    moves.Remove(nameToMove);
                    moves.Insert(toIndex, nameToMove);

                    movesToIndexes.Remove(toIndex);
                }

                    // Handle remove.
                var removalIndex = Array.IndexOf(removalsIndexes, i);
                
                if (removalIndex != -1)
                    removals.RemoveAt(removalIndex);
                
                // Handle update.
                var updateIndex = Array.IndexOf(updatesIndexes, i);

                if (updateIndex != -1)
                    updates[updateIndex] = dummyData[i];
            }

            var testData_Guid = GenerateBenchmarkData(originalData, dummyData, insertions, moves, removals, updates,
                _ => Guid.NewGuid(),
                _ => Guid.NewGuid());
            
            var testData_Int = GenerateBenchmarkData(originalData, dummyData, insertions, moves, removals, updates,
                x => Array.IndexOf(originalData, x),
                x => Array.IndexOf(dummyData, x));
            
            // Serialize and save the tests.
            using (var streamWriter = new StreamWriter(testDataPath_Guid))
                streamWriter.WriteLine(JsonSerializer.Serialize(testData_Guid));

            using (var streamWriter = new StreamWriter(testDataPath_Int))
                streamWriter.WriteLine(JsonSerializer.Serialize(testData_Int));
        }
        
        private static BenchmarkData<T> GenerateBenchmarkData<T>(IList<(string FirstName, string LastName)> originalData,
            IEnumerable<(string FirstName, string LastName)> dummyData,
            IEnumerable<(string FirstName, string LastName)> insertions,
            IEnumerable<(string FirstName, string LastName)> moves,
            IEnumerable<(string FirstName, string LastName)> removals,
            IList<(string FirstName, string LastName)> updates,
            Func<(string FirstName, string LastName), T> originalDataCalculateId,
            Func<(string FirstName, string LastName), T> dummyDataCalculateId)
        {
            var benchmarkData = new BenchmarkData<T>();

            var originalDataIds = originalData.ToDictionary(o => o, originalDataCalculateId);
            var dummyDataIds = dummyData.ToDictionary(d => d, dummyDataCalculateId);
            
            benchmarkData.OriginalData = originalData.Select(o => new Person<T>
            {
                FirstName = o.FirstName,
                LastName = o.LastName,
                ID = originalDataIds[o]
                
            }).ToArray();

            benchmarkData.InsertionTestData = insertions.Select(i => new Person<T>
            {
                FirstName = i.FirstName,
                LastName = i.LastName,
                ID = originalDataIds.TryGetValue(i, out var id) ? id : dummyDataIds[i]
                
            }).ToArray();

            benchmarkData.MovesTestData = moves.Select(m => new Person<T>
            {
                FirstName = m.FirstName,
                LastName = m.LastName,
                ID = originalDataIds[m]

            }).ToArray();
            
            benchmarkData.RemovalsTestData = removals.Select(r => new Person<T>
            {
                FirstName = r.FirstName,
                LastName = r.LastName,
                ID = originalDataIds[r]

            }).ToArray();
            
            benchmarkData.UpdatesTestData = updates.Select(u => new Person<T>
            {
                FirstName = u.FirstName,
                LastName = u.LastName,
                ID = originalDataIds[originalData[updates.IndexOf(u)]]
                
            }).ToArray();

            return benchmarkData;
        }
        
        private static int[] GetRandomIndexes(int count)
        {
            var indexes = new List<int>();

            for (var i = 0; i < count; i++)
                indexes.Add(i);
            
            while (count > 1)
            {
                count--;
                
                var k = _random.Next(count + 1);
                
                (indexes[k], indexes[count]) = (indexes[count], indexes[k]);
            }

            return indexes.ToArray();
        }
        #endregion
    }
}

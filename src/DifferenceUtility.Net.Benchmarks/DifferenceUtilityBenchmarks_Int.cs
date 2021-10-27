using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DifferenceUtility.Net.Benchmarks.Data;

namespace DifferenceUtility.Net.Benchmarks
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class DifferenceUtilityBenchmarks_Int
    {
        #region Fields
        private readonly PersonDiffCallback_Gen_Int _personDiffCallback_Gen = new();
        #endregion
        
        #region Benchmark Methods
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData))]
        public DiffResult<Person<int>> CalculateDiffResult_Insertions_Gen(BenchmarkData<int> benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.InsertionTestData, _personDiffCallback_Gen);
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData))]
        public DiffResult<Person<int>> CalculateDiffResult_Moves_Gen(BenchmarkData<int> benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.MovesTestData, _personDiffCallback_Gen);
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData))]
        public DiffResult<Person<int>> CalculateDiffResult_Removals_Gen(BenchmarkData<int> benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.RemovalsTestData, _personDiffCallback_Gen);
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData))]
        public DiffResult<Person<int>> CalculateDiffResult_Updates_Gen(BenchmarkData<int> benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.UpdatesTestData, _personDiffCallback_Gen);
        }
        #endregion
        
        #region Helper Methods
        public IEnumerable<object> GetBenchmarkData()
        {
            using var streamReader = new StreamReader(Path.Combine(Environment.GetEnvironmentVariable("PROJECT_DIRECTORY"), $"TestData/test_data_int_{Program.TestCount}.json"));
            
            yield return JsonSerializer.Deserialize<BenchmarkData<int>>(streamReader.ReadToEnd());
        }
        #endregion
    }
}

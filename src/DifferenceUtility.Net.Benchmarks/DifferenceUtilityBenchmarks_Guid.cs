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
    public class DifferenceUtilityBenchmarks_Guid
    {
        #region Fields
        private readonly PersonDiffCallback_Gen_Guid _personDiffCallback_Gen = new();
        #endregion
        
        #region Guid Benchmark Methods
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData))]
        public DiffResult<Person<Guid>> CalculateDiffResult_Insertions_Gen(BenchmarkData<Guid> benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.InsertionTestData, _personDiffCallback_Gen);
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData))]
        public DiffResult<Person<Guid>> CalculateDiffResult_Moves_Gen(BenchmarkData<Guid> benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.MovesTestData, _personDiffCallback_Gen);
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData))]
        public DiffResult<Person<Guid>> CalculateDiffResult_Removals_Gen(BenchmarkData<Guid> benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.RemovalsTestData, _personDiffCallback_Gen);
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData))]
        public DiffResult<Person<Guid>> CalculateDiffResult_Updates_Gen(BenchmarkData<Guid> benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.UpdatesTestData, _personDiffCallback_Gen);
        }
        #endregion

        #region Helper Methods
        public IEnumerable<object> GetBenchmarkData()
        {
            using var streamReader = new StreamReader(Path.Combine(Environment.GetEnvironmentVariable("PROJECT_DIRECTORY"), $"TestData/test_data_guid_{Program.TestCount}.json"));
            
            yield return JsonSerializer.Deserialize<BenchmarkData<Guid>>(streamReader.ReadToEnd());
        }
        #endregion
    }
}

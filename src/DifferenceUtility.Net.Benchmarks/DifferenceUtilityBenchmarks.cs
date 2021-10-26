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
    public class DifferenceUtilityBenchmarks
    {
        #region Fields
        private readonly PersonDiffCallback_Gen_Guid _personDiffCallback_Gen_Guid = new();
        private readonly PersonDiffCallback_Gen_Int _personDiffCallback_Gen_Int = new();
        #endregion
        
        #region Guid Benchmark Methods
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData_Guid))]
        public DiffResult<Person<Guid>> CalculateDiffResult_Insertions_Gen_Guid(BenchmarkData<Guid> benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.InsertionTestData, _personDiffCallback_Gen_Guid);
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData_Guid))]
        public DiffResult<Person<Guid>> CalculateDiffResult_Moves_Gen_Guid(BenchmarkData<Guid> benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.MovesTestData, _personDiffCallback_Gen_Guid);
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData_Guid))]
        public DiffResult<Person<Guid>> CalculateDiffResult_Removals_Gen_Guid(BenchmarkData<Guid> benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.RemovalsTestData, _personDiffCallback_Gen_Guid);
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData_Guid))]
        public DiffResult<Person<Guid>> CalculateDiffResult_Updates_Gen_Guid(BenchmarkData<Guid> benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.UpdatesTestData, _personDiffCallback_Gen_Guid);
        }
        #endregion
        
        #region Int Benchmark Methods
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData_Int))]
        public DiffResult<Person<int>> CalculateDiffResult_Insertions_Gen_Int(BenchmarkData<int> benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.InsertionTestData, _personDiffCallback_Gen_Int);
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData_Int))]
        public DiffResult<Person<int>> CalculateDiffResult_Moves_Gen_Int(BenchmarkData<int> benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.MovesTestData, _personDiffCallback_Gen_Int);
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData_Int))]
        public DiffResult<Person<int>> CalculateDiffResult_Removals_Gen_Int(BenchmarkData<int> benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.RemovalsTestData, _personDiffCallback_Gen_Int);
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData_Int))]
        public DiffResult<Person<int>> CalculateDiffResult_Updates_Gen_Int(BenchmarkData<int> benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.UpdatesTestData, _personDiffCallback_Gen_Int);
        }
        #endregion
        
        #region Helper Methods
        public IEnumerable<object> GetBenchmarkData_Guid()
        {
            using var streamReader = new StreamReader(Path.Combine(Environment.GetEnvironmentVariable("PROJECT_DIRECTORY"), $"TestData/test_data_guid_{Program.TestCount}.json"));
            
            yield return JsonSerializer.Deserialize<BenchmarkData<Guid>>(streamReader.ReadToEnd());
        }
        
        public IEnumerable<object> GetBenchmarkData_Int()
        {
            using var streamReader = new StreamReader(Path.Combine(Environment.GetEnvironmentVariable("PROJECT_DIRECTORY"), $"TestData/test_data_int_{Program.TestCount}.json"));
            
            yield return JsonSerializer.Deserialize<BenchmarkData<int>>(streamReader.ReadToEnd());
        }
        #endregion
    }
}

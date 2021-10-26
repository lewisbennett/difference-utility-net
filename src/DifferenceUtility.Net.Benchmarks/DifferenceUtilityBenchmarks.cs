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
        #endregion
        
        #region Benchmark Methods
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData_Guid))]
        public DiffResult<Person<Guid>> CalculateDiffResult_Insertions_Gen_Guid(BenchmarkData<Guid> benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.InsertionTestData, _personDiffCallback_Gen_Guid);
        }
        #endregion
        
        #region Helper Methods
        public IEnumerable<object> GetBenchmarkData_Guid()
        {
            using var streamReader = new StreamReader(Path.Combine(Environment.GetEnvironmentVariable("PROJECT_DIRECTORY"), $"TestData/test_data_guid_{Program.TestCount}.json"));
            
            yield return JsonSerializer.Deserialize<BenchmarkData<Guid>>(streamReader.ReadToEnd());
        }
        #endregion
    }
}

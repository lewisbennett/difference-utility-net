﻿using System;
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
        private readonly PersonDiffCallback_Guid _personDiffCallback = new();
        #endregion
        
        #region Guid Benchmark Methods
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData))]
        public DiffResult<Person<Guid>, Person<Guid>> CalculateDiffResult_Insertions(BenchmarkData<Guid> benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.InsertionTestData, _personDiffCallback);
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData))]
        public DiffResult<Person<Guid>, Person<Guid>> CalculateDiffResult_Moves(BenchmarkData<Guid> benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.MovesTestData, _personDiffCallback, false);
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData))]
        public DiffResult<Person<Guid>, Person<Guid>> CalculateDiffResult_Moves_DetectMoves(BenchmarkData<Guid> benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.MovesTestData, _personDiffCallback);
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData))]
        public DiffResult<Person<Guid>, Person<Guid>> CalculateDiffResult_Removals(BenchmarkData<Guid> benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.RemovalsTestData, _personDiffCallback);
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData))]
        public DiffResult<Person<Guid>, Person<Guid>> CalculateDiffResult_Updates(BenchmarkData<Guid> benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.UpdatesTestData, _personDiffCallback);
        }
        #endregion

        #region Helper Methods
        public static IEnumerable<object> GetBenchmarkData()
        {
            using var streamReader = new StreamReader(Path.Combine(Environment.GetEnvironmentVariable("PROJECT_DIRECTORY"), $"TestData/test_data_guid_{Program.TestCount}.json"));
            
            yield return JsonSerializer.Deserialize<BenchmarkData<Guid>>(streamReader.ReadToEnd());
        }
        #endregion
    }
}

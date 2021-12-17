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
        private readonly IntDiffCallback _intDiffCallback = new();
        #endregion
        
        #region Benchmark Methods
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData))]
        public DiffResult<int, int> CalculateDiffResult_Everything(BenchmarkData benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.EverythingTestData, _intDiffCallback, false);
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData))]
        public DiffResult<int, int> CalculateDiffResult_Everything_DetectMoves(BenchmarkData benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.EverythingTestData, _intDiffCallback);
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData))]
        public DiffResult<int, int> CalculateDiffResult_Insertions(BenchmarkData benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.InsertionTestData, _intDiffCallback, false);
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData))]
        public DiffResult<int, int> CalculateDiffResult_Insertions_DetectMoves(BenchmarkData benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.InsertionTestData, _intDiffCallback);
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData))]
        public DiffResult<int, int> CalculateDiffResult_Moves(BenchmarkData benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.MovesTestData, _intDiffCallback, false);
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData))]
        public DiffResult<int, int> CalculateDiffResult_Moves_DetectMoves(BenchmarkData benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.MovesTestData, _intDiffCallback);
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData))]
        public DiffResult<int, int> CalculateDiffResult_Removals(BenchmarkData benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.RemovalsTestData, _intDiffCallback, false);
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(GetBenchmarkData))]
        public DiffResult<int, int> CalculateDiffResult_Removals_DetectMoves(BenchmarkData benchmarkData)
        {
            return DiffUtil.CalculateDiff(benchmarkData.OriginalData, benchmarkData.RemovalsTestData, _intDiffCallback);
        }
        #endregion
        
        #region Helper Methods
        public static IEnumerable<object> GetBenchmarkData()
        {
            using var streamReader = new StreamReader(Path.Combine(Environment.GetEnvironmentVariable("PROJECT_DIRECTORY"), $"TestData/test_data_{Program.TestCount}.json"));
            
            yield return JsonSerializer.Deserialize<BenchmarkData>(streamReader.ReadToEnd());
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DifferenceUtility.Net.Benchmarks.Data;

namespace DifferenceUtility.Net.Benchmarks;

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
    public void Everything(BenchmarkData benchmarkData)
    {
        var observableCollection = new ObservableCollection<int>(benchmarkData.OriginalData);

        var diffResult = DiffUtil.CalculateDiff(observableCollection, benchmarkData.EverythingTestData, _intDiffCallback, false);

        diffResult.DispatchUpdatesTo(observableCollection);
    }

    [Benchmark]
    [ArgumentsSource(nameof(GetBenchmarkData))]
    public void Everything_DetectMoves(BenchmarkData benchmarkData)
    {
        var observableCollection = new ObservableCollection<int>(benchmarkData.OriginalData);

        var diffResult = DiffUtil.CalculateDiff(observableCollection, benchmarkData.EverythingTestData, _intDiffCallback);

        diffResult.DispatchUpdatesTo(observableCollection);
    }

    [Benchmark]
    [ArgumentsSource(nameof(GetBenchmarkData))]
    public void Insertions(BenchmarkData benchmarkData)
    {
        var observableCollection = new ObservableCollection<int>(benchmarkData.OriginalData);

        var diffResult = DiffUtil.CalculateDiff(observableCollection, benchmarkData.InsertionTestData, _intDiffCallback, false);

        diffResult.DispatchUpdatesTo(observableCollection);
    }

    [Benchmark]
    [ArgumentsSource(nameof(GetBenchmarkData))]
    public void Insertions_DetectMoves(BenchmarkData benchmarkData)
    {
        var observableCollection = new ObservableCollection<int>(benchmarkData.OriginalData);

        var diffResult = DiffUtil.CalculateDiff(observableCollection, benchmarkData.InsertionTestData, _intDiffCallback);

        diffResult.DispatchUpdatesTo(observableCollection);
    }

    [Benchmark]
    [ArgumentsSource(nameof(GetBenchmarkData))]
    public void Moves(BenchmarkData benchmarkData)
    {
        var observableCollection = new ObservableCollection<int>(benchmarkData.OriginalData);

        var diffResult = DiffUtil.CalculateDiff(observableCollection, benchmarkData.MovesTestData, _intDiffCallback, false);

        diffResult.DispatchUpdatesTo(observableCollection);
    }

    [Benchmark]
    [ArgumentsSource(nameof(GetBenchmarkData))]
    public void Moves_DetectMoves(BenchmarkData benchmarkData)
    {
        var observableCollection = new ObservableCollection<int>(benchmarkData.OriginalData);

        var diffResult = DiffUtil.CalculateDiff(observableCollection, benchmarkData.MovesTestData, _intDiffCallback);

        diffResult.DispatchUpdatesTo(observableCollection);
    }

    [Benchmark]
    [ArgumentsSource(nameof(GetBenchmarkData))]
    public void Removals(BenchmarkData benchmarkData)
    {
        var observableCollection = new ObservableCollection<int>(benchmarkData.OriginalData);

        var diffResult = DiffUtil.CalculateDiff(observableCollection, benchmarkData.RemovalsTestData, _intDiffCallback, false);

        diffResult.DispatchUpdatesTo(observableCollection);
    }

    [Benchmark]
    [ArgumentsSource(nameof(GetBenchmarkData))]
    public void Removals_DetectMoves(BenchmarkData benchmarkData)
    {
        var observableCollection = new ObservableCollection<int>(benchmarkData.OriginalData);

        var diffResult = DiffUtil.CalculateDiff(observableCollection, benchmarkData.RemovalsTestData, _intDiffCallback);

        diffResult.DispatchUpdatesTo(observableCollection);
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
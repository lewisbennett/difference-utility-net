# Benchmarks

This project uses [BenchmarkDotNet](https://benchmarkdotnet.org/articles/overview.html) to execute a series of benchmark
tests for [DifferenceUtility.Net](../DifferenceUtility.Net).

## Environment Variables

| Name | Description |
|-----|-----|
| PROJECT_DIRECTORY | The [project directory](../DifferenceUtility.Net.Benchmarks) on the host machine. |

## Generating New Benchmark Data

To generate new benchmark data, change the value of [`TestCount`](../DifferenceUtility.Net.Benchmarks/Program.cs#L17)
and run the project. The minimum value is `2`, and the maximum is 50% of the total number of names
in [`name_bank.json`](../DifferenceUtility.Net.Benchmarks/name_bank.json).

using System;
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
        public DiffResult<Person<Guid>> CalculateDiffResult_Insertions_Gen_Guid()
        {
            return DiffUtil.CalculateDiff(Program.BenchmarkData_Guid.OriginalData, Program.BenchmarkData_Guid.InsertionTestData, _personDiffCallback_Gen_Guid);
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using TestCoverage.CoverageCalculation;

namespace TestCoverage
{
    public class CoverageResult
    {
        public CoverageResult(LineCoverage[] coverage)
        {
            CoverageByDocument = coverage.
                GroupBy(x => x.DocumentPath).
                ToDictionary(x => x.Key, x => x.ToArray());
        }

        public Dictionary<string,LineCoverage[]> CoverageByDocument { get; set; }
    }
}
﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestSolution;

namespace Tests
{
    [TestFixture]
    public class PathHelperTests
    {
        [Test]
        public void GetRewrittenFilePath()
        {
            const string path = "c:\test.dll";
            const string expectedRewrittenPath = "c:\test.dll_testcove1rage";





            string rewrittenPath = PathHelper.GetRewrittenFilePath(path);


            Assert.That(rewrittenPath, Is.EqualTo(expectedRewrittenPath));
        }

        [Test]
        public void GetCoverageDllName()
        {
            const string expectedDllName = "Logic_COVERAGE.dll";
            string coverageDllName = PathHelper.GetCoverageDllName("Logic");

            Assert.That(coverageDllName, Is.EqualTo(expectedDllName));
        }
    }
}
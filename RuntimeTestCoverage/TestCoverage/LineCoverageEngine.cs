using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace TestCoverage
{
    public class LineCoverageEngine :MarshalByRefObject
    {
        public int[] CalculateForAllDocuments(string solutionPath)
        {
            var rewritter=new SolutionRewritter();
            RewriteResult rewriteResult = rewritter.RewriteAllClasses(solutionPath);  

            var lineCoverageCalc = new LineCoverageCalc();
            return lineCoverageCalc.CalculateForAllTests(solutionPath, rewriteResult);            
            
        }

        public int[] CalculateForTest(string solutionPath, string documentName, string className, string methodName)
        {
            var rewritter = new SolutionRewritter();
            Document[] allDocuments = rewritter.GetDocuments(solutionPath).ToArray();
            var document = allDocuments.Single(d => d.Name == documentName);
            string documentContent = document.GetTextAsync().Result.ToString();
            var rewriteResult = rewritter.RewriteTestClass(documentName, documentContent);

            var lineCoverageCalc = new LineCoverageCalc();
            SyntaxTree[] allTrees = allDocuments.Select(d => d.GetSyntaxTreeAsync().Result).ToArray();
            return lineCoverageCalc.CalculateForTest(allTrees,rewriteResult.Item1 ,solutionPath, documentContent, className, methodName);

        }
    }
}
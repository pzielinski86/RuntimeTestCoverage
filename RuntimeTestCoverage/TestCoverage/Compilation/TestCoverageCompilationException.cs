using System;
using System.Runtime.Serialization;

namespace TestCoverage.Compilation
{
    [Serializable]
    internal class TestCoverageCompilationException : Exception
    {
        public TestCoverageCompilationException(string[] errors) :base("Cannot compile test coverage exception.")
        {
            Errors = errors;
        }
        protected TestCoverageCompilationException(
          SerializationInfo info,
          StreamingContext context) : base(info, context)
        {
            Errors = (string[])info.GetValue("errors", typeof (string[]));
        }

        public string[] Errors { get; private set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("errors",Errors);
            base.GetObjectData(info, context);
        }
    }
}
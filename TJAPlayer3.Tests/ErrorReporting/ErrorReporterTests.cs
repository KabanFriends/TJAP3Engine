using NUnit.Framework;
using TJAPlayer3.ErrorReporting;

namespace TJAPlayer3.Tests.ErrorReporting
{
    [TestFixture]
    public sealed class ErrorReporterTests
    {
        [Test]
        [TestCase("", "47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=")]
        [TestCase(" ", "Nqnn8clbgv+5l0PgxcTOldg8mkMKrFn4TvPL+rYUUGg=")]
        [TestCase("Default", "IbERy/5uj8otGBxD9TrVSLIuOKypVbmCRwalBLCgei0=")]
        [TestCase("SimpleStyle", "U6QVPvJpFuDf1y6cxYbW9D+LvrG7PYLVFwDRY4xdLeM=")]
        [TestCase("This is only a test", "pPi+NdUkNVp81f+/9Vi7dvgVdtr6f6WpdqqjVD8ptCo=")]
        public void TestToSha256InBase64(string input, string expected)
        {
            var actual = ErrorReporter.ToSha256InBase64(input);

            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}

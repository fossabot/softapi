using NUnit.Framework;

namespace SoftAPIClient.Tests
{
    [Parallelizable(ParallelScope.Fixtures)]
    [SetUpFixture]
    public class GlobalSetup
    {
        [OneTimeSetUp]
        public void OneTimeBaseSetup()
        {
        }

        [OneTimeTearDown]
        public void AfterAll()
        {
        }
    }

    [TestFixture]
    public abstract class AbstractTest
    {
    }
}
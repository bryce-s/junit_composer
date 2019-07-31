using System;
using NUnit.Framework;
using junit_composer;
using System.IO;

namespace test
{
    [TestFixture]
    public class Class1
    {
        static private string get_test_cwd()
        {
            return TestContext.CurrentContext.TestDirectory;
        }

        [Test]
        public void hey()
        {
            string context = get_test_cwd();
            string test_target = String.Format("test_files{0}spec_example.junit", Path.DirectorySeparatorChar);
            string filename = Path.Combine(context, test_target);
            Composer.ComposeTestSuites(filename);
        }
    }
}

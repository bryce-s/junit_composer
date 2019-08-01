using System;
using NUnit.Framework;
using junit_composer;
using System.IO;

namespace test
{
    [TestFixture]
    public class Class1
    {

        private string spec_example = "spec_example.junit";

        static private string get_test_cwd()
        {
            return TestContext.CurrentContext.TestDirectory;
        }

        static private string build_filename(string test_target)
        {
            string context = get_test_cwd();
            string test_target_path = String.Format("test_files{0}spec_example.junit", Path.DirectorySeparatorChar);
            return Path.Combine(context, test_target_path);
        } 

        [Test]
        public void test_suites_spec()
        {
           Composer.ComposeTestSuites(build_filename(spec_example));
        }

        [Test]
        public void test_cases_spec()
        {
            Composer.ComposeTestCases(build_filename(spec_example));
            
        }

    }
}

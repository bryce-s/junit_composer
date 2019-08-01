using System;
using NUnit.Framework;
using junit_composer;
using System.IO;
using System.Text.RegularExpressions;

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

        static private void write_result_to_file(string content, string filename = "out.junit")
        {
            string context = get_test_cwd();
            string path_target = Path.Combine(context, filename);
            if (File.Exists(path_target))
            {
                File.Delete(path_target);
            }
            using (StreamWriter sw = File.CreateText(path_target))
            {
                sw.Write(content);
            }

        }

        static private void match_tag(string tag, string input)
        {
            string pattern = String.Format(@"<{0}[\s\S]+<\/{0}>", tag);
            Match m = Regex.Match(input, pattern, RegexOptions.ECMAScript);
            Assert.True(m.Success);
        }


        [Test]
        public void suites_spec()
        {
            string res = Composer.ComposeTestSuites(build_filename(spec_example));
            write_result_to_file(content: res);
            match_tag("testsuite", res);
            match_tag("testcase", res);
            match_tag("testsuites", res);
        }

        [Test]
        public void cases_spec()
        {
            //Composer.ComposeTestCases(build_filename(spec_example));
            
        }

    }
}

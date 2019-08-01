using System;
using NUnit.Framework;
using junit_composer;
using System.IO;
using System.Text.RegularExpressions;

namespace test
{
    [TestFixture]
    public class Test
    {

        private string spec_example = "spec_example.junit";

        private string mode_cases = "cases";
        private string mode_suites = "suites";

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

        
        static private string read_correct_file(string filename, string mode)
        {
            string context = get_test_cwd();
            string target = String.Format("correct{0}{1}-{2}.correct", Path.DirectorySeparatorChar, filename, mode);
            string target_path = Path.Combine(context, target);
            return File.ReadAllText(target_path);
        }

        // returns true if strings do not differ.
        static private bool diff_stings(string lhs_result, string rhs_result)
        {
            return lhs_result == rhs_result;
        }

        void match_main_tags(string res)
        {
            match_tag("testsuite", res);
            match_tag("testsuites", res);
            match_tag("testcase", res);
        }

        // Tests below:


        [Test]
        public void log_correct_tests()
        {
            bool correct = false;
            if (correct)
            {
                string suites_res = Composer.ComposeTestSuites(build_filename(spec_example));
                string cases_res = Composer.ComposeTestCases(build_filename(spec_example));
                string cases_target = String.Format("correct{0}{1}-{2}.correct", Path.DirectorySeparatorChar, spec_example, mode_cases);
                string suites_target = String.Format("correct{0}{1}-{2}.correct", Path.DirectorySeparatorChar, spec_example, mode_suites);
                write_result_to_file(cases_res, cases_target);
                write_result_to_file(suites_res, suites_target);

            }
            Assert.False(correct);

        }


        [Test]
        public void suites_spec_tags_present()
        {
            string res = Composer.ComposeTestSuites(build_filename(spec_example));
            write_result_to_file(content: res);
            match_main_tags(res);
        }

        [Test]
        public void suites_spec_diff()
        {
            string res = Composer.ComposeTestSuites(build_filename(spec_example));
            string correct_string_unsanitized = read_correct_file(spec_example, mode_suites);
            Assert.True(diff_stings(res, correct_string_unsanitized)); 
        }

        [Test]
        public void suites_contains_tests_failures_errors()
        {
            string res = Composer.ComposeTestSuites(build_filename(spec_example));
            Assert.True(res.Contains("<testsuites tests=\"45\" failures=\"17\" errors=\"0\">"));
        }


        [Test]
        public void cases_spec_tags_present()
        {
            string res = Composer.ComposeTestCases(build_filename(spec_example));
            write_result_to_file(res);
            match_tag("testsuite", res);
            match_tag("testcase", res);
        }

        [Test]
        public void cases_contains_test_failures_errors()
        {
            string res = Composer.ComposeTestCases(build_filename(spec_example));
            Assert.True(res.Contains("<testsuite tests=\"45\" failures=\"17\" errors=\"0\">"));

        }

        [Test]
        public void cases_spec_diff()
        {
            string res = Composer.ComposeTestCases(build_filename(spec_example));
            string correct_string_unsanitized = read_correct_file(spec_example, mode_cases);
            Assert.True(diff_stings(res, correct_string_unsanitized));
        }


    }
}

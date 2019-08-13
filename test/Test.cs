using System;
using NUnit.Framework;
using junit_composer;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Collections.Generic;

namespace test
{
    [TestFixture]
    public class Test
    {

        private string spec_example = "spec_example.junit";

        private string sample_report = "sample_report.junit";

        private string mode_cases = "cases";
        private string mode_suites = "suites";

        static private string get_test_cwd()
        {
            return TestContext.CurrentContext.TestDirectory;
        }

        static private string build_filename(string test_target)
        {
            string context = get_test_cwd();
            string test_target_path = String.Format("test_files{0}{1}", Path.DirectorySeparatorChar, test_target);
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
            bool active = false;
            if (active)
            {
                string suites_res = Composer.ComposeTestSuites(build_filename(spec_example));
                string cases_res = Composer.ComposeTestCases(build_filename(spec_example));
                string cases_target = String.Format("correct{0}{1}-{2}.correct", Path.DirectorySeparatorChar, spec_example, mode_cases);
                string suites_target = String.Format("correct{0}{1}-{2}.correct", Path.DirectorySeparatorChar, spec_example, mode_suites);
                write_result_to_file(cases_res, cases_target);
                write_result_to_file(suites_res, suites_target);

                string suites_sample_res = Composer.ComposeTestSuites(build_filename(sample_report));
                string cases_sample_res = Composer.ComposeTestCases(build_filename(sample_report));

                string cases_sample_target = String.Format("correct{0}{1}-{2}.correct", Path.DirectorySeparatorChar, sample_report, mode_cases);
                string suites_sample_target = String.Format("correct{0}{1}-{2}.correct", Path.DirectorySeparatorChar, sample_report, mode_suites);
                write_result_to_file(suites_sample_res, suites_sample_target);
                write_result_to_file(cases_sample_res, cases_sample_target);
               

            }
            Assert.False(active);

        }



        [Test]
        public void header_check()
        {
            XDocument xdoc = Composer.set_up_junit_document();
            string expected = String.Format("{0}", "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
            Assert.True(xdoc.Declaration.ToString() == expected);
        }


        // checks the testsuites node exsits
        [Test]
        public void header_check_testsuite()
        {
            XDocument xdoc = Composer.set_up_junit_document();
            Assert.True(xdoc.ToString() == "<testsuites />");
        }

        [Test]
        public void header_check_empty()
        {
            XDocument xdoc = Composer.set_up_junit_document(testsuites_b: false);
            Assert.True(xdoc.ToString() == "");
        }



        [Test]
        public void create_single_testsuite()
        {
            XDocument xdoc = Composer.set_up_junit_document(testsuites_b: false);
            Composer.add_single_testsuite(xdoc);
            Assert.True(xdoc.ToString() == "<testsuite />");
        }


        [Test]
        public void create_single_no_init()
        {
            XDocument xdoc = new XDocument();
            Composer.add_single_testsuite(xdoc);
            Assert.True(xdoc.ToString() == "<testsuite />");
        }


        [Test]
        public void test_return_xelement_errors()
        {
            string library_usage_exception = "Library usage exception: can't call with these params.";
            try
            {
                Composer.return_xelement(null, null);
            }
            catch (Exception e)
            {
                Assert.True(e.Message == library_usage_exception);
            }
            try
            {
                Composer.return_xelement(new XElement("testsuite"), new XDocument());
            } catch (Exception e)
            {
                Assert.True(e.Message == library_usage_exception);
            }
        }

        [Test]
        public void add_attributes_zero()
        {
            XDocument xdoc = Composer.set_up_junit_document();
            var xei = xdoc.Descendants("testsuites").GetEnumerator();
            xei.MoveNext();
            var xelt = xei.Current;
            Composer.add_attributes(xelt);
            string xdoc_str = xdoc.ToString();
            string correc_str = "<testsuites tests=\"0\" failures=\"0\" errors=\"0\" />";
            Assert.True(correc_str == xdoc_str);
        }
        
        [Test]
        public void build_suite_empty()
        {
            List<XElement> test_cases = new List<XElement>();
            XDocument xdoc = Composer.build_test_suite(test_cases);
            Assert.True(xdoc.ToString() == "<testsuite tests=\"0\" failures=\"0\" errors=\"0\" />");
        }

        [Test]
        public void build_test_suites_empty()
        {
            List<XElement> test_cases = new List<XElement>();
            XDocument xdoc = Composer.build_test_suites(test_cases);
            string correct = "<testsuites tests=\"0\" failures=\"0\" errors=\"0\" />";
            Assert.True(correct == xdoc.ToString());
        }




        // System-level tests:

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


        [Test]
        public void cases_sample_report_diff()
        {
            string res = Composer.ComposeTestCases(build_filename(sample_report));
            string correct_string_unsanitized = read_correct_file(sample_report, mode_cases);
            Assert.True(diff_stings(res, correct_string_unsanitized));

        }


        [Test]
        public void suites_sample_report_diff()
        {
            string res = Composer.ComposeTestCases(build_filename(sample_report));
            string correct_string_unsanitized = read_correct_file(sample_report, mode_cases);
            Assert.True(diff_stings(res, correct_string_unsanitized));
        }

        [Test]
        public void cases_sample_report_tags()
        {
            string res = Composer.ComposeTestCases(build_filename(sample_report));
            Assert.True(res.Contains("<testsuite tests=\"1\" failures=\"1\" errors=\"1\">"));
            Assert.True(res.Contains("<testcase name=\"Basic_test\" time=\"0\" classname=\"test_ui.Rxn1000.Multi.Bio.Emulator\">"));
        }

        [Test]
        public void suites_sample_report_tags()
        {
            string res = Composer.ComposeTestSuites(build_filename(sample_report));
            Assert.True(res.Contains("<testsuites tests=\"1\" failures=\"1\" errors=\"1\">"));
            Assert.True(res.Contains("<testsuite name=\"test_ui\" tests=\"1\" failures=\"1\" errors=\"1\" time=\"1\" skipped=\"0\" timestamp=\"2019-07-26T19:53:23\" hostname=\"DESKTOP-JK3JHVC\">"));

        }


    }
}

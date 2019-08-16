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

        private string specExample = "spec_example.junit";

        private string sampleReport = "sample_report.junit";

        private string modeCases = "cases";
        private string modeSuites = "suites";

        static private string GetTestCwd()
        {
            return TestContext.CurrentContext.TestDirectory;
        }

        static private string BuildFilename(string testTarget)
        {
            string context = GetTestCwd();
            string testTargetPath = $"test_files{Path.DirectorySeparatorChar}{testTarget}";
            return Path.Combine(context, testTargetPath);
        }

        static private void writeResultToFile(string content, string filename = "out.junit")
        {
            string context = GetTestCwd();
            string pathTarget = Path.Combine(context, filename);
            if (File.Exists(pathTarget))
            {
                File.Delete(pathTarget);
            }
            using (StreamWriter sw = File.CreateText(pathTarget))
            {
                sw.Write(content);
            }
        }

        static private void matchTag(string tag, string input)
        {
            string pattern = $@"<{tag}[\s\S]+<\/{tag}>";
            Match m = Regex.Match(input, pattern, RegexOptions.ECMAScript);
            Assert.True(m.Success);
        }

        
        static private string readCorrectFile(string filename, string mode)
        {
            string context = GetTestCwd();
            string target = $"correct{Path.DirectorySeparatorChar}{filename}-{mode}.correct"; 
            string target_path = Path.Combine(context, target);
            return File.ReadAllText(target_path);
        }

        // returns true if strings do not differ.
        static private bool diffStrings(string lhs_result, string rhs_result)
        {
            return lhs_result == rhs_result;
        }

        void matchMainTags(string res)
        {
            matchTag("testsuite", res);
            matchTag("testsuites", res);
            matchTag("testcase", res);
        }

        // Tests below:


        [Test]
        public void LogCorrectTests()
        {
            bool active = false;
            if (active)
            {
                string suitesRes = Composer.ComposeTestSuites(BuildFilename(specExample));
                string casesRes = Composer.ComposeTestCases(BuildFilename(specExample));
                string casesTarget = String.Format("correct{0}{1}-{2}.correct", Path.DirectorySeparatorChar, specExample, modeCases);
                string suitesTarget = String.Format("correct{0}{1}-{2}.correct", Path.DirectorySeparatorChar, specExample, modeSuites);
                writeResultToFile(casesRes, casesTarget);
                writeResultToFile(suitesRes, suitesTarget);

                string suitesSampleRes = Composer.ComposeTestSuites(BuildFilename(sampleReport));
                string casesSampleRes = Composer.ComposeTestCases(BuildFilename(sampleReport));

                string cases_sample_target = String.Format("correct{0}{1}-{2}.correct", Path.DirectorySeparatorChar, sampleReport, modeCases);
                string suites_sample_target = String.Format("correct{0}{1}-{2}.correct", Path.DirectorySeparatorChar, sampleReport, modeSuites);
                writeResultToFile(suitesSampleRes, suites_sample_target);
                writeResultToFile(casesSampleRes, cases_sample_target);
            }
            Assert.False(active);
        }



        [Test]
        public void HeaderCheck()
        {
            XDocument xdoc = Composer.SetUpJunitDocument();
            string expected = String.Format("{0}", "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
            Assert.True(xdoc.Declaration.ToString() == expected);
        }


        // checks the testsuites node exsits
        [Test]
        public void HeaderCheckTestSuite()
        {
            XDocument xdoc = Composer.SetUpJunitDocument();
            Assert.True(xdoc.ToString() == "<testsuites />");
        }

        [Test]
        public void HeaderCheckEmpty()
        {
            XDocument xdoc = Composer.SetUpJunitDocument(testsuites_b: false);
            Assert.True(xdoc.ToString() == "");
        }



        [Test]
        public void CreateSingleTestsuite()
        {
            XDocument xdoc = Composer.SetUpJunitDocument(testsuites_b: false);
            Composer.AddSingleTestSuite(xdoc);
            Assert.True(xdoc.ToString() == "<testsuite />");
        }


        [Test]
        public void CreateSingleNoInit()
        {
            XDocument xdoc = new XDocument();
            Composer.AddSingleTestSuite(xdoc);
            Assert.True(xdoc.ToString() == "<testsuite />");
        }


        [Test]
        public void TestReturnXelementErrors()
        {
            string libraryUsageException = "Library usage exception: can't call with these params.";
            try
            {
                Composer.ReturnXelement(null, null);
            }
            catch (Exception e)
            {
                Assert.True(e.Message == libraryUsageException);
            }
            try
            {
                Composer.ReturnXelement(new XElement("testsuite"), new XDocument());
            } catch (Exception e)
            {
                Assert.True(e.Message == libraryUsageException);
            }
        }

        [Test]
        public void AddAttributesZero()
        {
            XDocument xdoc = Composer.SetUpJunitDocument();
            var xei = xdoc.Descendants("testsuites").GetEnumerator();
            xei.MoveNext();
            var xelt = xei.Current;
            Composer.addAttributes(xelt);
            string xdocStr = xdoc.ToString();
            string correct = "<testsuites tests=\"0\" failures=\"0\" errors=\"0\" />";
            Assert.True(correct == xdocStr);
        }
        
        [Test]
        public void BuildTestSutiesEmpty()
        {
            List<XElement> testCases = new List<XElement>();
            XDocument xdoc = Composer.BuildTestSuite(testCases);
            Assert.True(xdoc.ToString() == "<testsuite tests=\"0\" failures=\"0\" errors=\"0\" />");
        }

        [Test]
        public void BuildTestSuiteEmpty()
        {
            List<XElement> testCases = new List<XElement>();
            XDocument xdoc = Composer.BuildTestSuites(testCases);
            string correct = "<testsuites tests=\"0\" failures=\"0\" errors=\"0\" />";
            Assert.True(correct == xdoc.ToString());
        }




        // System-level tests:

        [Test]
        public void SuitesSpecTagPresent()
        {
            string res = Composer.ComposeTestSuites(BuildFilename(specExample));
            writeResultToFile(content: res);
            matchMainTags(res);
        }

        [Test]
        public void SuitesSpecDiff()
        {
            string res = Composer.ComposeTestSuites(BuildFilename(specExample));
            string correctUnsanitized = readCorrectFile(specExample, modeSuites);
            Assert.True(diffStrings(res, correctUnsanitized)); 
        }

        [Test]
        public void SuitesContainTestFailures()
        {
            string res = Composer.ComposeTestSuites(BuildFilename(specExample));
            Assert.True(res.Contains("<testsuites tests=\"45\" failures=\"17\" errors=\"0\">"));
        }


        [Test]
        public void CasesSpecTagPresent()
        {
            string res = Composer.ComposeTestCases(BuildFilename(specExample));
            writeResultToFile(res);
            matchTag("testsuite", res);
            matchTag("testcase", res);
        }

        [Test]
        public void CasesContainTestFailuresErrors()
        {
            string res = Composer.ComposeTestCases(BuildFilename(specExample));
            Assert.True(res.Contains("<testsuite tests=\"45\" failures=\"17\" errors=\"0\">"));

        }

        [Test]
        public void CasesSpecDiff()
        {
            string res = Composer.ComposeTestCases(BuildFilename(specExample));
            string correctStringUnsanitized = readCorrectFile(specExample, modeCases);
            Assert.True(diffStrings(res, correctStringUnsanitized));
            
        }


        [Test]
        public void CasesSampleReportDiff()
        {
            string res = Composer.ComposeTestCases(BuildFilename(sampleReport));
            string correctStringUnsanitized = readCorrectFile(sampleReport, modeCases);
            Assert.True(diffStrings(res, correctStringUnsanitized));

        }


        [Test]
        public void SuitesSampleReportDiff()
        {
            string res = Composer.ComposeTestCases(BuildFilename(sampleReport));
            string correctStringUnsanitized = readCorrectFile(sampleReport, modeCases);
            Assert.True(diffStrings(res, correctStringUnsanitized));
        }

        [Test]
        public void CasesSampleReportTags()
        {
            string res = Composer.ComposeTestCases(BuildFilename(sampleReport));
            Assert.True(res.Contains("<testsuite tests=\"1\" failures=\"1\" errors=\"1\">"));
            Assert.True(res.Contains("<testcase name=\"Basic_test\" time=\"0\" classname=\"test_ui.Rxn1000.Multi.Bio.Emulator\">"));
        }

        [Test]
        public void SuitesSampleReportTags()
        {
            string res = Composer.ComposeTestSuites(BuildFilename(sampleReport));
            Assert.True(res.Contains("<testsuites tests=\"1\" failures=\"1\" errors=\"1\">"));
            Assert.True(res.Contains("<testsuite name=\"test_ui\" tests=\"1\" failures=\"1\" errors=\"1\" time=\"1\" skipped=\"0\" timestamp=\"2019-07-26T19:53:23\" hostname=\"DESKTOP-JK3JHVC\">"));

        }


    }
}

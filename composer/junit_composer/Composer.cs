using System;
using System.Xml.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;


#if DEBUG
[assembly: InternalsVisibleTo("test")]
#endif
    namespace junit_composer
{
     public static class Composer
    {

        public class BadJunitFileException : Exception
        {
            public BadJunitFileException() { }

            public BadJunitFileException(string message) : base(message) { }

            public BadJunitFileException(string message, Exception inner)
                : base(message, inner) { }

        }

        private static string Testsuite = "testsuite";
        private static string Testsuites = "testsuites";
        private static string UnexpectedChildElt = "Unexpected child element found while parsing.";

        private static int TestTotal, FailureTotal, ErrorTotal = 0;

        private static XDocument OpenXmlFile(string filename)
        {
            return XDocument.Load(filename);
        }

        // .Select() not working for Xelement doc, will use helper:

        internal static List<Type> IEnumerableToList<Type>(IEnumerable<Type> nodes)
        {
            List<Type> elements = new List<Type>();
            foreach (Type xelt in nodes)
            {
                elements.Add(xelt);
            }
            return elements;
        }


        internal static void PrintXElements(List<XElement> nodes)
        {
            foreach (XElement n in nodes)
            {
                Console.WriteLine(n.ToString());
            }
        }

        // returns one or more testsuite classes.
        internal static List<XElement> ExtractTestSuites(string filename)
        {
            List<XElement> testSuiteObjects = new List<XElement>();
            XDocument xdoc = OpenXmlFile(filename);
            IEnumerable<XElement> testNode = xdoc.Elements();
            // there's no reason we can't have multiple
            // testsuites, or simply emit testsuites alltogether
            // (i.e. use a testsuite)
            // in most cases, though, there will only be one xelt.
            foreach (XElement xelt in testNode)
            {
                if (xelt.Name.LocalName == Testsuites)
                {
                    testSuiteObjects.AddRange(IEnumerableToList<XElement>(xelt.Descendants(Testsuite)));
                }
                else if (xelt.Name.LocalName == Testsuite) { 
                    testSuiteObjects.Add(xelt);
                }
                else
                {
                    throw new BadJunitFileException(UnexpectedChildElt);
                }
            }
            return testSuiteObjects;
        }


        internal static List<XElement> ExtractTestCases(string filename)
        {
            List<XElement> testObjects = new List<XElement>();
            List<XElement> testSuiteList = ExtractTestSuites(filename);

            // reset static vars
            ZeroTotals();

            // we know they're all testsuite objects. We can extract the test cases only
            // and return those. Then manually build the suite and header.
            foreach (XElement testSuite in testSuiteList)
            {
                testObjects.AddRange(testSuite.Descendants("testcase"));
                LogTestsAndFailures(testSuite);
            }
            return testObjects;
        }


        // we cant use generics to call a types method without reflection. it's safer to just
        // use additional parms..
        // note: bad code, not used in library.
        internal static XElement ReturnXelement(XElement xelt = null, XDocument xdoc = null)
        {
            if ( ( xelt != null && xdoc != null ) || (xelt == null && xdoc == null) ) { 
                throw new Exception("Library usage exception: can't call with these params.");
            }
            IEnumerator<XElement> suiteEnumerator; 
            if (xelt != null) {
                suiteEnumerator = xelt.Descendants(Testsuite).GetEnumerator(); 
            } else
            {
                suiteEnumerator = xdoc.Descendants(Testsuites).GetEnumerator();
            }
            suiteEnumerator.MoveNext();
            return suiteEnumerator.Current;
        }

        internal static XDocument SetUpJunitDocument(bool testsuites_b = true)
        {
            XDocument resDoc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes")
                );
            if (testsuites_b)
            {
                resDoc.Add(new XElement(Testsuites));
            }
            return resDoc;     
        }


        // returns a testsuite xelement
        internal static XElement AddSingleTestSuite(XDocument xdoc)
        {
            xdoc.Add(new XElement(Testsuite));
            IEnumerator<XElement> testsuiteEnumerator = xdoc.Descendants(Testsuite).GetEnumerator();
            testsuiteEnumerator.MoveNext();

            XElement suite = testsuiteEnumerator.Current;
            return suite;
        }


        internal static void ZeroTotals()
        {
            TestTotal = 0;
            FailureTotal = 0;
            ErrorTotal = 0;
        }

        internal static void LogTestsAndFailures(XElement testsuite)
        {
            XAttribute tests = testsuite.Attribute("tests");
            XAttribute failures = testsuite.Attribute("failures");
            XAttribute errors = testsuite.Attribute("errors");
            try
            {
                if (tests != null)
                {
                    TestTotal += Convert.ToInt32(tests.Value.ToString());
                }
                if (errors != null)
                {
                    ErrorTotal += Convert.ToInt32(errors.Value.ToString());
                }
                if (failures != null)
                {
                    FailureTotal += Convert.ToInt32(failures.Value.ToString());
                }
            }
            catch (InvalidCastException e)
            {
                throw (new BadJunitFileException($"{e}: testsuite element attributes for tests, value, error must be integers."));
            }
        } 

        // testsuite or testsuites works on param
        internal static void addAttributes(XElement testObj)
        {
            testObj.Add(new XAttribute("tests", TestTotal.ToString()));
            testObj.Add(new XAttribute("failures", FailureTotal.ToString()));
            testObj.Add(new XAttribute("errors", ErrorTotal.ToString()));
        }
        
        internal static XDocument BuildTestSuites(List<XElement> test_cases)
        {
            XDocument res_doc = SetUpJunitDocument();

            // we grab the testsuite elt:
            IEnumerator<XElement> testsuitesIter = res_doc.Descendants(Testsuites).GetEnumerator();
            testsuitesIter.MoveNext();
            XElement testsuites_obj = testsuitesIter.Current;


            ZeroTotals();
            // add all testsuite element to the testsuites:
            foreach (XElement testsuite in test_cases)
            {
                testsuites_obj.Add(testsuite);
                LogTestsAndFailures(testsuite);
             
            }
            addAttributes(testsuites_obj);
            return res_doc;
        }

        internal static XDocument BuildTestSuite(List<XElement> test_cases)
        {
            XDocument resDoc = SetUpJunitDocument(testsuites_b: false);
            AddSingleTestSuite(resDoc);
            IEnumerator<XElement> test_suite_elt = resDoc.Descendants(Testsuite).GetEnumerator();
            test_suite_elt.MoveNext();
            XElement test_suite = test_suite_elt.Current;
            foreach (XElement tc in test_cases)
            {
                test_suite.Add(tc);
                
            }
            addAttributes(test_suite);
            return resDoc;
        }


        internal static string AppendEncoding(XDocument doc)
        {
            return $"{doc.Declaration.ToString()}{Environment.NewLine}{doc.ToString()}";
        }


        // public functions:
        public static List<string> GatherJunitFiles()
        {
            return new List<string>();
        }

       

        /// <summary>
        /// </summary>
        /// <param name="targets"></param>
        /// <returns>        
        /// Returns a string representing a .junit xml file with
        /// test cases composed.
        /// </returns>
        public static string ComposeTestSuites(params string[] targets)
        {
            List<XElement> testSuites = new List<XElement>();
            foreach (string target in targets)
            {
                testSuites.AddRange(ExtractTestSuites(filename: target));
            }
            XDocument TestSuiteDoc = BuildTestSuites(testSuites);
       
            return AppendEncoding(TestSuiteDoc);
        }


        /// <summary>
        /// </summary>
        /// <param name="targets"></param>
        /// <returns>
        /// A string representing a .junit xml file with a single
        /// test suite containing test cases. 
        /// </returns>
        public static string ComposeTestCases(params string[] targets)
        {
            List<XElement> testCases = new List<XElement>();
            foreach (string target in targets)
            {
                testCases.AddRange(ExtractTestCases(target));
            }
            XDocument testCaseDoc = BuildTestSuite(testCases);

            return AppendEncoding(testCaseDoc);
        }


        /// <summary>
        /// Gathers all junit files
        /// </summary>
        /// <param name="directory"></param>
        /// <returns>a string[] of files containing '.junit'</returns>
        public static string[] GatherJunitFiles(string directory)
        {
            return Directory.GetFiles(directory, "*.junit*", SearchOption.AllDirectories);
        }
    }
}

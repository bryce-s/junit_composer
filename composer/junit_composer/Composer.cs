using System;
using System.Xml.Linq;
using System.Collections.Generic;

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

        private static string testsuite = "testsuite";
        private static string testsuites = "testsuites";
        private static string unexpected_child_elt = "Unexpected child element found while parsing.";

        private static int test_total, failure_total, error_total = 0;

        private static XDocument OpenXmlFile(string filename)
        {
            return XDocument.Load(filename);
        }

        // .Select() not working for Xelement doc, will use helper:

        private static List<Type> IEnumerableToList<Type>(IEnumerable<Type> nodes)
        {
            List<Type> elements = new List<Type>();
            foreach (Type xelt in nodes)
            {
                elements.Add(xelt);
            }
            return elements;
        }


        private static void print_x_elements(List<XElement> nodes)
        {
            foreach (XElement n in nodes)
            {
                Console.WriteLine(n.ToString());
            }
        }

        // returns one or more testsuite classes.
        private static List<XElement> ExtractTestSuites(string filename)
        {
            List<XElement> test_suite_objects = new List<XElement>();
            XDocument xdoc = OpenXmlFile(filename);
            IEnumerable<XElement> test_node = xdoc.Elements();
            // there's no reason we can't have multiple
            // testsuites, or simply emit testsuites alltogether
            // (i.e. use a testsuite)
            // in most cases, though, there will only be one xelt.
            foreach (XElement xelt in test_node)
            {
                if (xelt.Name.LocalName == testsuites)
                {
                    test_suite_objects.AddRange(IEnumerableToList<XElement>(xelt.Descendants(testsuite)));
                }
                else if (xelt.Name.LocalName == testsuite) { 
                    test_suite_objects.Add(xelt);
                }
                else
                {
                    throw new BadJunitFileException(unexpected_child_elt);
                }
            }
            return test_suite_objects;
        }


        private static List<XElement> ExtractTestCases(string filename)
        {
            List<XElement> test_objects = new List<XElement>();
            List<XElement> test_suite_list = ExtractTestSuites(filename);

            // reset static vars
            zero_totals();

            // we know they're all testsuite objects. We can extract the test cases only
            // and return those. Then manually build the suite and header.
            foreach (XElement test_suite in test_suite_list)
            {
                test_objects.AddRange(test_suite.Descendants("testcase"));
                log_tests_failures_errors(test_suite);
            }
            return test_objects;
        }


        // we cant use generics to call a types method without reflection. it's safer to just
        // use additional parms..
        private static XElement return_xelement(XElement xelt = null, XDocument xdoc = null)
        {
            if ( ( xelt != null && xdoc != null ) || (xelt == null && xdoc == null) ) { 
                throw new Exception("Library implementation exception: can't call with these params.");
            }
            IEnumerator<XElement> suite_enumerator; 
            if (xelt != null) {
                suite_enumerator = xelt.Descendants(testsuite).GetEnumerator(); 
            } else
            {
                suite_enumerator = xdoc.Descendants(testsuites).GetEnumerator();
            }
            suite_enumerator.MoveNext();
            return suite_enumerator.Current;
        }

        private static XDocument set_up_junit_document(bool testsuites_b = true)
        {
            XDocument res_doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes")
                );
            if (testsuites_b)
            {
                res_doc.Add(new XElement(testsuites));
            }
            return res_doc;     
        }


        // returns a testsuite xelement
        private static XElement add_single_testsuite(XDocument xdoc)
        {
            xdoc.Add(new XElement(testsuite));
            IEnumerator<XElement> testsuite_enumerator = xdoc.Descendants(testsuite).GetEnumerator();
            testsuite_enumerator.MoveNext();



            XElement suite = testsuite_enumerator.Current;
            return suite;
        }


        private static void zero_totals()
        {
            test_total = 0;
            failure_total = 0;
            error_total = 0;
        }

        private static void log_tests_failures_errors(XElement testsuite)
        {
            XAttribute tests = testsuite.Attribute("tests");
            XAttribute failures = testsuite.Attribute("failures");
            XAttribute errors = testsuite.Attribute("errors");
            try
            {
                if (tests != null)
                {
                    test_total += Convert.ToInt32(tests.Value.ToString());
                }
                if (errors != null)
                {
                    error_total += Convert.ToInt32(errors.Value.ToString());
                }
                if (failures != null)
                {
                    failure_total += Convert.ToInt32(failures.Value.ToString());
                }
            }
            catch (InvalidCastException e)
            {
                throw (new BadJunitFileException("testsuite element attributes for tests, value, error must be integers."));
            }
        } 

        // testsuite or testsuites works on param
        static private void add_attributes(XElement test_suite_obj)
        {
            test_suite_obj.Add(new XAttribute("tests", test_total.ToString()));
            test_suite_obj.Add(new XAttribute("failures", failure_total.ToString()));
            test_suite_obj.Add(new XAttribute("errors", error_total.ToString()));
        }
        
        private static XDocument build_test_suites(List<XElement> test_cases)
        {
            XDocument res_doc = set_up_junit_document();

            // we grab the testsuite elt:
            IEnumerator<XElement> testsuites_iter = res_doc.Descendants(testsuites).GetEnumerator();
            testsuites_iter.MoveNext();
            XElement testsuites_obj = testsuites_iter.Current;

            zero_totals();
            // add all testsuite element to the testsuites:
            foreach (XElement testsuite in test_cases)
            {
                testsuites_obj.Add(testsuite);
                log_tests_failures_errors(testsuite);
             
            }
            add_attributes(testsuites_obj);
            return res_doc;
        }

        private static XDocument build_test_suite(List<XElement> test_cases)
        {
            XDocument res_doc = set_up_junit_document(testsuites_b: false);
            add_single_testsuite(res_doc);
            IEnumerator<XElement> test_suite_elt = res_doc.Descendants(testsuite).GetEnumerator();
            test_suite_elt.MoveNext();
            XElement test_suite = test_suite_elt.Current;
            foreach (XElement tc in test_cases)
            {
                test_suite.Add(tc);
                
            }
            add_attributes(test_suite);
            return res_doc;
        }


        private static string append_encoding(XDocument doc)
        {
            return String.Format("{0}{1}{2}", doc.Declaration.ToString(), Environment.NewLine, doc.ToString());
        }


        // public functions:
        public static List<string> gather_junit_files()
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
            List<XElement> test_suites = new List<XElement>();
            foreach (string target in targets)
            {
                test_suites.AddRange(ExtractTestSuites(filename: target));
            }
            XDocument test_suite_doc = build_test_suites(test_suites);
       
            return append_encoding(test_suite_doc);
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
            List<XElement> test_cases = new List<XElement>();
            foreach (string target in targets)
            {
                test_cases.AddRange(ExtractTestCases(target));
            }
            XDocument test_case_doc = build_test_suite(test_cases);

            return append_encoding(test_case_doc);
        }
       
    }
}

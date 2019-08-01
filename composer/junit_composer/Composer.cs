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
            List<XElement> xelt_list = ExtractTestSuites(filename);
            // we know they're all testsuite objects. We can extract the test cases only
            // and return those. Then manually build the suite and header.
            foreach (XElement xelt in xelt_list)
            {
                test_objects.AddRange(xelt.Descendants("testcase"));
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

        private static XDocument set_up_junit_document()
        {
            XDocument res_doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes")
                );
            res_doc.Add(new XElement(testsuites));
            return res_doc;     
        }


        private static XElement add_single_testsuite(XDocument res_doc)
        {
            // we grab the testsuites elt:
            XElement testsuites_obj = return_xelement(xdoc: res_doc);
            testsuites_obj.Add(new XElement(testsuite));

            //we greab the testsuite elt:
            XElement testsuite_obj = return_xelement(xelt: testsuites_obj);
            return testsuites_obj;
        }

        private static XDocument build_test_case_document(List<XElement> test_cases)
        {
            XDocument res_doc = set_up_junit_document();

            // we grab the testsuite elt:
            XElement testsuite_obj = add_single_testsuite(res_doc);

            // add all testcase elemnts to the testsuite:
            foreach (XElement xelt in test_cases.Descendants(testsuite))
            {
                testsuite_obj.Add(xelt);
            }
            return res_doc;
        }

        private static XDocument build_test_suite_document(List<XElement> test_suites)
        {
            XDocument res_doc = set_up_junit_document();
            IEnumerator<XElement> test_suite_elt = res_doc.Descendants(testsuite).GetEnumerator();
            test_suite_elt.MoveNext();
            XElement test_suite = test_suite_elt.Current;
            foreach (XElement suite in test_suites)
            {
                test_suite.Add(suite);
            }
            return res_doc;
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
            XDocument test_suite_doc = build_test_case_document(test_suites);
            return test_suite_doc.ToString();
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
            XDocument test_case_doc = build_test_case_document(test_cases);

            return test_case_doc.ToString();
        }
       
    }
}

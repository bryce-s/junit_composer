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

        private static List<T> IEnumerableToList<T>(IEnumerable<T> nodes)
        {
            List<T> elements = new List<T>();
            foreach (T xelt in nodes)
            {
                elements.Add(xelt);
            }
            return elements;
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



        // public functions:

        /// <summary>

        /// </summary>
        /// <param name="targets"></param>
        /// <returns>        
        /// Returns a string representing a .junit xml file with
        /// test cases composed.
        /// </returns>
        public static string ComposeTestSuites(params string[] targets)
        {
            foreach (string target in targets)
            {
                ExtractTestSuites(filename: target);
            }


            return "";
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

            return "";
        }
       
    }
}

using System;
using System.Xml.Linq;


namespace junit_composer
{
     public static class Composer
    {

        private static XDocument OpenXmlFile(string filename)
        {
            return XDocument.Load(filename);
        }
        

        // combines paramas into a single junit file with multiple test suites.
        public static string ComposeTestSuites(params string[] targets)
        {

            return "";
        }


        // combines params into a single junit file, with a suite of test cases.
        public static string ComposeTestCases(params string[] targets)
        {

            return "";
        }
       
    }
}

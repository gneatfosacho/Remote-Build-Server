/////////////////////////////////////////////////////////////////////////////
//  TestRequest.cs - Performs test request operations                      //
//  ver 4.0                                                                //
//  Language:     C#, VS 2017                                              //
//  Platform:     Lenovo Yoga i7 Quad Core Windows 10                      //
//  Application:  Project 4 for CSE681 - Software Modeling & Analysis      //
//  Author:       Rahul Kadam, Syracuse University                         //
//                (315) 751-8862, rkadam@syr.edu                           //
//  Source:       Jim Fawcett, CST 2-187, Syracuse University(Professor)   //
//                (315) 443-3948, jfawcett@twcny.rr.com                    //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Package Operations
 *   ------------------
 *    Contains TestElement and TestRequest classes
 *    BuildElement   ->     information about a single build
 *    BuildRequest   ->     a container for one or more BuildElements
 *	  TestRequest    ->     information about a single test request
 *	  BuildLog       ->     information about processing of a single build log
 *	  BuildLogs      ->     a container for one or more BuildLog instances
 *	  TestLog        ->     information about processing of a single test log
 *	  TestLogs       ->     a container for one or more TestLog instances 
 *	     
 *   Build Process
 *   -------------
 *   - Required files - RequestMgr.cs
 *   - Compiler command: csc RequestMgr.cs
 *    
 *   Maintenance History
 *   -------------------
 *   ver 4.0 : 6 Dec 2017
 * - Project 4 release
 * 
 *   ver 3.0 : 25 Oct 2017
 * - Project 3 release
 * 
 *   ver 1.0 : 4 Oct 2017
 * - Project 2 release
 * 
 */

using System;
using System.Collections.Generic;
using Utilities;
using System.IO;
using System.Text;

namespace Federation
{
    
    public class BuildElement  /* information about a single build */
    {
        public string testName { get; set; }
        public string testDriver { get; set; }
        public List<string> testCodes { get; set; } = new List<string>();

        public BuildElement() { }
        public BuildElement(string name)
        {
            testName = name;
        }
        public void addDriver(string name)
        {
            testDriver = name;
        }
        public void addCode(string name)
        {
            testCodes.Add(name);
        }
        public override string ToString()
        {
            string temp = "\n    test: " + testName;
            temp += "\n      testDriver: " + testDriver;
            foreach (string testCode in testCodes)
                temp += "\n      testCode:   " + testCode;
            return temp;
        }
    }

    public class BuildRequest  /* a container for one or more BuildElements */
    {
        public string author { get; set; } = "";
        public string dateTime { get; set; } = "";
        public List<BuildElement> tests { get; set; } = new List<BuildElement>();

        public BuildRequest() { }
        public BuildRequest(string auth)
        {
            author = auth;
        }
        public override string ToString()
        {
            string temp = "\n  author: " + author;
            temp += "\n  dateTime: " + dateTime;
            foreach (BuildElement te in tests)
                temp += te.ToString();
            return temp;
        }
    }

    public class TestRequest  /* information about a single test request */
    {
        public string author { get; set; } = "";
        public string dateTime { get; set; } = "";
        public List<string> tests { get; set; } = new List<string>();

        public TestRequest() { }
        public TestRequest(string auth)
        {
            author = auth;
        }
        public override string ToString()
        {
            string temp = "\n  author: " + author;
            temp += "\n  dateTime: " + dateTime;
            foreach (string te in tests)
                temp += te.ToString();
            return temp;
        }
    }

    public class BuildLog  /* information about processing of a single build log */
    {
        public string buildName { get; set; }
        public bool passed { get; set; }
        public string log { get; set; }

        public BuildLog() { }
        public BuildLog(string name, bool status)
        {
            buildName = name;
            passed = status;
        }
        public void addLog(string logItem)
        {
            log += logItem;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n    build: " + buildName + " " + passed);
            sb.Append("\n    log:  " + log);
            return sb.ToString();
        }
    }

    public class BuildLogs  /* a container for one or more BuildLog instances */
    {
        public string author { get; set; }
        public DateTime timeStamp { get; set; }
        public List<BuildLog> results { get; set; } = new List<BuildLog>();

        public BuildLogs() { }
        public BuildLogs(string auth, DateTime ts)
        {
            author = auth;
            timeStamp = ts;
        }
        public BuildLogs add(BuildLog rslt)
        {
            results.Add(rslt);
            return this;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n  Author: " + author + " " + timeStamp.ToString());
            foreach (BuildLog rslt in results)
            {
                sb.Append(rslt.ToString());
            }
            return sb.ToString();
        }
    }

    public class TestLog  /* information about processing of a single test log */
    {
        public string testName { get; set; }
        public string log { get; set; }

        public TestLog() { }
        public TestLog(string name)
        {
            testName = name;
        }
        public void addLog(string logItem)
        {
            log += logItem;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n    test: " + testName);
            sb.Append("\n    log:  " + log);
            return sb.ToString();
        }
    }

    public class TestLogs  /* a container for one or more TestLog instances */
    {
        public string author { get; set; }
        public DateTime timeStamp { get; set; }
        public List<TestLog> results { get; set; } = new List<TestLog>();

        public TestLogs() { }
        public TestLogs(string auth, DateTime ts)
        {
            author = auth;
            timeStamp = ts;
        }
        public TestLogs add(TestLog rslt)
        {
            results.Add(rslt);
            return this;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n  Author: " + author + " " + timeStamp.ToString());
            foreach (TestLog rslt in results)
            {
                sb.Append(rslt.ToString());
            }
            return sb.ToString();
        }
    }

    //----< test stub >------------------------------------------------

#if (Test_TestRequest)

    class Test_TestRequest
  {
    static void Main(string[] args)
    {
      "Testing Test_TestRequest Class".title('=');
      Console.WriteLine();

      ///////////////////////////////////////////////////////////////
      // Serialize and Deserialize TestRequest data structure

      "Testing Serialization of TestRequest data structure".title();
      
      BuildElement te1 = new BuildElement();
      te1.testName = "test1";
      te1.addDriver("TestDriver.cs");
      te1.addCode("TestedOne.cs");
      te1.addCode("TestedTwo.cs");

      BuildElement te2 = new BuildElement();
      te2.testName = "test2";
      te2.addDriver("TestLib.cs");
      te2.addCode("TestedLib.cs");
      te2.addCode("TestedLibDependency.cs");
      te2.addCode("Interfaces.cs");

      BuildRequest tr = new BuildRequest();
      tr.author = "Rahul Kadam";
      tr.dateTime = DateTime.Now.ToString();
      tr.tests.Add(te1);
      tr.tests.Add(te2);
      string trXml = tr.ToXml();
      Console.Write("\n  Serialized TestRequest data structure:\n\n  {0}\n", trXml);      
    
      string filePath = "../../../StorageRepoMock/TestRequests/TestRequest3.xml";
      Console.Write("\n  Saving TestRequest to xml file: {0}\n", filePath);   
      File.WriteAllText(filePath, trXml);
      string trXml2 = File.ReadAllText(filePath);
      Console.Write("\n  Serialized TestRequest data structure:\n\n{0}\n", trXml2);
        
      "Testing Deserialization of TestRequest from XML".title();

      BuildRequest newRequest = trXml.FromXml<BuildRequest>();
      string typeName = newRequest.GetType().Name;
      Console.Write("\n  deserializing xml string results in type: {0}\n", typeName);
      Console.Write(newRequest);
      Console.WriteLine();

    }
  }
#endif


}

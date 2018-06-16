/////////////////////////////////////////////////////////////////////////////
//  TestHarnessMock.cs - Performs mock test harness operations             //
//  ver 1.0                                                                //
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
 *   - Stores the libraries sent from Build Server
 *   - Loads and Executes the libraries.
 *   - sends test logs to repo mock
 *   - sends test status notification to gui-client
 *	     
 *	 Important Functions:
 *   ------------------ 
 *	    loadAndExerciseTesters()        -> load assemblies from testHarnessStorage and run their tests    
 *	    runSimulatedTest(...)           -> run tester t from assembly asm      
 *      sendTestStatus 	                -> send test status notification to gui-client
 *      sendTestLog	                    -> send test logs to repo mock 
 *      sendFileRequest	                -> send file request to child builde
 *      parseTestRequest                -> parse test request from child builder 
 *      
 *   Build Process
 *   -------------
 *   - Required files - TestHarnessMock.cs
 *   - Compiler command: csc TestHarnessMock.cs
 *    
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 6 Dec 2017
 * - Project 4 release
 * 
 */

using System;
using System.Reflection;
using System.IO;
using System.Threading;
using MessagePassingComm;
using System.Collections.Generic;
using Utilities;
using System.Diagnostics;

namespace Federation
{
    ///////////////////////////////////////////////////////////////////
    // TestHarnessMock classes
    //

    public class TestHarnessMock
    {
        public static string testHarnessStorage { get; set; } = "../../../StorageTestHarnessMock";
        public static string testHarnessStorageLib { get; set; } = "../../../StorageTestHarnessMock/Libraries"; 
        public string testHarnessAddress { get; set; } = "http://localhost:8090/IMessagePassingComm";
        public string repoMockAddress { get; set; } = "http://localhost:8081/IMessagePassingComm";
        public string clientGUIAddress { get; set; } = "http://localhost:8080/IMessagePassingComm";
        public TestRequest testRequest { get; set; } = null;
        public TestLogs testLogs { get; set; } = null;
        public int dllcount { get; set; } = 0;
        const int port = 8090;
        public Comm comm { get; set; } = null;
        Thread msgHandler;

        public TestHarnessMock()
        {
            if (!Directory.Exists(testHarnessStorage))
                Directory.CreateDirectory(testHarnessStorage);
            if (!Directory.Exists(testHarnessStorageLib))
                Directory.CreateDirectory(testHarnessStorageLib);
            dllcount = Directory.GetFiles(testHarnessStorage).Length;
            comm = new Comm("http://localhost", port);
            msgHandler = new Thread(processMessages);
            msgHandler.Start();
        }

        /*----< processes messages >--------*/
        void processMessages()
        {
            while (true)
            {
                CommMessage msg = comm.getMessage();
                if (msg.command != null)
                {
                    if (!msg.type.Equals("connect"))
                        msg.show();

                    if (msg.command.Equals("TestRequest"))
                    {
                        List<string> libraries = parseTestRequest(msg.body);
                        sendFileRequest(msg.from, libraries,msg.testRequest);
                    }

                    if (msg.command.Equals("SuccessSentFiles"))
                    {
                        testLogs = new TestLogs(msg.author, DateTime.Now);
                        TestLog tl = new TestLog(msg.testRequest);
                        StringWriter sw = new StringWriter();
                        var actualconsole = Console.Out;
                        Console.SetOut(sw);

                        foreach (string lib in msg.testFiles)
                            loadAndExerciseTesters(lib);

                        tl.addLog(sw.ToString());
                        testLogs.add(tl);
                        Console.SetOut(actualconsole);

                        //foreach (string lib in msg.testFiles)
                        //    deleteLibraries(lib);
                        sendTestLog(msg.testRequest);
                        sendTestStatus(msg.testRequest);
                    }

                    if (msg.command.Equals("quit"))
                    {                        
                        Console.Write("\n  Quit message Received\n");
                        Console.Write("\n  To start Building Process back again Please close all Console Windows.\n");
                        break;
                    }
                }
            }
        }

        /*----< send test status notification to gui-client >--------*/
        private void sendTestStatus(string testHarnessAddress)
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.reply);
            csndMsg.command = "TestStatus";
            csndMsg.author = "Rahul Kadam";
            csndMsg.to = clientGUIAddress;
            csndMsg.from = testHarnessAddress;
            csndMsg.testRequest = testHarnessAddress;
            csndMsg.body = "\t  Test Process Completed";
            comm.postMessage(csndMsg);
            csndMsg.show();
        }

        /*----< send test logs to repo mock >--------*/
        private void sendTestLog(string itestRequest)
        {
            string xmlbuildlog = testLogs.ToXml();
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.reply);
            csndMsg.command = "TestLog";
            csndMsg.author = "Rahul Kadam";
            csndMsg.to = repoMockAddress;
            csndMsg.from = testHarnessAddress;
            csndMsg.testRequest = itestRequest;
            csndMsg.body = xmlbuildlog;
            comm.postMessage(csndMsg);
            csndMsg.show();

        }

        /*----< send file request to child builder >--------*/
        private void sendFileRequest(string childbuilderaddress, List<string> libraries, string itestRequest)
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            csndMsg.command = "FileRequest";
            csndMsg.author = "Rahul Kadam";
            csndMsg.to = childbuilderaddress;
            csndMsg.from = testHarnessAddress;
            csndMsg.testFiles = libraries;
            csndMsg.testRequest = itestRequest;
            comm.postMessage(csndMsg);
            Console.Write("\n\n  Sending FileRequest from TestHarnessMock to ChildBuilder\n");
            csndMsg.show();

        }

        /*----< parse test request from child builder >--------*/
        private List<string> parseTestRequest(string body)
        {
            List<string> libraries = new List<string>();
            testRequest = body.FromXml<TestRequest>();

            foreach (string te in testRequest.tests)
            {
                libraries.Add(te);
            }
            return libraries;
        }

        /*----< library binding error event handler >------------------*/
        /*
         *  This function is an event handler for binding errors when
         *  loading libraries.  These occur when a loaded library has
         *  dependent libraries that are not located in the directory
         *  where the Executable is running.
         */
        static Assembly LoadFromComponentLibFolder(object sender, ResolveEventArgs args)
        {
            Console.Write("\n  called binding error event handler");
            string folderPath = testHarnessStorageLib;
            string assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
            if (!File.Exists(assemblyPath)) return null;
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }

        //----< load assemblies from testHarnessStorage and run their tests >-----
        public string loadAndExerciseTesters(string ilibraryname)
        {            
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(LoadFromComponentLibFolder);

            try
            {               
                string fullpath = renameLibrary(ilibraryname);

                // load each assembly found in testHarnessStorage

                //Assembly asm = Assembly.LoadFrom(file);
                Assembly asm = Assembly.LoadFile(fullpath);                
              
                // exercise each tester found in assembly

                Type[] types = asm.GetTypes();
                foreach (Type t in types)
                {
                    // if type supports ITest interface then run test

                    if (t.GetInterface("TestBuild.ITest", true) != null)
                        if (!runSimulatedTest(t, asm))
                        {
                            Console.Write("\n  test {0} failed to run", t.ToString());
                            Console.WriteLine("\n\n  ----------------------------------------------------");
                            Console.Write("  Test Failed !!!");
                            Console.WriteLine("\n  ----------------------------------------------------");
                        }
                }
               
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Testing completed";
        }

        //----< rename libraries >-----
        private string renameLibrary(string ilibraryname)
        {
            // Generate New Library Name
           
            dllcount++;
            string newlibname = "MyLib" + dllcount.ToString() + ".dll";

            // Get Full Path of Old Lib Name
            string relativepath = testHarnessStorageLib + "/" + ilibraryname;
            string fullpath = Path.GetFullPath(relativepath);

            // Get Full Path of New Lib Name
            string relativepath2 = testHarnessStorageLib + "/" + newlibname;
            string fullpath2 = Path.GetFullPath(relativepath2);

            File.Move(fullpath, fullpath2);

            return fullpath2;
        }
        //
        //----< run tester t from assembly asm >-------------------------------
        bool runSimulatedTest(Type t, Assembly asm)
        {
            try
            {
                Console.Write(
                  "\n  attempting to create instance of {0}", t.ToString()
                  );
                object obj = asm.CreateInstance(t.ToString());

                // announce test
                MethodInfo method = t.GetMethod("say");
                if (method != null)
                    method.Invoke(obj, new object[0]);

                // run test
                bool status = false;
                method = t.GetMethod("test");
                if (method != null)
                    status = (bool)method.Invoke(obj, new object[0]);

                Func<bool, string> act = (bool pass) =>
                {
                    if (pass)
                        return "Passed";
                    return "Failed";
                };
                Console.WriteLine("\n\n  ----------------------------------------------------");
                Console.Write("  Test {0} !!!", act(status));
                Console.WriteLine("\n  ----------------------------------------------------");
            }
            catch (Exception ex)
            {
                Console.Write("\n  test failed with message \"{0}\"", ex.Message);
                return false;
            }
            ///////////////////////////////////////////////////////////////////
            //  You would think that the code below should work, but it fails
            //  with invalidcast exception, even though the types are correct.
            //
            //    DllLoaderDemo.ITest tester = (DllLoaderDemo.ITest)obj;
            //    tester.say();
            //    tester.test();
            //
            //  This is a design feature of the .Net loader.  If code is loaded 
            //  from two different sources, then it is considered incompatible
            //  and typecasts fail, even thought types are Liskov substitutable.
            //
            return true;
        }

        //----< run demonstration >--------------------------------------------
        [STAThread]
        static void Main(string[] args)
        {
            Console.Title = "TestHarnessMock@" + port;

            Console.Write("\n  TestHarnessMock Process");
            Console.Write("\n ==========================");

            TestHarnessMock testHarness = new TestHarnessMock();
            Console.Write("\n  TestHarnessMock Server: " + testHarness.testHarnessAddress);

            /*
            // convert testers relative path to absolute path

            TestHarnessMock.testHarnessStorage = Path.GetFullPath(TestHarnessMock.testHarnessStorage);
            Console.Write("\n  Loading Test Modules from:\n    {0}\n", TestHarnessMock.testHarnessStorage);

            // run load and tests

            string result = loader.loadAndExerciseTesters("*.dll");

            Console.Write("\n\n  {0}", result);
            Console.Write("\n\n");

            */
        }
    }
}

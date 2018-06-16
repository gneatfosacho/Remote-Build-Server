/////////////////////////////////////////////////////////////////////////////
//  ChildBuilder.cs - Child Build Server in Federation                     //
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
 *  
 *   It has following main functions:
 *   1.  To ask for BuildRequest(.xml) and TestFiles(.cs) from RepoMock and receive them
 *   2.  To Build TestFiles into libraries(.dll's)
 *   3.  To create and send build logs to repo mock and build status notification to gui-client
 *   4.  To create test request and send libraries to test harness mock for execution
 *             
 *   Important functions:
 *   ------------------
 *   
 *   processMessages        -> processes msgs
 *   sendBuildStatus 	    -> sends build status notification to GUI-Client
 *   sendBuildLog		    -> sends build logs to repo mock
 *   successSendFiles	    -> sends successfully file send msg
 *   fileTransferLibraries	-> transfer libraries to test harness mock
 *   sendTestRequest		-> create ans send Test Request msg to test harness mock
 *   buildLibraries         -> Attempts to Build Libraries
 *   parseTestRequest       -> parses Test Request
 *   sendReadyMessage       -> sends "ready" msg back to Mother Builder
 *   sendGetFileMessage     -> sends get TestRequest(.xml) msg to RepoMock
 *   sendGetFilesMessage    -> send get TestFiles(.cs) msg to RepoMock
 *     
 *   Build Process
 *   -------------
 *   - Required files - ChildBuilder.cs
 *   - Compiler command: csc ChildBuilder.cs
 *    
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 6 Dec 2017
 * - first release
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using MessagePassingComm;
using System.Threading;
using System.IO;
using Utilities;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Federation
{
    ///////////////////////////////////////////////////////////////////
    // ChildBuilder class
    //

    public class ChildBuilder
    {
        
        public string childBuilderAddress { get; set; } = "";
        public string motherBuilderAddress { get; set; } = "http://localhost:8082/IMessagePassingComm";
        public string repoMockAddress { get; set; } = "http://localhost:8081/IMessagePassingComm";
        public string testHarnessAddress { get; set; } = "http://localhost:8090/IMessagePassingComm";
        public string clientGUIAddress { get; set; } = "http://localhost:8080/IMessagePassingComm";
        public string childBuilderstoragePath { get; set; } = "../../../StorageChildBuilder";
        public string librariespath { get; set; } = null;
        public BuildRequest buildRequest { get; set; } = null;
        public TestRequest testRequest { get; set; } = null;
        public List<string> files { get; set; } = new List<string>();
        public BuildLogs buildLogs { get; set; } = null;
        public Comm comm { get; set; } = null;
        Thread msgHandler;
        int port;

        public ChildBuilder(int iport)
        {
            port = iport;
            childBuilderstoragePath = childBuilderstoragePath + "/" + "ChildBuilder#" + port;
            if (!Directory.Exists(childBuilderstoragePath))
                Directory.CreateDirectory(childBuilderstoragePath);
            librariespath = childBuilderstoragePath + "/" + "MyLibraries";
            if (!Directory.Exists(librariespath))
                Directory.CreateDirectory(librariespath);
            comm = new Comm("http://localhost", port);
            childBuilderAddress = "http://localhost:" + port.ToString() + "/IMessagePassingComm";
            sendReadyMessage();
            buildRequest = new BuildRequest();
            testRequest = new TestRequest();
            msgHandler = new Thread(processMessages);
            msgHandler.Start();            
        }

        /*----< processes msgs >-----------*/
        void processMessages()
        {
            while (true)
            {
                CommMessage msg = comm.getMessage();
                if (msg.command != null)
                {
                    // Build Request Recived from Mother Builder
                    if (!msg.type.Equals("connect"))
                        msg.show();

                    if (msg.command.Equals("quit"))
                    {
                        Console.Write("\n  Quit message Received\n");
                        Console.Write("\n  To start Building Process back again Please close all Console Windows.\n");
                        break;
                    }
                     
                    if (msg.command.Equals("BuildRequest"))
                    {
                        sendGetFileMessage(msg.testRequest);
                        sendReadyMessage();
                    }

                    if (msg.command.Equals("successSentFile"))
                    {
                        parseTestRequest(msg.testRequest);
                        sendGetFilesMessage(msg.testRequest);
                    }

                    if (msg.command.Equals("successSentFiles"))
                    {
                        buildLogs = new BuildLogs(msg.author, DateTime.Now);
                        List<string> libraries = buildLibraries(msg.testRequest);
                        if (libraries.Count > 0)
                            sendTestRequest(msg, libraries,msg.testRequest);
                        sendBuildLog(msg.testRequest);                        
                        sendBuildStatus(msg.testRequest);
                    }
                    if (msg.command.Equals("FileRequest"))
                    {
                        fileTransferLibraries(msg.testFiles);
                        successSendFiles(msg.from , msg.testFiles, msg.testRequest);
                    }
                }
            }
        }

        /*----< sends build status notification to GUI-Client >-----------*/
        private void sendBuildStatus(string buildRequest)
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.reply);
            csndMsg.command = "BuildStatus";
            csndMsg.author = "Rahul Kadam";
            csndMsg.to = clientGUIAddress;
            csndMsg.from = childBuilderAddress;
            csndMsg.testRequest = buildRequest;
            csndMsg.body = "\t  Build Process Completed";
            comm.postMessage(csndMsg);
            csndMsg.show();
        }

        /*----< sends build logs to repo mock >-----------*/
        private void sendBuildLog(string buildRequest)
        {
            string xmlbuildlog = buildLogs.ToXml();
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.reply);
            csndMsg.command = "BuildLog";
            csndMsg.author = "Rahul Kadam";
            csndMsg.to = repoMockAddress;
            csndMsg.from = childBuilderAddress;
            csndMsg.testRequest = buildRequest;
            csndMsg.body = xmlbuildlog;
            comm.postMessage(csndMsg);
            csndMsg.show();
        }

        /*----< sends successfully file send msg >-----------*/
        private void successSendFiles(string from , List<string> testFiles, string itestRequest)
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.reply);
            csndMsg.command = "SuccessSentFiles";
            csndMsg.author = "Rahul Kadam";
            csndMsg.to = from;
            csndMsg.from = childBuilderAddress;
            csndMsg.testFiles = testFiles;
            csndMsg.testRequest = itestRequest;
            comm.postMessage(csndMsg);
            csndMsg.show();
        }

        /*----< transfer libraries to test harness mock >-----------*/
        private void fileTransferLibraries(List<string> testFiles)
        {
            foreach (string file in testFiles)
            {
                Console.WriteLine("\n  Sending File: {0} To: {1}", file, testHarnessAddress);
                bool transferSuccess = comm.postFile(file, librariespath , true);

                if (transferSuccess)
                {
                    Console.WriteLine("\n  Successfully Sent !");
                }
                else
                {
                    Console.WriteLine("  Send Failed !");
                }

            }
        }

        /*----< create ans send Test Request msg to test harness mock >-----------*/
        private void sendTestRequest(CommMessage msg, List<string> libraries, string itestRequest)
        {
            testRequest.author = msg.author;
            testRequest.dateTime = DateTime.Now.ToString();
            testRequest.tests = libraries;
            
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            csndMsg.command = "TestRequest";
            csndMsg.author = "Rahul Kadam";
            csndMsg.to = testHarnessAddress;
            csndMsg.from = childBuilderAddress;
            csndMsg.testRequest = itestRequest;
            csndMsg.body = testRequest.ToXml();
            comm.postMessage(csndMsg);
            Console.Write("\n\n  Sending TestRequest from Child Builder to TestHarness\n");
            csndMsg.show();
        }

        /*----< Attempts to Build Libraries >-----------*/
        public List<string> buildLibraries(string ibuildRequest)
        {          
            var frameworkPath = RuntimeEnvironment.GetRuntimeDirectory();
            var cscPath = Path.Combine(frameworkPath, "csc.exe");
            List<string> libraries = new List<string>();
            foreach (BuildElement te in buildRequest.tests){
                string libraryName = te.testName.Substring(0, te.testName.Length - 3) + ".dll";
                Console.WriteLine("\n\n  ----------------------------------------------------");
                Console.WriteLine("  Trying to build library:  {0}", libraryName);
                Console.WriteLine("  ----------------------------------------------------");
                Process p = new Process();
                p.StartInfo.FileName = cscPath;
                p.StartInfo.WorkingDirectory = childBuilderstoragePath;
                string str = @"/t:library /out:MyLibraries/" + libraryName + " ";
                StringBuilder sb = new StringBuilder(str);
                sb.Append(te.testDriver);
                sb.Append(" ");
                foreach (string testCode in te.testCodes) {
                    sb.Append(testCode);
                    sb.Append(" ");
                }
                p.StartInfo.Arguments = sb.ToString();
                Console.WriteLine("\n\n  ----------------------------------------------------");
                Console.WriteLine("  csc.exe "+sb.ToString());
                Console.WriteLine("  ----------------------------------------------------");
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;  p.StartInfo.RedirectStandardError = true;
                p.Start();      p.WaitForExit();
                string output = p.StandardOutput.ReadToEnd();
                bool status = false;
                if (isPresent(libraryName)){
                    status = true;
                    Console.WriteLine("\n\n  ----------------------------------------------------");
                    Console.WriteLine("  Build Successfull !!!");
                    Console.WriteLine("  ----------------------------------------------------");
                    libraries.Add(libraryName);
                }else{
                    status = false;
                    Console.WriteLine("\n\n  ----------------------------------------------------");
                    Console.WriteLine("  Build Fail !!!");
                    Console.WriteLine("\nErrors:", output);
                    Console.WriteLine("\n{0}", output);
                    Console.WriteLine("  ----------------------------------------------------");
                }
                BuildLog bl = new BuildLog(ibuildRequest, status);
                bl.addLog(output);
                buildLogs.add(bl);
            }
            return libraries;
        }

        /*----< checks for given library's presence in current Folder>---------------------------*/
        private bool isPresent(string libraryName)
        {
            return File.Exists(librariespath + "/" + libraryName);
        }

        /*----< parses Test Request >-----------*/
        void parseTestRequest(string filename)
        {
            string newPath = Path.Combine(childBuilderstoragePath, filename);
            string trXml = File.ReadAllText(newPath);
            buildRequest = trXml.FromXml<BuildRequest>();
            Console.Write("\n  BuildRequest: {0}", filename);
            Console.Write("\n\n");
            Console.Write(trXml);
            Console.Write("\n\n");

            foreach (BuildElement te in buildRequest.tests)
            {
                files.Add(te.testDriver);
                foreach (string testCode in te.testCodes)
                {
                    files.Add(testCode);
                }
            }            
        }

        /*----< sends "ready" msg back to Mother Builder >-----------*/
        void sendReadyMessage()
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            csndMsg.command = "ready";
            csndMsg.author = "Rahul Kadam";
            csndMsg.to = motherBuilderAddress;
            csndMsg.from = childBuilderAddress;
            comm.postMessage(csndMsg);
            Console.Write("\n\n  Ready message sent back to MotherBuildServer\n");
        }

        /*----< sends get TestRequest(.xml) msg to RepoMock >-----------*/
        void sendGetFileMessage(string filename)
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            csndMsg.command = "getFile";
            csndMsg.author = "Rahul Kadam";
            csndMsg.to = repoMockAddress;
            csndMsg.from = childBuilderAddress;
            csndMsg.testRequest = filename;
            comm.postMessage(csndMsg);
            csndMsg.show();
        }

        /*----< send get TestFiles(.cs) msg to RepoMock >-----------*/
        void sendGetFilesMessage(string filename)
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            csndMsg.command = "getFiles";
            csndMsg.author = "Rahul Kadam";
            csndMsg.to = repoMockAddress;
            csndMsg.from = childBuilderAddress;
            csndMsg.testRequest = filename;
            foreach (string file in files)
                csndMsg.testFiles.Add(file);           
            comm.postMessage(csndMsg);
            csndMsg.show();
            files.Clear();
        }

        /*----< Test Stub >-----------*/
        static void Main(string[] args)
        {
            Console.Title = "ChildBuilder@"+ args[0];

            Console.Write("\n  ChildBuilder Process");
            Console.Write("\n ====================");

            ChildBuilder b = new ChildBuilder(Convert.ToInt32(args[0]));
            Console.Write("\n  ChildBuilder: " + b.childBuilderAddress);
                       
        }
    }
}

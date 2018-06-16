/////////////////////////////////////////////////////////////////////////////
//  RepoMock.cs - RepoMock for Build Server in Federation                  //
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
 *    It has following main functions:
 *    1.  To Send BuildRequests msg to Mother Builder received from GUI-Client
 *    2.  To Search and Send BuildRequest(.xml) and TestFiles(.cs) to Child Builder when requested.
 *    3.  Save build logs from Child Builder and test logs test harness mock.
 *    4.  Provide a list of source code files, build request, build logs and test logs in Repository to GUI client when requested.
 *           
 *   Important functions:
 *   ------------------
 *   
 *   processMessages            -> processes messages
 *   executeRequest		        -> execute request messages
 *   sendingXMLFileList  	    -> sends xml file list
 *   sendingCodeFileList  	    -> ends source code file list
 *   StoreBuildRequest          -> saves build requests
 *   saveBuildLog		        -> saves build logs
 *   sendingBuildLogFileList	-> sends build logs file list
 *   savetestLog		        -> saves test logs
 *   sendingTestLogFileList	    -> sends test logs file list
 *   sendQuitMessage            -> sends Quit msg to Mother Builder
 *   sendFile	                -> sends TestRequest(.xml) or TestFiles(.cs) to childaddress 
 *   successSendFiles           -> sends success TestFiles(.cs) msg to childaddress
 *   sendingBuildRequests       -> sends Build Requests to Mother Builder
 *   getFilesHelper             -> private helper function for RepoMock.getFiles
 *   getFiles	                -> find all the files in RepoMock.storagePath
 *    
 *   
 *   Build Process
 *   -------------
 *   - Required files - RepoMock.cs
 *   - Compiler command: csc RepoMock.cs
 *    
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 6 Dec 2017
 * - first release
 * 
 */
using System;
using System.Collections.Generic;
using System.IO;
using MessagePassingComm;
using System.Threading;

namespace Federation
{
    ///////////////////////////////////////////////////////////////////
    // RepoMock class
    //

    public class RepoMock
    {
        public string clientGUIAddress { get; set; } = "http://localhost:8080/IMessagePassingComm";
        public string repoMockAddress { get; set; } = "http://localhost:8081/IMessagePassingComm";
        public string motherBuilderAddress { get; set; } = "http://localhost:8082/IMessagePassingComm";
        public string testHarnessAddress { get; set; } = "http://localhost:8090/IMessagePassingComm";
        public string storageCodePath { get; set; } = "../../../StorageRepoMock/Code";
        public string storageBRPath { get; set; } = "../../../StorageRepoMock/BuildRequests";
        public string storageBLPath { get; set; } = "../../../StorageRepoMock/BuildLogs";
        public string storageTLPath { get; set; } = "../../../StorageRepoMock/TestLogs";
        public List<string> codefiles { get; set; } = new List<string>();
        public List<string> xmlfiles { get; set; } = new List<string>();
        public List<string> buildLogfiles { get; set; } = new List<string>();
        public List<string> testLogfiles { get; set; } = new List<string>();
        public Comm comm { get; set; } = null;
        const int port = 8081;
        Thread msgHandler;

        public RepoMock()
        {
            if (!Directory.Exists(storageCodePath))
                Directory.CreateDirectory(storageCodePath);
            if (!Directory.Exists(storageBRPath))
                Directory.CreateDirectory(storageBRPath);
            if (!Directory.Exists(storageBLPath))
                Directory.CreateDirectory(storageBLPath);
            if (!Directory.Exists(storageTLPath))
                Directory.CreateDirectory(storageTLPath);
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

                    if (msg.command.Equals("quit"))
                    {
                        sendQuitMessage();
                        Console.Write("\n  Quit message Received\n"); Console.Write("\n  To start Building Process back again Please close all Console Windows.\n");
                        break;
                    }
                    executeRequest(msg);                    
                }
            }
        }

        /*----< execute request messages >--------*/
        private void executeRequest(CommMessage msg)
        {
            switch (msg.command)
            {
                case "CodeFileRequest":
                    getFiles("*.cs");
                    sendingCodeFileList();
                    break;
                case "XMLFileRequest":
                    getFiles("*.xml");
                    sendingXMLFileList();
                    break;
                case "SendBuildRequest":
                    StoreBuildRequest(msg.testRequest, msg.body);
                    break;
                case "BuildRequest":
                    sendingBuildRequests(msg.testRequest);
                    break;
                case "getFile":
                    if (sendFile(msg.testRequest, msg.from))
                        successSendFile(msg.testRequest, msg.from);
                    break;
                case "getFiles":
                    foreach (string filename in msg.testFiles)
                        sendFile(filename, msg.from);
                    successSendFiles(msg.from, msg.testRequest);
                    break;
                case "BuildLog":
                    saveBuildLog(msg.body, msg.testRequest);
                    break;
                case "BuildLogsFileRequest":
                    getFiles("*BuildLog.xml");
                    sendingBuildLogFileList();
                    break;
                case "TestLog":
                    getFiles("*BuildLog.xml");
                    savetestLog(msg.body, msg.testRequest);
                    break;
                case "TestLogsFileRequest":
                    getFiles("*TestLog.xml");
                    sendingTestLogFileList();
                    break;
            } 
        }

        /*----< sends test logs file list  >--------*/
        private void sendingTestLogFileList()
        {
            CommMessage csndMsg1 = new CommMessage(CommMessage.MessageType.reply);
            csndMsg1.command = "TestLogsFileList";
            csndMsg1.author = "Rahul Kadam";
            csndMsg1.to = clientGUIAddress;
            csndMsg1.from = repoMockAddress;
            csndMsg1.testFiles = testLogfiles;
            comm.postMessage(csndMsg1);
            Console.Write("\n\n  Sending TestLogsFileList from RepositoryMock to GUI-Client: ");
            csndMsg1.show();
        }

        /*----< saves test logs  >--------*/
        private void savetestLog(string body, string filename)
        {
            string ofilename = filename.Substring(0, filename.Length - 4);
            string filePath = storageTLPath + "/" + ofilename + "TestLog.xml";
            Console.WriteLine("TestLog Stored at: " + filePath);
            File.WriteAllText(filePath, body);
        }

        /*----< sends build logs file list  >--------*/
        private void sendingBuildLogFileList()
        {
            CommMessage csndMsg1 = new CommMessage(CommMessage.MessageType.reply);
            csndMsg1.command = "BuildLogsFileList";
            csndMsg1.author = "Rahul Kadam";
            csndMsg1.to = clientGUIAddress;
            csndMsg1.from = repoMockAddress;
            csndMsg1.testFiles = buildLogfiles;
            comm.postMessage(csndMsg1);
            Console.Write("\n\n  Sending BuildLogsFileList from RepositoryMock to GUI-Client: ");
            csndMsg1.show();
        }

        /*----< saves build logs  >--------*/
        private void saveBuildLog(string body, string filename)
        {
            string ofilename = filename.Substring(0, filename.Length - 4);
            string filePath = storageBLPath + "/" + ofilename + "BuildLog.xml";
            Console.WriteLine("BuildLog Stored at: " + filePath);
            File.WriteAllText(filePath, body);
        }

        /*----< saves build requests  >--------*/
        void StoreBuildRequest(string buildReqestFilename, string xmlstring)
        {
            string filePath = storageBRPath + "/" + buildReqestFilename;
            File.WriteAllText(filePath, xmlstring);
        }

        /*----< sends source code file list  >--------*/
        void sendingCodeFileList()
        {
            CommMessage csndMsg1 = new CommMessage(CommMessage.MessageType.reply);
            csndMsg1.command = "CodeFileList";
            csndMsg1.author = "Rahul Kadam";
            csndMsg1.to = clientGUIAddress;
            csndMsg1.from = repoMockAddress;
            csndMsg1.testFiles = codefiles;
            comm.postMessage(csndMsg1);
            Console.Write("\n\n  Sending CodeFileList from RepositoryMock to GUI-Client: ");
            csndMsg1.show();
        }

        /*----< sends xml file list  >--------*/
        private void sendingXMLFileList()
        {
            CommMessage csndMsg1 = new CommMessage(CommMessage.MessageType.reply);
            csndMsg1.command = "XMLFileList";
            csndMsg1.author = "Rahul Kadam";
            csndMsg1.to = clientGUIAddress;
            csndMsg1.from = repoMockAddress;
            csndMsg1.testFiles = xmlfiles;
            comm.postMessage(csndMsg1);
            Console.Write("\n\n  Sending XMLFileList from RepositoryMock to GUI-Client: ");
            csndMsg1.show();
        }

        /*----< sends Quit msg to Mother Builder >--------*/
        void sendQuitMessage()
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            csndMsg.command = "quit";
            csndMsg.author = "Rahul Kadam";
            csndMsg.to = motherBuilderAddress;
            csndMsg.from = repoMockAddress;
            comm.postMessage(csndMsg);
            Console.Write("\n\n  Sending Quit Message from RepositoryMock to MotherBuildServer ");
            csndMsg.show();

            CommMessage csndMsg2 = new CommMessage(CommMessage.MessageType.request);
            csndMsg2.command = "quit";
            csndMsg2.author = "Rahul Kadam";
            csndMsg2.to = testHarnessAddress;
            csndMsg2.from = repoMockAddress;
            comm.postMessage(csndMsg2);
            Console.Write("\n\n  Sending Quit Message from RepositoryMock to TestHarnessMock ");
            csndMsg2.show();
        }

        /*----< sends BuildRequest(.xml) or TestFiles(.cs) to childaddress >--------*/
        bool sendFile(string filename, string childaddress)
        {

            Console.WriteLine("\n  Sending File: {0} To: {1}", filename, childaddress);
            bool transferSuccess = comm.postFile(filename, childaddress);

            if (transferSuccess)
            {
                Console.WriteLine("\n  Successfully Sent !");
                return true;
            }
            else
            {
                Console.WriteLine("Send Failed !");
                return false;
            }
        }

        /*----< sends success BuildRequest(.xml) msg to childaddress >--------*/
        void successSendFile(string filename, string childaddress)
        {

            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.reply);
            csndMsg.command = "successSentFile";
            csndMsg.author = "Rahul Kadam";
            csndMsg.to = childaddress;
            csndMsg.from = repoMockAddress;
            csndMsg.testRequest = filename;
            comm.postMessage(csndMsg);
            csndMsg.show();
        }

        /*----< sends success TestFiles(.cs) msg to childaddress >--------*/
        void successSendFiles(string childaddress, string buildRequest)
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.reply);
            csndMsg.command = "successSentFiles";
            csndMsg.author = "Rahul Kadam";
            csndMsg.to = childaddress;
            csndMsg.from = repoMockAddress;
            csndMsg.testRequest = buildRequest;
            comm.postMessage(csndMsg);
            csndMsg.show();
        }

        /*----< sends Build Requests to Mother Builder >--------*/
        public void sendingBuildRequests(string buildRequest)
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            csndMsg.command = "BuildRequest";
            csndMsg.author = "Rahul Kadam";
            csndMsg.to = motherBuilderAddress;
            csndMsg.from = repoMockAddress;
            csndMsg.testRequest = Path.GetFileName(buildRequest);
            comm.postMessage(csndMsg);
            Console.Write("\n\n  Sending BuildRequest from RepositoryMock to MotherBuildServer: ");
            csndMsg.show();
        }

        /*----< private helper function for RepoMock.getFiles >--------*/
        private void getFilesHelper(string path, string pattern)
        {
            string[] tempFiles = Directory.GetFiles(path, pattern);
            for (int i = 0; i < tempFiles.Length; ++i)
            {
                tempFiles[i] = tempFiles[i];
            }
            if (pattern.Equals("*.cs"))
                codefiles.AddRange(tempFiles);
            else if (pattern.Equals("*.xml"))
                xmlfiles.AddRange(tempFiles);
            else if (pattern.Equals("*BuildLog.xml"))
                buildLogfiles.AddRange(tempFiles);
            else if (pattern.Equals("*TestLog.xml"))
                testLogfiles.AddRange(tempFiles);

            string[] dirs = Directory.GetDirectories(path);
            foreach (string dir in dirs)
            {
                getFilesHelper(dir, pattern);
            }
        }

        /*----< find all the files in RepoMock.storagePath >-----------*/
        /*
        *  Finds all the files, matching pattern, in the entire 
        *  directory tree rooted at repo.storagePath.
        */
        public void getFiles(string pattern)
        {
            if (pattern.Equals("*.cs"))
                codefiles.Clear();
            if (pattern.Equals("*.xml"))
                xmlfiles.Clear();
            if (pattern.Equals("*BuildLog.xml"))
                buildLogfiles.Clear();
            if (pattern.Equals("*TestLog.xml"))
                testLogfiles.Clear();

            if (pattern.Equals("*.cs"))
                getFilesHelper(storageCodePath, pattern);
            else if (pattern.Equals("*.xml"))
                getFilesHelper(storageBRPath, pattern);
            else if (pattern.Equals("*BuildLog.xml"))
                getFilesHelper(storageBLPath, pattern);
            else if (pattern.Equals("*TestLog.xml"))
                getFilesHelper(storageTLPath, pattern);
        }

        /*----< Test Stub >-----------*/
        static void Main(string[] args)
        {
            Console.Title = "RepoMock@" + port;

            Console.Write("\n  RepoMock Process");
            Console.Write("\n =====================");

            RepoMock repo = new RepoMock();
            Console.Write("\n  RepoMock Server: " + repo.repoMockAddress);
            /*
            // For Testing Separately from RepoMock to MotherBuildServer to ChildBuilder 
            repo.getFiles("*.xml");
            Console.Write("\n\n  Files Selected:");
            foreach (string file in repo.files)
                Console.Write("\n  \"{0}\"", file);
            repo.sendingBuildRequests();
            */
        }
    }

}

/////////////////////////////////////////////////////////////////////////////
//  MainWindow.xaml.cs - GUI-Client for Build Server in Federation         //
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
 *    GUI-Client for Build Server has 2 main functions:
 *    1.  To Create BuildRequests(.xml) 
 *        Tab 1 - create is used to satisfy this function
 *    2.  To Build and Test BuildRequests and view build logs and test logs
 *        Tab 2 - build and test is used to satisfy this function
 *            
 *   Main Operations:
 *   ------------------
 *   1.   Add Test Button  
 *        Adds 1 Test Driver and Multiple Test Files in 1 Test Element.
 *          
 *   2.   Build Request Button  
 *        Adds Multiple Test Elements into 1 Test Request. Finalizes the creation of a BuildRequest xml file stored in 'StorageClient/' Folder 
 *   
 *   3.   Send Build Requests Button 
 *        Sends Build Request files in  'StorageClient/' Folder to 'StorageRepoMock/BuildRequests/'
 *   
 *   3.   Build and Test Button 
 *        ->  If Selected TestRequests is 1 or more then:
 *            -> It sends "buildrequest" for all the selected items in the listbox to Repo Mock and thus starts entire process
 *             
 *   4.   Quit Button
 *        ->  It sends a "quit" message to RepoMock which RepoMock then forwards to Mother Build Server and then to Child Builders and also to Test Harness Mock 
 *        ->  This shutsdown the RepoMock , Mother Builder , Child Builders and Test Harness Mock.
 *        ->  Pressing the Build Button again won't work as all the connections are closed.
 *        ->  To start Build Process again close all console windows.
 *        
 *   Important Note:
 *   ------------------
 *   For Demonstration, I am programatically setting the input parameters for entire Build Process to execute in run.bat.
 *   In GUI package -> MainWindow.xaml.cs file -> MainWindow class -> Window_Loaded() function ->  Test_Exec(sender, e) 
 *   Test_Exec(sender, e)  does following :
 *   -> Demonstrates creation of Build Requests
 *   -> Sends build requests to Repo Mock
 *   -> Sends build command to build all the build requests in Repo Mock
 * 
 *   Build Process
 *   -------------
 *   - Required files - MainWindow.xaml.cs
 *   - Compiler command: csc MainWindow.xaml.cs
 *    
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 6 Dec 2017
 * - first release
 * 
 */
using System;
using System.Windows;
using System.IO;
using Utilities;
using System.Windows.Controls;
using MessagePassingComm;
using System.Threading;
using System.Collections.Generic;
using GUI;

namespace Federation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string repoBuildRequestsPath { get; set; } = "../../../StorageRepoMock/BuildRequests";
        public string repoBuildLogsPath { get; set; } = "../../../StorageRepoMock/BuildLogs";
        public string repoTestLogsPath { get; set; } = "../../../StorageRepoMock/TestLogs";
        public string repoCodePath { get; set; } = "../../../StorageRepoMock/Code";
        public string clientStoragePath { get; set; } = "../../../StorageClient";
        public string repoMockAddress { get; set; } = "http://localhost:8081/IMessagePassingComm";
        public string clientGUIAddress { get; set; } = "http://localhost:8080/IMessagePassingComm";
        public string motherBuilderAddress { get; set; } = "http://localhost:8082/IMessagePassingComm";
        public BuildRequest tr { get; set; } = null;
        public Comm comm { get; set; } = null;
        Dictionary<string, Action<CommMessage>> messageDispatcher = new Dictionary<string, Action<CommMessage>>();
        Thread rcvThread = null;
        const int port = 8080;

        public MainWindow()
        {
            Console.Title = "GUI-Client@" + port;
            Console.Write("\n  GUI-Client Process");
            Console.Write("\n =====================");
            Console.Write("\n  GUI-Client Server: {0}", clientGUIAddress);
            InitializeComponent();
            comm = new Comm("http://localhost", port);
            tr = new BuildRequest();
            tr.author = "Rahul Kadam";
            Display.Text = "null";
            Display.IsReadOnly = true;
            initializeMessageDispatcher();
            rcvThread = new Thread(rcvThreadProc);
            rcvThread.Start();
        }

        /*----< when window gets loaded >-----------*/
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GetContents(sender, e);
            initializeClientBuildRequestsListBox();
            GetRepositoryBuildRequests(sender, e);
            Test_Exec(sender, e);
        }

        /*----< Test Executive function >-----------*/
        private void Test_Exec(object sender, RoutedEventArgs e)
        {
            tabCntrl.SelectedItem = Builder;

            Console.Write("\n\n  Demonstrating Creating a Build Request ");
            Console.Write("\n -------------------------------------------");

            BuildRequest tr1 = new BuildRequest();
            BuildElement be1 = new BuildElement();
            be1.testName = "TestDriver1.cs";
            be1.addDriver("TestDriver1.cs");
            be1.addCode("TestDriver1TestFile1.cs");
            be1.addCode("TestDriver1TestFile2.cs");
            tr1.author = "Rahul Kadam";
            tr1.dateTime = DateTime.Now.ToString();
            tr1.tests.Add(be1);
            BuildElement be2 = new BuildElement();
            be2.testName = "TestDriver2.cs";
            be2.addDriver("TestDriver2.cs");
            be2.addCode("TestDriver2TestFile1.cs");
            be2.addCode("TestDriver2TestFile2.cs");
            tr1.author = "Rahul Kadam";
            tr1.dateTime = DateTime.Now.ToString();
            tr1.tests.Add(be2);
            
            Console.Write("\n  Build Request Created: ");
            Console.Write("\n  "+tr1.ToString());
            Console.Write("\n\n  Serializing Build Request to XML: ");
            string xmlstring = tr1.ToXml();
            Console.Write("\n\n" + xmlstring);

            List<string> repobuildrequestslist = new List<string>();
            repobuildrequestslist.Add("BuildRequest1.xml"); repobuildrequestslist.Add("BuildRequest2.xml");
            repobuildrequestslist.Add("BuildRequest3.xml"); repobuildrequestslist.Add("BuildRequest4.xml");

            foreach (string testRequest in repobuildrequestslist)
            {
                CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
                csndMsg.command = "BuildRequest";
                csndMsg.author = "Rahul Kadam";
                csndMsg.to = repoMockAddress;
                csndMsg.from = clientGUIAddress;
                csndMsg.testRequest = Path.GetFileName(testRequest);
                comm.postMessage(csndMsg);
                Console.Write("\n\n  Sending BuildRequest from GUI-Client to RepositoryMock ");
                csndMsg.show();
            }
        }

        //----< define how to process each message command >-------------

        void initializeMessageDispatcher()
        {
            messageDispatcher["CodeFileList"] = (CommMessage msg) =>{
                TestDriver.Items.Clear();
                TestFiles.Items.Clear();
                foreach (string file in msg.testFiles){
                    TestDriver.Items.Add(Path.GetFileName(file));
                    TestFiles.Items.Add(Path.GetFileName(file));
                }
            };
            messageDispatcher["XMLFileList"] = (CommMessage msg) =>{
                RepoBuildRequests.Items.Clear();
                foreach (string file in msg.testFiles)
                    RepoBuildRequests.Items.Add(Path.GetFileName(file));
            };
            messageDispatcher["BuildStatus"] = (CommMessage msg) =>{
                Console.WriteLine("\n\n  Received Build Status for " + msg.testRequest + " from Builder: " + msg.body);
                CommMessage csndMsg1 = new CommMessage(CommMessage.MessageType.request);
                csndMsg1.command = "BuildLogsFileRequest";
                csndMsg1.author = "Rahul Kadam";
                csndMsg1.to = repoMockAddress;
                csndMsg1.from = clientGUIAddress;
                comm.postMessage(csndMsg1);
                Console.Write("\n\n  Sending BuildLogsFileRequest from GUI-Client to RepositoryMock: ");
                csndMsg1.show();
            };
            messageDispatcher["BuildLogsFileList"] = (CommMessage msg) =>{
                RepoBuildLogs.Items.Clear();
                foreach (string file in msg.testFiles)
                    RepoBuildLogs.Items.Add(Path.GetFileName(file));
            };
            messageDispatcher["TestStatus"] = (CommMessage msg) =>{
                Console.WriteLine("\n\n  Received Test Status for " + msg.testRequest + " from TestHarness: " + msg.body);
                CommMessage csndMsg1 = new CommMessage(CommMessage.MessageType.request);
                csndMsg1.command = "TestLogsFileRequest";
                csndMsg1.author = "Rahul Kadam";
                csndMsg1.to = repoMockAddress;
                csndMsg1.from = clientGUIAddress;
                comm.postMessage(csndMsg1);
                Console.Write("\n\n  Sending TestLogsFileRequest from GUI-Client to RepositoryMock: ");
                csndMsg1.show();
            };
            messageDispatcher["TestLogsFileList"] = (CommMessage msg) =>{
                RepoTestLogs.Items.Clear();
                foreach (string file in msg.testFiles)
                    RepoTestLogs.Items.Add(Path.GetFileName(file)); 
            };
        }
        //----< define processing for GUI's receive thread >-------------

        void rcvThreadProc()
        {
            Console.Write("\n  starting client's receive thread");
            while (true)
            {
                CommMessage msg = comm.getMessage();

                if (msg.command == null)
                    continue;
                if (!msg.type.Equals("connect"))
                    msg.show();

                // pass the Dispatcher's action value to the main thread for execution

                Dispatcher.Invoke(messageDispatcher[msg.command], new object[] { msg });
            }
        }

        /*----< Initializes ClientBuildRequests ListBox >-----------*/
        void initializeClientBuildRequestsListBox()
        {
            ClientBuildRequests.Items.Clear();
            string[] files = Directory.GetFiles(clientStoragePath);

            if (files.Length > 0)
            {
                foreach (string file in files)
                {
                    ClientBuildRequests.Items.Add(Path.GetFileName(file));
                }
            }

        }

        /*----< get contents from repo mock about C# code files >-----------*/
        private void GetContents(object sender, RoutedEventArgs e)
        {
            CommMessage csndMsg1 = new CommMessage(CommMessage.MessageType.request);
            csndMsg1.command = "CodeFileRequest";
            csndMsg1.author = "Rahul Kadam";
            csndMsg1.to = repoMockAddress;
            csndMsg1.from = clientGUIAddress;
            comm.postMessage(csndMsg1);
            Console.Write("\n\n  Sending CodeFileRequest from GUI-Client to RepositoryMock: ");
            csndMsg1.show();
        }

        /*----< get contents from repo mock about xml build request files >-----------*/
        private void GetRepositoryBuildRequests(object sender, RoutedEventArgs e)
        {
            CommMessage csndMsg1 = new CommMessage(CommMessage.MessageType.request);
            csndMsg1.command = "XMLFileRequest";
            csndMsg1.author = "Rahul Kadam";
            csndMsg1.to = repoMockAddress;
            csndMsg1.from = clientGUIAddress;
            comm.postMessage(csndMsg1);
            Console.Write("\n\n  Sending XMLFileRequest from GUI-Client to RepositoryMock: ");
            csndMsg1.show();
        }

        /*----< send build request processing >-----------*/
        private void SendBuildRequest(object sender, RoutedEventArgs e)
        {
            foreach (string request in ClientBuildRequests.SelectedItems)
            {
                CommMessage csndMsg1 = new CommMessage(CommMessage.MessageType.request);
                csndMsg1.command = "SendBuildRequest";
                csndMsg1.author = "Rahul Kadam";
                csndMsg1.to = repoMockAddress;
                csndMsg1.from = clientGUIAddress;
                csndMsg1.testRequest = request;
                csndMsg1.body = File.ReadAllText(clientStoragePath + "/" + request);
                comm.postMessage(csndMsg1);
                Console.Write("\n\n  Sending SendBuildRequest from GUI-Client to RepositoryMock: ");
                csndMsg1.show();
            }
            tabCntrl.SelectedItem = Builder;
            GetRepositoryBuildRequests(sender, e);
        }

        /*----< Processes Add Test Button >-----------*/
        private void AddTest_Click(object sender, RoutedEventArgs e)
        {

            BuildElement te1 = new BuildElement();
            te1.testName = TestDriver.SelectedItem.ToString();
            te1.addDriver(TestDriver.SelectedItem.ToString());
            foreach (string test in TestFiles.SelectedItems)
                te1.addCode(test);
            tr.author = "Rahul Kadam";
            tr.dateTime = DateTime.Now.ToString();
            tr.tests.Add(te1);
            Display.Text = tr.ToXml();

        }

        /*----< Processes Create Test request Button >-----------*/
        private void CreateTestRequest_Click(object sender, RoutedEventArgs e)
        {
            int count = ClientBuildRequests.Items.Count;
            string filePath = clientStoragePath + "/BuildRequest" + (++count) + ".xml";
            File.WriteAllText(filePath, Display.Text);
            Display.Text = "null";
            initializeClientBuildRequestsListBox();
            tr = new BuildRequest();
        }


        /*----< Processes Build and Test Button in Tab 2 >-----------*/
        private void Build_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Inside Build_Click");
            if (RepoBuildRequests.SelectedItems.Count != 0)
            {
                foreach (string testRequest in RepoBuildRequests.SelectedItems)
                {
                    CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
                    csndMsg.command = "BuildRequest";
                    csndMsg.author = "Rahul Kadam";
                    csndMsg.to = repoMockAddress;
                    csndMsg.from = clientGUIAddress;
                    csndMsg.testRequest = Path.GetFileName(testRequest);
                    comm.postMessage(csndMsg);
                    Console.Write("\n\n  Sending BuildRequest from GUI-Client to RepositoryMock ");
                    csndMsg.show();
                }
            }
            else
                Console.Write("\n\n  Please select atleast 1 TestRequest file to Build!");
        }

        /*----< Processes Quit Button in Tab 2 >-----------*/
        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            csndMsg.command = "quit";
            csndMsg.author = "Rahul Kadam";
            csndMsg.to = repoMockAddress;
            csndMsg.from = clientGUIAddress;
            comm.postMessage(csndMsg);
            Console.Write("\n\n  Sending Quit Message from GUI-Client to RepositoryMock ");
            csndMsg.show();
            Console.Write("\n  To start Building Process back again Please close all Console Windows.\n");
        }

        /*----< Double Click Client Build Requests >-----------*/
        private void ClientBuildRequestsDC(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string fileName = (string)ClientBuildRequests.SelectedValue;
            try
            {
                string path = clientStoragePath + "/" + fileName;
                string contents = File.ReadAllText(path);
                Console.WriteLine(contents);
                CodePopUp popup = new CodePopUp();
                popup.codeView.Text = contents;
                popup.Show();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }
        /*----< Double Click Test Driver Files >-----------*/
        private void TestDriverDC(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string fileName = (string)TestDriver.SelectedValue;
            try
            {
                string path = repoCodePath + "/" + fileName;
                string contents = File.ReadAllText(path);
                CodePopUp popup = new CodePopUp();
                popup.codeView.Text = contents;
                popup.Show();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }
        /*----< Double Click Test Files >-----------*/
        private void TestFilesDC(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string fileName = (string)TestFiles.SelectedValue;
            try
            {
                string path = repoCodePath + "/" + fileName;
                string contents = File.ReadAllText(path);
                CodePopUp popup = new CodePopUp();
                popup.codeView.Text = contents;
                popup.Show();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }
        /*----< Double Click Repo Build Request files >-----------*/
        private void RepoBuildRequestsDC(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string fileName = (string)RepoBuildRequests.SelectedValue;
            try
            {
                string path = repoBuildRequestsPath + "/" + fileName;
                string contents = File.ReadAllText(path);
                CodePopUp popup = new CodePopUp();
                popup.codeView.Text = contents;
                popup.Show();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }
        /*----< Double Click Repo Build Logs files >-----------*/
        private void RepoBuildLogsDC(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string fileName = (string)RepoBuildLogs.SelectedValue;
            try
            {
                string path = repoBuildLogsPath + "/" + fileName;
                string contents = File.ReadAllText(path);
                CodePopUp popup = new CodePopUp();
                popup.codeView.Text = contents;
                popup.Show();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }
        /*----< Double Click Repo Test Logs files >-----------*/
        private void RepoTestLogsDC(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string fileName = (string)RepoTestLogs.SelectedValue;
            try
            {
                string path = repoTestLogsPath + "/" + fileName;
                string contents = File.ReadAllText(path);
                CodePopUp popup = new CodePopUp();
                popup.codeView.Text = contents;
                popup.Show();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }
    }
}

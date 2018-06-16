/////////////////////////////////////////////////////////////////////////////
//  BuildServer.cs - Mother Build Server in Federation                     //
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
 *   It spawns Child Builder Processes for Build Server Processing with input of number of Processes from command line arguments.
 *   
 *   It has 2 additional BlockingQueues:
 *   1.  readyMessagesQ which stores the "ready" messages from Child Buidler
 *   2.  buildRequestsQ which stores the "buildrequest" messages from RepoMock
 *   
 *   It has following main functions:
 *   1.  To spawn number of processes with input of number of Processes from GUI-Client.
 *   2.  To Send "buildrequest" msg to Child Builder which are available for processing based on ready msg queue 
 *   3.  To shutdown child processes with "quit" msg
 *           
 *   Important functions:
 *   ------------------
 *   
 *   processMessages            -> processes Messages
 *   spawnNumberOfProcesses     -> spawns child builders depending on input numberOfProcesses
 *   spawnProcess               -> spawns child builders
 *   stopChildBuilderProcesses  -> sends quit msg to stop child builders 
 *   sendTestRequests           -> sends test requests to child builders
 *     
 *   Build Process
 *   -------------
 *   - Required files - BuildServer.cs
 *   - Compiler command: csc BuildServer.cs
 *    
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 6 Dec 2017
 * - first release
 * 
 */
using System;
using MessagePassingComm;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Federation
{
    ///////////////////////////////////////////////////////////////////
    // BuildServer class
    //

    public class BuildServer
    {       
        public string motherBuilderAddress { get; set; } = "http://localhost:8082/IMessagePassingComm";
        public static SWTools.BlockingQueue<CommMessage> readyMessagesQ { get; set; } = null;
        public static SWTools.BlockingQueue<CommMessage> buildRequestsQ { get; set; } = null;
        public Comm comm { get; set; } = null;
        public int numberOfProcesses { get; set; }
        Thread msgHandler;
        const int port = 8082;

        public BuildServer(int number)
        {
                numberOfProcesses = number;
                comm = new Comm("http://localhost", port);                
                readyMessagesQ = new SWTools.BlockingQueue<CommMessage>();
                buildRequestsQ = new SWTools.BlockingQueue<CommMessage>();
                msgHandler = new Thread(processMessages);
                msgHandler.Start();
                spawnNumberOfProcesses(numberOfProcesses);
        }

        /*----< processes Messages >-----------*/
        void processMessages()
        {
            while (true)
            {
                CommMessage msg = comm.getMessage();
                if (msg.command != null)
                {
                    if (!msg.type.Equals("connect"))
                    {
                        if (!msg.command.Equals("ready"))
                            msg.show();
                    }

                    if (msg.command.Equals("ready"))
                    {
                        Console.Write("\n\n  Ready message received from ChildBuilder @ {0}\n", msg.from);
                        readyMessagesQ.enQ(msg);
                    }
                    else if (msg.command.Equals("BuildRequest"))
                    {
                        buildRequestsQ.enQ(msg);
                    }

                    if (msg.command.Equals("quit"))
                    {
                        stopChildBuilderProcesses();
                        Console.Write("\n  Quit message Received\n");
                        Console.Write("\n  To start Building Process back again Please close all Console Windows.\n");
                        break;
                    }
                }
            }
        }

        /*----< spawns child builders depending on input numberOfProcesses >-----------*/
        private void spawnNumberOfProcesses(int numberOfProcesses)
        {
            int iport = port;
            for (int i = 1; i <= numberOfProcesses; ++i)
            {
                if (spawnProcess(++iport))
                {
                    //Console.Write(" - succeeded");
                }
                else
                {
                    Console.Write(" - failed");
                }
            }
        }

        /*----< spawns child builders >-----------*/
        private bool spawnProcess(int i)
        {
            Process proc = new Process();
            string fileName = "..\\..\\..\\Builder\\bin\\debug\\Builder.exe";
            string absFileSpec = Path.GetFullPath(fileName);

            Console.Write("\n  attempting to start {0}", absFileSpec);
            string commandline = i.ToString();
            try
            {
                Process.Start(fileName, commandline);
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
                return false;
            }
            return true;
        }

        /*----< sends quit msg to stop child builders >-----------*/
        public void stopChildBuilderProcesses()
        {
            for (int i = 1; i <= numberOfProcesses; i++)
            {
                CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
                csndMsg.command = "quit";
                csndMsg.author = "Rahul Kadam";
                csndMsg.to = "http://localhost:" + (port + i).ToString() + "/IMessagePassingComm";
                csndMsg.from = motherBuilderAddress;
                comm.postMessage(csndMsg);
                csndMsg.show();
            }
        }

        /*----< sends test requests to child builders >-----------*/
        public void sendTestRequests()
        {
            while (readyMessagesQ != null && buildRequestsQ != null)
            {
                CommMessage buildRequestMsg = buildRequestsQ.deQ();
                CommMessage readyMsg = readyMessagesQ.deQ();
                buildRequestMsg.from = motherBuilderAddress;
                buildRequestMsg.to = readyMsg.from;
                comm.postMessage(buildRequestMsg);
                buildRequestMsg.show();
            }
            Console.Write("\n  Both queues Empty!");
        }

        /*----< Test Stub >-----------*/
        static void Main(string[] args)
        {
            Console.Title = "BuildServer@" + port;

            Console.Write("\n  BuildServer Process");
            Console.Write("\n =====================");

            int numberofPro = 3;

            if (args.Length > 0)
            {
                try
                {
                    numberofPro = Convert.ToInt32(args[0]);
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  Please Enter Integers only!");
                    Console.Write("\n\n  Exception: {0}", ex.Message);
                }
            }
            else
            {
                Console.Write("\n\n  No CommandLine Args Provided");
                Console.Write("\n\n  Proceeding with default 3 Number of Processes");
            }

            BuildServer bs = new BuildServer(numberofPro);
            Console.Write("\n  Build Server: " + bs.motherBuilderAddress);
            Thread msgHandler2 = new Thread(bs.sendTestRequests);
            msgHandler2.Start();
            /*
             // Only for Testing Mother Build Server separately with ChildBuilder
            Thread.Sleep(1000);
            bs.stopChildBuilderProcesses();
            */
        }
    }
}

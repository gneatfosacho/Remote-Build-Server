/////////////////////////////////////////////////////////////////////////////
//  Executive.cs - Test Executive for Project 4                        //
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
 *    Shows all Requirements satisfied for Project 4
 *    
 *   Build Process
 *   -------------
 *   - Required files - Executive.cs
 *   - Compiler command: csc Executive.cs
 *    
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 6 Dec 2017
 * - Project 4 release
 * 
 */
using System;

namespace Federation
{
    ///////////////////////////////////////////////////////////////////
    // Executive class
    //
    class Executive
    {
        /*----< introduction >-----------------------------*/
        void intro()
        {
            Console.WriteLine("\n\n  \t\t\t\t====================================================");
            Console.Write("  \t\t\t\t\t\tProject 4\n");
            Console.WriteLine("  \t\t\t\t====================================================");
        }

        /*----< Requirement 1 >-----------------------------*/
        void req1()
        {
            Console.WriteLine("\n\n  ===============");
            Console.Write("  Requirement 1\n");
            Console.WriteLine("  ===============");
            Console.WriteLine("\n  Project was prepared using C#, the .Net Framework, and Visual Studio 2017. ");
        }

        /*----< Requirement 2 >-----------------------------*/
        void req2()
        {
            Console.WriteLine("\n\n  ===============");
            Console.Write("  Requirement 2\n");
            Console.WriteLine("  ===============");
            Console.WriteLine("\n  Project includes a Message-Passing Communication Service built with WCF");
            Console.WriteLine("\n  Packages IMessagePassingCommService and MessagePassingCommService are used for Message-Passing Communication Service in WCF ");

            Console.WriteLine("\n  Entire Process using WCF Communication is as follows:");
            Console.WriteLine("\n  -> GUI-Client-Port#8080 , RepoMock-Port#8081 , MotherBuildServer-Port#8082 and TestHarnessMock-Port#8090 will be started at the same time");
            Console.WriteLine("\n  -> For Demonstration, 3 ChildBuilders will be spawned at different ports by MotherBuildServer");
            Console.WriteLine("\n  -> ChildBuilder - Port#8083,ChildBuilder-Port#8084 and ChildBuilder-Port#8085");
            Console.WriteLine("\n  -> For Demonstration, 4 BuildRequests will be processed by 3 Child Builders (Reusing 1 Child Builder)");
            Console.WriteLine("\n  -> The 4 BuildRequests are as follows:");
            Console.WriteLine("\n  \t - BuildRequest1.xml -> demo of single test i.e 1 test case of 1 test driver and 2 test files ");
            Console.WriteLine("\n  \t - BuildRequest2.xml -> demo of multiple tests i.e 2 test cases");
            Console.WriteLine("\n  \t - BuildRequest3.xml -> demo of handling test failure with exeception handling");
            Console.WriteLine("\n  \t - BuildRequest4.xml -> demo of build fail, hence no TestLog will be created for this");
        }

        /*----< Requirement 3 >-----------------------------*/
        void req3()
        {
            Console.WriteLine("\n\n  ===============");
            Console.Write("  Requirement 3\n");
            Console.WriteLine("  ===============");
            Console.WriteLine("\n  The Communication Service supports following: ");
            Console.WriteLine("\n  1. Accessing build requests by Pool Processes from the mother Builder process");
            Console.WriteLine("\n  2. Sending and Receiving build requests");
            Console.WriteLine("\n  3. Sending and Receiving files");

            Console.WriteLine("\n\n  -> For Demonstration, following is the folder structure:");
            Console.WriteLine("\n  -> 'StorageClient/' Folder stores BuildRequests created by user using GUI");
            Console.WriteLine("\n  -> 'StorageRepoMock/Code' Folder stores C# source code files");
            Console.WriteLine("\n  -> 'StorageRepoMock/BuildRequests' Folder stores BuildRequest xml files sent by user through GUI");
            Console.WriteLine("\n  -> 'StorageRepoMock/BuildLogs' Folder stores Build Logs xml files");
            Console.WriteLine("\n  -> 'StorageRepoMock/TestLogs' Folder stores Test Logs xml files");
            Console.WriteLine("\n  -> 'StorageChildBuilder/ChildBuilder#PortNum' Folder stores BuildRequest and TestFiles for each Child Builder which specific unique port number");
            Console.WriteLine("\n  -> 'StorageChildBuilder/ChildBuilder#PortNum/MyLibraries' Folder stores libraries(dlls) for each Child Builder which specific unique port number");
            Console.WriteLine("\n  -> 'StorageTestHarnessMock/Libraries' Folder stores libraries(dlls) for each TestRequest");

            Console.WriteLine("\n\n  For Sending and Receiving of BuildRequest and Test Files, Please refer above Folder structure");

        }
        /*----< Requirement 4 >-----------------------------*/
        void req4()
        {
            Console.WriteLine("\n\n  ===============");
            Console.Write("  Requirement 4\n");
            Console.WriteLine("  ===============");
            Console.WriteLine("\n  RepoMock package provides: ");
            Console.WriteLine("\n  1. Client browsing to find files to build");
            Console.WriteLine("\n  \t -> GUI Tab 2 displays Repository Build Requests which user can select to build");
            Console.WriteLine("\n  2. Build a XML build request based on user selection by GUI");
            Console.WriteLine("\n  \t -> GUI Tab 1 displays Repository C# Code files which user can select to create Build Request Files");
            Console.WriteLine("\n  3. Sends XML build request and .cs files to child builder on request");
            Console.WriteLine("\n  \t -> GUI Tab 2 'build and test' button will eventually send build request to Child Builder for processing");
        }

        /*----< Requirement 5 >-----------------------------*/
        void req5()
        {
            Console.WriteLine("\n\n  ===============");
            Console.Write("  Requirement 5\n");
            Console.WriteLine("  ===============");
            Console.WriteLine("\n  BuildServer package takes number of child builder processes to be created as commandline arguments");
            Console.WriteLine("\n  You can change run.bat file with 'start BuildServer.exe 3' modifying default 3 to any number of Child Builders");

        }

        /*----< Requirement 6 >-----------------------------*/
        void req6()
        {
            Console.WriteLine("\n\n  ===============");
            Console.Write("  Requirement 6\n");
            Console.WriteLine("  ===============");
            Console.WriteLine("\n  BuildServer package and ChildBuilder communicate using Message-Passing Communication Service built with WCF");
            Console.WriteLine("\n  Child Builder sends ready message after completion of Build Process");
            Console.WriteLine("\n  BuildServer provides the build request to an available Child Builder");
            Console.WriteLine("\n  Please check BuildServer and any Child Builder Consoles for messages");
        }
        /*----< Requirement 7 >-----------------------------*/
        void req7()
        {
            Console.WriteLine("\n\n  ===============");
            Console.Write("  Requirement 7\n");
            Console.WriteLine("  ===============");
            Console.WriteLine("\n  Each Pool Process will attempt to build each library, cited in a retrieved build request, logging warnings and errors.");
            Console.WriteLine("\n  \t-> 'StorageChildBuilder/ChildBuilder#PortNum' Folder stores BuildRequest and TestFiles for each Child Builder which specific unique port number");
            Console.WriteLine("\n  \t-> 'StorageChildBuilder/ChildBuilder#PortNum/MyLibraries' Folder stores libraries(dlls) for each Child Builder which specific unique port number");

        }
        /*----< Requirement 8 >-----------------------------*/
        void req8()
        {
            Console.WriteLine("\n\n  ===============");
            Console.Write("  Requirement 8\n");
            Console.WriteLine("  ===============");
            Console.WriteLine("\n  If the build succeeds, Child Builder creates a test request and sends libraries to the Test Harness for execution");
            Console.WriteLine("\n  \t-> 'StorageChildBuilder/ChildBuilder#PortNum/MyLibraries' Folder stores libraries(dlls) for each Child Builder which specific unique port number");
            Console.WriteLine("\n  \t-> 'StorageTestHarnessMock/Libraries' Folder stores libraries(dlls) for each TestRequest");
            Console.WriteLine("\n  \t-> 'StorageRepoMock/BuildLogs' Folder stores Build Logs xml files");

        }
        /*----< Requirement 9 >-----------------------------*/
        void req9()
        {
            Console.WriteLine("\n\n  ===============");
            Console.Write("  Requirement 9\n");
            Console.WriteLine("  ===============");
            Console.WriteLine("\n  The Test Harness performs following actions:");
            Console.WriteLine("\n  1. Attempts to load each test library it receives and execute it");
            Console.WriteLine("\n  \t-> 'StorageTestHarnessMock/Libraries' Folder stores libraries(dlls) for each TestRequest");
            Console.WriteLine("\n  2. Submits the results of testing to the Repository");
            Console.WriteLine("\n  \t-> 'StorageRepoMock/TestLogs' Folder stores Test Logs xml files");
        }
        /*----< Requirement 10 >-----------------------------*/
        void req10()
        {
            Console.WriteLine("\n\n  ===============");
            Console.Write("  Requirement 10\n");
            Console.WriteLine("  ===============");
            Console.WriteLine("\n  GUI package is a Graphical User Interface, built using WPF");

        }

        /*----< Requirement 11 >-----------------------------*/
        void req11()
        {
            Console.WriteLine("\n\n  ===============");
            Console.Write("  Requirement 11\n");
            Console.WriteLine("  ===============");
            Console.WriteLine("\n  GUI client is a  a separate process, implemented with WPF and uses message-passing communication");
            Console.WriteLine("\n  It performs following actions:");
            Console.WriteLine("\n  1. Provides mechanisms to get file lists from the Repository, and select files for packaging into a test library");
            Console.WriteLine("\n  \t-> List of files from 'StorageRepoMock/Code' will be displayed in GUI - Tab 1 ");
            Console.WriteLine("\n  \t-> User can select 1 test driver and multiple test file and press 'Add Test' button to create one test case");
            Console.WriteLine("\n  2. Provides the capability of repeating the process to add other test libraries to the build request structure");
            Console.WriteLine("\n  \t-> User can follow above process again to create multiple test cases and eventually press 'Build Request' button to finalize the Build Request ");

        }

        /*----< Requirement 12 >-----------------------------*/
        void req12()
        {
            Console.WriteLine("\n\n  ===============");
            Console.Write("  Requirement 12\n");
            Console.WriteLine("  ===============");
            Console.WriteLine("\n  GUI client sends buildrequests(.xml) files for storage and transmission to RepositoryMock");
            Console.WriteLine("\n  \t->  GUI-Tab 1 bottom right Listbox shows 'StorageClient/' build requests");
            Console.WriteLine("\n  \t->  user can select multiple build request in this listbox and press 'send build requests' to send files from 'StorageClient/' to 'StorageRepoMock/BuildRequests' ");
            Console.WriteLine("\n  \t->  on pressing above button Tab 1 will be changed to Tab 2 automatically and Repository contents will also be updated simultaneously");


        }

        /*----< Requirement 13 >-----------------------------*/
        void req13()
        {
            Console.WriteLine("\n\n  ===============");
            Console.Write("  Requirement 13\n");
            Console.WriteLine("  ===============");
            Console.WriteLine("\n  GUI client can request the repository to send a build request in its storage to the Build Server for build processing");
            Console.WriteLine("\n  \t->  GUI-Tab 2 top left Listbox shows 'StorageRepoMock/BuildRequests' build requests");
            Console.WriteLine("\n  \t->  user can select multiple build request in this listbox and press 'build and test' for starting the process");

        }

        /*----< Requirement ocd >-----------------------------*/
        void ocd()
        {
            Console.WriteLine("\n\n  ==============================");
            Console.Write("  As-Built Design Document\n");
            Console.WriteLine("  ==============================");
            Console.WriteLine("\n  1. OCD was built on Project 1, Please find 'OCD_Build_Server_Rahul_Kadam_Project4.pdf' in Project 4 Folder");
            Console.WriteLine("\n  2. OCD uses Activity diagrams, Package diagrams and Class diagrams");
            Console.WriteLine("\n  3. Commented on changes to the core concept as design evolved, and on deficiencies of my project");

        }

        /*----< driver >-----------------------------*/
        static void Main(string[] args)
        {
            Console.Title = "Executive";
            Executive ex = new Executive();
            ex.intro();
            ex.req1();
            ex.req2();
            ex.req3();
            ex.req4();
            ex.req5();
            ex.req6();
            ex.req7();
            ex.req8();
            ex.req9();
            ex.req10();
            ex.req11();
            ex.req12();
            ex.req13();
            ex.ocd();
            Console.WriteLine("\n\n");
        }

    }
}

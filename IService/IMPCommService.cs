/////////////////////////////////////////////////////////////////////////////
//  IMPCommService.cs - service interface for MessagePassingComm           //
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
 * Added references to:
 * - System.ServiceModel
 * - System.Runtime.Serialization
 */
/*
 * This package provides:
 * ----------------------
 * - ClientEnvironment          : client-side path and address
 * - ServiceEnvironment         : server-side path and address
 * - TestharnessMockEnvironment : TestharnessMock-side path and address
 * - RepoMockEnvironment        : RepoMock-side path and address
 * - ChildBuilderEnvironment    : ChildBuilder-side path and address
 * - IMessagePassingComm        : interface used for message passing and file transfer
 * - CommMessage                : class representing serializable messages
 * 
 * Required Files:
 * ---------------
 * - IMPCommService.cs         : Service interface and Message definition
 * 
 * Maintenance History:
 * --------------------
 * ver 4.0 : 6 Dec 2017
 * - Project 4 release
 * ver 3.0 : 25 Oct 2017
 * - Project 3 release
 * ver 2.0 : 19 Oct 2017
 * - renamed namespace and ClientEnvironment
 * - added verbose property to ClientEnvironment
 * ver 1.0 : 15 Jun 2017
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Threading;
using System.IO;

namespace MessagePassingComm
{
    using Command = String;             // Command is key for message dispatching, e.g., Dictionary<Command, Func<bool>>
    using EndPoint = String;            // string is (ip address or machine name):(port number)
    using Files = String;
    using ErrorMessage = String;

    public struct ClientEnvironment
    {
        public static string fileStorage { get; set; } = "../../../ClientFileStore";
        public static long blockSize { get; set; } = 1024;
        public static string baseAddress { get; set; }
        public static bool verbose { get; set; }
    }

    public struct ServiceEnvironment
    {
        public static string fileStorage { get; set; } = "../../../ServiceFileStore";
        public static string baseAddress { get; set; }
    }

    public struct TestharnessMockEnvironment
    {
        public static string fileStorage { get; set; } = "../../../StorageTestHarnessMock/Libraries";
        public static string baseAddress { get; set; }
    }

    public struct RepoMockEnvironment
    {
        public static string codeStorage { get; set; } = "../../../StorageRepoMock/Code";
        public static string testRequestsStorage { get; set; } = "../../../StorageRepoMock/BuildRequests";
        public static long blockSize { get; set; } = 1024;
        public static string baseAddress { get; set; }
        public static bool verbose { get; set; }
    }

    public struct ChildBuilderEnvironment
    {
        public static string fileStorage { get; set; } = "../../../StorageChildBuilder";
        public static long blockSize { get; set; } = 1024;
        public static string baseAddress { get; set; }
        public static bool verbose { get; set; }
    }
    [ServiceContract(Namespace = "MessagePassingComm")]
    public interface IMessagePassingComm
    {
        /*----< support for message passing >--------------------------*/

        [OperationContract(IsOneWay = true)]
        void postMessage(CommMessage msg);

        // private to receiver so not an OperationContract
        CommMessage getMessage();

        /*----< support for sending file in blocks >-------------------*/

        [OperationContract]
        bool openFileForWrite(string name);

        [OperationContract]
        bool openFileForWriteAddress(string name, string address);

        [OperationContract]
        bool writeFileBlock(byte[] block);

        [OperationContract(IsOneWay = true)]
        void closeFile();
    }

    [DataContract]
    public class CommMessage
    {
        public enum MessageType
        {
            [EnumMember]
            connect,           // initial message sent on successfully connecting
            [EnumMember]
            request,           // request for action from receiver
            [EnumMember]
            reply,             // response to a request
            [EnumMember]
            closeSender,       // close down client
            [EnumMember]
            closeReceiver      // close down server for graceful termination
        }

        /*----< constructor requires message type >--------------------*/

        public CommMessage(MessageType mt)
        {
            type = mt;
        }
        /*----< data members - all serializable public properties >----*/

        [DataMember]
        public MessageType type { get; set; } = MessageType.connect;

        [DataMember]
        public string to { get; set; }

        [DataMember]
        public string from { get; set; }

        [DataMember]
        public string author { get; set; }

        [DataMember]
        public Command command { get; set; }

        [DataMember]
        public List<Files> testFiles { get; set; } = new List<Files>();

        [DataMember]
        public int threadId { get; set; } = Thread.CurrentThread.ManagedThreadId;

        [DataMember]
        public ErrorMessage errorMsg { get; set; } = "no error";

        [DataMember]
        public string testRequest { get; set; }

        [DataMember]
        public string body { get; set; }

        public void show()
        {
            Console.Write("\n  CommMessage:");
            Console.Write("\n    MessageType : {0}", type.ToString());
            Console.Write("\n    to          : {0}", to);
            Console.Write("\n    from        : {0}", from);
            Console.Write("\n    author      : {0}", author);
            Console.Write("\n    command     : {0}", command);
            Console.Write("\n    testRequest : {0}", testRequest);
            Console.Write("\n    testFiles   :");
            if (testFiles.Count > 0)
                Console.Write("\n      ");
            foreach (string file in testFiles)
            {
                Console.Write("{0} \n", Path.GetFileName(file));
                Console.Write("      ");
            }
            Console.Write("\n    ThreadId    : {0}", threadId);
            Console.Write("\n    errorMsg    : {0}", errorMsg);
            Console.Write("\n    body        : \n{0}\n", body);
        }
    }
}

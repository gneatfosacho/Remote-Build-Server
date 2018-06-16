/////////////////////////////////////////////////////////////////////////////
//  TestUtilities.cs - aids for testing                                    //
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
 * Package Operations:
 * -------------------
 * This package contains a single class TestUtilities with public functions:
 * - checkResult  : displays pass-fail message
 * - checkNull    : displays message and returns value == null
 * - handleInvoke : accepts runnable object and invokes it, displays exception message
 * - title        : writes underlined title text to console
 * - putLine      : writes optional message and newline to console
 * 
 * Required Files:
 * ---------------
 * - TestUtilities  - Helper class that is used mostly for testing
 * - IPluggable     - Repository interfaces and shared data
 *  
 * Maintenance History:
 * -------------------- 
 * ver 4.0 : 6 Dec 2017
 * - Project 4 release
 * ver 3.0 : 25 Oct 2017
 * - Project 3 release
 * ver 2.0 : 19 Oct 2017
 * - renamed namespace
 * ver 1.0 : 31 May 2017
 * - first release
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagePassingComm
{
  public class TestUtilities
  {
    /*----< saves duplicating presentation of results >------------*/

    public static void checkResult(bool result, string compName)
    {
      if (!ClientEnvironment.verbose)
        return;
      if (result)
        Console.Write("\n -- {0} test passed", compName);
      else
        Console.Write("\n -- {0} test failed", compName);
    }
    /*----< check for null reference >-----------------------------*/

    public static bool checkNull(object value, string msg="")
    {
      if (value == null && ClientEnvironment.verbose)
      {
        Console.Write("\n  null reference");
        if (msg.Length > 0)
          Console.Write("\n  {0}", msg);
        return true;
      }
      return (value == null);
    }
    /*----< saves duplicating exception handling >-----------------*/
    /*
     * - invokes runnable inside try/catch block
     * - returns true only if no exception is thrown and
     *   runnable returns true;
     * - intent is that runnable only returns true if 
     *   its operation succeeds.
     */
    public static bool handleInvoke(Func<bool> runnable, string msg="")
    {
      try
      {
        return runnable();
      }
      catch(Exception ex)
      {
        if (ClientEnvironment.verbose)
        {
          Console.Write("\n  {0}", ex.Message);
          if (msg.Length > 0)
            Console.Write("\n  {0}", msg);
        }
        return false;
      }
    }
    /*----< pretty print titles >----------------------------------*/

    public static void title(string msg, char ch = '-')
    {
      Console.Write("\n  {0}", msg);
      Console.Write("\n {0}", new string(ch, msg.Length + 2));
    }
    /*----< pretty print titles >----------------------------------*/

    public static void vbtitle(string msg, char ch = '-')
    {
      if(ClientEnvironment.verbose)
      {
        Console.Write("\n  {0}", msg);
        Console.Write("\n {0}", new string(ch, msg.Length + 2));
      }
    }
    /*----< write line of text only if verbose mode is set >-------*/

    public static void putLine(string msg="")
    {
      if (ClientEnvironment.verbose)
      {
        if (msg.Length > 0)
          Console.Write("\n  {0}", msg);
        else
          Console.Write("\n");
      }
    }

#if(TEST_TESTUTILITIES)

    /*----< construction test >------------------------------------*/

    static void Main(string[] args)
    {
      title("Testing TestUtilities", '=');
      ClientEnvironment.verbose = true;

      Func<bool> passTest = () => { Console.Write("\n  pass test"); return true; };
      checkResult(handleInvoke(passTest), "TestUtilities.handleInvoke");

      Func<bool> failTest = () => { Console.Write("\n  fail test"); return false; };
      checkResult(handleInvoke(failTest), "TestUtilities.handleInvoke");

      //Func<bool> throwTest = () => { Console.Write("\n  throw test"); throw new Exception(); return false; };
      //checkResult(handleInvoke(throwTest), "TestUtilities.handleInvoke");

      Console.Write("\n\n");
    }
  }
}
#endif

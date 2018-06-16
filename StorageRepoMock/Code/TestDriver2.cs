﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBuild
{
  interface ITest
  {
    bool test();
  }

  public class TestDriver : ITest
  {
    public bool test()
    {
      TestedOne one = new TestedOne();
      one.sayOne();
      TestedTwo two = new TestedTwo();
      two.sayTwo();
      return true;  // just pretending to test something
    }
    static void Main(string[] args)
    {
      Console.Write("\n  TestDriver running:");
      TestDriver td = new TestDriver();
      td.test();
      Console.Write("\n\n");
    }
  }
}

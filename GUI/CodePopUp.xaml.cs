/////////////////////////////////////////////////////////////////////////////
//  CodePopUp.xaml.cs - View any File Contents in a formatted window       //
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
 *   Build Process
 *   -------------
 *   - Required files - CodePopUp.xaml.cs
 *   - Compiler command: csc CodePopUp.xaml.cs
 *    
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 6 Dec 2017
 * - first release
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GUI
{
    /// <summary>
    /// Interaction logic for CodePopUp.xaml
    /// </summary>
    public partial class CodePopUp : Window
    {
        public CodePopUp()
        {
            InitializeComponent();
        }
    }
}

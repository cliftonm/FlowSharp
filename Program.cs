/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Drawing;
using System.Windows.Forms;

using FlowSharpServiceInterfaces;

namespace FlowSharp
{
    static partial class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Bootstrap();
            Icon icon = Properties.Resources.FlowSharp;
            Form form = ServiceManager.Get<IFlowSharpService>().CreateDockingForm(icon);
            Application.Run(form);
        }
    }
}


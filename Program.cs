/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using FlowSharpServiceInterfaces;

namespace FlowSharp
{
    static partial class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string modules = "modules.xml";

            if (args.Length == 1)
            {
                modules = args[0];
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Bootstrap(modules);
            Icon icon = Properties.Resources.FlowSharp;
            Form form = ServiceManager.Get<IFlowSharpService>().CreateDockingForm(icon);
            Application.Run(form);
        }

        private static void ShowAnyExceptions(List<Exception> exceptions)
        {
            foreach (var ex in exceptions)
            {
                MessageBox.Show(ex.Message, "Module Finalizer Exception", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}


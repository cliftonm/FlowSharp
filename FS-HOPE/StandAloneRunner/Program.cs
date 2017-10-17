using System;
using System.Drawing;
using System.Windows.Forms;

namespace StandAloneRunner
{
    /// <summary>
    /// For testing the FlowSharpHopeService.StandAloneRunner.
    /// Note that this project references a DLL built by FlowSharpHopeService.
    /// </summary>
    class Program
    {
        public static TextBox tbLog;
        public static Form form;

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            WebServer webServer = new WebServer();
            webServer.Start("localhost", new int[] { 5001 });
            form = new Form();
            form.Location = new Point(100, 100);
            form.Size = new Size(500, 200);
            form.Text = "Stand Alone Runner";
            tbLog = new TextBox();
            tbLog.Multiline = true;
            tbLog.Dock = DockStyle.Fill;
            form.Controls.Add(tbLog);
            Application.Run(form);
        }
    }
}

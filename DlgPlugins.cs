using System;
using System.IO;
using System.Windows.Forms;

using FlowSharpServiceInterfaces;

namespace FlowSharp
{
    public partial class DlgPlugins : Form
    {
        public DlgPlugins()
        {
            InitializeComponent();
            string plugins = File.ReadAllText(Constants.PLUGIN_FILE_LIST);
            tbPlugins.Text = plugins;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            File.WriteAllText(Constants.PLUGIN_FILE_LIST, tbPlugins.Text);
            Close();
        }
    }
}

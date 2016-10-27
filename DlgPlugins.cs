using System;
using System.IO;
using System.Windows.Forms;

namespace FlowSharp
{
    public partial class DlgPlugins : Form
    {
        public DlgPlugins()
        {
            InitializeComponent();
            string plugins = File.ReadAllText(FlowSharpUI.PLUGIN_FILE_LIST);
            tbPlugins.Text = plugins;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            File.WriteAllText(FlowSharpUI.PLUGIN_FILE_LIST, tbPlugins.Text);
            Close();
        }
    }
}

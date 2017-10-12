using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Clifton.Core.Semantics;

using FlowSharpHopeServiceInterfaces;

namespace FlowSharpCodeShapes
{
    public partial class PublishSemanticType : Form
    {
        protected IHigherOrderProgrammingService hope;

        public PublishSemanticType(ISemanticType st, IHigherOrderProgrammingService hope)
        {
            this.hope = hope;
            InitializeComponent();
            pgSemanticType.SelectedObject = st;
        }

        private void btnPublish_Click(object sender, EventArgs e)
        {
            hope.Publish((ISemanticType)pgSemanticType.SelectedObject);
        }
    }
}

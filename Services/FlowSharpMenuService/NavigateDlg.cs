/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using Clifton.Core.ServiceManagement;

using FlowSharpLib;
using FlowSharpServiceInterfaces;

namespace FlowSharpMenuService
{
    public partial class NavigateDlg : Form
    {
        protected IServiceManager serviceManager;
        protected BaseController canvasController;

        public NavigateDlg(IServiceManager serviceManager, List<NavigateToShape> navNames)
        {
            this.serviceManager = serviceManager;
            InitializeComponent();
            lbShapes.Items.AddRange(navNames.ToArray());
        }

        private void lbShapes_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case (char)Keys.Escape:
                    e.Handled = true;
                    Close();
                    break;

                case (char)Keys.Enter:
                    e.Handled = true;
                    Close();
                    GraphicElement shape = ((NavigateToShape)lbShapes.SelectedItem).Shape;
                    serviceManager.Get<IFlowSharpEditService>().FocusOnShape(shape);
                    break;
            }
        }

        private void lbShapes_MouseClick(object sender, MouseEventArgs e)
        {
            Close();
            GraphicElement shape = ((NavigateToShape)lbShapes.SelectedItem).Shape;
            serviceManager.Get<IFlowSharpEditService>().FocusOnShape(shape);
        }
    }
}

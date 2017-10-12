/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.Semantics;
using Clifton.Core.ServiceManagement;

using FlowSharpLib;
using FlowSharpHopeShapeInterfaces;
using FlowSharpServiceInterfaces;
using FlowSharpHopeServiceInterfaces;

namespace FlowSharpCodeShapes
{
    public class SemanticTypeShape : Box, ISemanticTypeShape
    {
        public SemanticTypeShape(Canvas canvas) : base(canvas)
        {
            Text = "SemType";
            TextFont.Dispose();
            TextFont = new Font(FontFamily.GenericSansSerif, 6);
            FillBrush.Color = Color.LightGreen;
        }

        public override GraphicElement CloneDefault(Canvas canvas, Point offset)
        {
            GraphicElement el = base.CloneDefault(canvas, offset);
            el.Text = "SemType";
            FillBrush.Color = Color.LightGreen;

            return el;
        }

        public override void RightClick()
        {
            IServiceManager serviceManager = canvas.ServiceManager;
            IHigherOrderProgrammingService hope = serviceManager.Get<IHigherOrderProgrammingService>();
            hope.LoadHopeAssembly();
            hope.InstantiateReceptors();
            ISemanticType st = hope.InstantiateSemanticType(Text);
            PublishSemanticType pst = new PublishSemanticType(st, hope);
            pst.ShowDialog();
            hope.UnloadHopeAssembly();
        }
    }
}

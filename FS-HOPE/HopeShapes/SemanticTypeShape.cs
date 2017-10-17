/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Drawing;

using Clifton.Core.Semantics;
using Clifton.Core.ServiceManagement;

using FlowSharpLib;
using FlowSharpHopeShapeInterfaces;
using FlowSharpHopeServiceInterfaces;

namespace HopeShapes
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
            hope.UnloadHopeAssembly();
            hope.LoadHopeAssembly();
            hope.InstantiateReceptors();
            object st = hope.InstantiateSemanticType(Text);
            PublishSemanticType pst = new PublishSemanticType(Text, st, hope);
            pst.Show();
        }
    }
}

/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;

using FlowSharpLib;
using FlowSharpCodeShapeInterfaces;

namespace FlowSharpCodeShapes
{
    public class WorkflowBox : Box, IWorkflowBox
    {
        public WorkflowBox(Canvas canvas) : base(canvas)
        {
            Text = "WF";
            TextFont.Dispose();
            TextFont = new Font(FontFamily.GenericSansSerif, 6);
            TextAlign = ContentAlignment.TopCenter;
            FillBrush.Color = Color.LightBlue;
        }
    }
}

/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.ComponentModel;

namespace FlowSharpLib
{
    public class CanvasProperties : IPropertyObject
    {
        [Category("Canvas")]
        public string Name { get; set; }
        [Category("Canvas")]
        public string Filename { get; set; }

        protected Canvas canvas;

        public CanvasProperties(Canvas canvas)
        {
            this.canvas = canvas;
            Name = canvas.Controller.CanvasName;
            Filename= canvas.Controller.Filename;
        }

        public void Update(string label)
        {
            (label == nameof(Name)).If(() => canvas.Controller.CanvasName = Name);
            (label == nameof(Filename)).If(() => canvas.Controller.Filename = Filename);
        }
    }
}

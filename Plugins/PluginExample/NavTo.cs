/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using Clifton.Core.ExtensionMethods;

using FlowSharpLib;

namespace PluginExample
{
    public class NavTo : Box
    {
        public string NavigateTo { get; set; }

        protected Rectangle target;

        public NavTo(Canvas canvas) : base(canvas)
        {
            TextAlign = ContentAlignment.TopCenter;
            canvas.MouseUp += OnMouseUp;
        }

        /// <summary>
        /// Tests whether mouse up occurred in target, and if so, focuses on the shape with the Name value of the NavigateTo value.
        /// </summary>
        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (target.Contains(e.Location) && !string.IsNullOrEmpty(NavigateTo))
            {
                var navto = canvas.Controller.Elements.Where(el => el.Name == NavigateTo);

                if (navto.Count() == 1)
                {
                    canvas.Controller.FocusOn(navto.First());
                }
            }
        }

        public override ElementProperties CreateProperties()
        {
            return new NavToProperties(this);
        }

        public override void Serialize(ElementPropertyBag epb, IEnumerable<GraphicElement> elementsBeingSerialized)
        {
            Json["NavitageTo"] = NavigateTo;
            base.Serialize(epb, elementsBeingSerialized);
        }

        public override void Deserialize(ElementPropertyBag epb)
        {
            base.Deserialize(epb);

            string navigateTo;
            Json.TryGetValue("NavigateTo", out navigateTo);
            NavigateTo = navigateTo;
        }

        public override void Draw(Graphics gr)
        {
            base.Draw(gr);
            int min = DisplayRectangle.Width.Min(DisplayRectangle.Height);
            min = min / 2;
            Point topleft = DisplayRectangle.Center();
            topleft.X -= min / 2;
            topleft.Y -= 3;
            target = new Rectangle(topleft, new Size(min, min));
            Point innerTopLeft = target.Center();
            innerTopLeft.Offset(-min / 4, -min / 4);
            Rectangle targetInner = new Rectangle(innerTopLeft, new Size(min/2, min/2));
            Brush fillBrush = new SolidBrush(Color.White);
            Brush middleTarget = new SolidBrush(Color.Red);
            Pen borderPen = new Pen(Color.Red);
            gr.FillEllipse(fillBrush, target);
            gr.FillEllipse(middleTarget, targetInner);
            gr.DrawEllipse(borderPen, target);
            fillBrush.Dispose();
            borderPen.Dispose();
            middleTarget.Dispose();
        }
    }
}

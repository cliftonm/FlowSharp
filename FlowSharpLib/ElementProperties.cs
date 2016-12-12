/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.ComponentModel;
using System.Drawing;

using Clifton.Core.ExtensionMethods;

namespace FlowSharpLib
{
	public abstract class ElementProperties : IPropertyObject
	{
		protected GraphicElement element;

        [Category("Element")]
        public string Name { get; set; }
        [Category("Element")]
		public string ShapeName { get { return element?.GetType().Name; } }
		[Category("Element")]
		public Rectangle Rectangle { get; set; }

		[Category("Border")]
		public Color BorderColor { get; set; }
		public int BorderWidth { get; set; }

		[Category("Fill")]
		public Color FillColor { get; set; }

		public ElementProperties(GraphicElement el)
		{
			this.element = el;
			Rectangle = el.DisplayRectangle;
			BorderColor = el.BorderPen.Color;
			BorderWidth = (int)el.BorderPen.Width;
			FillColor = el.FillBrush.Color;
            Name = el.Name;
		}

		public virtual void UpdateFrom(GraphicElement el)
		{
			// The only property that can change.
			Rectangle = el.DisplayRectangle;
		}

		public virtual void Update(GraphicElement el, string label)
		{
            // X1
            //(label == nameof(Rectangle)).If(() => this.ChangePropertyWithUndoRedo<Rectangle>(el, nameof(el.DisplayRectangle), nameof(Rectangle)));
            //(label == nameof(BorderColor)).If(() => this.ChangePropertyWithUndoRedo<Color>(el, nameof(el.BorderPenColor), nameof(BorderColor)));
            //(label == nameof(BorderWidth)).If(() => this.ChangePropertyWithUndoRedo<int>(el, nameof(el.BorderPenWidth), nameof(BorderWidth)));
            //(label == nameof(FillColor)).If(() => this.ChangePropertyWithUndoRedo<Color>(el, nameof(el.FillColor), nameof(FillColor)));
            (label == nameof(Rectangle)).If(() => el.DisplayRectangle = Rectangle);
            (label == nameof(BorderColor)).If(() => el.BorderPenColor = BorderColor);
            (label == nameof(BorderWidth)).If(() => el.BorderPenWidth = BorderWidth);
            (label == nameof(FillColor)).If(() => el.FillColor = FillColor);
            (label == nameof(Name)).If(() => el.Name = Name);
        }

        public virtual void Update(string label) { }
    }
}

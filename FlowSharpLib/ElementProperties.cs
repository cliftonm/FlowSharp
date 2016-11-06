/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.ComponentModel;
using System.Drawing;

namespace FlowSharpLib
{
	public abstract class ElementProperties
	{
		protected GraphicElement element;

		[Category("Element")]
		public string Name { get { return element?.GetType().Name; } }
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
		}

		public virtual void UpdateFrom(GraphicElement el)
		{
			// The only property that can change.
			Rectangle = el.DisplayRectangle;
		}

		public virtual void Update(GraphicElement el, string label)
		{
            (label == "DisplayRectangle").If(() => el.DisplayRectangle = Rectangle);
            (label == "BorderColor").If(() => el.BorderPen.Color = BorderColor);
            (label == "BorderWidth").If(() => el.BorderPen.Width = BorderWidth);
            (label == "FillColor").If(() => el.FillBrush.Color = FillColor);
			// el.PropertiesChanged.Fire(this, new PropertiesChangedEventArgs(el));
		}
	}
}

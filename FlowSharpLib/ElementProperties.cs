using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace FlowSharpLib
{
	public class ElementProperties
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
			// The only thing that can change.
			Rectangle = el.DisplayRectangle;
		}

		public virtual void Update(GraphicElement el)
		{
			el.DisplayRectangle = Rectangle;
			el.BorderPen.Color = BorderColor;
			el.BorderPen.Width = BorderWidth;
			el.FillBrush.Color = FillColor;
		}
	}
}

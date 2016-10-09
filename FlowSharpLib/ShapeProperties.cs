/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.ComponentModel;
using System.Drawing;

namespace FlowSharpLib
{
	public class ShapeProperties : ElementProperties
	{
		[Category("Text")]
		public string Text { get; set; }
		[Category("Text")]
		public Font Font { get; set; }
		[Category("Text")]
		public Color TextColor { get; set; }

		public ShapeProperties(GraphicElement el) : base(el)
		{
			Text = el.Text;
			Font = el.TextFont;
			TextColor = el.TextColor;
		}

		public override void Update(GraphicElement el)
		{
			base.Update(el);
			el.Text = Text;
			el.TextFont = Font;
			el.TextColor = TextColor;
		}
	}
}

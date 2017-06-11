/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;

using Clifton.Core.ExtensionMethods;

using FlowSharpLib;
using FlowSharpCodeShapeInterfaces;

namespace FlowSharpCodeShapes
{
    public class CSharpClassBox : Box, ICSharpClass
    {
        public string Filename { get; set; }
		public string NamespaceName { get; set; } = "Namespace";
		public string ClassName { get; set; } = "Class";
		public string MethodName { get; set; } = "Method";

		public CSharpClassBox(Canvas canvas) : base(canvas)
        {
            Text = ".cs";
            TextFont.Dispose();
            TextFont = new Font(FontFamily.GenericSansSerif, 6);
            TextAlign = ContentAlignment.TopLeft;
        }

        public void UpdateCodeBehind()
        {
            string data = "";

            if (File.Exists(Filename))
            {
                data = File.ReadAllText(Filename);
            }

            Json["csharp"] = data;
        }

        public override GraphicElement CloneDefault(Canvas canvas)
        {
            GraphicElement el = base.CloneDefault(canvas);
            el.TextFont.Dispose();
            el.TextFont = new Font(FontFamily.GenericSansSerif, 10);
            el.TextAlign = ContentAlignment.TopLeft;

            return el;
        }

        public override GraphicElement CloneDefault(Canvas canvas, Point offset)
        {
            GraphicElement el = base.CloneDefault(canvas, offset);
            el.TextFont.Dispose();
            el.TextFont = new Font(FontFamily.GenericSansSerif, 10);
            el.TextAlign = ContentAlignment.TopLeft;

            return el;
        }

        public override ElementProperties CreateProperties()
        {
            return new CSharpClassBoxProperties(this);
        }

        public override void Serialize(ElementPropertyBag epb, IEnumerable<GraphicElement> elementsBeingSerialized)
        {
            // TODO: Use JSON dictionary instead.
            epb.ExtraData = Filename;

            // Also update the backing file.
            if (Json.ContainsKey("csharp"))
            {
                File.WriteAllText(Filename, Json["csharp"]);
            }

			Json["NamespaceName"] = NamespaceName;
			Json["ClassName"] = ClassName;
			Json["MethodName"] = MethodName;

            base.Serialize(epb, elementsBeingSerialized);
        }

        public override void Deserialize(ElementPropertyBag epb)
        {
            base.Deserialize(epb);
            // TODO: Use JSON dictionary instead.
            Filename = epb.ExtraData;

			string strClassName;
			string strMethodName;
			string strNamespaceName;

			if (Json.TryGetValue("NamespaceName", out strNamespaceName))
			{
				NamespaceName = strNamespaceName;
			}

			if (Json.TryGetValue("ClassName", out strClassName))
			{
				ClassName = strClassName;
			}

			if (Json.TryGetValue("MethodName", out strMethodName))
			{
				MethodName = strMethodName;
			}
		}
	}

    public class CSharpClassBoxProperties : ShapeProperties
    {
        [Category("Class")]
        public string Filename { get; set; }
		[Category("Class")]
		public string NamespaceName { get; set; }
		[Category("Class")]
		public string ClassName { get; set; }
		[Category("Class")]
		public string MethodName { get; set; }

		public CSharpClassBoxProperties(CSharpClassBox el) : base(el)
        {
            Filename = el.Filename;
			NamespaceName = el.NamespaceName;
			ClassName = el.ClassName;
			MethodName = el.MethodName;
        }

        public override void Update(GraphicElement el, string label)
        {
            CSharpClassBox box = (CSharpClassBox)el;

            (label == nameof(Filename)).If(() =>
            {
                box.Filename = Filename;
                box.UpdateCodeBehind();
                box.Text = string.IsNullOrEmpty(Filename) ? "?.cs" : (Path.GetFileName(Filename));
            });

			(label == nameof(NamespaceName)).If(() => box.NamespaceName = NamespaceName);
			(label == nameof(ClassName)).If(() => box.ClassName = ClassName);
			(label == nameof(MethodName)).If(() => box.MethodName = MethodName);

            base.Update(el, label);
        }
    }
}

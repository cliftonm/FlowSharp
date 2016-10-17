/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace FlowSharpLib
{
	public class ConnectionPropertyBag
	{
		public Guid ToElementId { get; set; }
		public ConnectionPoint ToConnectionPoint { get; set; }
		public ConnectionPoint ElementConnectionPoint { get; set; }
	}

	public class ElementPropertyBag
	{
		// For deserialization fixups.
		[XmlIgnore]
		public GraphicElement Element { get; set; }

		[XmlAttribute]
		public string ElementName { get; set; }
		[XmlAttribute]
		public Guid Id { get; set; }
		[XmlAttribute]
		public string Text { get; set; }

		public Rectangle DisplayRectangle { get; set; }
		public Point StartPoint { get; set; }
		public Point EndPoint { get; set; }

		public int BorderPenWidth { get; set; }

		[XmlAttribute]
		public bool HasCornerAnchors { get; set; }
		[XmlAttribute]
		public bool HasCenterAnchors { get; set; }
		[XmlAttribute]
		public bool HasLeftRightAnchors { get; set; }
		[XmlAttribute]
		public bool HasTopBottomAnchors { get; set; }

		[XmlAttribute]
		public bool HasCornerConnections { get; set; }
		[XmlAttribute]
		public bool HasCenterConnections { get; set; }
		[XmlAttribute]
		public bool HasLeftRightConnections { get; set; }
		[XmlAttribute]
		public bool HasTopBottomConnections { get; set; }

		public AvailableLineCap StartCap { get; set; }
		public AvailableLineCap EndCap { get; set; }

		public Guid StartConnectedShapeId { get; set; }
		public Guid EndConnectedShapeId { get; set; }

		public string TextFontFamily { get; set; }
		public float TextFontSize { get; set; }
		public bool TextFontUnderline { get; set; }
		public bool TextFontStrikeout { get; set; }
		public bool TextFontItalic { get; set; }

		public List<ConnectionPropertyBag> Connections { get; set; }

		[XmlIgnore]
		public Color TextColor { get; set; }
		[XmlElement("TextColor")]
		public int XTextColor
		{
			get { return TextColor.ToArgb(); }
			set { TextColor = Color.FromArgb(value); }
		}

		[XmlIgnore]
		public Color BorderPenColor { get; set; }
		[XmlElement("BorderPenColor")]
		public int XBorderPenColor
		{
			get { return BorderPenColor.ToArgb(); }
			set { BorderPenColor = Color.FromArgb(value); }
		}

		[XmlIgnore]
		public Color FillBrushColor { get; set; }
		[XmlElement("FillBrushColor")]
		public int XFillBrushColor
		{
			get { return FillBrushColor.ToArgb(); }
			set { FillBrushColor = Color.FromArgb(value); }
		}

		public ElementPropertyBag()
		{
			Connections = new List<ConnectionPropertyBag>();
		}
	}

	public static class Persist
	{
		public static string Serialize(List<GraphicElement> elements)
		{
			List<ElementPropertyBag> sps = new List<ElementPropertyBag>();
			elements.ForEach(el =>
			{
				ElementPropertyBag epb = new ElementPropertyBag();
				el.Serialize(epb, elements);
				sps.Add(epb);
			});

			XmlSerializer xs = new XmlSerializer(sps.GetType());
			StringBuilder sb = new StringBuilder();
			TextWriter tw = new StringWriter(sb);
			xs.Serialize(tw, sps);

			return sb.ToString();
		}

		public static string Serialize(GraphicElement el)
		{
			ElementPropertyBag epb = new ElementPropertyBag();
			el.Serialize(epb, new List<GraphicElement>() { el });
			XmlSerializer xs = new XmlSerializer(typeof(ElementPropertyBag));
			StringBuilder sb = new StringBuilder();
			TextWriter tw = new StringWriter(sb);
			xs.Serialize(tw, epb);

			return sb.ToString();
		}

        /// <summary>
        /// Remap is false when loading from a file, true when copying and pasting.
        /// </summary>
		public static List<GraphicElement> Deserialize(Canvas canvas, string data)
		{
            Dictionary<Guid, Guid> oldNewIdMap = new Dictionary<Guid, Guid>();
            Tuple<List<GraphicElement>, List<ElementPropertyBag>> collections = InternalDeserialize(canvas, data, oldNewIdMap);
            FixupConnections(collections, oldNewIdMap);
            FinalFixup(collections, oldNewIdMap);

            return collections.Item1;
		}

        public static GraphicElement DeserializeElement(Canvas canvas, string data)
        {
            XmlSerializer xs = new XmlSerializer(typeof(ElementPropertyBag));
            TextReader tr = new StringReader(data);
            ElementPropertyBag epb = (ElementPropertyBag)xs.Deserialize(tr);
            Type t = Type.GetType(epb.ElementName);
            GraphicElement el = (GraphicElement)Activator.CreateInstance(t, new object[] { canvas });
            el.Deserialize(epb);        // A specific deserialization does not preserve connections.
            el.Id = Guid.NewGuid();     // We get a new GUID when deserializing a specific element.

            return el;
        }

        private static Tuple<List<GraphicElement>, List<ElementPropertyBag>> InternalDeserialize(Canvas canvas, string data, Dictionary<Guid, Guid> oldNewIdMap)
        {
            List<GraphicElement> elements = new List<GraphicElement>();
            XmlSerializer xs = new XmlSerializer(typeof(List<ElementPropertyBag>));
            TextReader tr = new StringReader(data);
            List<ElementPropertyBag> sps = (List<ElementPropertyBag>)xs.Deserialize(tr);

            foreach (ElementPropertyBag epb in sps)
            {
                Type t = Type.GetType(epb.ElementName);
                GraphicElement el = (GraphicElement)Activator.CreateInstance(t, new object[] { canvas });
                el.Deserialize(epb);
                Guid elGuid = el.Id;
                elGuid = Guid.NewGuid();
                oldNewIdMap[el.Id] = elGuid;
                el.Id = elGuid;
                elements.Add(el);
                epb.Element = el;
            }

            return new Tuple<List<GraphicElement>, List<ElementPropertyBag>>(elements, sps);
        }

        private static void FixupConnections(Tuple<List<GraphicElement>, List<ElementPropertyBag>> collections, Dictionary<Guid, Guid> oldNewGuidMap)
        {
            foreach (ElementPropertyBag epb in collections.Item2)
            {
                epb.Connections.Where(c => c.ToElementId != Guid.Empty).ForEach(c =>
                {
                    Connection conn = new Connection();
                    conn.Deserialize(collections.Item1, c, oldNewGuidMap);
                    epb.Element.Connections.Add(conn);
                });
            }
        }

        private static void FinalFixup(Tuple<List<GraphicElement>, List<ElementPropertyBag>> collections, Dictionary<Guid, Guid> oldNewGuidMap)
        {
            collections.Item2.ForEach(epb => epb.Element.FinalFixup(collections.Item1, epb, oldNewGuidMap));
        }
	}
}

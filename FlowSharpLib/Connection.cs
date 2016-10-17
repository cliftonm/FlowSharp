/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace FlowSharpLib
{
	// !!! If this class ends up being subclassed for any reason, the serializer must be updated to account for subclasses !!!
	/// <summary>
	/// Used for shapes connecting to lines.
	/// </summary>
	public class Connection
	{
		public GraphicElement ToElement { get; set; }
		public ConnectionPoint ToConnectionPoint { get; set; }
		public ConnectionPoint ElementConnectionPoint { get; set; }

		public void Serialize(ElementPropertyBag epb, List<GraphicElement> elementsBeingSerialized)
		{
            // If partial serialization (like copying a subset of shapes, don't include unselected connectors.)
            if (elementsBeingSerialized.Contains(ToElement))
            {
                ConnectionPropertyBag cpb = new ConnectionPropertyBag();
                cpb.ToElementId = ToElement.Id;
                cpb.ToConnectionPoint = ToConnectionPoint;
                cpb.ElementConnectionPoint = ElementConnectionPoint;
                epb.Connections.Add(cpb);
            }
		}

		public void Deserialize(List<GraphicElement> elements, ConnectionPropertyBag cpb, Dictionary<Guid, Guid> oldNewGuidMap)
		{
            ToElement = elements.Single(e => e.Id == oldNewGuidMap[cpb.ToElementId]);
            ToConnectionPoint = cpb.ToConnectionPoint;
            ElementConnectionPoint = cpb.ElementConnectionPoint;
		}
	}
}


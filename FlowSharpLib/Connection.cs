/* The MIT License (MIT)
* 
* Copyright (c) 2016 Marc Clifton
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

using System.Collections.Generic;
using System.Linq;

namespace FlowSharpLib
{
	// !!! If this class ends up being subclassed for any reason, the serializer must be updated to account for subclasses !!!
	/// <summary>
	/// Used for non-line shapes connecting to lines.
	/// </summary>
	public class Connection
	{
		public GraphicElement ToElement { get; set; }
		public ConnectionPoint ToConnectionPoint { get; set; }
		public ConnectionPoint ElementConnectionPoint { get; set; }

		public void Serialize(ElementPropertyBag epb)
		{
			ConnectionPropertyBag cpb = new ConnectionPropertyBag();
			cpb.ToElementId = ToElement.Id;
			cpb.ToConnectionPoint = ToConnectionPoint;
			cpb.ElementConnectionPoint = ElementConnectionPoint;
			epb.Connections.Add(cpb);
		}

		public void Deserialize(List<GraphicElement> elements, ConnectionPropertyBag cpb)
		{
			ToElement = elements.Single(e => e.Id == cpb.ToElementId);
			ToConnectionPoint = cpb.ToConnectionPoint;
			ElementConnectionPoint = cpb.ElementConnectionPoint;
		}
	}
}


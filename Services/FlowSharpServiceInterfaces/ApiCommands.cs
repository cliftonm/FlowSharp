/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System.Collections.Generic;

using Newtonsoft.Json;

using Clifton.Core.Semantics;

namespace FlowSharpServiceInterfaces
{
    public class CmdUpdateProperty : ISemanticType
    {
        public string Name { get; set; }
        public string PropertyName { get; set; }
        public string Value { get; set; }
    }

    public class CmdShowShape : ISemanticType
    {
        // Options for indicating what shape to show:
        // By ID, Text, or Name
        public string Id { get; set; }
        public string Text { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// Return the filenames for shapes that implement IFileBox.
    /// Used, for example, to FTP files to a server.
    /// </summary>
    public class CmdGetShapeFiles : ISemanticType, IHasResponse
    {
        public List<string> Filenames { get; protected set; }

        public CmdGetShapeFiles()
        {
            Filenames = new List<string>();
        }

        public string SerializeResponse()
        {
            return JsonConvert.SerializeObject(Filenames);
        }
    }

    public class CmdOutputMessage : ISemanticType
    {
        public string Text { get; set; }
    }

    /// <summary>
    /// Place a shape on the drawing.
    /// </summary>
    public class CmdDropShape : ISemanticType
    {
        public string ShapeName { get; set; }       // shape name to drop
        public string Name { get; set; }            // name of shape to assign to Name property
        public int X { get; set; }
        public int Y { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string Text { get; set; }
        public string FillColor { get; set; }
        public string BorderColor { get; set; }
        public string TextColor { get; set; }
    }

    public class CmdDropConnector : ISemanticType
    {
        public string ConnectorName { get; set; }
        public string Name { get; set; }
        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int X2 { get; set; }
        public int Y2 { get; set; }
    }
}

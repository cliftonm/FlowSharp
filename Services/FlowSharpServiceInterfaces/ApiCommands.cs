/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

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
}

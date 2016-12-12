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
        public string ShapeName { get; set; }
        public string PropertyName { get; set; }
        public string Value { get; set; }
    }
}

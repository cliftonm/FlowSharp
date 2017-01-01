/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using Clifton.Core.Semantics;

namespace FlowSharpServiceInterfaces
{
    public class HttpSend : ISemanticType
    {
        public string Url { get; set; }
        public string Data { get; set; }
    }

    public class WebSocketSend : ISemanticType
    {
        public string Data { get; set; }
    }
}

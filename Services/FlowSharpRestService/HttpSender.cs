/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using Clifton.Core.Semantics;

using FlowSharpServiceInterfaces;

namespace FlowSharpRestService
{
    public class HttpSender : IReceptor
    {
        // Ex: localhost:8001:flowsharp?cmd=CmdUpdateProperty&Name=btnTest&PropertyName=Text&Value=Foobar
        public void Process(ISemanticProcessor proc, IMembrane membrane, HttpSend cmd)
        {
            Http.Get(cmd.Url + "?" + cmd.Data);
        }
    }
}

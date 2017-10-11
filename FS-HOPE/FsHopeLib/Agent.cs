/* 
* Copyright (c) Marc Clifton
* The Code Project Open License (CPOL) 1.02
* http://www.codeproject.com/info/cpol10.aspx
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.ModuleManagement;
using Clifton.Core.Semantics;
using Clifton.Core.ServiceManagement;

namespace FsHopeLib
{
    public abstract class Agent : IReceptor
	{
		// ServiceManager.Get<ISemanticProcessor>().Register<FlowSharpMembrane, FlowSharpCanvasControllerReceptor>();
    }
}

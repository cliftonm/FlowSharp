using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Clifton.Core.Semantics;

// When the runner is moved to its own AppDomain DLL, remove the reference in FlowSharpHopeService to the Clifton.SemanticProcessorService.

using Clifton.Core.Services.SemanticProcessorService;

namespace FlowSharpHopeService
{
    public class HopeMembrane : Membrane { }

    public class Runner
    {
        protected SemanticProcessor sp;
        protected Assembly assy;                // Eventually will be an AppDomain
        protected HopeMembrane membrane;

        public Runner()
        {
            sp = new SemanticProcessor();
            membrane = new HopeMembrane();
            sp.RegisterMembrane<HopeMembrane>();
        }

        public void Load(string dll)
        {
            assy = Assembly.LoadFrom(dll);
        }

        public void InstantiateReceptor(Type t)
        {
            // Get the agent type from our assembly, not the "loaded for reflection only" assembly type.
            var agent = assy.GetTypes().SingleOrDefault(at => at.IsClass && t.IsPublic && at.Name == t.Name);
            IReceptor receptor = (IReceptor)Activator.CreateInstance(agent);
            sp.Register<HopeMembrane>(receptor);
        }

        public dynamic InstantiateSemanticType(string typeName)
        {
            Type st = assy.GetTypes().SingleOrDefault(t => t.IsClass && t.IsPublic && t.GetInterfaces().Any(i => i.Name == nameof(ISemanticType)));
            object inst = Activator.CreateInstance(st);

            return inst;
        }

        public void Publish(ISemanticType st)
        {
            sp.ProcessInstance<HopeMembrane>(st);
        }
    }
}

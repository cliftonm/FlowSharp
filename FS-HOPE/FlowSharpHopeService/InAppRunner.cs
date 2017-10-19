using System;
using System.Linq;
using System.Reflection;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.Semantics;
using Clifton.Core.Services.SemanticProcessorService;

using FlowSharpHopeCommon;

namespace FlowSharpHopeService
{
    /// <summary>
    /// For in-memory, no app-domain, testing.
    /// Incomplete implementation:
    /// Processing
    /// EnableDisableReceptor
    /// Unload can't do anything because this is an in-memory load, the assembly cannot be unloaded.
    /// </summary>
    public class InAppRunner : IRunner
    {
        public event EventHandler<HopeRunnerAppDomainInterface.ProcessEventArgs> Processing;

        protected SemanticProcessor sp;
        protected Assembly assy;                // Eventually will be an AppDomain
        protected HopeMembrane membrane;

        public InAppRunner()
        {
            sp = new SemanticProcessor();
            membrane = new HopeMembrane();
            sp.RegisterMembrane<HopeMembrane>();
            sp.Processing += ProcessingSemanticType;
        }

        private void ProcessingSemanticType(object sender, Clifton.Core.Semantics.ProcessEventArgs args)
        {
			var stMsg = new HopeRunnerAppDomainInterface.ProcessEventArgs()
			{
				FromMembraneTypeName = args.FromMembrane?.GetType()?.FullName,
				FromReceptorTypeName = args.FromReceptor?.GetType()?.FullName,
				ToMembraneTypeName = args.ToMembrane.GetType().FullName,
				ToReceptorTypeName = args.ToReceptor.GetType().FullName,
				SemanticTypeTypeName = args.SemanticType.GetType().FullName,
			};

			Processing.Fire(this, stMsg);
		}

        public void Load(string dll)
        {
            assy = Assembly.LoadFrom(dll);
        }

        public void Unload()
        {
        }

        public void EnableDisableReceptor(string typeName, bool state)
        {
        }

        public void InstantiateReceptor(Type t)
        {
            // Get the agent type from our assembly, not the "loaded for reflection only" assembly type.
            var agent = assy.GetTypes().SingleOrDefault(at => at.IsClass && t.IsPublic && at.Name == t.Name);
            IReceptor receptor = (IReceptor)Activator.CreateInstance(agent);
            sp.Register<HopeMembrane>(receptor);
        }

        public void InstantiateReceptor(string name)
        {
            // TODO: Implement.
        }

        public void InstantiateReceptors()
        {
            // TODO: Implement.
        }

        public object InstantiateSemanticType(string typeName)
        {
            Type st = assy.GetTypes().Single(t => t.Name == typeName);
            object inst = Activator.CreateInstance(st);

            return inst;
        }

        public PropertyContainer DescribeSemanticType(string typeName)
        {
            return null;
        }

        public void Publish(string _, object st)
        {
            sp.ProcessInstance<HopeMembrane>((ISemanticType)st, true);
        }

        public void Publish(string typeName, string json)
        {
        }
    }
}

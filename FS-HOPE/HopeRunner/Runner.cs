using System;
using System.Linq;
using System.Reflection;

using Clifton.Core.Semantics;

using Clifton.Core.Services.SemanticProcessorService;
using HopeRunnerAppDomainInterface;

namespace HopeRunner
{
    public class HopeMembrane : Membrane { }

    [Serializable]
    public class Runner : MarshalByRefObject, IHopeRunner
    {
        protected SemanticProcessor sp;
        protected HopeMembrane membrane;

        public Runner()
        {
            sp = new SemanticProcessor();
            membrane = new HopeMembrane();
            sp.RegisterMembrane<HopeMembrane>();
        }

        public void InstantiateReceptor(string typeName)
        {
            var agent = Assembly.GetExecutingAssembly().GetTypes().SingleOrDefault(at => at.IsClass && at.IsPublic && at.Name == typeName);
            IReceptor receptor = (IReceptor)Activator.CreateInstance(agent);
            sp.Register<HopeMembrane>(receptor);
        }

        public ISemanticType InstantiateSemanticType(string typeName)
        {
            Type st = Assembly.GetExecutingAssembly().GetTypes().SingleOrDefault(t => t.IsClass && t.IsPublic && t.GetInterfaces().Any(i => i.Name == nameof(ISemanticType)));
            ISemanticType inst = (ISemanticType)Activator.CreateInstance(st);

            return inst;
        }

        public void Publish(ISemanticType st)
        {
            sp.ProcessInstance<HopeMembrane>(st);
        }
    }
}

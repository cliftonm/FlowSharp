using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

using Newtonsoft.Json;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.Semantics;
using Clifton.Core.Services.SemanticProcessorService;

using FlowSharpHopeCommon;

namespace App
{
    public class HopeMembrane : IMembrane { };
}

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
        protected Assembly assy;
        protected IMembrane membrane;

        public InAppRunner()
        {
            sp = new SemanticProcessor();

            // membrane = new HopeMembrane();
            // membrane = sp.RegisterMembrane<HopeMembrane>();
            // membrane = new App.HopeMembrane();
            membrane = sp.RegisterMembrane<App.HopeMembrane>();

            sp.Processing += ProcessingSemanticType;
        }

        private void ProcessingSemanticType(object sender, ProcessEventArgs args)
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
            Type t = assy.GetTypes().Single(at => at.Name == "HopeMembrane");
            membrane = (IMembrane)Activator.CreateInstance(t);
            sp.RegisterMembrane(membrane);
        }

        public void Unload()
        {
        }

        public void EnableDisableReceptor(string typeName, bool state)
        {
        }

        public void InstantiateReceptor(string name)
        {
            Type t = assy.GetTypes().Single(at => at.Name == name);
            IReceptor inst = (IReceptor)Activator.CreateInstance(t);
            // sp.Register(membrane, inst);
            sp.Register<App.HopeMembrane>(inst);
        }

        public object InstantiateSemanticType(string typeName)
        {
            Type st = assy.GetTypes().Single(t => t.Name == typeName);
            object inst = Activator.CreateInstance(st);

            return inst;
        }

        public PropertyContainer DescribeSemanticType(string typeName)
        {
            Type t = assy.GetTypes().Single(at => at.Name == typeName);
            PropertyInfo[] pis = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyContainer pc = new PropertyContainer();
            BuildTypes(pc, pis);

            return pc;
        }

        public void Publish(string _, object st)
        {
            sp.ProcessInstance(membrane, (ISemanticType)st, true);
        }

        public void Publish(string typeName, string json)
        {
            Type t = assy.GetTypes().Single(at => at.Name == typeName);
            ISemanticType st = (ISemanticType)JsonConvert.DeserializeObject(json, t);
            // sp.ProcessInstance(membrane, st, true);
            sp.ProcessInstance<App.HopeMembrane>(st, true);
        }

        protected void BuildTypes(PropertyContainer pc, PropertyInfo[] pis)
        {
            foreach (PropertyInfo pi in pis)
            {
                PropertyData pd = new PropertyData() { Name = pi.Name, TypeName = pi.PropertyType.FullName };
                var cat = pi.GetCustomAttribute<CategoryAttribute>();
                var desc = pi.GetCustomAttribute<DescriptionAttribute>();
                pd.Category = cat == null ? null : cat.Category;
                pd.Description = desc == null ? null : desc.Description;
                pc.Types.Add(pd);

                if ((!pi.PropertyType.IsValueType) && (pd.TypeName != "System.String"))
                {
                    PropertyInfo[] pisChild = pi.PropertyType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    pd.ChildType = new PropertyContainer();
                    BuildTypes(pd.ChildType, pisChild);
                }
            }
        }
    }
}

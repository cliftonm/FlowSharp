using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Clifton.Core.ExtensionMethods;
using Clifton.Core.Semantics;

// When the runner is moved to its own AppDomain DLL, remove the reference in FlowSharpHopeService to the Clifton.SemanticProcessorService.

using HopeRunnerAppDomainInterface;

namespace FlowSharpHopeService
{
    public class HopeMembrane : Membrane { }

    public class Runner
    {
        protected AppDomain appDomain;
        protected IHopeRunner runner;

        public Runner()
        {
        }

        public void Load(string fullDllName)
        {
            string dll = fullDllName.LeftOf(".");
            appDomain = CreateAppDomain(dll);
            runner = InstantiateRunner(dll, appDomain);
        }

        public void Unload()
        {
            AppDomain.Unload(appDomain);
        }

        public void InstantiateReceptor(Type t)
        {
            runner.InstantiateReceptor(t.Name);
        }

        public void EnableDisableReceptor(string typeName, bool state)
        {
            // Runner may not be up when we get this.
            runner?.EnableDisableReceptor(typeName, state);
        }

        public dynamic InstantiateSemanticType(string typeName)
        {
            ISemanticType st = runner.InstantiateSemanticType(typeName);

            return st;
        }

        public void Publish(ISemanticType st)
        {
            runner.Publish(st);
        }

        private AppDomain CreateAppDomain(string dllName)
        {
            AppDomainSetup setup = new AppDomainSetup()
            {
                ApplicationName = dllName,
                ConfigurationFile = dllName + "dll.config",
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory
            };

            AppDomain appDomain = AppDomain.CreateDomain(
              setup.ApplicationName,
              AppDomain.CurrentDomain.Evidence,
              setup);

            return appDomain;
        }

        private IHopeRunner InstantiateRunner(string dllName, AppDomain domain)
        {
            IHopeRunner runner = domain.CreateInstanceAndUnwrap(dllName, "HopeRunner.Runner") as IHopeRunner;

            return runner;
        }
    }
}

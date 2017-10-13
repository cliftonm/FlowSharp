using System;

using Clifton.Core.Assertions;
using Clifton.Core.ExtensionMethods;
using Clifton.Core.Semantics;

// When the runner is moved to its own AppDomain DLL, remove the reference in FlowSharpHopeService to the Clifton.SemanticProcessorService.

using HopeRunnerAppDomainInterface;

namespace FlowSharpHopeService
{
    public class HopeMembrane : Membrane { }

    [Serializable]
    public class Runner
    {
        protected AppDomain appDomain;
        protected IHopeRunner runner;

        public Runner()
        {
        }

        public void Load(string fullDllName)
        {
            if (runner == null)
            {
                string dll = fullDllName.LeftOf(".");
                appDomain = CreateAppDomain(dll);
                runner = InstantiateRunner(dll, appDomain);
                appDomain.DomainUnload += AppDomainUnloading;
            }
        }

        public void Unload()
        {
            if (appDomain != null)
            {
                appDomain.DomainUnload -= AppDomainUnloading;
                runner.Processing -= ProcessingSemanticType;
                Assert.SilentTry(() => AppDomain.Unload(appDomain));
                appDomain = null;
                runner = null;
            }
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
            runner.Processing += ProcessingSemanticType;

            return runner;
        }

        private void ProcessingSemanticType(object sender, HopeRunnerAppDomainInterface.ProcessEventArgs e)
        {
        }

        /// <summary>
        /// Unexpected app domain unload.  Unfortunately, the stack trace doesn't indicate where this is coming from!
        /// </summary>
        private void AppDomainUnloading(object sender, EventArgs e)
        {
            appDomain = null;
            runner = null;
        }
    }
}

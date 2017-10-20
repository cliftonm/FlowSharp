using System;

using Clifton.Core.Assertions;
using Clifton.Core.ExtensionMethods;
using Clifton.Core.Semantics;

using FlowSharpHopeCommon;
using HopeRunnerAppDomainInterface;

namespace FlowSharpHopeService
{
    // Must be derived from MarshalByRefObject so that the Processing event handler stays wired up to its handler on the callback from the app domain.
    [Serializable]
    public class AppDomainRunner : MarshalByRefObject, IRunner
    {
        // We need this attribute, otherwise the class HigherOrderProgrammingService needs to be marked serializable,
        // which if you do that, starts a horrid cascade of classes (like Clifton.Core to start with) that must be marked as serializable as well.
        // Gross.
        // So why does Runner need to be marked as Serializable, and how do we avoid that?
        // Because this Runner wires up an event in the AppDomain, which requires serialiation of this class.
        [field:NonSerialized]
        public event EventHandler<HopeRunnerAppDomainInterface.ProcessEventArgs> Processing;

        [NonSerialized]
        protected AppDomain appDomain;
        [NonSerialized]
        public IHopeRunner appDomainRunner;

        public AppDomainRunner()
        {
        }

        public void Load(string fullDllName)
        {
            if (appDomainRunner == null)
            {
                string dll = fullDllName.LeftOf(".");
                appDomain = CreateAppDomain(dll);
                appDomainRunner = InstantiateRunner(dll, appDomain);
                appDomain.DomainUnload += AppDomainUnloading;
            }
        }

        public void Unload()
        {
            if (appDomain != null)
            {
                appDomain.DomainUnload -= AppDomainUnloading;
                Assert.SilentTry(() => AppDomain.Unload(appDomain));
                appDomain = null;
                appDomainRunner = null;
            }
        }

        public void InstantiateReceptor(string name)
        {
            // TODO: Implement.
        }

        public void EnableDisableReceptor(string typeName, bool state)
        {
            // Runner may not be up when we get this.
            appDomainRunner?.EnableDisableReceptor(typeName, state);
        }

        public object InstantiateSemanticType(string typeName)
        {
            var st = appDomainRunner.InstantiateSemanticType(typeName);

            return st;
        }

        public PropertyContainer DescribeSemanticType(string typeName)
        {
            return null;
        }

        public void Publish(string _, object st)
        {
            appDomainRunner.Publish((ISemanticType)st);
        }

        public void Publish(string typeName, string json)
        {
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
            runner.Processing += (sender, args) => Processing.Fire(this, args);

            return runner;
        }

        /// <summary>
        /// Unexpected app domain unload.  Unfortunately, the stack trace doesn't indicate where this is coming from!
        /// </summary>
        private void AppDomainUnloading(object sender, EventArgs e)
        {
            appDomain = null;
            appDomainRunner = null;
        }
    }
}

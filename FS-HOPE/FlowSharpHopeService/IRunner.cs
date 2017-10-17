using System;

using Clifton.Core.Semantics;

namespace FlowSharpHopeService
{
    public interface IRunner
    {
        event EventHandler<HopeRunnerAppDomainInterface.ProcessEventArgs> Processing;

        /// <summary>
        /// DLL or EXE name
        /// </summary>
        void Load(string fullName);

        void Unload();
        [Obsolete]
        void InstantiateReceptor(Type t);
        void InstantiateReceptors();
        void EnableDisableReceptor(string typeName, bool state);
        dynamic InstantiateSemanticType(string typeName);
        void Publish(string typeName, object st);
    }
}

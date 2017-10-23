using System;
using System.Collections.Generic;

using FlowSharpHopeCommon;

namespace FlowSharpHopeService
{
    public interface IRunner
    {
        bool Loaded { get; }
        event EventHandler<HopeRunnerAppDomainInterface.ProcessEventArgs> Processing;

        /// <summary>
        /// DLL or EXE name
        /// </summary>
        void Load(string fullName);

        void Unload();
        List<ReceptorDescription> DescribeReceptor(string name);
        void InstantiateReceptor(string name);
        void EnableDisableReceptor(string typeName, bool state);
        PropertyContainer DescribeSemanticType(string typeName);
        void Publish(string typeName, object st);
        void Publish(string typeName, string json);
    }
}

using Clifton.Core.ServiceManagement;

using FlowSharpHopeCommon;

namespace FlowSharpHopeServiceInterfaces
{
    public interface IHigherOrderProgrammingService : IService
    {
        bool RunnerLoaded { get; }
        void LoadHopeAssembly();
        void UnloadHopeAssembly();
        void InstantiateReceptors();
        void EnableDisableReceptor(string typeName, bool state);
        PropertyContainer DescribeSemanticType(string typeName);
        void Publish(string typeName, object st);
        void Publish(string typeName, string json);
    }
}

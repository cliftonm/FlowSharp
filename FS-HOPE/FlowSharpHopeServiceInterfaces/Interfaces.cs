using Clifton.Core.Semantics;
using Clifton.Core.ServiceManagement;

namespace FlowSharpHopeServiceInterfaces
{
    public interface IHigherOrderProgrammingService : IService
    {
        void LoadHopeAssembly();
        void UnloadHopeAssembly();
        void InstantiateReceptors();
        void EnableDisableReceptor(string typeName, bool state);
        object InstantiateSemanticType(string typeName);
        void Publish(string typeName, object st);
    }
}

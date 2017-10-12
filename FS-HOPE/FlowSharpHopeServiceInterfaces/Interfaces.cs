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
        ISemanticType InstantiateSemanticType(string typeName);
        void Publish(ISemanticType st);
    }
}

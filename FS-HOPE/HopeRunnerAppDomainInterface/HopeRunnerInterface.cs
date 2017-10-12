using Clifton.Core.Semantics;

namespace HopeRunnerAppDomainInterface
{
    public interface IHopeRunner
    {
        void InstantiateReceptor(string typeName);
        void EnableDisableReceptor(string typeName, bool state);
        ISemanticType InstantiateSemanticType(string typeName);
        void Publish(ISemanticType st);
    }
}

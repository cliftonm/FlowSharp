using Clifton.Core.Semantics;

namespace HopeRunnerAppDomainInterface
{
    public interface IHopeRunner
    {
        void InstantiateReceptor(string typeName);
        ISemanticType InstantiateSemanticType(string typeName);
        void Publish(ISemanticType st);
    }
}

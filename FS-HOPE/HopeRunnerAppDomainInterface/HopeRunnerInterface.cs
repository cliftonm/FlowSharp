using System;

using Clifton.Core.Semantics;

namespace HopeRunnerAppDomainInterface
{
    [Serializable]
    public class ProcessEventArgs : EventArgs
    {
        public string FromMembraneTypeName { get; protected set; }
        public string FromReceptorTypeName { get; protected set; }
        public string ToMembraneTypeName { get; protected set; }
        public string ToReceptorTypeName { get; protected set; }
        public string SemanticTypeTypeName { get; protected set; }

        public ProcessEventArgs(string fromMembraneTypeName, string fromReceptorTypeName, string toMembraneTypeName, string toReceptorTypeName, string stTypeName)
        {
            FromMembraneTypeName = fromMembraneTypeName;
            FromReceptorTypeName = fromReceptorTypeName;
            ToMembraneTypeName = toMembraneTypeName;
            ToReceptorTypeName = toReceptorTypeName;
            SemanticTypeTypeName = stTypeName;
        }
    }

    public interface IHopeRunner
    {
        event EventHandler<ProcessEventArgs> Processing;
        void InstantiateReceptor(string typeName);
        void EnableDisableReceptor(string typeName, bool state);
        ISemanticType InstantiateSemanticType(string typeName);
        void Publish(ISemanticType st);
    }
}

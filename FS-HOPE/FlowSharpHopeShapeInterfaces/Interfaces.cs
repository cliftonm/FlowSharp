using System.Drawing;

namespace FlowSharpHopeShapeInterfaces
{
    public interface IAgent { }

    public interface IAgentReceptor
    {
        string AgentName { get;}
        string Text { get; }
        bool Enabled { get; }
        Color EnabledColor { get; }
    }

    public interface ISemanticTypeShape { }
}

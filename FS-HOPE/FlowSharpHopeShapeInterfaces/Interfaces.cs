namespace FlowSharpHopeShapeInterfaces
{
    public interface IAgentReceptor
    {
        string AgentName { get;}
        string Text { get; }
        bool Enabled { get; }
    }

    public interface ISemanticTypeShape { }
}

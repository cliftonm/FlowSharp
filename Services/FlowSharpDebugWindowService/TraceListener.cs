using System.Diagnostics;

namespace FlowSharpDebugWindowService
{
    public class TraceListener : ConsoleTraceListener
    {
        public DlgDebugWindow DebugWindow { get; set; }

        public override void WriteLine(string msg)
        {
            if (DebugWindow != null)
            {
                DebugWindow.Trace(msg + "\r\n");
            }
        }
    }
}

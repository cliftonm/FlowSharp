using System.Diagnostics;

namespace FlowSharp
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

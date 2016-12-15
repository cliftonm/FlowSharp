using System.Net;
using System.IO;

namespace FlowSharpWindowsControlShapes
{
    public static class Http
    {
        public static string Get(string uri)
        {
            WebClient client = new WebClient();

            // Add a user agent header in case the 
            // requested URI contains a query.

            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            Stream data = client.OpenRead(uri);
            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd();
            data.Close();
            reader.Close();

            return s;
        }
    }
}
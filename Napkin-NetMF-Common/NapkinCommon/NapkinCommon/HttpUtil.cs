using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.SPOT;

namespace NapkinCommon
{
    public class HttpUtil
    {

        public static string DoHttpMethod(string method, string uri, NetworkCredential credential, string requestText, bool readResponse = true)
        {
            string responseText = null;

            using (HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri))
            {
                request.Method = method;
                request.Credentials = credential;

                if (requestText != null)
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(requestText);
                    request.ContentLength = buffer.Length;
                    request.ContentType = "text/plain";

                    Stream stream = request.GetRequestStream();
                    stream.Write(buffer, 0, buffer.Length);
                }

                if (readResponse)
                {
                    // PollHaveResponse(request);
                    responseText = GetResponseText(request);
                }
            }

            return responseText;
        }

        public static string GetResponseText(HttpWebRequest request)
        {
            string responseText = "";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                // PollHaveResponse(request);

                int contentLength = (int)response.ContentLength;
                byte[] buffer = new byte[contentLength];
                Stream stream = response.GetResponseStream();
                int i = 0;
                while (i < contentLength)
                {
                    int readCount = stream.Read(buffer, i, contentLength - i);
                    i += readCount;
                }

                char[] responseChars = Encoding.UTF8.GetChars(buffer);
                responseText = new string(responseChars);
            }

            // PollHaveResponse(request);

            return responseText;
        }

        private static void PollHaveResponse(HttpWebRequest request, int timeoutMilliseconds = 2000)
        {
            DateTime start = DateTime.Now;
            DateTime timeout = start.AddMilliseconds(timeoutMilliseconds);
            while (!request.HaveResponse && (DateTime.Now < timeout))
            {
                Thread.Sleep(10);
            }
            DateTime finish = DateTime.Now;
            TimeSpan consumed = finish.Subtract(start);
            Debug.Print("HaveResponse: " + request.HaveResponse + " after: " + consumed);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Collections.Specialized;
using System.Net;
using System.IO;
using System.Web;
using System.Web.SessionState;
using System.IO.Compression;

namespace GetexceptionalPlugin
{
    /// <summary>
    /// This is the class responsible for sending the data.  It is structured in a paramaterless
    /// way so it can be executed in a thread.
    /// </summary>
    class ExceptionSender
    {
        private string apiKey;
        private string urlEndPoint = "http://api.getexceptional.com/api/errors";

        private ExceptionData exceptionData;

        public ExceptionSender(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public ExceptionSender(string apiKey, ExceptionData data)
            : this(apiKey)
        {
            exceptionData = data;
        }

//        public void SendException()
//        {
//            string jsonData = exceptionData.ToJson();
//            //string jsonDataCompressed = GZipString.ZipToString(jsonData);
//            //byte[] bytes = Encoding.UTF8.GetBytes(jsonDataCompressed);
//            byte[] jsonDataCompressed = GZipString.ZipToByteArray(jsonData);
//            WebRequest req = WebRequest.Create(urlEndPoint + "?api_key=" + apiKey +
//                "&protocol_version=6");
//            req.Method = "POST";
//            req.ContentType = "application/json";
//            req.Headers.Add("Content-Encoding: gzip");
//            //req.ContentLength = bytes.Length;
//            req.ContentLength = jsonDataCompressed.Length;
//            using (Stream requestStream = req.GetRequestStream())
//            {
//                //requestStream.Write(bytes, 0, bytes.Length);
//                requestStream.Write(jsonDataCompressed, 0, jsonDataCompressed.Length);
//            }

//            HttpStatusCode code;
//            try
//            {
//                using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
//                {
//                    code = response.StatusCode;
//                }
//                if (code != HttpStatusCode.Created && code != HttpStatusCode.OK)
//                {
//                    //we only throw exceptions in debug mode, otherwise they're swallowed for production.
//#if DEBUG
//                    throw new Exception("Unexpected response from " + urlEndPoint + " - " + code);
//#endif
//                }
//            }
//            catch (Exception)
//            {
//                //These exceptions will happen if GetExceptional goes down altogether.  Swallow
//                //in production again
//#if DEBUG
//                throw;
//#endif
//            }
//        }

        public void SendException()
        {
            string jsonData = exceptionData.ToJson();
            WebRequest req = WebRequest.Create(urlEndPoint + "?api_key=" + apiKey +
                "&protocol_version=6");
            req.Method = "POST";
            req.ContentType = "application/json";
            req.Headers.Add("Content-Encoding: gzip");

            Stream reqStream = req.GetRequestStream();
            GZipStream gz = new GZipStream(reqStream, CompressionMode.Compress);

            StreamWriter sw = new System.IO.StreamWriter(gz, Encoding.ASCII);
            sw.Write(jsonData);
            sw.Close();

            gz.Close();
            reqStream.Close();

            HttpStatusCode code;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
                {
                    code = response.StatusCode;
                }
                if (code != HttpStatusCode.Created && code != HttpStatusCode.OK)
                {
                    //we only throw exceptions in debug mode, otherwise they're swallowed for production.
                    throw new Exception("Unexpected response from " + urlEndPoint + " - " + code);
                }
            }
            catch (Exception)
            {
                //These exceptions will happen if GetExceptional goes down altogether.  Swallow
                //in production again
                throw;
            }
        }

    }

}

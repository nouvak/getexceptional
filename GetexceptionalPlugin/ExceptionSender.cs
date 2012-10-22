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

namespace GetexceptionalPlugin
{
    /// <summary>
    /// This is the class responsible for sending the data.  It is structured in a paramaterless
    /// way so it can be executed in a thread.
    /// </summary>
    class ExceptionSender
    {
        private string apiKey;
        private string urlEndPoint = "http://api.exceptional.io/api/errors";

        private ExceptionData m_Data;

        public ExceptionSender(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public ExceptionSender(string apiKey, ExceptionData data)
            : this(apiKey)
        {
            m_Data = data;
        }

        public void SendException()
        {
            string jsonData = m_Data.ToJson();
            //string jsonDataCompressed = GZipString.ZipToString(jsonData);
            //byte[] bytes = Encoding.UTF8.GetBytes(jsonDataCompressed);
            byte[] jsonDataCompressed = GZipString.ZipToByteArray(jsonData);
            WebRequest req = WebRequest.Create(urlEndPoint + "?api_key=" + apiKey +
                "&protocol_version=6");
            req.Method = "POST";
            req.ContentType = "application/json";
            //req.ContentLength = bytes.Length;
            req.ContentLength = jsonDataCompressed.Length;
            using (Stream requestStream = req.GetRequestStream())
            {
                //requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Write(jsonDataCompressed, 0, jsonDataCompressed.Length);
            }

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
#if DEBUG
                    throw new Exception("Unexpected response from " + urlEndPoint + " - " + code);
#endif
                }
            }
            catch (Exception)
            {
                //These exceptions will happen if GetExceptional goes down altogether.  Swallow
                //in production again
#if DEBUG
                throw;
#endif
            }


            //            string xmlData = m_Data.ToXml();
            //            byte[] bytes = Encoding.UTF8.GetBytes(xmlData);
            //            WebRequest req = WebRequest.Create(urlEndPoint + "?api_key=" + apiKey + "&protocol_version=2");
            //            req.Method = "POST";
            //            req.ContentType = "text/xml";
            //            req.ContentLength = bytes.Length;
            //            using (Stream requestStream = req.GetRequestStream())
            //            {
            //                requestStream.Write(bytes, 0, bytes.Length);
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
        }
    }

}

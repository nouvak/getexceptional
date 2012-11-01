using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Collections.Specialized;
using System.Web;
using System.Web.SessionState;
using System.IO;
using Newtonsoft.Json;

namespace GetexceptionalPlugin
{
    class ExceptionData
    {
        private Exception exception;
        private HttpRequest request;
        private HttpSessionState session;
        private DateTime exceptionTime;

        public ExceptionData(Exception ex)
        {
            exception = ex;
            exceptionTime = DateTime.Now.ToUniversalTime();
        }

        public ExceptionData(Exception ex, HttpRequest req)
            : this(ex)
        {
            request = req;
        }

        public ExceptionData(Exception ex, HttpRequest req, HttpSessionState sess)
            : this(ex, req)
        {
            session = sess;
        }

        public string ExceptionClass
        {
            get
            {
                if (exception == null)
                    return null;
                return exception.GetType().ToString();
            }
        }

        public string ToJson()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.None;
                writer.WriteStartObject();
                ToJsonEnvironmentInfo(writer);
                ToJsonRequestInfo(writer);
                ToJsonExceptionInfo(writer);
                writer.WriteEndObject();
            }
            return sb.ToString();
        }

        private void ToJsonExceptionInfo(JsonWriter writer)
        {
            writer.WritePropertyName("exception");

            writer.WriteStartObject();
            writer.WritePropertyName("exception_class");
            writer.WriteValue(ExceptionClass ?? "[unknown]");
            writer.WritePropertyName("message");
            writer.WriteValue(exception.Message);
            writer.WritePropertyName("occurred_at");
            
            
            writer.WriteValue(exceptionTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"));
            writer.WritePropertyName("backtrace");
            writer.WriteStartArray();
            string stackTrace = exception.StackTrace;
            writer.WriteValue(stackTrace);
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        private void ToJsonRequestInfo(JsonWriter writer)
        {
            writer.WritePropertyName("request");

            writer.WriteStartObject();
            if ( session != null)
            {
                writer.WritePropertyName("session");
                writer.WriteStartObject();
                System.Collections.IEnumerator ie = session.GetEnumerator();
                try
                {
                    while (ie.MoveNext())
                    {
                        string key = (string)ie.Current;
                        string value = (session[key] == null) ? "NULL" : session[key].ToString();
                        writer.WritePropertyName(key);
                        writer.WriteValue(value);
                    }
                }
                catch (Exception ex)
                {
                    writer.WritePropertyName("ExceptionalException");
                    writer.WriteValue(ex.Message);
                }
                writer.WriteEndObject();
            }
            if (request != null)
            {
                writer.WritePropertyName("remote_ip");
                writer.WriteValue(request.UserHostAddress);

                NameValueCollection serverVariables = request.ServerVariables;
                writer.WritePropertyName("controller");
                writer.WriteValue(serverVariables["HTTP_HOST"]);
                writer.WritePropertyName("action");
                writer.WriteValue(request.FilePath);
                writer.WritePropertyName("url");
                writer.WriteValue(request.Url.ToString());

                writer.WritePropertyName("parameters");
                writer.WriteStartObject();
                if (request.QueryString != null)
                {
                    writer.WritePropertyName("queryString");
                    writer.WriteValue(serverVariables["HTTP_HOST"]);
                }
                if (request.Form != null)
                {
                    NameValueCollection formParameters = request.Form;
                    foreach (String key in formParameters)
                    {
                        writer.WritePropertyName(key);
                        writer.WriteValue(formParameters[key]);
                    }
                }
                writer.WriteEndObject();

                writer.WritePropertyName("headers");
                writer.WriteStartObject();
                NameValueCollection headers = request.Headers;
                foreach (String key in headers)
                {
                    writer.WritePropertyName(key);
                    writer.WriteValue(headers[key]);
                }
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
        }

        private void ToJsonEnvironmentInfo(JsonWriter writer)
        {
            writer.WritePropertyName("application_environment");

            writer.WriteStartObject();
            writer.WritePropertyName("framework");
            writer.WriteValue(".NET");
            writer.WritePropertyName("env");
            writer.WriteStartObject();
            if (request != null)
            {
                NameValueCollection serverVariables = request.ServerVariables;
                foreach (String key in serverVariables)
                {
                    writer.WritePropertyName(key);
                    writer.WriteValue(serverVariables[key]);
                }
            }
            writer.WriteEndObject();
            writer.WritePropertyName("language");
            writer.WriteValue("C#");
            writer.WritePropertyName("language_version");
            writer.WriteValue("3.5");
            writer.WritePropertyName("application_root_directory");
            if (request != null)
            {
                writer.WriteValue(request.ApplicationPath);
            }
            else
            {
                writer.WriteValue("no-root-path");
            }
            
            writer.WriteEndObject();

            writer.WritePropertyName("client");

            writer.WriteStartObject();
            writer.WritePropertyName("name");
            writer.WriteValue("GetExceptionalPlugin.NET");
            writer.WritePropertyName("version");
            writer.WriteValue("1.0.0.0");
            writer.WritePropertyName("protocol_version");
            writer.WriteValue(6);
            writer.WriteEndObject();
        }
    }
}

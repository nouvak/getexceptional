using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Collections.Specialized;
using System.Web;
using System.Web.SessionState;

namespace GetexceptionalPlugin
{
    class ExceptionData
    {
        private Exception exception;
        private HttpRequest request;
        private HttpSessionState session;
        private DateTime exceptionTime = DateTime.Now.ToUniversalTime();

        public ExceptionData(Exception ex)
        {
            exception = ex;
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
                if (null == exception) return null;
                return exception.GetType().ToString();
            }
        }

        /// <summary>
        /// Given an HTTP Request and an Exception, turns it into XML in a format that 
        /// getexceptional.com can consume
        /// </summary>
        /// <param name="ex">Exception you want to report</param>
        /// <param name="req">Request during which the exception was thrown.  This can be
        ///                   null, so that you cann use this in non-web environments.
        ///                   </param>
        /// <returns></returns>
        public string ToXml()
        {
            string controller = "[no controller]";
            string action = "[no action]";
            string root = "[no root]";
            string url = "[no url]";

            //create a new copy of the servervariables because sometimes they can change during the 
            //iteration of them, which causes errors
            NameValueCollection serverVariables = null;
            try
            {
                if (request != null) 
                    serverVariables = new NameValueCollection(request.ServerVariables);
            }
            catch (Exception)
            {
                //I have seen occasions where IIS throws exceptions accessing server variables
#if DEBUG
                throw;
#endif
            }

            if (request != null)
            {
                action = request.FilePath;
                if (null != serverVariables) controller = serverVariables["HTTP_HOST"];
                root = request.ApplicationPath;
                url = (null != request.Url) ? request.Url.ToString() : "";
            }



            string exceptionMessage = "[no message]";
            string exceptionStack = "[no stack trace]";
            if (exception != null)
            {
                exceptionMessage = exception.Message;
                exceptionStack = exception.StackTrace;
            }

            //do this with a stringbuilder instead of XML tools (ex. LINQ) for the sake of speed.
            StringBuilder sb = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
            sb.Append("<error>\n");
            sb.Append("\t<controller_name>");
            sb.Append(controller);
            sb.Append("</controller_name>\n");
            sb.Append("\t<action_name>");
            sb.Append(action);
            sb.Append("</action_name>\n");
            sb.Append("\t<error_class>");
            sb.Append(ExceptionClass ?? "[unknown]");
            sb.Append("</error_class>\n");
            sb.Append("\t<message>");
            sb.Append(XMLEscape(exceptionMessage));
            sb.Append("</message>\n");
            sb.Append("\t<backtrace>");
            sb.Append(XMLEscape(exceptionStack));
            sb.Append("</backtrace>\n");
            sb.Append("\t<occurred_at>");
            sb.Append(exceptionTime.ToString());
            sb.Append("</occurred_at>\n");
            sb.Append("\t<rails_root>");
            sb.Append(root);
            sb.Append("</rails_root>\n");
            sb.Append("\t<url>");
            sb.Append(XMLEscape(url));
            sb.Append("</url>\n");
            sb.Append("\t<environment>\n");
            if (serverVariables != null)
                AppendXML(serverVariables, sb);
            sb.Append("</environment>\n");
            sb.Append("\t<session>\n");
            if (session != null)
            {
                System.Collections.IEnumerator ie = session.GetEnumerator();
                string currentSessionItemName = "";
                string currentSessionItemValue = "";
                try
                {
                    while (ie.MoveNext())
                    {
                        currentSessionItemName = (string)ie.Current;
                        currentSessionItemValue = (null == session[currentSessionItemName]) ? "NULL" : session[currentSessionItemName].ToString();
                        AppendXML(currentSessionItemName, currentSessionItemValue, sb);
                    }
                }
                catch (Exception ex)
                {
                    AppendXML("ExceptionalException", ex.Message, sb);
                }
            }
            sb.Append("</session>\n");
            sb.Append("\t<parameters>\n");
            if (null != request && null != request.QueryString) ExceptionData.AppendXML(request.QueryString, sb);
            if (null != request && null != request.Form) ExceptionData.AppendXML(request.Form, sb);
            sb.Append("</parameters>\n");
            sb.Append("</error>");

            return sb.ToString();
        }


        //Appends a NameValueCollection to a StringBuilder as XML
        private static void AppendXML(NameValueCollection nvc, StringBuilder sb)
        {
            if (nvc == null)
                return;
            foreach (string key in nvc.Keys) { 
                AppendXML(key, nvc[key], sb); 
            }
        }

        //Turns the name into a tag, and makes the value it's inner text, and appends to the stringbuilder
        private static void AppendXML(string name, string value, StringBuilder sb)
        {
            if (name == null || value == null) 
                return;
            sb.Append("<"); sb.Append(name); sb.Append(">");
            sb.Append(ExceptionData.XMLEscape(value));
            sb.Append("</"); sb.Append(name); sb.Append(">\n");
        }

        private static string XMLEscape(string text)
        {
            return SecurityElement.Escape(text);
        }

        public string ToJson()
        {
            return @"
                    {
                       ""request"": {
                           ""session"": {
                               ""session_variable_1"": ""Session 1 Variable Value"",
                               ""session_variable_2"": ""Session 2 Variable Value"",
                           },
                           ""remote_ip"": ""127.0.0.1"",
                           ""parameters"": {
                               ""action"": ""gonuts"",
                               ""controller"": ""spike""
                           },
                           ""action"": ""gonuts"",
                           ""url"": ""http://localhost/spike/gonuts"",
                           ""request_method"": ""get"",
                           ""controller"": ""SpikeController"",
                           ""headers"": {
                               ""Version"": ""HTTP/1.1"",
                               ""User-Agent"": ""Mozilla/5.0"",
                               ""Accept-Encoding"": ""gzip,deflate"",
                               ""Keep-Alive"": ""300"",
                               ""Accept"": ""text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"",
                               ""Accept-Charset"": ""ISO-8859-1,utf-8;q=0.7,*;q=0.7"",
                               ""Accept-Language"": ""en-us,en;q=0.5"",
                               ""Connection"": ""keep-alive"",
                               ""Host"": ""myapp.com""
                           }
                       },
                       ""application_environment"": {
                           ""framework"": ""rails"",
                           ""env"": {
                               ""MANPATH"": ""/usr/local/git/man:/opt/local/share/man"",
                               ""SHELL"": ""/bin/bash"",
                               ""DISPLAY"": ""/tmp/launch-SYxDj8/:0"",
                               ""CVSEDITOR"": ""mate -w"",
                               ""LANG"": ""en_IE.UTF-8"",
                               ""PWD"": ""/var/www/myapp"",
                               ""PATH"": ""/opt/local/bin:/usr/local/git/bin:/opt/local/bin"",
                           },
                           ""language"": ""ruby"",
                           ""language_version"": ""1.8.7 p72 2008-08-11 i686-darwin9"",
                           ""application_root_directory"": ""/var/www/myapp""
                       },
                       ""exception"": {
                           ""occurred_at"": ""2010-10-29T10:48:54+01:00"",
                           ""message"": ""NoMethodError: undefined method `horse!' for nil:NilClass"",
                           ""backtrace"": [
                               ""/Users/wal/work/myapp/app/controllers/spike_controller.rb:16:in `gonuts'"", 
                               ""/actionpack-2.3.2/lib/action_controller/base.rb:1322:in `send'"", 
                               ""/actionpack-2.3.2/lib/action_controller/base.rb:1322:in `perform_action_without_filters'"",
                               ""/actionpack-2.3.2/lib/action_controller/filters.rb:617:in `call_filters'"",
                               ""/actionpack-2.3.2/lib/action_controller/filters.rb:610:in `perform_action_without_benchmark'"",
                               ""/actionpack-2.3.2/lib/action_controller/benchmarking.rb:68:in `perform_action_without_rescue'"",
                               ""/activesupport-2.3.2/lib/active_support/core_ext/benchmark.rb:17:in `ms'"", 
                               ""/ruby/1.8/benchmark.rb:308:in `realtime'"", 
                               ""/ruby/gems/1.8/gems/activesupport-2.3.2/lib/active_support/core_ext/benchmark.rb:17:in `ms'""],
                           ""exception_class"": ""RuntimeError""
                       },
                       ""client"": {
                           ""name"": ""getexceptional-rails-plugin"",
                           ""version"": ""0.2.0"",
                           ""protocol_version"": 6
                       }
                    }
                ";
        }
    }
}

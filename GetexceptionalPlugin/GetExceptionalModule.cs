using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Configuration;

namespace GetexceptionalPlugin
{
    public class GetExceptionalModule : IHttpModule
    {
        internal class NotFoundException : Exception { }

        private const string CONF_API_KEY = "GetExceptionalApiKey";

        /// <summary>
        /// You will need to configure this module in the web.config file of your
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpModule Members

        public void Dispose()
        {
            //clean-up code here.
        }

        public void Init(HttpApplication context)
        {
            context.Error += new EventHandler(OnError);
        }

        #endregion

        public void OnError(Object source, EventArgs e)
        {
            HttpContext ctx = HttpContext.Current;
            HttpRequest request = ctx.Request;
            Exception exception = ctx.Server.GetLastError();

            //the one we ultimately want to report may be inner to an httpunhandled exception
            if (exception.InnerException != null) 
                exception = exception.InnerException;

            string apiKey = ConfigurationSettings.AppSettings[CONF_API_KEY];
            if (null == apiKey)
            {
                throw new Exception("The ApiKey for GetExceptional was not provided.  Did you include " + CONF_API_KEY + " in the appSettings section of your web.config?");
            }
            //report a 404 if found
            GetExceptional exReporter = new GetExceptional(apiKey);
            if (exception is HttpException && ((HttpException)exception).GetHttpCode() == 404)
            {
                exReporter.ReportException(new NotFoundException(), request, null, true);
            }
            else
            {
                exReporter.ReportException(exception, request, ctx.Session, true);
            }
        }
    }

}

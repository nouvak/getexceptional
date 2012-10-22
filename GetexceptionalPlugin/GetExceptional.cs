using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.SessionState;

namespace GetexceptionalPlugin
{
    public class GetExceptional
    {
        private string apiKey;

        public GetExceptional(string apiKey)
        {
            this.apiKey = apiKey;
        }

        /**
         * Send true to fire the request in a thread, false to do it synchronously
         */
        public void ReportException(Exception ex, bool async)
        {
            ReportException(new ExceptionData(ex), async);
        }

        /**
         * Send true to fire the request in a thread, false to do it synchronously
         */
        public void ReportException(Exception ex, HttpRequest req, HttpSessionState sess, bool async)
        {
            ReportException(new ExceptionData(ex, req, sess), async);
        }

        private void ReportException(ExceptionData exData, bool async)
        {
            ExceptionSender sender = new ExceptionSender(apiKey,  exData);
            if (async)
            {
                Thread senderThread = new Thread(new ThreadStart(sender.SendException));
                senderThread.Start();
            }
            else
            {
                sender.SendException();
            }
        }
    }
}

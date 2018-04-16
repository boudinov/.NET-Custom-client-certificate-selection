using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Code
{
    class SomeServiceHacked : SomeService
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            var req = (HttpWebRequest)base.GetWebRequest(uri);
	    /*
	    	This is the only viable hook i found, where we can install the internal delegate.
	    	I tried using a thread, which monitors (by polling super frequently) the HttpWebRequest internals for the right moment to setup the internal delegate, but did not appear to work reliably.
	    	In case you do not need to override the Server certificate validation, like it is done here, you need to provide a working Server certificate validation behavior (like https://stackoverflow.com/questions/28679120/how-to-call-default-servercertificatevalidationcallback-inside-customized-valida?lq=1)
	    	Note, that at least, this is more fine grained (affects only this specific HttpWebRequest) than the common ServicePointManager.ServerCertValidationCallback, which affects all HttpWebRequests in the current process.
	    */
            req.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
            {
                HttpWebRequestHack.SetupInternalClientCertDelegate(req);
                return true;
            };

            return req;
        }

    }
}

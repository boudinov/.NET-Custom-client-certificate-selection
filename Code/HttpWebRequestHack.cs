using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Code
{
    public class HttpWebRequestHack
    {
        private static X509Certificate TheDelegate(string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            return localCertificates[0];
        }

        public static void SetupInternalClientCertDelegate(HttpWebRequest request)
        {
            object submitWriteStream = request.GetType().GetField("_SubmitWriteStream", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(request);
            object connection = submitWriteStream.GetType().GetField("m_Connection", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(submitWriteStream);

            object tlsNetworkStream = connection.GetType().GetProperty("NetworkStream", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(connection);
            //should be TlsStream, as at some point it gets replaced by NetworkStream = new TlsStream(...)
            if (tlsNetworkStream != null && tlsNetworkStream.GetType().Name == "TlsStream")
            {
                object sslState = tlsNetworkStream.GetType().GetField("m_Worker", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(tlsNetworkStream);

                //for some reason we cannot set _CertSelectionDelegate to anonymous Func<...>, neither can we Delegate.CreateDelegate from anonymous func
                var internalDelegType = Assembly.GetAssembly(typeof(System.Net.Security.LocalCertificateSelectionCallback)).GetType("System.Net.Security.LocalCertSelectionCallback");
                var deleg = Delegate.CreateDelegate(internalDelegType, typeof(HttpWebRequestHack).GetMethod(nameof(TheDelegate), BindingFlags.Static | BindingFlags.NonPublic));
                sslState.GetType().GetField("_CertSelectionDelegate", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(sslState, deleg);

                //_Context type is SecureChannel
                var context = sslState.GetType().GetField("_Context", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(sslState);
                if (context != null)
                    context.GetType().GetField("m_CertSelectionDelegate", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(context, deleg);
            }
        }
    }
}

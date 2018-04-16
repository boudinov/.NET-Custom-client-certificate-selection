# .NET-Custom-client-certificate-selection
A hack into .NET (pre-Core) framework which allows custom CLIENT (not server, usually customized using ServicePointManager.ServerCertValidationCallback) certificate selection/validation

You would need this in case your Client certificate does not have a valid trusted Certificate Authority certificate (intermediate or root) on it's chain, installed in the client computer's (user or machine) certificate store

A common case is a Azure Web App, where you do not have permissions to install trusted intermediate/root certificates

It works for web requests using System.Net.HttpWebRequest, which means it works also works for web service references based on System.Web.Services.Protocols.SoapHttpClientProtocol

The hack is performed using reflection, by setting an internal System.Net.Security.LocalCertificateSelectionCallback delegate, deep into the System.Net stack

This was developed while working for https://www.klearlending.com/

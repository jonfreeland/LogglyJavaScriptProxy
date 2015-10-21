using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace LogglyJavaScriptProxy {
    /// <summary>
    /// Proxies requests from Loggly's JavaScript logger.
    /// </summary>
    public class LogglyProxyController : Controller {
        public string LogglyToken { get; set; }

        /// <summary>
        /// Creates a new instance of LogglyProxyController.
        /// Attempts to set the Loggly token via ConfigurationManager.AppSettings["LogglyToken"] if it exists.
        /// </summary>
        public LogglyProxyController() {
            if (ConfigurationManager.AppSettings.AllKeys.Contains("LogglyToken"))
                LogglyToken = ConfigurationManager.AppSettings["LogglyToken"];
        }

        /// <summary>
        /// Creates a new instance of LogglyProxyController using the provided Loggly token.
        /// </summary>
        /// <param name="token">The Loggly token.</param>
        public LogglyProxyController(string token) {
            LogglyToken = token;
        }

        [Route("loggly/inputs/{token}/tag/{tags}")]
        [HttpPost]
        public HttpWebResponseResult Index(string token, string tags) {
            token = LogglyToken; // override with real token here
            var externalRequest = (HttpWebRequest)WebRequest.Create($"https://logs-01.loggly.com/inputs/{token}/tag/{tags}");   // TODO: make URL configurable
            CopyRequestHeadersContent(this.Request, externalRequest);
            HttpWebResponse externalResponse;
            try {
                externalResponse = (HttpWebResponse)externalRequest.GetResponse();
            } catch (WebException ex) {
                externalResponse = (HttpWebResponse)ex.Response;
            }

            return new HttpWebResponseResult(externalResponse);
        }

        /// <summary>
        /// Copies all headers and content (except the URL) from an incoming to an outgoing request.
        /// See: http://stackoverflow.com/a/12506079/90227
        /// </summary>
        /// <param name="source">The request to copy from</param>
        /// <param name="destination">The request to copy to</param>
        private void CopyRequestHeadersContent(HttpRequestBase source, HttpWebRequest destination) {
            destination.Method = source.HttpMethod;

            // Copy unrestricted headers (including cookies, if any)
            foreach (var headerKey in source.Headers.AllKeys) {
                switch (headerKey) {
                    case "Connection":
                    case "Content-Length":
                    case "Date":
                    case "Expect":
                    case "Host":
                    case "If-Modified-Since":
                    case "Range":
                    case "Transfer-Encoding":
                    case "Proxy-Connection":
                        // Let IIS handle these
                        break;
                    case "Accept":
                    case "Content-Type":
                    case "Referer":
                    case "User-Agent":
                        // Restricted - copied below
                        break;
                    default:
                        destination.Headers[headerKey] = source.Headers[headerKey];
                        break;
                }
            }

            // Copy restricted headers
            if (source.AcceptTypes.Any())
                destination.Accept = string.Join(",", source.AcceptTypes);
            destination.ContentType = source.ContentType;
            destination.Referer = source.UrlReferrer.AbsoluteUri;
            destination.UserAgent = source.UserAgent;

            // Copy content (if content body is allowed)
            if (source.HttpMethod != "GET"
                && source.HttpMethod != "HEAD"
                && source.ContentLength > 0) {
                var destinationStream = destination.GetRequestStream();
                source.InputStream.CopyTo(destinationStream);
                destinationStream.Close();
            }
        }
    }
}
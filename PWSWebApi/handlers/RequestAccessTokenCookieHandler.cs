
using System;  
using System.Collections.Generic;  
using System.Linq;  
using System.Net.Http;  
using System.Net.Http.Headers;  
using System.Web;
namespace PWSWebApi.handlers
{
    public class RequestAccessTokenCookieHandler : DelegatingHandler
    {
        static public string CookieStampToken = "session-id";
        protected async override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            string cookie_stamp;
            //var req = HttpContext.Current.Request;
            var token = request.Headers.FirstOrDefault(x=> x.Key=="Authorization").Value.FirstOrDefault();
            var cookie = request.Headers.GetCookies(CookieStampToken).FirstOrDefault();
            if (cookie == null)
            {
                cookie_stamp = token;
            }
            else
            {
                cookie_stamp = cookie[CookieStampToken].Value;
                
            }
            request.Properties[CookieStampToken] = cookie_stamp;
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            response.Headers.AddCookies(new CookieHeaderValue[] {
               new CookieHeaderValue(CookieStampToken,cookie_stamp)
              });

            
            return response;
        }
    }
}

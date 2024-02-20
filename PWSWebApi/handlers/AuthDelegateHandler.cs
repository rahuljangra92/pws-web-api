using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace PWSWebApi.handlers
{
    public class AuthDelegateHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = request.Headers.Authorization != null ? request.Headers.Authorization.Parameter : string.Empty;
            request.Properties.Add("accessToken", token);
            var req = HttpContext.Current.Request;
            var headerValues = req.Headers.GetValues("apps");
            var devMode = Convert.ToBoolean(ConfigurationManager.Configuration.GetSection("appSettings")["devMode"]);
            //if (headerValues == null)
            //{ 
            //    var keys = req.Params.AllKeys.ToList();
            //    if(keys.IndexOf("SSISIdentifierType") > -1)
            //    {
            //        request.Properties.Add("isvalid", "true");
            //    }
            //    else if (keys.IndexOf("pwsaccess") > -1)
            //    {
            //        request.Properties.Add("isvalid", "true");
            //    }
            //}
            if (devMode)
            {
                request.Properties.Add("isvalid", "true");
            }
            else if (req.UrlReferrer == null//&& !headerValues.Any()
            )
            {
                request.Properties.Add("isvalid", "false");
            }
            else
            {
                var authizedSource = req.UrlReferrer != null && ((req.UrlReferrer.Authority.Contains("localhost:3000") && devMode) || req.UrlReferrer.Authority.Contains("dev2.privatewealthsystems.com") || req.UrlReferrer.Authority.Contains("uat2.privatewealthsystems.com") || req.UrlReferrer.Authority.Contains("uat-2.privatewealthsystems.com") || req.UrlReferrer.Authority.Contains("app2.privatewealthsystems.com") || req.UrlReferrer.Authority.Contains("dev.privatewealthsystems.com") || req.UrlReferrer.Authority.Contains("uat.privatewealthsystems.com") || req.UrlReferrer.Authority.Contains("app.privatewealthsystems.com") || req.UrlReferrer.Authority.Contains("app-prod.privatewealthsystems.com") || req.UrlReferrer.Authority.Contains("app-dev.privatewealthsystems.com") || req.UrlReferrer.Authority.Contains("portal-dev.privatewealthsystems.com/") || req.UrlReferrer.Authority.Contains("portal-uat.privatewealthsystems.com") || req.UrlReferrer.Authority.Contains("portal-uat-2.privatewealthsystems.com") || req.UrlReferrer.Authority.Contains("portal.privatewealthsystems.com") || req.UrlReferrer.Authority.Contains("portal.au.privatewealthsystems.com") || req.UrlReferrer.Authority.Contains("app.au.privatewealthsystems.com") || (req.UrlReferrer.Authority.Contains("localhost") && devMode));
                //if (headerValues != null)
                //{
                //    request.Properties.Add("isvalid", "true");
                //}
                if (authizedSource)
                {
                    request.Properties.Add("isvalid", "true");
                }
                else
                {
                    request.Properties.Add("isvalid", "false");
                }
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
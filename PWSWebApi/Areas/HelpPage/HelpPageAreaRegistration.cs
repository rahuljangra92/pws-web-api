using System.Web.Http;
using Microsoft.AspNetCore.Mvc;

namespace PWSWebApi.Areas.HelpPage
{
    public class HelpPageAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "HelpPage";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute("HelpPage_Default", "Help/{action}/{apiId}", new { controller = "Help", action = "Index", apiId = UrlParameter.Optional });
            HelpPageConfig.Register(GlobalConfiguration.Configuration);
        }
    }
}
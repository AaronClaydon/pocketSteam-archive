using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "AJAXCommandShort", // Route name
                "AjaxCommand/{SessionToken}/{Type}/{Command}", // URL with parameters
                new { controller = "Home", action = "AJAXCommand", SessionToken = UrlParameter.Optional, Type = UrlParameter.Optional, Command = UrlParameter.Optional } // Parameter defaults
            );

            routes.MapRoute(
                "AJAXReplyShort", // Route name
                "AjaxReply/{SessionToken}", // URL with parameters
                new { controller = "Home", action = "AJAXReply", SessionToken = UrlParameter.Optional } // Parameter defaults
            );

            routes.MapRoute(
                "LoginUsingAddress", // Route name
                "Login/{userName}/{passWord}", // URL with parameters
                new { controller = "Home", action = "Login" } // Parameter defaults
            );

            routes.MapRoute(
                "DisplayShort", // Route name
                "Session/{SessionToken}", // URL with parameters
                new { controller = "Home", action = "Display", SessionToken = UrlParameter.Optional } // Parameter defaults
            );

            routes.MapRoute(
                "FAQShort", // Route name
                "FAQ", // URL with parameters
                new { controller = "Home", action = "FAQ" } // Parameter defaults
            );

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }
}
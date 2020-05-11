using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Website
{
    public class MvcApplication : System.Web.HttpApplication
    {
        // Set httpclient with a base Url here
        public static HttpClient httpClient = new HttpClient() { BaseAddress = new Uri(WebConfigurationManager.AppSettings["apiBaseAddress"]) };

        protected void Application_Start()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            httpClient.DefaultRequestHeaders.Accept.Clear();
            // Sets the Accept header to "application/json". Setting this header tells the server to send data in JSON format.
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
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
        // Set baseUrl here
        public static string baseUrl = WebConfigurationManager.AppSettings["apiBaseAddress"];

        public static HttpClient httpClient = new HttpClient() { BaseAddress = new Uri(baseUrl) };

        protected void Application_Start()
        {
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

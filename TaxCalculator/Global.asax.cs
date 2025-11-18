using System.Web.Mvc;
using System.Web.Routing;
using TaxCalculator.App_Start;

namespace TaxCalculator
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}

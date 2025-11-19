using System.Web.Http;
using TaxCalculator.Configuration;

namespace TaxCalculator
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebAPIConfig.Register);
        }
    }
}

using System;
using System.Web.Http;
using PasswordGenerator.Configuration;

namespace PasswordGenerator
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalConfiguration.Configure(WebAPIConfig.Register);
        }
    }
}

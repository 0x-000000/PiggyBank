using System;
using System.Linq;
using System.Web;

namespace Bank.Modules
{
    public class AuthMiddleware : IHttpModule
    {
        private static readonly string[] ProtectedPrefixes = new[] { "~/member", "~/staff" };

        // register middleware listener
        public void Init(HttpApplication context)
        {
            context.AuthenticateRequest += OnAuthenticate;
        }

        private static void OnAuthenticate(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            var path = app.Request.AppRelativeCurrentExecutionFilePath ?? string.Empty;
            var loggedIn = app.Request.Cookies["user"] != null;

            if (IsProtected(path) && !loggedIn)
            {
                var target = HttpUtility.UrlEncode(app.Request.RawUrl ?? "/");
                var loginUrl = VirtualPathUtility.ToAbsolute("~/Account/Login");
                app.Response.Redirect(loginUrl + "?returnUrl=" + target, true);
                return;
            }

            // redirect if logged in to user page
            if (IsLogin(path) && loggedIn)
            {
                var memberUrl = VirtualPathUtility.ToAbsolute("~/Member");
                app.Response.Redirect(memberUrl, true);
            }
        }

        // only check for staff and user endpoints
        private static bool IsProtected(string path)
        {
            return ProtectedPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsLogin(string path)
        {
            return path.StartsWith("~/account/login", StringComparison.OrdinalIgnoreCase);
        }

        public void Dispose()
        {
        }
    }
}

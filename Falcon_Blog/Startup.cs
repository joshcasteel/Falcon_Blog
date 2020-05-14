using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Falcon_Blog.Startup))]
namespace Falcon_Blog
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

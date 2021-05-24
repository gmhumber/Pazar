using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Pazar.Startup))]
namespace Pazar
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

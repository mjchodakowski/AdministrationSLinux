using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Test_2.Startup))]
namespace Test_2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

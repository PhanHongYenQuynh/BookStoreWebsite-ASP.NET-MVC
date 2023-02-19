using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BookGrotto.Startup))]
namespace BookGrotto
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

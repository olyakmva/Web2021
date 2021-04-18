using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(BookShop2021.Areas.Identity.IdentityHostingStartup))]
namespace BookShop2021.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}
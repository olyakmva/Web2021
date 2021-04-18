using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BookShop2021
{
    
    public class EmailService: IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            
        }
    }
}

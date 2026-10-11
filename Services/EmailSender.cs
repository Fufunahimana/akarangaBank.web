using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; 
using AkarangaBank.Web.Data; 
using AkarangaBank.Web.Models; 
using AkarangaBank.Web.ViewModels; 
 

namespace AkarangaBank.Web.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Ne fait rien (évite l'erreur d'injection de dépendance)
            return Task.CompletedTask;
        }
    }
}
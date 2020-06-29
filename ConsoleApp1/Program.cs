using RazorTemplatEngine;
using RazorTemplatEngine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal static class Program
    {
        private const string TemplatesFolder = "Templates";
        
        public static async Task Main(string[] args)
        {
            var enterpeiseSystem = "Build Orbit";
            var application = "Marketing Application";
            var title = "Load Balanced Virtual Machines";
            var jobStart = new JobStart(title, enterpeiseSystem, application,
                new List<VirtualMachine>
                {
                    new VirtualMachine("A12345678", 90d, 27d),
                    new VirtualMachine("B24681012", 84d, 15.8d),
                    new VirtualMachine("C135791113", 47d, 63d)
                });

            var razorTemplateEngine = new RazorTemplateEngineV2();
            var htmlContent = await razorTemplateEngine.RenderTemplateAsync(jobStart);
            
            var tempFile = Path.Combine(Path.GetTempPath(), "temp.html");
            await File.WriteAllTextAsync(tempFile, htmlContent);
            Process.Start(@"cmd.exe ", $@"/c {tempFile}");

            ////SendEmail(htmlContent);
        }

        private static void SendEmail(string htmlBody)
        {
            var fromAddress = new MailAddress("<From Email Address>", "<Display Name>");
            var toAddress = new MailAddress("<To Email Address>", "<Display Name>");
            var mailMessage = new MailMessage(fromAddress, toAddress);
            mailMessage.Subject = "Testing Razor Template Engine";
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = htmlBody;

            var smtpClient = new SmtpClient("smtp.office365.com", 587);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.Credentials = new System.Net.NetworkCredential("<Your Username>", "<Your Password>");
            smtpClient.EnableSsl = true;
            smtpClient.Send(mailMessage);
        }
    }
}

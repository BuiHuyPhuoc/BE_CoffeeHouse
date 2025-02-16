﻿using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity.UI.Services;
using FirebaseAdmin.Messaging;

namespace CoffeeHouseAPI.Services.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public EmailSender(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(List<string> recipients, string subject, string message, List<string>? cc = null, List<string>? bcc = null)
        {
            using var mailMessage = new MailMessage()
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            foreach (var recipient in recipients)
            {
                mailMessage.To.Add(recipient);
            }

            if (cc != null)
            {
                foreach (var item in cc)
                {
                    mailMessage.CC.Add(item);
                }
            }

            if (bcc != null)
            {
                foreach (var item in bcc)
                {
                    mailMessage.Bcc.Add(item);
                }
            }

            using (var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port))
            {
                smtpClient.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
                smtpClient.EnableSsl = true;
                await smtpClient.SendMailAsync(mailMessage);
            }

        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var mailMessage = new MailMessage()
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            using (var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port))
            {
                smtpClient.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
                smtpClient.EnableSsl = true;
                await smtpClient.SendMailAsync(mailMessage);
            }
        }
    }
}

﻿using System.Threading.Tasks;

namespace CET322.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}

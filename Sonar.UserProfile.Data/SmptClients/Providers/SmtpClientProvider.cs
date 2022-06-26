﻿using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using Sonar.UserProfile.Core.Domain.SmtpClients.Providers;
using Microsoft.Extensions.Configuration;

namespace Sonar.UserProfile.Data.SmptClients.Providers;

public class SmtpClientProvider : ISmtpClientProvider
{
    private readonly SmtpClient _smtpClient;
    private static bool _isMailSent;

    public SmtpClientProvider(IConfiguration configuration)
    {
        _smtpClient = new SmtpClient
        {
            UseDefaultCredentials = false,
            EnableSsl = true,
            Host = configuration["SmtpHost"],
            Port = Convert.ToInt32(configuration["SmtpPort"]),
            Credentials = new NetworkCredential(configuration["SmtpNoReplyMail"], configuration["SmtpNoReplyMailPassword"]),
        };
    }

    public async Task<bool> SendEmailAsync(MailMessage mailMessage, string userState)
    {
        _isMailSent = false;
        _smtpClient.SendCompleted += SendCompletedCallback;

        _smtpClient.SendAsync(mailMessage, userState);

        if (_isMailSent == false)
        {
            _smtpClient.SendAsyncCancel();
        }

        mailMessage.Dispose();
        return _isMailSent;
    }


    private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
    {
        // TODO: make this a log?
        var token = (string)e.UserState;

        if (e.Cancelled)
        {
            Console.WriteLine("[{0}] Send canceled.", token);
        }

        else if (e.Error is not null)
        {
            Console.WriteLine("[{0}] {1}", token, e.Error.ToString());
        }
        
        else
        {
            Console.WriteLine($"[{e.UserState}] Message sent.");
        }

        _isMailSent = true;
    }
}
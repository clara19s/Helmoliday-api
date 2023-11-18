﻿using HELMoliday.Options;
using MimeKit;

namespace HELMoliday.Services.Email;
public interface IEmailSender
{
    Task SendEmailAsync(Message message);
}

public class Message
{
    public List<MessageAddress> To { get; set; }
    public string Subject { get; set; }
    public string Content { get; set; }
   
    
}
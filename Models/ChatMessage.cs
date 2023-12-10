﻿namespace HELMoliday.Models;
public class ChatMessage
{
    public Guid Id { get; set; }
    public string User { get; set; }
    public Guid UserId { get; set; }
    public Holiday Holiday { get; set; }
    public virtual Guid HolidayId { get; set; }
    public string Content { get; set; }
    public DateTime SentAt { get; set; }
    public List<string> Images { get; set; }
}
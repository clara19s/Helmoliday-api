namespace HELMoliday.Services.Calendar
{
    public interface ICalendarService
    {
        string CreateIcs(List<IEvent> events);
    }
}

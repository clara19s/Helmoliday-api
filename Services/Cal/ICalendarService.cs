namespace HELMoliday.Services.Cal
{
    public interface ICalendarService
    {
        string CreateIcs(List<IEvent> events);
    }
}

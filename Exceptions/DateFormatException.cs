using HELMoliday.Filters;

namespace HELMoliday.Exceptions
{
    public class DateFormatException : HttpResponseException
    {
        public DateFormatException() : base(400, "La chaîne de caractères n'est pas au format valide (yyyy-MM-dd HH:ss).") { }
    }
}
using HELMoliday.Exceptions;

namespace HELMoliday.Helpers
{
    public class DateConverter
    {
        public static DateTime ConvertStringToDate(string dateString)
        {
            if (DateTime.TryParseExact(
            dateString, "yyyy-MM-dd HH:mm",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out DateTime date)
            )
            {
                return date;
            }
            else
            {
                throw new DateFormatException();
            }
        }
    }
}

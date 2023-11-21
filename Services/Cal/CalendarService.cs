using HELMoliday.Services.Cal;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace HELMoliday.Services.Cal
{
    public class CalendarService : ICalendarService 
    {

        public string CreateIcs(List<IEvent> events)
        {
            var calendar = new Calendar();

            foreach (IEvent evt in events){

                var calendarEvent = new CalendarEvent
                {
                    Description = evt.Description,
                    Summary = evt.Name,
                    Start = new CalDateTime(evt.StartDate.UtcDateTime),
                    End = new CalDateTime(evt.EndDate.UtcDateTime)
                };
                calendar.Events.Add(calendarEvent);
            }

            var serializer = new CalendarSerializer();
            return serializer.SerializeToString(calendar);
            

           
        }
    }
}

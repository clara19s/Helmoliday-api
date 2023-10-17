using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HELMoliday.Data;
using HELMoliday.Models;
using HELMoliday.Contracts.Holiday;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using HELMoliday.Helpers;
using HELMoliday.Contracts.Common;
using HELMoliday.Services.Weather;

namespace HELMoliday.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HolidaysController : ControllerBase
    {
        private readonly HELMolidayContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IWeatherService _weatherService;

        public HolidaysController(HELMolidayContext context, UserManager<User> userManager, IWeatherService weatherService)
        {
            _context = context;
            _userManager = userManager;
            _weatherService = weatherService;
        }

        // GET: api/Holidays
        [Route("published")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HolidayResponse>>> GetHolidays([FromQuery] HolidayFilter? filter)
        {
            if (_context.Holidays == null)
            {
                return NotFound();
            }
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var holidays = _context.Holidays.Include(h => h.Inviters).Include(h => h.Unfoldings)
            .Where(h => h.Published)
            .AsQueryable();

            if (filter is not null)
            {
                if (string.IsNullOrEmpty(filter.query))
                {
                    holidays = holidays.Where(h => h.Name.Contains(filter.query) || h.Description.Contains(filter.query));
                }
                if (!string.IsNullOrEmpty(filter.StartDate))
                {
                    holidays = holidays.Where(h => h.StartDate >= DateConverter.ConvertStringToDate(filter.StartDate));
                }
                if (!string.IsNullOrEmpty(filter.EndDate))
                {
                    holidays = holidays.Where(h => h.StartDate >= DateConverter.ConvertStringToDate(filter.EndDate));
                }
            }

            var holidaysDto = holidays.ToList().Select(holiday =>
           {
               var listGuests = holiday.Inviters.Select(i => i.UserId.ToString());
               var listActivities = holiday.Unfoldings.Select(u => u.ActivityId.ToString());
               var holidayResponse = new HolidayResponse(

                   holiday.Name,
                    holiday.Description,
                    holiday.StartDate.ToString(),
                    holiday.EndDate.ToString(),
                    AddressConverter.CreateFromModel(holiday.Address),
                    holiday.Published,
                    listGuests,
                    listActivities
               );
               return holidayResponse;
           });

            return Ok(holidaysDto);
        }

        // GET: api/Holidays
        [Route("invited")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HolidayResponse>>> GetMyHolidays([FromQuery] HolidayFilter? filter)
        {
            if (_context.Holidays == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(HttpContext.User);
            var holidays = _context.Holidays.Include(h => h.Inviters).Include(h => h.Unfoldings)
            .Where(h => h.Inviters.Any(i => i.UserId == user.Id))
            .AsQueryable();

            if (filter is not null)
            {
                if (string.IsNullOrEmpty(filter.query))
                {
                    holidays = holidays.Where(h => h.Name.Contains(filter.query) || h.Description.Contains(filter.query));
                }
                if (!string.IsNullOrEmpty(filter.StartDate))
                {
                    holidays = holidays.Where(h => h.StartDate >= DateConverter.ConvertStringToDate(filter.StartDate));
                }
                if (!string.IsNullOrEmpty(filter.EndDate))
                {
                    holidays = holidays.Where(h => h.StartDate >= DateConverter.ConvertStringToDate(filter.EndDate));
                }
            }

            var holidaysDto = holidays.ToList().Select(holiday =>
            {
                var listGuests = holiday.Inviters.Select(i => i.UserId.ToString());
                var listActivities = holiday.Unfoldings.Select(u => u.ActivityId.ToString());
                var holidayResponse = new HolidayResponse(

                    holiday.Name,
                     holiday.Description,
                     holiday.StartDate.ToString(),
                     holiday.EndDate.ToString(),
                     AddressConverter.CreateFromModel(holiday.Address),
                     holiday.Published,
                     listGuests,
                     listActivities
                );
                return holidayResponse;
            });

            return Ok(holidaysDto);
        }

        // GET: api/Holidays/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HolidayResponse>> GetHoliday(Guid id)
        {
            if (_context.Holidays == null)
            {
                return NotFound();
            }
            var holiday = await _context.Holidays.Include(h => h.Inviters).Include(h => h.Unfoldings).Where(h => h.Id == id).FirstOrDefaultAsync();

            if (holiday == null)
            {
                return NotFound();
            }
            var listGuests = holiday.Inviters.Select(i => i.UserId.ToString());
            var listActivities = holiday.Unfoldings.Select(u => u.ActivityId.ToString());
            var holidayResponse = new HolidayResponse(

                holiday.Name,
                 holiday.Description,
                 holiday.StartDate.ToString(),
                 holiday.EndDate.ToString(),
                 AddressConverter.CreateFromModel(holiday.Address),
                 holiday.Published,
                 listGuests,
                 listActivities
            );
            return holidayResponse;
        }

        // GET: api/Holidays/5
        [HttpGet("{id}/weather")]
        public async Task<ActionResult<HolidayResponse>> GetHolidayWeather(Guid id)
    
        {
            var holiday = _context.Holidays.Where(h=> h.Id == id).FirstOrDefault();
            var city = holiday.Address.City;
            var weather = await _weatherService.GetWeatherForCityAsync(city);
            return weather is null ? NotFound() : Ok(weather);

        }

        // PUT: api/Holidays/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
       
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHoliday(Guid id, HolidayRequest holiday)
        {
            if (holiday == null)
            {
                return NotFound(new { error = "Holiday not found." });
            }

            var holidayBd = await _context.Holidays.FindAsync(id);
            holidayBd.Description = holiday.Description;
            holidayBd.Name = holiday.Name;
            holidayBd.Address = AddressConverter.CreateFromDto(holiday.Address);
            holidayBd.StartDate = DateConverter.ConvertStringToDate(holiday.StartDate);
            holidayBd.EndDate = DateConverter.ConvertStringToDate(holiday.EndDate);
            holidayBd.Published = holiday.published;


            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Holidays
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Holiday>> PostHoliday(HolidayRequest holidayDto)
        {
            if (_context.Holidays == null)
            {
                return Problem("Entity set 'HELMolidayContext.Holidays'  is null.");
            }
            var holiday = new Holiday
            {
                Name = holidayDto.Name,
                Description = holidayDto.Description,
                StartDate = DateConverter.ConvertStringToDate(holidayDto.StartDate),
                EndDate = DateConverter.ConvertStringToDate(holidayDto.EndDate),
                Address = AddressConverter.CreateFromDto(holidayDto.Address)


            };
            _context.Holidays.Add(holiday);
            await _context.SaveChangesAsync();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var invitation = new Invitation
            {
                UserId = user.Id,
                HolidayId = holiday.Id,
                Accepted = true

            };
            holiday.Inviters.Add(invitation);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Holidays/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHoliday(Guid id)
        {
            if (_context.Holidays == null)
            {
                return NotFound();
            }
            var holiday = await _context.Holidays.FindAsync(id);
            if (holiday == null)
            {
                return NotFound();
            }

            _context.Holidays.Remove(holiday);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool HolidayExists(Guid id)
        {
            return (_context.Holidays?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

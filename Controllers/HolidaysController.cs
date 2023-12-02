using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HELMoliday.Data;
using HELMoliday.Models;
using HELMoliday.Contracts.Holiday;
using Microsoft.AspNetCore.Identity;
using HELMoliday.Helpers;
using HELMoliday.Contracts.Common;
using HELMoliday.Services.Weather;
using HELMoliday.Contracts.User;
using PusherServer;
using HELMoliday.Services.Cal;
using System.Text;
using HELMoliday.Exceptions;

namespace HELMoliday.Controllers;
[Route("holidays")]
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

        var holidaysQuery = _context.Holidays
            .Include(h => h.Invitations)
                .ThenInclude(i => i.User)
            .Include(h => h.Activities)
            .Where(h => h.Published)
            .AsQueryable();

        if (filter is not null)
        {
            if (!string.IsNullOrEmpty(filter.Query))
            {
                holidaysQuery = holidaysQuery.Where(h => h.Name.Contains(filter.Query) || h.Description.Contains(filter.Query));
            }
            if (!string.IsNullOrEmpty(filter.StartDate))
            {
                holidaysQuery = holidaysQuery.Where(h => h.StartDate >= DateConverter.ConvertStringToDate(filter.StartDate));
            }
            if (!string.IsNullOrEmpty(filter.EndDate))
            {
                holidaysQuery = holidaysQuery.Where(h => h.StartDate <= DateConverter.ConvertStringToDate(filter.EndDate));
            }
        }

        var holidays = await holidaysQuery.ToListAsync();

        var holidaysDto = holidays.Select(holiday =>
        {
            var listGuests = holiday.Invitations.Select(i => i.User).ToList();
            var listActivities = holiday.Activities.Select(u => u.Id.ToString());
            var holidayResponse = new HolidayResponse(
                holiday.Id,
                holiday.Name,
                holiday.Description,
                holiday.StartDate.ToString("yyyy-MM-dd HH:mm"),
                holiday.EndDate.ToString("yyyy-MM-dd HH:mm"),
                AddressConverter.CreateFromModel(holiday.Address),
                holiday.Published,
                listGuests.Select(g => new GuestResponse(g.Id, g.FirstName, g.LastName)),
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
        var holidaysQuery = _context.Holidays
            .Include(h => h.Invitations)
                .ThenInclude(i => i.User)
            .Include(h => h.Activities)
            .Where(h => h.Invitations.Any(i => i.UserId == user.Id))
            .AsQueryable();

        if (filter is not null)
        {
            if (!string.IsNullOrEmpty(filter.Query))
            {
                holidaysQuery = holidaysQuery.Where(h => h.Name.Contains(filter.Query) || h.Description.Contains(filter.Query));
            }
            if (!string.IsNullOrEmpty(filter.StartDate))
            {
                holidaysQuery = holidaysQuery.Where(h => h.StartDate >= DateConverter.ConvertStringToDate(filter.StartDate));
            }
            if (!string.IsNullOrEmpty(filter.EndDate))
            {
                holidaysQuery = holidaysQuery.Where(h => h.StartDate <= DateConverter.ConvertStringToDate(filter.EndDate));
            }
        }

        var holidays = await holidaysQuery.ToListAsync();

        var holidaysDto = holidays.Select(holiday =>
        {
            var listGuests = holiday.Invitations.Select(i => i.User).ToList();
            var listActivities = holiday.Activities.Select(u => u.Id.ToString());
            var holidayResponse = new HolidayResponse(
                holiday.Id,
                holiday.Name,
                holiday.Description,
                holiday.StartDate.ToString("yyyy-MM-dd HH:mm"),
                holiday.EndDate.ToString("yyyy-MM-dd HH:mm"),
                AddressConverter.CreateFromModel(holiday.Address),
                holiday.Published,
                listGuests.Select(g => new GuestResponse(g.Id, g.FirstName, g.LastName)),
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
        var holiday = GetHolidayIfExist(id,
            query => query.Include(h => h.Invitations).ThenInclude(i => i.User),
            query => query.Include(h => h.Activities));

        CheckIfIsGuest(holiday);

        var listGuests = holiday.Invitations.Select(i => i.User).ToList();
        var listActivities = holiday.Activities.Select(u => u.Id.ToString());
        var holidayResponse = new HolidayResponse(
            holiday.Id,
            holiday.Name,
            holiday.Description,
            holiday.StartDate.ToString("yyyy-MM-dd HH:mm"),
            holiday.EndDate.ToString("yyyy-MM-dd HH:mm"),
            AddressConverter.CreateFromModel(holiday.Address),
            holiday.Published,
            listGuests.Select(g => new GuestResponse(
                g.Id, g.FirstName, g.LastName
            )),
            listActivities
        );

        return Ok(holidayResponse);
    }

    // GET: api/calendar
    [HttpGet("{id}/calendar")]
    public async Task<ActionResult> GetCalendar(Guid id, [FromServices] ICalendarService calendarService)
    {
       
            var holiday = _context.Holidays.Where(h => h.Id == id).Include(h => h.Activities).FirstOrDefault();

            var events = new List<IEvent>();

            events.Add(holiday);
            foreach (var activity in holiday.Activities)
            {
                events.Add(activity);
            }

            var calendar = calendarService.CreateIcs(events);
            byte[] data = Encoding.UTF8.GetBytes(calendar);
            return File(data, "text/calendar", "event.ics");
       
            }

    // GET: api/Holidays/5
    [HttpGet("{id}/weather")]
    public async Task<ActionResult<HolidayResponse>> GetHolidayWeather(Guid id)
    {
        try
        {
            var holiday = GetHolidayIfExist(id,
                query => query.Include(h => h.Invitations));
            CheckIfIsGuest(holiday);
            var city = holiday.Address.City;
            var weather = await _weatherService.GetWeatherForCityAsync(city);
            return weather is null ? NotFound("Aucune donnée météorologique n'a été trouvée pour cette période de vacances") : Ok(weather);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    // PUT: api/Holidays/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

    [HttpPut("{id}")]
    public async Task<IActionResult> PutHoliday(Guid id, HolidayRequest holiday)
    {
        var holidayBd = GetHolidayIfExist(id,
            query => query.Include(h => h.Invitations));
        CheckIfIsGuest(holidayBd);

        holidayBd.Description = holiday.Description;
        holidayBd.Name = holiday.Name;
        holidayBd.Address = AddressConverter.CreateFromDto(holiday.Address);
        holidayBd.StartDate = DateConverter.ConvertStringToDate(holiday.StartDate);
        holidayBd.EndDate = DateConverter.ConvertStringToDate(holiday.EndDate);
        holidayBd.Published = holiday.Published;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // POST: api/Holidays
    [HttpPost]
    public async Task<ActionResult<Holiday>> PostHoliday(HolidayRequest holidayDto)
    {
        if (_context.Holidays == null)
        {
            return NotFound();
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
            HolidayId = holiday.Id
        };
        holiday.Invitations.Add(invitation);

        await _context.SaveChangesAsync();

        return Created("GetHoliday", new { id = holiday.Id });
    }

    // DELETE: api/Holidays/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHoliday(Guid id)
    {
        var holiday = GetHolidayIfExist(id,
            query => query.Include(h => h.Invitations));
        CheckIfIsGuest(holiday);

        _context.Holidays.Remove(holiday);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/chat/auth")]
    public async Task<ActionResult> ChatAuthentication([FromRoute] Guid id, [FromBody] ChatAuthRequest authRequest)
    {
        var holiday = GetHolidayIfExist(id,
            query => query.Include(h => h.Invitations));
        CheckIfIsGuest(holiday);
        var user = await _userManager.GetUserAsync(HttpContext.User);
        var options = new PusherOptions
        {
            Cluster = "eu",
            Encrypted = false
        };

        // TODO: Get from config
        var pusher = new Pusher(
          "1700454",
          "c79fa94e85416eeb4f1e",
          "bca1b2adb1b72d81f3f3",
          options);

        var channelData = new PresenceChannelData()
        {
            user_id = user.Id.ToString(),
            user_info = new
            {
                name = $"{user.FirstName} {user.LastName}",
                avatar = "https://picsum.photos/200"
            }
        };

        var auth = pusher.Authenticate(authRequest.ChannelName, authRequest.SocketId, channelData);
        var json = auth.ToJson();
        return new ContentResult { Content = json, ContentType = "application/json" };
    }

    [HttpPost("{id}/chat/messages")]
    public async Task<ActionResult> SendMessage([FromRoute] Guid id, [FromBody] ChatMessageRequest request)
    {
        var holiday = GetHolidayIfExist(id,
            query => query.Include(h => h.Invitations));
        CheckIfIsGuest(holiday);
        var user = await _userManager.GetUserAsync(HttpContext.User);
        var options = new PusherOptions
        {
            Cluster = "eu",
            Encrypted = false
        };

        // TODO: Get from config
        var pusher = new Pusher(
          "1700454",
          "c79fa94e85416eeb4f1e",
          "bca1b2adb1b72d81f3f3",
          options);

        await pusher.TriggerAsync(
            channelName: $"presence-{id}",
            eventName: "message",
            data: new
            {
                sentAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                data = new
                {
                    clientId = request.ClientId,
                    text = request.Text,
                    images = Array.Empty<string>()
                },
                from = new
                {
                    id = user.Id.ToString(),
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    email = user.Email
                }
            });
        return Ok();
    }

    private void CheckIfIsGuest(Holiday holiday)
    {
        var userId = Guid.Parse(_userManager.GetUserId(HttpContext.User));
        if (!holiday.Invitations.Any(i => i.UserId == userId))
        {
            throw new ForbiddenAccessException("Vous ne faites pas partie de la période de vacances.");
        }
    }

    private Holiday GetHolidayIfExist(Guid id, params Func<IQueryable<Holiday>, IQueryable<Holiday>>[] queryExpressions)
    {
        if (_context.Holidays == null)
        {
            throw new NotFoundException("Période de vacances non trouvée.");
        }

        IQueryable<Holiday> query = _context.Holidays;

        foreach (var queryExpression in queryExpressions)
        {
            if (queryExpression != null)
            {
                query = queryExpression(query);
            }
        }

        var holiday = query.FirstOrDefault(h => h.Id == id);

        return holiday ?? throw new NotFoundException("Période de vacances non trouvée.");
    }
}
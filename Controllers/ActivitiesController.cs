using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HELMoliday.Data;
using HELMoliday.Models;
using HELMoliday.Contracts.Activity;
using HELMoliday.Contracts.Common;
using HELMoliday.Helpers;
using Activity = HELMoliday.Models.Activity;
using HELMoliday.Services.Weather;
using HELMoliday.Contracts.Weather;

namespace HELMoliday.Controllers
{
    [Route("activities")]
    [ApiController]
    public class ActivitiesController : ControllerBase
    {
        private readonly HELMolidayContext _context;

        public ActivitiesController(HELMolidayContext context)
        {
            _context = context;
        }

        // GET: api/Activities
        [HttpGet("holiday/{holidayId}")]
        public async Task<ActionResult<IEnumerable<Activity>>> GetActivitiesFromHoliday(Guid holidayId)
        {
            if (_context.Activities == null)
            {
                return NotFound();
            }

            var holiday = await _context.Holidays
                .Include(h => h.Unfoldings)
                .ThenInclude(u => u.Activity)
                .FirstOrDefaultAsync(h => h.Id == holidayId);

            if (holiday == null)
            {
                return NotFound(new { error = "Holiday not found." });
            }

            return Ok(holiday.Unfoldings
                .Select(u => u.Activity)
                .Select(a => new ActivityResponse(
                    a.Id,
                    a.Name,
                    a.Description,
                    a.StartDate.ToString(),
                    a.EndDate.ToString(),
                    AddressConverter.CreateFromModel(a.Address)))
                .ToList());
        }

        // GET: api/Activities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ActivityResponse>> GetActivity(Guid id)
        {
            if (_context.Activities == null)
            {
                return NotFound();
            }
            var activity = await _context.Activities.FindAsync(id);

            if (activity == null)
            {
                return NotFound();
            }

            return new ActivityResponse(
                activity.Id,
                activity.Name,
                activity.Description,
                activity.StartDate.ToString(),
                activity.EndDate.ToString(),
                AddressConverter.CreateFromModel(activity.Address));
        }

        [HttpGet("{id}/weather")]
        public async Task<ActionResult<WeatherResponse>> GetActivityWeather([FromServices] IWeatherService weatherService, [FromRoute] Guid id)
        {
            if (_context.Activities == null)
                return NotFound();

            var activity = _context.Activities.Find(id);

            if (activity == null)
                return NotFound();

            var weather = await weatherService.GetWeatherForCityAsync(activity.Address.City);
            return weather is null ? NotFound() : Ok(weather);
        }

        // PUT: api/Activities/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutActivity(Guid id, UpsertActivityRequest activityDto)
        {
            var activity = await _context.Activities.FindAsync(id);

            if (activity == null)
            {
                return NotFound();
            }

            activity.Name = activityDto.Name;
            activity.Description = activityDto.Description;
            activity.StartDate = DateConverter.ConvertStringToDate(activityDto.StartDate);
            activity.StartDate = DateConverter.ConvertStringToDate(activityDto.EndDate);
            activity.Address = AddressConverter.CreateFromDto(activityDto.Address);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Activities
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("holiday/{holidayId}")]
        public async Task<ActionResult<ActivityResponse>> PostActivity([FromRoute] Guid holidayId, [FromBody] UpsertActivityRequest activityDto)
        {
            // Vérifiez si la période de vacances existe
            var holiday = await _context.Holidays.FirstOrDefaultAsync(h => h.Id == holidayId);
            if (holiday == null)
            {
                return NotFound(new {error = "Période de vacances non trouvée." });
            }

            // Création de l'activité
            var activity = new Activity
            {
                Name = activityDto.Name,
                Description = activityDto.Description,
                StartDate = DateConverter.ConvertStringToDate(activityDto.StartDate),
                EndDate = DateConverter.ConvertStringToDate(activityDto.EndDate),
                Address = AddressConverter.CreateFromDto(activityDto.Address)
            };

            _context.Activities.Add(activity);

            // Créez un nouvel objet Unfolding pour établir la relation entre l'activité et la période de vacances
            var unfolding = new Unfolding
            {
                Activity = activity,
                Holiday = holiday
            };

            // Ajoutez l'objet Unfolding à la base de données
            _context.Unfoldings.Add(unfolding);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Activities/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActivity(Guid id)
        {
            if (_context.Activities == null)
            {
                return NotFound();
            }
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null)
            {
                return NotFound();
            }

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

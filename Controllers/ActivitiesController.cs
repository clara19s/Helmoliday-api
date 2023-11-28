﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HELMoliday.Data;
using HELMoliday.Contracts.Activity;
using HELMoliday.Contracts.Common;
using HELMoliday.Helpers;
using Activity = HELMoliday.Models.Activity;
using HELMoliday.Services.Weather;
using HELMoliday.Contracts.Weather;
using HELMoliday.Models;
using Microsoft.AspNetCore.Identity;
using HELMoliday.Exceptions;

namespace HELMoliday.Controllers;
[Route("activities")]
[ApiController]
public class ActivitiesController : ControllerBase
{
    private readonly HELMolidayContext _context;
    private readonly UserManager<User> _userManager;

    public ActivitiesController(HELMolidayContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
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
            .Include(h => h.Activities)
            .Include(h => h.Invitations)
            .FirstOrDefaultAsync(h => h.Id == holidayId);

        if (holiday == null)
        {
            return NotFound(new { error = "Holiday not found." });
        }

        CheckIfIsGuest(holiday);

        return Ok(holiday.Activities.Select(a => new ActivityResponse(
                a.Id,
                a.Name,
                a.Description,
                a.StartDate.ToString("yyyy-MM-dd HH:ss"),
                a.EndDate.ToString("yyyy-MM-dd HH:ss"),
                AddressConverter.CreateFromModel(a.Address),
                a.Category.ToString() 
                ))
            .ToList()); ;
    }

    // GET: api/Activities/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ActivityResponse>> GetActivity(Guid id)
    {
        if (_context.Activities == null)
        {
            return NotFound();
        }
        var activity = await _context.Activities.Include(a => a.Holiday).ThenInclude(h => h.Invitations).Where(a => a.Id == id).FirstOrDefaultAsync();

        if (activity == null)
        {
            return NotFound();
        }

        CheckIfIsGuest(activity.Holiday);

        return new ActivityResponse(
            activity.Id,
            activity.Name,
            activity.Description,
            activity.StartDate.ToString("yyyy-MM-dd HH:ss"),
            activity.EndDate.ToString("yyyy-MM-dd HH:ss"),
            AddressConverter.CreateFromModel(activity.Address),
            activity.Category.ToString()
            );
    }

    [HttpGet("{id}/weather")]
    public async Task<ActionResult<WeatherResponse>> GetActivityWeather([FromServices] IWeatherService weatherService, [FromRoute] Guid id)
    {
        try
        {
            if (_context.Activities == null)
                return NotFound();

            var activity = _context.Activities.Find(id);

            if (activity == null)
                return NotFound();

            var weather = await weatherService.GetWeatherForCityAsync(activity.Address.City);
            return weather is null ? NotFound() : Ok(weather);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    // PUT: api/Activities/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutActivity(Guid id, UpsertActivityRequest activityDto)
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

        activity.Name = activityDto.Name;
        activity.Description = activityDto.Description;
        activity.StartDate = DateConverter.ConvertStringToDate(activityDto.StartDate);
        activity.EndDate = DateConverter.ConvertStringToDate(activityDto.EndDate);
        activity.Address = AddressConverter.CreateFromDto(activityDto.Address);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // POST: api/Activities
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost("holiday/{holidayId}")]
    public async Task<ActionResult<ActivityResponse>> PostActivity([FromRoute] Guid holidayId, [FromBody] UpsertActivityRequest activityDto)
    {
        var holiday = await _context.Holidays.FindAsync(holidayId);

        if (holiday == null)
        {
            return NotFound();
        }

        CheckIfIsGuest(holiday);

        var activity = new Activity
        {
            Name = activityDto.Name,
            Description = activityDto.Description,
            StartDate = DateConverter.ConvertStringToDate(activityDto.StartDate),
            EndDate = DateConverter.ConvertStringToDate(activityDto.EndDate),
            Address = AddressConverter.CreateFromDto(activityDto.Address)
        };

        holiday.Activities.Add(activity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetActivity), new { id = activity.Id }, new ActivityResponse(
            activity.Id,
            activity.Name,
            activity.Description,
            activity.StartDate.ToString(),
            activity.EndDate.ToString(),
            AddressConverter.CreateFromModel(activity.Address),
            activity.Category.ToString()
            ));
    }

    // DELETE: api/Activities/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteActivity(Guid id)
    {
        if (_context.Activities == null)
        {
            return NotFound();
        }
        var activity = await _context.Activities.Include(a => a.Holiday).ThenInclude(h => h.Invitations).Where(a => a.Id == id).FirstOrDefaultAsync();
        if (activity == null)
        {
            return NotFound();
        }

        CheckIfIsGuest(activity.Holiday);

        _context.Activities.Remove(activity);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    private void CheckIfIsGuest(Holiday holiday)
    {
        var userId = Guid.Parse(_userManager.GetUserId(HttpContext.User));
        if (!holiday.Invitations.Any(i => i.UserId == userId))
        {
            throw new ForbiddenAccessException("Vous ne faites pas partie de la période de vacances.");
        }
    }
}

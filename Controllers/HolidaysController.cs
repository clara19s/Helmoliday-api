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
using HELMoliday.Services.ImageUpload;

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

    /// <summary>
    /// Récupère l'ensemble des périodes de vacances publiées.
    /// </summary>
    /// <returns>Un tableau de périodes de vacances qui sont publiées.</returns>
    /// <response code="200">Retourne un tableau de périodes de vacances.</response>
    /// <response code="404">Aucune période de vacances n'a été trouvée.</response>
    [Route("published")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<HolidayResponse>>> GetHolidays()
    {
        if (_context.Holidays == null)
        {
            return NotFound();
        }
        var user = await _userManager.GetUserAsync(HttpContext.User);

        var holidays = await _context.Holidays
            .Include(h => h.Invitations)
                .ThenInclude(i => i.User)
            .Include(h => h.Activities)
            .Where(h => h.Published).ToListAsync();

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
                listGuests.Select(g => new GuestResponse(g.Id, g.FirstName, g.LastName, ConvertToUrl(g.ProfilePicture))),
                listActivities
            );
            return holidayResponse;
        });

        return Ok(holidaysDto);
    }

    /// <summary>
    /// Récupère l'ensemble des périodes de vacances auxquelles l'utilisateur participe.
    /// </summary>
    /// <returns>Un tableau de périodes de vacances auxquelles l'utilisateur participe.</returns>
    /// <response code="200">Retourne un tableau de périodes de vacances.</response>
    /// <response code="404">Aucune période de vacances n'a été trouvée.</response>
    [Route("invited")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<HolidayResponse>>> GetMyHolidays()
    {
        if (_context.Holidays == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(HttpContext.User);
        var holidays = await _context.Holidays
            .Include(h => h.Invitations)
                .ThenInclude(i => i.User)
            .Include(h => h.Activities)
            .Where(h => h.Invitations.Any(i => i.UserId == user.Id))
            .ToListAsync();

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
                listGuests.Select(g => new GuestResponse(g.Id, g.FirstName, g.LastName, ConvertToUrl(g.ProfilePicture))),
                listActivities
            );
            return holidayResponse;
        });

        return Ok(holidaysDto);
    }

    /// <summary>
    /// Récupère une période de vacances en fonction de son identifiant.
    /// </summary>
    /// <param name="id">L'identifiant unique de la période de vacances.</param>
    /// <returns>Les détails de la période de vacances.</returns>
    /// <response code="200">Retourne les détails de la période de vacances.</response>
    /// <response code="404">Aucune période de vacances n'a été trouvée.</response>
    /// <response code="403">Si l'utilisateur n'est pas invité à la période de vacances.</response>
    [HttpGet("{id}")]
    public async Task<ActionResult<HolidayResponse>> GetHoliday(Guid id)
    {
        var holiday = GetHolidayIfExist(id,
            query => query.Include(h => h.Invitations).ThenInclude(i => i.User),
            query => query.Include(h => h.Activities));

        CheckIfIsAllowed(holiday);

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
                g.Id, g.FirstName, g.LastName, ConvertToUrl(g.ProfilePicture)
            )),
            listActivities
        );

        return Ok(holidayResponse);
    }

    /// <summary>
    /// Génère et retourne un fichier .ics contenant les informations de la période de vacances et de ses activités.
    /// </summary>
    /// <param name="id">L'identifiant unique de la période de vacances.</param>
    /// <returns>Un fichier .ics contenant les informations de la période de vacances et de ses activités.</returns>
    /// <response code="200">Retourne un fichier .ics contenant les informations de la période de vacances et de ses activités.</response>
    /// <response code="403">Si l'utilisateur n'est pas invité à la période de vacances.</response>
    /// <response code="404">Aucune période de vacances n'a été trouvée.</response>
    [HttpGet("{id}/calendar")]
    public async Task<ActionResult> GetCalendar(Guid id, [FromServices] ICalendarService calendarService)
    {
        var holiday = _context.Holidays.Where(h => h.Id == id).Include(h => h.Activities).FirstOrDefault();

        if (holiday == null)
        {
            return NotFound();
        }

        CheckIfIsAllowed(holiday);

        var events = new List<IEvent>
        {
            holiday
        };
        foreach (var activity in holiday.Activities)
        {
            events.Add(activity);
        }
        var calendar = calendarService.CreateIcs(events);
        byte[] data = Encoding.UTF8.GetBytes(calendar);
        return File(data, "text/calendar", "event.ics");

    }

    /// <summary>
    /// Récupère les informations météorologiques d'une période de vacances.
    /// </summary>
    /// <param name="id">Identifiant unique de la période de vacances.</param>
    /// <returns>Les données météorologiques d'une période de vacances.</returns>
    /// <response code="200">Retourne les données météorologiques d'une période de vacances.</response>
    /// <response code="400">Une ou plusieurs informations sont manquantes ou invalides.</response>
    /// <response code="403">Si l'utilisateur n'est pas invité à la période de vacances.</response>
    /// <response code="404">Aucune période de vacances n'a été trouvée.</response>
    [HttpGet("{id}/weather")]
    public async Task<ActionResult<HolidayResponse>> GetHolidayWeather(Guid id)
    {
        try
        {
            var holiday = GetHolidayIfExist(id,
                query => query.Include(h => h.Invitations));
            CheckIfIsAllowed(holiday);
            var city = holiday.Address.City;
            var weather = await _weatherService.GetWeatherForCityAsync(city);
            return weather is null ? NotFound("Aucune donnée météorologique n'a été trouvée pour cette période de vacances") : Ok(weather);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    /// <summary>
    /// Met à jour une période de vacances en fonction de son identifiant.
    /// </summary>
    /// <param name="id">L'identifiant unique de la période de vacances.</param>
    /// <param name="holiday">Les détails de la période de vacances.</param>
    /// <returns></returns>
    /// <response code="204">La période de vacances a été mise à jour.</response>
    /// <response code="400">Une ou plusieurs informations sont manquantes ou invalides.</response>
    /// <response code="403">Si l'utilisateur n'est pas invité à la période de vacances.</response>
    /// <response code="404">Aucune période de vacances n'a été trouvée.</response>
    [HttpPut("{id}")]
    public async Task<IActionResult> PutHoliday(Guid id, HolidayRequest holiday)
    {
        var holidayBd = GetHolidayIfExist(id,
            query => query.Include(h => h.Invitations));
        CheckIfIsAllowed(holidayBd);

        holidayBd.Description = holiday.Description;
        holidayBd.Name = holiday.Name;
        holidayBd.Address = AddressConverter.CreateFromDto(holiday.Address);
        holidayBd.StartDate = DateConverter.ConvertStringToDate(holiday.StartDate);
        holidayBd.EndDate = DateConverter.ConvertStringToDate(holiday.EndDate);
        holidayBd.Published = holiday.Published;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Crée une période de vacances.
    /// </summary>
    /// <param name="holidayDto">Les détails d'une période de vacances.</param>
    /// <returns>Une réponse Created avec l'identifiant de la période de vacances.</returns>
    /// <response code="201">La période de vacances a été créée.</response>
    /// <response code="400">Une ou plusieurs informations sont manquantes ou invalides.</response>
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

    /// <summary>
    /// Supprime une période de vacances en fonction de son identifiant.
    /// </summary>
    /// <param name="id">L'identifiant unique de la période de vacances.</param>
    /// <returns></returns>
    /// <response code="204">La période de vacances a été supprimée.</response>
    /// <response code="403">Si l'utilisateur n'est pas invité à la période de vacances.</response>
    /// <response code="404">Aucune période de vacances n'a été trouvée.</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHoliday(Guid id)
    {
        var holiday = GetHolidayIfExist(id,
            query => query.Include(h => h.Invitations));
        CheckIfIsAllowed(holiday);

        _context.Holidays.Remove(holiday);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Permet à l'utilisateur de s'authentifier et de s'abonner à Pusher.
    /// </summary>
    /// <param name="id">L'identifiant unique de la période de vacances.</param>
    /// <param name="authRequest">Objet comprenant le socketId ainsi que le nom du channel auquel se connecter.</param>
    /// <returns>La clé permettant à l'utilisateur de s'authentifier et de s'abonner à Pusher.</returns>
    /// <response code="200">Retourne la clé permettant à l'utilisateur de s'authentifier et de s'abonner à Pusher.</response>
    /// <response code="400">Une ou plusieurs informations sont manquantes ou invalides.</response>
    /// <response code="403">Si l'utilisateur n'est pas invité à la période de vacances.</response>
    /// <response code="404">Aucune période de vacances n'a été trouvée.</response>
    [HttpPost("{id}/chat/auth")]
    public async Task<ActionResult> ChatAuthentication([FromRoute] Guid id, [FromBody] ChatAuthRequest authRequest)
    {
        var holiday = GetHolidayIfExist(id,
            query => query.Include(h => h.Invitations));
        CheckIfIsAllowed(holiday);
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
                name = $"{user.FirstName} {user.LastName}"
            }
        };

        var auth = pusher.Authenticate(authRequest.ChannelName, authRequest.SocketId, channelData);
        var json = auth.ToJson();
        return new ContentResult { Content = json, ContentType = "application/json" };
    }

    /// <summary>
    /// Poste un message dans le chat d'une période de vacances.
    /// </summary>
    /// <param name="id">Identifiant unique de la période de vacances.</param>
    /// <param name="request">Un objet représentant un message</param>
    /// <returns></returns>
    /// <response code="200">Le message a été envoyé.</response>
    /// <response code="400">Une ou plusieurs informations sont manquantes ou invalides.</response>
    /// <response code="403">Si l'utilisateur n'est pas invité à la période de vacances.</response>
    /// <response code="404">Aucune période de vacances n'a été trouvée.</response>
    [HttpPost("{id}/chat/messages")]
    public async Task<ActionResult> SendMessage([FromRoute] Guid id, [FromForm] ChatMessageRequest request, [FromServices] IFileUploadService fileUploadService)
    {
        var holiday = GetHolidayIfExist(id,
            query => query.Include(h => h.Invitations));
        CheckIfIsAllowed(holiday);
        var user = await _userManager.GetUserAsync(HttpContext.User);
        var options = new PusherOptions
        {
            Cluster = "eu",
            Encrypted = false
        };

        var imagesUrl = new List<string>();
        if (request.Images != null)
        {
            foreach (var image in request.Images)
            {
                var imageUrl = await fileUploadService.UploadFileAsync(image, Guid.NewGuid().ToString());
                imagesUrl.Add(ConvertToUrl(imageUrl));
            }
        }

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
                    images = imagesUrl
                },
                from = new
                {
                    id = user.Id.ToString(),
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    email = user.Email,
                    profilePicture = ConvertToUrl(user.ProfilePicture)
                }
            });

        var chatMessage = new ChatMessage
        {
            UserId = user.Id,
            HolidayId = id,
            Content = request.Text,
            SentAt = DateTime.Now,
            Images = imagesUrl.Select(i => new ChatImage { Path = i }).ToList()
        };

        _context.ChatMessages.Add(chatMessage);
        await _context.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// Récupère les 100 derniers messages du chat d'une période de vacances.
    /// </summary>
    /// <param name="id">Identifiant unique de la période de vacances.</param>
    /// <returns>Les 100 derniers messages du chat d'une période de vacances.</returns>
    /// <response code="200">Retourne les 100 derniers messages du chat d'une période de vacances.</response>
    /// <response code="403">Si l'utilisateur n'est pas invité à la période de vacances.</response>
    /// <response code="404">Aucune période de vacances n'a été trouvée.</response>
    [HttpGet("{id}/chat/messages")]
    public async Task<ActionResult> GetMessages([FromRoute] Guid id)
    {
        var holiday = GetHolidayIfExist(id,
                       query => query.Include(h => h.Invitations));
        CheckIfIsAllowed(holiday);

        var chatMessages = await _context.ChatMessages
            .Include(m => m.User)
            .Include(m => m.Images)
            .Where(m => m.HolidayId == id)
            .OrderBy(m => m.SentAt)
            .Take(100)
            .ToListAsync();

        return Ok(chatMessages.Select(m => new
        {
            sentAt = m.SentAt.ToString("yyyy-MM-dd HH:mm"),
            data = new
            {
                clientId = m.Id,
                text = m.Content,
                images = m.Images.Select(i => i.Path)
            },
            from = new
            {
                id = m.UserId.ToString(),
                firstName = m.User.FirstName,
                lastName = m.User.LastName,
                email = m.User.Email,
                profilePicture = ConvertToUrl(m.User.ProfilePicture)
            }
        }));
    }

    private void CheckIfIsAllowed(Holiday holiday)
    {
        var userId = Guid.Parse(_userManager.GetUserId(HttpContext.User));
        if (!holiday.Published && !holiday.Invitations.Any(i => i.UserId == userId))
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

    private string ConvertToUrl(string filePath)
    {
        var protocol = HttpContext.Request.IsHttps ? "https" : "http";
        var domaineName = HttpContext.Request.Host.Value.Contains("localhost") ? HttpContext.Request.Host.Value : "porthos-intra.cg.helmo.be/Q210266";
        return $"{protocol}://{domaineName}{filePath}";
    }
}
using System.ComponentModel.DataAnnotations.Schema;

namespace HELMoliday.Models;

public partial class Unfolding
{
    public Guid ActivityId { get; set; }

    public Guid HolidayId { get; set; }

    public DateTimeOffset? StartDate { get; set; } = null;

    public DateTimeOffset? EndDate { get; set; } = null;

    [ForeignKey("ActivityId")]
    public virtual Activity Activity { get; set; } = null!;

    [ForeignKey("HolidayId")]
    public virtual Holiday Holiday { get; set; } = null!;
}

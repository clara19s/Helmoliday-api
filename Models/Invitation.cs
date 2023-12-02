using System.ComponentModel.DataAnnotations.Schema;

namespace HELMoliday.Models;

public partial class Invitation
{
    public Guid UserId { get; set; }

    public Guid HolidayId { get; set; }

    [ForeignKey("HolidayId")]
    public virtual Holiday Holiday { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}

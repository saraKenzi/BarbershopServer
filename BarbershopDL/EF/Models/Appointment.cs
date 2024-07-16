using System;
using System.Collections.Generic;

namespace BarbershopDL.EF.Models;

public partial class Appointment
{
    public int AppointmentId { get; set; }

    public int? UserId { get; set; }

    public DateTime AppointmentDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual User? User { get; set; }
}

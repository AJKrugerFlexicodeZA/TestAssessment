using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDTIER.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        public string? Message { get; set; }
        public int? Code { get; set; }
        public NotificationType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public int DurationMs { get; set; } = 5000;
        public bool IsTimerStarted { get; set; } = false;

    }

    public enum NotificationType{Success, Error, Warning, Info}
}

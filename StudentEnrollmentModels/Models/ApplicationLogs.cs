using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDTIER.Models
{
    public enum LogLevels
    {
        Info,
        Warning,
        Error,
        Critical
    }
    public class ApplicationLogs
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string TableName { get; set; } = string.Empty;
        public LogLevels? Level { get; set; } = LogLevels.Info;
        public int? UserId { get; set; }
    }
}

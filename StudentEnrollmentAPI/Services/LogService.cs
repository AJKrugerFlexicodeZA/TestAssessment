using MIDTIER.Models;
using StudentEnrollmentAPI.Data;

namespace StudentCourseEnrollments.Services
{
    public class LogService
    {
        private static int id = DataStore.NextLogId;
        private static readonly object _lock = new();

        private static void AddLog(ApplicationLogs log)
        {
            lock (_lock)
            {
                log.Id = id;
                DataStore.ApplicationLogs[log.Id] = log;
            }
        }
        private static void Write(string message, LogLevels level, string tableName, int userId)
        {
            AddLog(new ApplicationLogs
            {
                Message = message,
                Level = level,
                TableName = tableName,
                UserId = userId
            });
        }

        // Easy shortcuts — ALL include UserId
        public static void Info(string message, string tableName, int userId)
            => Write(message, LogLevels.Info, tableName, userId);

        public static void Warn(string message, string tableName, int userId)
            => Write(message, LogLevels.Warning, tableName, userId);

        public static void Error(string message, string tableName, int userId)
            => Write(message, LogLevels.Error, tableName, userId);

        public static void Critical(string message, string tableName, int userId)
            => Write(message, LogLevels.Critical, tableName, userId);

        // With formatted message — still requires userId
        public static void Info(string message, string tableName, int userId, params object[] args)
            => Write(string.Format(message, args), LogLevels.Info, tableName, userId);

        public static void Warn(string message, string tableName, int userId, params object[] args)
            => Write(string.Format(message, args), LogLevels.Warning, tableName, userId);

        public static void Error(string message, string tableName, int userId, params object[] args)
            => Write(string.Format(message, args), LogLevels.Error, tableName, userId);

        public static void Critical(string message, string tableName, int userId, params object[] args)
            => Write(string.Format(message, args), LogLevels.Critical, tableName, userId);

        // Optional: For system actions (no user)
        public static void System(string message, string tableName = "System")
            => Write(message, LogLevels.Info, tableName, userId: 0);
    }
}

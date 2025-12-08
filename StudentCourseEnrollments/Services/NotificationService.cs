using System.Collections.Concurrent;
using MIDTIER.Models;
namespace StudentCourseEnrollments.Services
{
    public class NotificationService
    {
        // Thread-safe queue
        private readonly ConcurrentQueue<Notification> _notifications = new();
        public event Action? OnChange;

        public void Show(string message, int? Code, int durationMs = 4000)
        {
            NotificationType type = Code switch
            {
                int n when (n >= 200 && n < 300) => NotificationType.Success,
                int n when (n >= 400 && n < 500) => NotificationType.Warning,
                int n when (n >= 500) => NotificationType.Error,
                _ => NotificationType.Info
            };
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                Message= message,
                Type= type,
                CreatedAt= DateTime.UtcNow,
                DurationMs= durationMs
            };

            _notifications.Enqueue(notification);
            OnChange?.Invoke();
        }

        public IEnumerable<Notification> GetNotifications() => _notifications;

        public void Remove(Guid id)
        {
            var dummy = new Notification { Id = id, Message= "",
                Type= NotificationType.Success,
                CreatedAt= DateTime.UtcNow,
                DurationMs= 0};
            while (_notifications.TryPeek(out var n) && n.Id == id)
                _notifications.TryDequeue(out _);

            OnChange?.Invoke();
        }
    }

    
}

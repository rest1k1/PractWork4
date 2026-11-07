namespace NotificationLibrary;

public enum NotificationType
{
    Info,
    Warning,
    Error,
    System
}

public enum SortNotificationBy
{
    Date,
    Priority,
    Title
}

public class Notification
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public NotificationType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Priority { get; set; }
    public bool IsRead { get; set; }
}

public class NotificationFilterOptions
{
    public bool? IsRead { get; set; }
    public NotificationType[]? Types { get; set; }
    public string? SearchText { get; set; }
    public int? MinPriority { get; set; }
    public SortNotificationBy? SortBy { get; set; }
    public bool Descending { get; set; }
}

public class NotificationFilterService
{
    public IEnumerable<Notification> FilterAndSort(IEnumerable<Notification> notifications, NotificationFilterOptions options)
    {
        ArgumentNullException.ThrowIfNull(notifications);

        var filterOptions = options ?? new NotificationFilterOptions();
        var query = notifications
            .Where(notification => notification is not null)
            .AsEnumerable();

        if (filterOptions.IsRead is bool isRead)
        {
            query = query.Where(notification => notification.IsRead == isRead);
        }

        if (filterOptions.Types is { Length: > 0 })
        {
            var allowedTypes = new HashSet<NotificationType>(filterOptions.Types);
            query = query.Where(notification => allowedTypes.Contains(notification.Type));
        }

        if (!string.IsNullOrWhiteSpace(filterOptions.SearchText))
        {
            var searchText = filterOptions.SearchText.Trim();
            query = query.Where(notification =>
                (notification.Title ?? string.Empty).Contains(searchText, StringComparison.OrdinalIgnoreCase)
                || (notification.Content ?? string.Empty).Contains(searchText, StringComparison.OrdinalIgnoreCase));
        }

        if (filterOptions.MinPriority is int minPriority)
        {
            query = query.Where(notification => notification.Priority >= minPriority);
        }

        var sortBy = filterOptions.SortBy ?? SortNotificationBy.Date;
        query = sortBy switch
        {
            SortNotificationBy.Priority => filterOptions.Descending
                ? query.OrderByDescending(notification => notification.Priority)
                : query.OrderBy(notification => notification.Priority),
            SortNotificationBy.Title => filterOptions.Descending
                ? query.OrderByDescending(notification => notification.Title)
                : query.OrderBy(notification => notification.Title),
            _ => filterOptions.Descending
                ? query.OrderByDescending(notification => notification.CreatedAt)
                : query.OrderBy(notification => notification.CreatedAt)
        };

        return query.ToList();
    }
}

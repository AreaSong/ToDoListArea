using System;
using System.Collections.Generic;

namespace DbContextHelp.Models;

public partial class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public string Status { get; set; } = null!;

    public bool EmailVerified { get; set; }

    public bool PhoneVerified { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string Role { get; set; } = null!;

    public virtual ICollection<GanttDatum> GanttData { get; set; } = new List<GanttDatum>();

    public virtual ICollection<InvitationCodeUsage> InvitationCodeUsages { get; set; } = new List<InvitationCodeUsage>();

    public virtual ICollection<InvitationCode> InvitationCodes { get; set; } = new List<InvitationCode>();

    public virtual ICollection<NotificationSetting> NotificationSettings { get; set; } = new List<NotificationSetting>();

    public virtual ICollection<ProductivityMetric> ProductivityMetrics { get; set; } = new List<ProductivityMetric>();

    public virtual ICollection<ReminderHistory> ReminderHistories { get; set; } = new List<ReminderHistory>();

    public virtual ICollection<ReminderRule> ReminderRules { get; set; } = new List<ReminderRule>();

    public virtual ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();

    public virtual ICollection<SystemLog> SystemLogs { get; set; } = new List<SystemLog>();

    public virtual ICollection<TaskStatistic> TaskStatistics { get; set; } = new List<TaskStatistic>();

    public virtual ICollection<TaskTemplate> TaskTemplates { get; set; } = new List<TaskTemplate>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

    public virtual ICollection<TimeBlock> TimeBlocks { get; set; } = new List<TimeBlock>();

    public virtual ICollection<TimelineEvent> TimelineEvents { get; set; } = new List<TimelineEvent>();

    public virtual ICollection<TimelineNode> TimelineNodes { get; set; } = new List<TimelineNode>();

    public virtual ICollection<UserActivity> UserActivities { get; set; } = new List<UserActivity>();

    public virtual ICollection<UserOauthAccount> UserOauthAccounts { get; set; } = new List<UserOauthAccount>();

    public virtual ICollection<UserProfile> UserProfiles { get; set; } = new List<UserProfile>();

    public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
}

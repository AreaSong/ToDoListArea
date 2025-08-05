using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DbContextHelp.Models;

public partial class ToDoListAreaDbContext : DbContext
{
    public ToDoListAreaDbContext()
    {
    }

    public ToDoListAreaDbContext(DbContextOptions<ToDoListAreaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<FeatureFlag> FeatureFlags { get; set; }

    public virtual DbSet<GanttDatum> GanttData { get; set; }

    public virtual DbSet<InvitationCode> InvitationCodes { get; set; }

    public virtual DbSet<InvitationCodeUsage> InvitationCodeUsages { get; set; }

    public virtual DbSet<NotificationSetting> NotificationSettings { get; set; }

    public virtual DbSet<ProductivityMetric> ProductivityMetrics { get; set; }

    public virtual DbSet<Reminder> Reminders { get; set; }

    public virtual DbSet<ReminderHistory> ReminderHistories { get; set; }

    public virtual DbSet<ReminderRule> ReminderRules { get; set; }

    public virtual DbSet<SystemConfig> SystemConfigs { get; set; }

    public virtual DbSet<SystemLog> SystemLogs { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<TaskCategory> TaskCategories { get; set; }

    public virtual DbSet<TaskDependency> TaskDependencies { get; set; }

    public virtual DbSet<TaskDetail> TaskDetails { get; set; }

    public virtual DbSet<TaskStatistic> TaskStatistics { get; set; }

    public virtual DbSet<TaskTemplate> TaskTemplates { get; set; }

    public virtual DbSet<TimeBlock> TimeBlocks { get; set; }

    public virtual DbSet<TimelineEvent> TimelineEvents { get; set; }

    public virtual DbSet<TimelineNode> TimelineNodes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserActivity> UserActivities { get; set; }

    public virtual DbSet<UserOauthAccount> UserOauthAccounts { get; set; }

    public virtual DbSet<UserProfile> UserProfiles { get; set; }

    public virtual DbSet<UserSession> UserSessions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // 连接字符串应该从配置文件中读取，而不是硬编码
        // 这个方法在使用依赖注入时通常不会被调用
        if (!optionsBuilder.IsConfigured)
        {
            // 仅在未配置时提供默认配置（开发环境）
            optionsBuilder.UseSqlServer("Server=.;Database=ToDoListArea;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FeatureFlag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__feature___3213E83F8B61B4BC");

            entity.ToTable("feature_flags");

            entity.HasIndex(e => e.FlagKey, "UQ__feature___5A28B80FE7F0060B").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.FlagKey)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("flag_key");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.TargetUsers).HasColumnName("target_users");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<GanttDatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__gantt_da__3213E83F11E48B47");

            entity.ToTable("gantt_data");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Dependencies).HasColumnName("dependencies");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.Progress)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("progress");
            entity.Property(e => e.Resources).HasColumnName("resources");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Task).WithMany(p => p.GanttData)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__gantt_dat__task___25518C17");

            entity.HasOne(d => d.User).WithMany(p => p.GanttData)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__gantt_dat__user___245D67DE");
        });

        modelBuilder.Entity<InvitationCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__invitati__3213E83F1F655162");

            entity.ToTable("invitation_codes");

            entity.HasIndex(e => e.Code, "UQ__invitati__357D4CF98A24A5A6").IsUnique();

            entity.HasIndex(e => e.Code, "idx_invitation_codes_code");

            entity.HasIndex(e => e.CreatedBy, "idx_invitation_codes_created_by");

            entity.HasIndex(e => e.ExpiresAt, "idx_invitation_codes_expires_at");

            entity.HasIndex(e => e.Status, "idx_invitation_codes_status");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.MaxUses)
                .HasDefaultValue(1)
                .HasColumnName("max_uses");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("active")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UsedCount).HasColumnName("used_count");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InvitationCodes)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_invitation_codes_created_by");
        });

        modelBuilder.Entity<InvitationCodeUsage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__invitati__3213E83FE15E98B4");

            entity.ToTable("invitation_code_usages");

            entity.HasIndex(e => new { e.InvitationCodeId, e.UserId }, "UK_invitation_code_usages_code_user").IsUnique();

            entity.HasIndex(e => e.InvitationCodeId, "idx_invitation_code_usages_invitation_code");

            entity.HasIndex(e => e.UsedAt, "idx_invitation_code_usages_used_at");

            entity.HasIndex(e => e.UserId, "idx_invitation_code_usages_user");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.InvitationCodeId).HasColumnName("invitation_code_id");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("ip_address");
            entity.Property(e => e.UsedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("used_at");
            entity.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .HasColumnName("user_agent");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.InvitationCode).WithMany(p => p.InvitationCodeUsages)
                .HasForeignKey(d => d.InvitationCodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_invitation_code_usages_invitation_code");

            entity.HasOne(d => d.User).WithMany(p => p.InvitationCodeUsages)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_invitation_code_usages_user");
        });

        modelBuilder.Entity<NotificationSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__notifica__3213E83FEF9C86B8");

            entity.ToTable("notification_settings");

            entity.HasIndex(e => new { e.UserId, e.Channel }, "UQ__notifica__A57421ED7583554F").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Channel)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("channel");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsEnabled)
                .HasDefaultValue(true)
                .HasColumnName("is_enabled");
            entity.Property(e => e.QuietHoursEnd).HasColumnName("quiet_hours_end");
            entity.Property(e => e.QuietHoursStart).HasColumnName("quiet_hours_start");
            entity.Property(e => e.Settings).HasColumnName("settings");
            entity.Property(e => e.Timezone)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("UTC")
                .HasColumnName("timezone");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.NotificationSettings)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__notificat__user___4E53A1AA");
        });

        modelBuilder.Entity<ProductivityMetric>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__producti__3213E83F4335C7C8");

            entity.ToTable("productivity_metrics");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Metadata).HasColumnName("metadata");
            entity.Property(e => e.MetricType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("metric_type");
            entity.Property(e => e.MetricUnit)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("metric_unit");
            entity.Property(e => e.MetricValue)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("metric_value");
            entity.Property(e => e.PeriodEnd).HasColumnName("period_end");
            entity.Property(e => e.PeriodStart).HasColumnName("period_start");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.ProductivityMetrics)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__productiv__user___634EBE90");
        });

        modelBuilder.Entity<Reminder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__reminder__3213E83F9C20F7A1");

            entity.ToTable("reminders");

            entity.HasIndex(e => e.ReminderTime, "idx_reminders_reminder_time");

            entity.HasIndex(e => e.Status, "idx_reminders_status");

            entity.HasIndex(e => e.TaskId, "idx_reminders_task_id");

            entity.HasIndex(e => e.UserId, "idx_reminders_user_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Channels)
                .HasDefaultValue("[\"web\"]")
                .HasColumnName("channels");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.ReminderTime).HasColumnName("reminder_time");
            entity.Property(e => e.RepeatPattern).HasColumnName("repeat_pattern");
            entity.Property(e => e.SnoozeUntil).HasColumnName("snooze_until");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("pending")
                .HasColumnName("status");
            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Task).WithMany(p => p.Reminders)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("FK__reminders__task___37703C52");

            entity.HasOne(d => d.User).WithMany(p => p.Reminders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__reminders__user___367C1819");
        });

        modelBuilder.Entity<ReminderHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__reminder__3213E83FA6D99850");

            entity.ToTable("reminder_history");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Channel)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("channel");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.ReminderId).HasColumnName("reminder_id");
            entity.Property(e => e.ResponseData).HasColumnName("response_data");
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("sent_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Reminder).WithMany(p => p.ReminderHistories)
                .HasForeignKey(d => d.ReminderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__reminder___remin__44CA3770");

            entity.HasOne(d => d.User).WithMany(p => p.ReminderHistories)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__reminder___user___45BE5BA9");
        });

        modelBuilder.Entity<ReminderRule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__reminder__3213E83FF000C0BA");

            entity.ToTable("reminder_rules");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Actions).HasColumnName("actions");
            entity.Property(e => e.Conditions).HasColumnName("conditions");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Priority).HasColumnName("priority");
            entity.Property(e => e.RuleName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rule_name");
            entity.Property(e => e.RuleType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("rule_type");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.ReminderRules)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__reminder___user___3F115E1A");
        });

        modelBuilder.Entity<SystemConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__system_c__3213E83F61BF3BCB");

            entity.ToTable("system_configs");

            entity.HasIndex(e => e.ConfigKey, "UQ__system_c__BDF6033DDD758293").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.ConfigKey)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("config_key");
            entity.Property(e => e.ConfigValue).HasColumnName("config_value");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.IsEncrypted).HasColumnName("is_encrypted");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<SystemLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__system_l__3213E83F3CC575F7");

            entity.ToTable("system_logs");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Exception).HasColumnName("exception");
            entity.Property(e => e.LogLevel)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("log_level");
            entity.Property(e => e.LoggerName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("logger_name");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Properties).HasColumnName("properties");
            entity.Property(e => e.RequestId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("request_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.SystemLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__system_lo__user___681373AD");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tasks__3213E83F2C1F4DC5");

            entity.ToTable("tasks");

            entity.HasIndex(e => e.CategoryId, "idx_tasks_category_id");

            entity.HasIndex(e => e.EndTime, "idx_tasks_end_time");

            entity.HasIndex(e => e.ParentTaskId, "idx_tasks_parent_task_id");

            entity.HasIndex(e => e.Priority, "idx_tasks_priority");

            entity.HasIndex(e => e.StartTime, "idx_tasks_start_time");

            entity.HasIndex(e => e.Status, "idx_tasks_status");

            entity.HasIndex(e => e.UserId, "idx_tasks_user_id");

            entity.HasIndex(e => new { e.UserId, e.Status, e.Priority }, "idx_tasks_user_status_priority");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.ActualDuration).HasColumnName("actual_duration");
            entity.Property(e => e.Attachments).HasColumnName("attachments");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CompletionPercentage)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("completion_percentage");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.EstimatedDuration).HasColumnName("estimated_duration");
            entity.Property(e => e.IsRecurring).HasColumnName("is_recurring");
            entity.Property(e => e.ParentTaskId).HasColumnName("parent_task_id");
            entity.Property(e => e.Priority)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("medium")
                .HasColumnName("priority");
            entity.Property(e => e.RecurrencePattern).HasColumnName("recurrence_pattern");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("pending")
                .HasColumnName("status");
            entity.Property(e => e.Tags).HasColumnName("tags");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Category).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__tasks__category___787EE5A0");

            entity.HasOne(d => d.ParentTask).WithMany(p => p.InverseParentTask)
                .HasForeignKey(d => d.ParentTaskId)
                .HasConstraintName("FK__tasks__parent_ta__778AC167");

            entity.HasOne(d => d.User).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__tasks__user_id__76969D2E");
        });

        modelBuilder.Entity<TaskCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__task_cat__3213E83F6376B9A4");

            entity.ToTable("task_categories");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Color)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasDefaultValue("#007bff")
                .HasColumnName("color");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.Icon)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("icon");
            entity.Property(e => e.IsSystem).HasColumnName("is_system");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<TaskDependency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__task_dep__3213E83F02B1820A");

            entity.ToTable("task_dependencies");

            entity.HasIndex(e => new { e.TaskId, e.DependsOnTaskId }, "UQ__task_dep__916FD09563CE24AE").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.DependencyType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("finish_to_start")
                .HasColumnName("dependency_type");
            entity.Property(e => e.DependsOnTaskId).HasColumnName("depends_on_task_id");
            entity.Property(e => e.LagTime).HasColumnName("lag_time");
            entity.Property(e => e.TaskId).HasColumnName("task_id");

            entity.HasOne(d => d.DependsOnTask).WithMany(p => p.TaskDependencyDependsOnTasks)
                .HasForeignKey(d => d.DependsOnTaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__task_depe__depen__07C12930");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskDependencyTasks)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("FK__task_depe__task___06CD04F7");
        });

        modelBuilder.Entity<TaskDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__task_det__3213E83FF69FA4A0");

            entity.ToTable("task_details");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.DetailKey)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("detail_key");
            entity.Property(e => e.DetailType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("detail_type");
            entity.Property(e => e.DetailValue).HasColumnName("detail_value");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskDetails)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("FK__task_deta__task___7F2BE32F");
        });

        modelBuilder.Entity<TaskStatistic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__task_sta__3213E83F1424B034");

            entity.ToTable("task_statistics");

            entity.HasIndex(e => new { e.UserId, e.Date }, "UQ__task_sta__6423D51140462B81").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CompletedTasks).HasColumnName("completed_tasks");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.OverdueTasks).HasColumnName("overdue_tasks");
            entity.Property(e => e.PendingTasks).HasColumnName("pending_tasks");
            entity.Property(e => e.ProductivityScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("productivity_score");
            entity.Property(e => e.TotalTasks).HasColumnName("total_tasks");
            entity.Property(e => e.TotalTimeSpent).HasColumnName("total_time_spent");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.TaskStatistics)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__task_stat__user___5E8A0973");
        });

        modelBuilder.Entity<TaskTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__task_tem__3213E83F7A0B35A6");

            entity.ToTable("task_templates");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("category");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsPublic).HasColumnName("is_public");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.TemplateData).HasColumnName("template_data");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UsageCount).HasColumnName("usage_count");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.TaskTemplates)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__task_temp__user___0F624AF8");
        });

        modelBuilder.Entity<TimeBlock>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__time_blo__3213E83FF1DA2851");

            entity.ToTable("time_blocks");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.BlockType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("work")
                .HasColumnName("block_type");
            entity.Property(e => e.Color)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasDefaultValue("#007bff")
                .HasColumnName("color");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.IsLocked).HasColumnName("is_locked");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Task).WithMany(p => p.TimeBlocks)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("FK__time_bloc__task___2EDAF651");

            entity.HasOne(d => d.User).WithMany(p => p.TimeBlocks)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__time_bloc__user___2DE6D218");
        });

        modelBuilder.Entity<TimelineEvent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__timeline__3213E83F0A484C7A");

            entity.ToTable("timeline_events");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.EventData).HasColumnName("event_data");
            entity.Property(e => e.EventDescription).HasColumnName("event_description");
            entity.Property(e => e.EventTitle)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("event_title");
            entity.Property(e => e.EventType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("event_type");
            entity.Property(e => e.OccurredAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("occurred_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.TimelineEvents)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__timeline___user___1DB06A4F");
        });

        modelBuilder.Entity<TimelineNode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__timeline__3213E83F90B93F3A");

            entity.ToTable("timeline_nodes");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Color)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasDefaultValue("#007bff")
                .HasColumnName("color");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.NodeType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("task")
                .HasColumnName("node_type");
            entity.Property(e => e.PositionData).HasColumnName("position_data");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Task).WithMany(p => p.TimelineNodes)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("FK__timeline___task___17F790F9");

            entity.HasOne(d => d.User).WithMany(p => p.TimelineNodes)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__timeline___user___17036CC0");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3213E83F5D98334C");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "UQ__users__AB6E616433C7AE41").IsUnique();

            entity.HasIndex(e => e.Phone, "UQ__users__B43B145F4C78ECFF").IsUnique();

            entity.HasIndex(e => e.CreatedAt, "idx_users_created_at");

            entity.HasIndex(e => e.Email, "idx_users_email");

            entity.HasIndex(e => e.Role, "idx_users_role");

            entity.HasIndex(e => e.Status, "idx_users_status");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AvatarUrl).HasColumnName("avatar_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.EmailVerified).HasColumnName("email_verified");
            entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.PhoneVerified).HasColumnName("phone_verified");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("user")
                .HasColumnName("role");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("active")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<UserActivity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user_act__3213E83F72B90131");

            entity.ToTable("user_activities");

            entity.HasIndex(e => e.ActivityType, "idx_user_activities_activity_type");

            entity.HasIndex(e => e.CreatedAt, "idx_user_activities_created_at");

            entity.HasIndex(e => e.UserId, "idx_user_activities_user_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.ActivityDescription)
                .HasMaxLength(500)
                .HasColumnName("activity_description");
            entity.Property(e => e.ActivityType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("activity_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.EntityType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("entity_type");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("ip_address");
            entity.Property(e => e.Metadata).HasColumnName("metadata");
            entity.Property(e => e.UserAgent).HasColumnName("user_agent");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserActivities)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__user_acti__user___531856C7");
        });

        modelBuilder.Entity<UserOauthAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user_oau__3213E83F30009FAC");

            entity.ToTable("user_oauth_accounts");

            entity.HasIndex(e => new { e.Provider, e.ProviderUserId }, "UQ__user_oau__0341DC90A6594CA6").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Provider)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("provider");
            entity.Property(e => e.ProviderData).HasColumnName("provider_data");
            entity.Property(e => e.ProviderEmail)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("provider_email");
            entity.Property(e => e.ProviderUserId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("provider_user_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserOauthAccounts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__user_oaut__user___6D0D32F4");
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user_pro__3213E83F3F7241FD");

            entity.ToTable("user_profiles");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.DateFormat)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("YYYY-MM-DD")
                .HasColumnName("date_format");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("first_name");
            entity.Property(e => e.Language)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("zh-CN")
                .HasColumnName("language");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("last_name");
            entity.Property(e => e.NotificationPreferences).HasColumnName("notification_preferences");
            entity.Property(e => e.ThemePreferences).HasColumnName("theme_preferences");
            entity.Property(e => e.TimeFormat)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("24h")
                .HasColumnName("time_format");
            entity.Property(e => e.Timezone)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("UTC")
                .HasColumnName("timezone");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserProfiles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__user_prof__user___5CD6CB2B");
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user_ses__3213E83F6102478A");

            entity.ToTable("user_sessions");

            entity.HasIndex(e => e.RefreshToken, "UQ__user_ses__7FB69BAD20BABBB4").IsUnique();

            entity.HasIndex(e => e.SessionToken, "UQ__user_ses__E598F5C8FD17D2DF").IsUnique();

            entity.HasIndex(e => e.ExpiresAt, "idx_user_sessions_expires_at");

            entity.HasIndex(e => e.SessionToken, "idx_user_sessions_session_token");

            entity.HasIndex(e => e.UserId, "idx_user_sessions_user_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeviceInfo)
                .HasMaxLength(500)
                .HasColumnName("device_info");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("ip_address");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.RefreshToken)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("refresh_token");
            entity.Property(e => e.SessionToken)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("session_token");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserAgent).HasColumnName("user_agent");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserSessions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__user_sess__user___656C112C");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

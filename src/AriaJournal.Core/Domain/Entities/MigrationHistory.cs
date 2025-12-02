// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Entities/MigrationHistory.cs
// شماره فایل: 15
// =============================================================================
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AriaJournal.Core.Domain.Entities
{
    public class MigrationHistory
    {
        /// <summary>
        /// شناسه یکتا
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// نسخه مایگریشن
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// نام مایگریشن
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// توضیحات
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// تاریخ اجرا
        /// </summary>
        public DateTime AppliedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// آیا موفق بوده
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// پیام خطا (در صورت وجود)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// مدت زمان اجرا (میلی‌ثانیه)
        /// </summary>
        public long ExecutionTimeMs { get; set; }
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Entities/MigrationHistory.cs
// =============================================================================
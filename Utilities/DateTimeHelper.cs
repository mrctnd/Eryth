namespace Eryth.Utilities
{
    // Tarih ve zaman yardımcı sınıfı
    public static class DateTimeHelper
    {
        // Zaman aralığını insan okunabilir formata çevir
        public static string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "Az önce";
            if (timeSpan.TotalHours < 1)
                return $"{(int)timeSpan.TotalMinutes} dakika önce";
            if (timeSpan.TotalDays < 1)
                return $"{(int)timeSpan.TotalHours} saat önce";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} gün önce";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} hafta önce";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} ay önce";

            return $"{(int)(timeSpan.TotalDays / 365)} yıl önce";
        }

        // Süreyi formatla (HH:MM:SS)
        public static string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalHours >= 1)
                return duration.ToString(@"h\:mm\:ss");

            return duration.ToString(@"m\:ss");
        }

        // Süreyi saniyeden formatla
        public static string FormatDuration(int totalSeconds)
        {
            var duration = TimeSpan.FromSeconds(totalSeconds);
            return FormatDuration(duration);
        }

        // Tarihi Türkçe formatta göster
        public static string FormatDateTurkish(DateTime dateTime)
        {
            var monthNames = new string[]
            {
                "", "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran",
                "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"
            };

            return $"{dateTime.Day} {monthNames[dateTime.Month]} {dateTime.Year}";
        }

        // Haftanın günü Türkçe
        public static string GetDayOfWeekTurkish(DateTime dateTime)
        {
            return dateTime.DayOfWeek switch
            {
                DayOfWeek.Monday => "Pazartesi",
                DayOfWeek.Tuesday => "Salı",
                DayOfWeek.Wednesday => "Çarşamba",
                DayOfWeek.Thursday => "Perşembe",
                DayOfWeek.Friday => "Cuma",
                DayOfWeek.Saturday => "Cumartesi",
                DayOfWeek.Sunday => "Pazar",
                _ => ""
            };
        }
    }
}

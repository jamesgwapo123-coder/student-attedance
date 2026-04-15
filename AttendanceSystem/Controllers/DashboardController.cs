using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Data;
using AttendanceSystem.Models;
using AttendanceSystem.Models.ViewModels;

namespace AttendanceSystem.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;

        public DashboardController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var weekStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek));

            var todayLogs = await _db.AttendanceLogs
                .Include(a => a.Student)
                .Where(a => a.Date == today)
                .OrderByDescending(a => a.CheckInTime)
                .ToListAsync();

            var weekLogs = await _db.AttendanceLogs
                .Where(a => a.Date >= weekStart)
                .CountAsync();

            var recentActivity = todayLogs.Select(log => new RecentActivityItem
            {
                StudentName = log.Student?.FullName ?? "Unknown",
                Status = log.Status == AttendanceStatus.OnTime ? "On Time" : "Late",
                CheckInTime = log.CheckInTime,
                TimeAgo = GetTimeAgo(log.CheckInTime)
            }).Take(10).ToList();

            var model = new DashboardViewModel
            {
                TotalStudents = await _db.Students.CountAsync(),
                CheckedInToday = todayLogs.Count,
                OnTimeToday = todayLogs.Count(l => l.Status == AttendanceStatus.OnTime),
                LateToday = todayLogs.Count(l => l.Status == AttendanceStatus.Late),
                TotalLogsThisWeek = weekLogs,
                RecentActivity = recentActivity
            };

            return View(model);
        }

        private string GetTimeAgo(DateTime time)
        {
            var diff = DateTime.UtcNow - time;
            if (diff.TotalMinutes < 1) return "Just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min ago";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} hour(s) ago";
            return $"{(int)diff.TotalDays} day(s) ago";
        }
    }
}

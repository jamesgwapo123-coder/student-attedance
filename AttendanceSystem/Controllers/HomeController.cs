using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Data;
using AttendanceSystem.Models;
using AttendanceSystem.Models.ViewModels;

namespace AttendanceSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View(new CheckInViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CheckIn(CheckInViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Result = new CheckInResultViewModel
                {
                    Success = false,
                    Message = "Please enter your School ID."
                };
                return View("Index", model);
            }

            // Find student
            var student = await _db.Students
                .FirstOrDefaultAsync(s => s.SchoolId == model.SchoolId.Trim());

            if (student == null)
            {
                ViewBag.Result = new CheckInResultViewModel
                {
                    Success = false,
                    Message = "Student ID not found. Please check your ID or contact the administrator."
                };
                return View("Index", model);
            }

            // Check if already checked in today
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var alreadyCheckedIn = await _db.AttendanceLogs
                .AnyAsync(a => a.StudentId == student.Id && a.Date == today);

            if (alreadyCheckedIn)
            {
                ViewBag.Result = new CheckInResultViewModel
                {
                    Success = false,
                    Message = "You have already checked in today!"
                };
                return View("Index", model);
            }

            var now = DateTime.UtcNow;
            var currentDay = now.DayOfWeek;
            var currentTime = now.TimeOfDay;

            // Find matching class schedule for today
            var todaySchedules = (await _db.ClassSchedules
                .Where(s => s.DayOfWeek == currentDay)
                .ToListAsync())
                .OrderBy(s => s.StartTime)
                .ToList();

            ClassSchedule? matchedSchedule = null;
            AttendanceStatus status = AttendanceStatus.OnTime;

            if (todaySchedules.Any())
            {
                // Find the schedule that the student is checking into
                // Match: current time is between (StartTime - 30min) and EndTime
                matchedSchedule = todaySchedules.FirstOrDefault(s =>
                    currentTime >= s.StartTime.Add(TimeSpan.FromMinutes(-30)) &&
                    currentTime <= s.EndTime);

                if (matchedSchedule != null)
                {
                    var deadline = matchedSchedule.StartTime
                        .Add(TimeSpan.FromMinutes(matchedSchedule.GracePeriodMinutes));

                    status = currentTime <= deadline
                        ? AttendanceStatus.OnTime
                        : AttendanceStatus.Late;
                }
            }

            // Create attendance log
            var log = new AttendanceLog
            {
                StudentId = student.Id,
                ScheduleId = matchedSchedule?.Id,
                CheckInTime = now,
                Date = today,
                Status = status,
                Notes = matchedSchedule != null
                    ? $"Class: {matchedSchedule.SubjectName}"
                    : "No matching class schedule"
            };

            _db.AttendanceLogs.Add(log);
            await _db.SaveChangesAsync();

            ViewBag.Result = new CheckInResultViewModel
            {
                Success = true,
                StudentName = student.FullName,
                SchoolId = student.SchoolId,
                Status = status.ToString(),
                SubjectName = matchedSchedule?.SubjectName ?? "General",
                CheckInTime = now,
                Message = status == AttendanceStatus.OnTime
                    ? "You're on time! Attendance recorded."
                    : "You're late, but attendance has been recorded."
            };

            return View("Index", new CheckInViewModel());
        }
    }
}

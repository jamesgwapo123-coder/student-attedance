using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Data;
using AttendanceSystem.Models;
using AttendanceSystem.Models.ViewModels;

namespace AttendanceSystem.Controllers
{
    [Authorize]
    public class StudentsController : Controller
    {
        private readonly AppDbContext _db;

        public StudentsController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var query = _db.Students.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s =>
                    s.FullName.Contains(search) ||
                    s.SchoolId.Contains(search));
            }

            var students = await query.OrderBy(s => s.FullName).ToListAsync();

            // Get latest check-in for each student
            var studentIds = students.Select(s => s.Id).ToList();
            var latestLogs = await _db.AttendanceLogs
                .Where(a => studentIds.Contains(a.StudentId))
                .GroupBy(a => a.StudentId)
                .Select(g => new
                {
                    StudentId = g.Key,
                    LastCheckIn = g.Max(a => a.CheckInTime),
                    LastStatus = g.OrderByDescending(a => a.CheckInTime).First().Status
                })
                .ToListAsync();

            ViewBag.LatestLogs = latestLogs.ToDictionary(l => l.StudentId);
            ViewBag.Search = search;
            return View(students);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateStudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields.";
                return RedirectToAction("Index");
            }

            // Check duplicate school ID
            var exists = await _db.Students.AnyAsync(s => s.SchoolId == model.SchoolId.Trim());
            if (exists)
            {
                TempData["Error"] = "A student with this School ID already exists.";
                return RedirectToAction("Index");
            }

            var student = new Student
            {
                SchoolId = model.SchoolId.Trim(),
                FullName = model.FullName.Trim(),
                Grade = model.Grade?.Trim() ?? "",
                CreatedAt = DateTime.UtcNow
            };

            _db.Students.Add(student);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Student '{student.FullName}' added successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _db.Students.FindAsync(id);
            if (student != null)
            {
                // Delete related attendance logs first
                var logs = _db.AttendanceLogs.Where(a => a.StudentId == id);
                _db.AttendanceLogs.RemoveRange(logs);
                _db.Students.Remove(student);
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Student '{student.FullName}' deleted.";
            }
            return RedirectToAction("Index");
        }
    }
}

# 🔍 Code Explanation — Student Attendance Logging System

### Cebu Eastern College — Final Project

This document explains **every source code file** in the system, line by line. It is written for IT students who may be new to C# and ASP.NET.

---

## Table of Contents

1. [Program.cs — Application Entry Point](#1-programcs--application-entry-point)
2. [AppDbContext.cs — Database Configuration](#2-appdbcontextcs--database-configuration)
3. [Student.cs — Student Model](#3-studentcs--student-model)
4. [ClassSchedule.cs — Schedule Model](#4-classschedulecs--schedule-model)
5. [AttendanceLog.cs — Attendance Record Model](#5-attendancelogcs--attendance-record-model)
6. [AdminUser.cs — Admin Login Model](#6-adminusercs--admin-login-model)
7. [ViewModels.cs — Data Transfer Objects](#7-viewmodelscs--data-transfer-objects)
8. [HomeController.cs — Student Check-In Logic](#8-homecontrollercs--student-check-in-logic)
9. [AccountController.cs — Admin Authentication](#9-accountcontrollercs--admin-authentication)
10. [DashboardController.cs — Statistics & Overview](#10-dashboardcontrollercs--statistics--overview)
11. [StudentsController.cs — Student Management](#11-studentscontrollercs--student-management)
12. [SchedulesController.cs — Schedule Management](#12-schedulescontrollercs--schedule-management)
13. [HistoryController.cs — Attendance Logs & Export](#13-historycontrollercs--attendance-logs--export)
14. [Key C# Concepts Used](#14-key-c-concepts-used)

---

## 1. Program.cs — Application Entry Point

This is **the first file that runs** when you start the application. It configures all the services and starts the web server.

```csharp
// These 'using' statements import libraries that we need
using AttendanceSystem.Data;                              // Our database context
using Microsoft.EntityFrameworkCore;                      // EF Core (database ORM)
using Microsoft.AspNetCore.Authentication.Cookies;        // Cookie-based login

var builder = WebApplication.CreateBuilder(args);          // Create the app builder
```

### What This Does:
- `using` = import a library (like `import` in Python or Java)
- `WebApplication.CreateBuilder()` = starts setting up the web application

```csharp
// Add MVC support (Controllers + Views)
builder.Services.AddControllersWithViews();
```

### What This Does:
- Tells the app to use the **MVC pattern** (Model-View-Controller)
- Without this, the app wouldn't know how to handle web requests

```csharp
// Configure SQLite database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=attendance.db"));
```

### What This Does:
- Registers our database context (`AppDbContext`) with the app
- `"Data Source=attendance.db"` = the database file will be called `attendance.db`
- This file is created automatically in the project folder

```csharp
// Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";       // Where to redirect if not logged in
        options.LogoutPath = "/Account/Logout";     // Where to redirect on logout
        options.ExpireTimeSpan = TimeSpan.FromHours(8);  // Session lasts 8 hours
    });
```

### What This Does:
- Sets up **cookie authentication** — when an admin logs in, a small piece of data (cookie) is stored in their browser
- If someone tries to access an admin page without being logged in, they get redirected to `/Account/Login`
- The login session expires after 8 hours

```csharp
var app = builder.Build();                            // Build the app with all the settings

// Auto-create database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();                      // Creates tables if they don't exist
}
```

### What This Does:
- `app.Build()` = finalize all the configuration
- `EnsureCreated()` = checks if the `attendance.db` file exists; if not, creates all the tables automatically
- This means **you never need to manually create the database**

```csharp
app.UseStaticFiles();                                 // Serve CSS, JS, images from wwwroot/
app.UseRouting();                                     // Enable URL routing
app.UseAuthentication();                              // Enable login/logout
app.UseAuthorization();                               // Enable access control

// Set the default URL pattern
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
// This means: website.com/ → HomeController.Index()
//             website.com/Students → StudentsController.Index()
//             website.com/Students/Delete/5 → StudentsController.Delete(id: 5)

app.Run();                                            // Start the web server!
```

### What This Does:
- Sets up the **middleware pipeline** — the order in which the app processes each web request
- The URL pattern `{controller=Home}/{action=Index}/{id?}` means:
  - Default controller is `Home`, default action is `Index`
  - `id?` means the ID parameter is optional

---

## 2. AppDbContext.cs — Database Configuration

This file is the **bridge between C# and the SQLite database**. It uses Entity Framework Core (EF Core) to translate C# code into SQL queries automatically.

```csharp
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Models;
using System.Security.Cryptography;         // For SHA-256 hashing
using System.Text;                           // For string encoding

namespace AttendanceSystem.Data
{
    // DbContext = the main class that talks to the database
    public class AppDbContext : DbContext
    {
        // Constructor — receives database settings from Program.cs
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
```

### What This Does:
- `AppDbContext` **inherits from** `DbContext` — this is the EF Core base class
- The constructor receives configuration (like the SQLite connection string) from `Program.cs`

```csharp
        // Each DbSet = one table in the database
        public DbSet<Student> Students { get; set; }                // → "Students" table
        public DbSet<ClassSchedule> ClassSchedules { get; set; }    // → "ClassSchedules" table
        public DbSet<AttendanceLog> AttendanceLogs { get; set; }    // → "AttendanceLogs" table
        public DbSet<AdminUser> AdminUsers { get; set; }            // → "AdminUsers" table
```

### What This Does:
- Each `DbSet<T>` property represents one **database table**
- `DbSet<Student>` means: "There is a table called Students, and each row is a Student object"
- You use these to **query** the database: `_db.Students.Where(s => s.Grade == "Grade 10")`

```csharp
        // Called when the database is first created
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Make SchoolId unique (no two students can have the same ID)
            modelBuilder.Entity<Student>()
                .HasIndex(s => s.SchoolId)
                .IsUnique();

            // Seed the default admin user
            modelBuilder.Entity<AdminUser>().HasData(new AdminUser
            {
                Id = 1,
                Username = "admin",
                PasswordHash = HashPassword("admin123"),   // Hashed, not plain text!
                DisplayName = "Administrator"
            });
        }
```

### What This Does:
- `OnModelCreating` = runs when EF Core is building the database schema
- `.HasIndex().IsUnique()` = adds a **UNIQUE constraint** on SchoolId (prevents duplicates)
- `.HasData()` = **seeds** (pre-inserts) a default admin account so you can log in immediately

```csharp
        // Hashes a password using SHA-256 (one-way encryption)
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();                           // Create hasher
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));  // Hash it
            return Convert.ToBase64String(hashedBytes);                   // Convert to string
        }
    }
}
```

### What This Does:
- Takes a plain password like `"admin123"`
- Converts it to a **hash** like `"JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk="`
- This is **one-way** — you cannot reverse a hash back to the original password
- This is how the password is stored securely in the database

---

## 3. Student.cs — Student Model

A **Model** in MVC represents a database table. Each property becomes a column.

```csharp
using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.Models
{
    public class Student
    {
        public int Id { get; set; }                // Primary Key (auto-incremented by database)

        [Required]                                  // This field cannot be empty
        [StringLength(50)]                          // Maximum 50 characters
        public string SchoolId { get; set; } = string.Empty;   // e.g., "STU-001"

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;   // e.g., "Juan Dela Cruz"

        [StringLength(20)]
        public string Grade { get; set; } = string.Empty;      // e.g., "Grade 10"

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Auto-set to current time

        // Navigation property — links to all attendance records for this student
        public ICollection<AttendanceLog> AttendanceLogs { get; set; } = new List<AttendanceLog>();
    }
}
```

### Key Concepts:
| Concept | Explanation |
|---|---|
| `[Required]` | Validation: this field must have a value |
| `[StringLength(50)]` | Validation: max 50 characters |
| `= string.Empty` | Default value: empty string (prevents null) |
| `ICollection<AttendanceLog>` | **Navigation property** — EF Core uses this to join with the AttendanceLogs table |

### Resulting SQL Table:
```sql
CREATE TABLE Students (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    SchoolId    TEXT NOT NULL,
    FullName    TEXT NOT NULL,
    Grade       TEXT NOT NULL,
    CreatedAt   TEXT NOT NULL
);
```

---

## 4. ClassSchedule.cs — Schedule Model

```csharp
public class ClassSchedule
{
    public int Id { get; set; }                           // Primary Key

    [Required]
    [StringLength(100)]
    public string SubjectName { get; set; } = string.Empty; // e.g., "Mathematics"

    [Required]
    public DayOfWeek DayOfWeek { get; set; }              // Enum: Sunday=0, Monday=1 ... Saturday=6

    [Required]
    public TimeSpan StartTime { get; set; }               // e.g., 08:00 (stored as hours:minutes)

    [Required]
    public TimeSpan EndTime { get; set; }                 // e.g., 09:30

    /// <summary>
    /// Minutes after StartTime that a student can still be considered "On Time"
    /// </summary>
    public int GracePeriodMinutes { get; set; } = 15;     // Default: 15 minutes

    public ICollection<AttendanceLog> AttendanceLogs { get; set; } = new List<AttendanceLog>();
}
```

### Key Concepts:
| Property | Type | Purpose |
|---|---|---|
| `DayOfWeek` | `enum` | Built-in C# enum: 0=Sunday, 1=Monday, ..., 6=Saturday |
| `TimeSpan` | `struct` | Represents a time value like 08:00 or 14:30 |
| `GracePeriodMinutes` | `int` | How many minutes after class start a student can still be "On Time" |

---

## 5. AttendanceLog.cs — Attendance Record Model

```csharp
// This enum defines the two possible attendance statuses
public enum AttendanceStatus
{
    OnTime,    // Value = 0 in the database
    Late       // Value = 1 in the database
}

public class AttendanceLog
{
    public int Id { get; set; }                            // Primary Key

    [Required]
    public int StudentId { get; set; }                     // Foreign Key → Students table

    [ForeignKey("StudentId")]
    public Student? Student { get; set; }                  // Navigation to Student

    public int? ScheduleId { get; set; }                   // Foreign Key → ClassSchedules (nullable)

    [ForeignKey("ScheduleId")]
    public ClassSchedule? Schedule { get; set; }           // Navigation to Schedule

    [Required]
    public DateTime CheckInTime { get; set; }              // Exact time of check-in

    [Required]
    public DateOnly Date { get; set; }                     // Date of check-in (no time)

    [Required]
    public AttendanceStatus Status { get; set; }           // OnTime or Late

    [StringLength(200)]
    public string? Notes { get; set; }                     // Optional notes (e.g., "Class: Math")
}
```

### Key Concepts:
| Concept | Explanation |
|---|---|
| `enum` | A set of named constants. `AttendanceStatus.OnTime = 0`, `AttendanceStatus.Late = 1` |
| `[ForeignKey]` | Tells EF Core this property links to another table |
| `int?` / `string?` | The `?` means **nullable** — this field can be empty/null |
| `DateOnly` | A date without time (e.g., `2026-04-04`) |
| `DateTime` | A date with time (e.g., `2026-04-04 08:15:32`) |

---

## 6. AdminUser.cs — Admin Login Model

```csharp
public class AdminUser
{
    public int Id { get; set; }                            // Primary Key

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;   // e.g., "admin"

    [Required]
    public string PasswordHash { get; set; } = string.Empty; // SHA-256 hash (NOT plain text)

    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;  // e.g., "Administrator"
}
```

### Important:
- The password is **never stored in plain text**
- `"admin123"` is stored as `"JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk="` (SHA-256 hash)
- When someone logs in, we hash their input and **compare the hashes**

---

## 7. ViewModels.cs — Data Transfer Objects

**ViewModels** are special classes used to pass data between Controllers and Views. They are **not** stored in the database.

### Why do we need ViewModels?

Models represent database tables. But sometimes, a View needs data from **multiple tables** or needs data in a **different shape** than the database. ViewModels solve this.

```csharp
// Used for the student check-in form
public class CheckInViewModel
{
    [Required(ErrorMessage = "School ID is required")]
    public string SchoolId { get; set; } = string.Empty;
}
```
→ The check-in form only needs one field: `SchoolId`

```csharp
// The result shown after a student checks in
public class CheckInResultViewModel
{
    public string StudentName { get; set; } = string.Empty;
    public string SchoolId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;      // "OnTime" or "Late"
    public string SubjectName { get; set; } = string.Empty;  // e.g., "Mathematics"
    public DateTime CheckInTime { get; set; }
    public bool Success { get; set; }                        // true = check-in worked
    public string Message { get; set; } = string.Empty;      // Success/error message
}
```
→ Contains all the data we need to show the check-in result card

```csharp
// Dashboard page data
public class DashboardViewModel
{
    public int TotalStudents { get; set; }        // Total registered students
    public int CheckedInToday { get; set; }       // Students who checked in today
    public int OnTimeToday { get; set; }          // On-time count for today
    public int LateToday { get; set; }            // Late count for today
    public int TotalLogsThisWeek { get; set; }    // Total logs this week
    public List<RecentActivityItem> RecentActivity { get; set; } = new();  // Recent check-ins
}
```
→ Aggregated statistics from multiple queries, packaged for the Dashboard view

```csharp
// Form data for adding a new student
public class CreateStudentViewModel
{
    [Required(ErrorMessage = "School ID is required")]
    public string SchoolId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required")]
    public string FullName { get; set; } = string.Empty;

    public string Grade { get; set; } = string.Empty;      // Optional
}
```
→ Only the fields needed to create a student (no Id, no CreatedAt — those are auto-generated)

```csharp
// Form data for adding a new schedule
public class CreateScheduleViewModel
{
    [Required(ErrorMessage = "Subject name is required")]
    public string SubjectName { get; set; } = string.Empty;

    [Required]
    public DayOfWeek DayOfWeek { get; set; }

    [Required(ErrorMessage = "Start time is required")]
    public string StartTime { get; set; } = string.Empty;   // Received as a string like "08:00"

    [Required(ErrorMessage = "End time is required")]
    public string EndTime { get; set; } = string.Empty;     // Converted to TimeSpan in controller

    public int GracePeriodMinutes { get; set; } = 15;
}
```
→ Time is received as a **string** from the HTML form, then **converted** to `TimeSpan` in the controller

```csharp
// History page with filters
public class HistoryViewModel
{
    public List<AttendanceLog> Logs { get; set; } = new();  // The attendance records
    public int TotalEntries { get; set; }                    // Total count
    public int OnTimeCount { get; set; }                     // On-time count
    public int LateCount { get; set; }                       // Late count
    public string? FilterStatus { get; set; }                // Active filter: "OnTime", "Late", "All"
    public string? FilterDate { get; set; }                  // Active date filter
}
```
→ Contains both the data AND the current filter state, so the View knows which filter is active

---

## 8. HomeController.cs — Student Check-In Logic

This is the **most important controller** — it handles the student check-in process.

```csharp
public class HomeController : Controller
{
    private readonly AppDbContext _db;        // Database access

    // Constructor — ASP.NET automatically provides the database context
    // This is called "Dependency Injection"
    public HomeController(AppDbContext db)
    {
        _db = db;
    }
```

### What is Dependency Injection?
Instead of creating our own database connection, ASP.NET **injects** it for us. We just declare `AppDbContext db` in the constructor and the framework handles everything.

```csharp
    // GET /  (homepage)
    public IActionResult Index()
    {
        return View(new CheckInViewModel());   // Show the check-in form (empty)
    }
```

### The Check-In Process (POST /Home/CheckIn):

```csharp
    [HttpPost]    // This method only runs on form submission (POST request)
    public async Task<IActionResult> CheckIn(CheckInViewModel model)
    {
```

**Step 1: Validate the form input**
```csharp
        if (!ModelState.IsValid)    // If SchoolId is empty
        {
            ViewBag.Result = new CheckInResultViewModel
            {
                Success = false,
                Message = "Please enter your School ID."
            };
            return View("Index", model);
        }
```

**Step 2: Look up the student in the database**
```csharp
        var student = await _db.Students
            .FirstOrDefaultAsync(s => s.SchoolId == model.SchoolId.Trim());
        // FirstOrDefaultAsync = find the first match, or return null
        // .Trim() = remove whitespace from both ends

        if (student == null)     // Student not found
        {
            ViewBag.Result = new CheckInResultViewModel
            {
                Success = false,
                Message = "Student ID not found..."
            };
            return View("Index", model);
        }
```

**Step 3: Check if already checked in today**
```csharp
        var today = DateOnly.FromDateTime(DateTime.Now);
        var alreadyCheckedIn = await _db.AttendanceLogs
            .AnyAsync(a => a.StudentId == student.Id && a.Date == today);
        // AnyAsync = returns true if at least one matching record exists

        if (alreadyCheckedIn)
        {
            ViewBag.Result = new CheckInResultViewModel
            {
                Success = false,
                Message = "You have already checked in today!"
            };
            return View("Index", model);
        }
```

**Step 4: Find today's class schedule**
```csharp
        var now = DateTime.Now;
        var currentDay = now.DayOfWeek;       // e.g., DayOfWeek.Friday
        var currentTime = now.TimeOfDay;      // e.g., 08:15 (TimeSpan)

        // Get all class schedules for today
        var todaySchedules = (await _db.ClassSchedules
            .Where(s => s.DayOfWeek == currentDay)   // Filter: only today's classes
            .ToListAsync())                           // Load into memory
            .OrderBy(s => s.StartTime)                // Sort by start time
            .ToList();
```

**Step 5: Match the current time to a class period**
```csharp
        ClassSchedule? matchedSchedule = null;
        AttendanceStatus status = AttendanceStatus.OnTime;   // Default: on time

        if (todaySchedules.Any())    // If there are classes today
        {
            // Find a class where current time is between (Start - 30min) and End
            matchedSchedule = todaySchedules.FirstOrDefault(s =>
                currentTime >= s.StartTime.Add(TimeSpan.FromMinutes(-30)) &&
                currentTime <= s.EndTime);
            // This allows students to check in up to 30 minutes before class starts

            if (matchedSchedule != null)
            {
                // Calculate the deadline: class start + grace period
                var deadline = matchedSchedule.StartTime
                    .Add(TimeSpan.FromMinutes(matchedSchedule.GracePeriodMinutes));

                // Compare: Is current time within the deadline?
                status = currentTime <= deadline
                    ? AttendanceStatus.OnTime     // Yes → On Time
                    : AttendanceStatus.Late;      // No → Late
            }
        }
```

### Visual Example:
```
Class: Mathematics (Monday, 8:00 AM - 9:30 AM, Grace: 15 min)

Timeline:
  7:30 AM ─── can start checking in (30 min before)
  8:00 AM ─── class starts
  8:15 AM ─── grace period ends (deadline)
  9:30 AM ─── class ends

  Check in at 7:45 AM → ✅ On Time (before deadline 8:15)
  Check in at 8:10 AM → ✅ On Time (before deadline 8:15)
  Check in at 8:20 AM → ⚠️ Late (after deadline 8:15)
```

**Step 6: Save the attendance log**
```csharp
        var log = new AttendanceLog
        {
            StudentId = student.Id,
            ScheduleId = matchedSchedule?.Id,     // null if no matching class
            CheckInTime = now,
            Date = today,
            Status = status,
            Notes = matchedSchedule != null
                ? $"Class: {matchedSchedule.SubjectName}"
                : "No matching class schedule"
        };

        _db.AttendanceLogs.Add(log);          // Add to the tracking list
        await _db.SaveChangesAsync();          // Write to database (SQL INSERT)
```

**Step 7: Return the result to the student**
```csharp
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

        return View("Index", new CheckInViewModel());  // Show the check-in page with result
    }
```

---

## 9. AccountController.cs — Admin Authentication

Handles admin login and logout using **cookie-based authentication**.

```csharp
[HttpPost]
public async Task<IActionResult> Login(LoginViewModel model)
{
    if (!ModelState.IsValid) return View(model);

    // Hash the entered password and compare with database
    var passwordHash = AppDbContext.HashPassword(model.Password);
    var admin = await _db.AdminUsers
        .FirstOrDefaultAsync(a => a.Username == model.Username && a.PasswordHash == passwordHash);
```

### How Login Works:
1. User types `"admin123"`
2. System hashes it → `"JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk="`
3. System looks in the database for a user with matching username AND hash
4. If found → login success; if not → show error

```csharp
    // Create identity claims (info stored in the cookie)
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, admin.Username),        // Who is logged in
        new Claim(ClaimTypes.Role, "Admin"),                // Their role
        new Claim("DisplayName", admin.DisplayName)        // Friendly name
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    // Sign in — creates an encrypted cookie in the browser
    await HttpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        principal);

    return RedirectToAction("Index", "Dashboard");   // Go to Dashboard
}
```

### What are Claims?
Claims are pieces of information about the logged-in user stored inside the cookie:
- "My name is admin"
- "My role is Admin"
- "My display name is Administrator"

The `[Authorize]` attribute on other controllers checks these claims to verify the user is logged in.

```csharp
public async Task<IActionResult> Logout()
{
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    // This deletes the authentication cookie
    return RedirectToAction("Login");
}
```

---

## 10. DashboardController.cs — Statistics & Overview

```csharp
[Authorize]   // ← Only logged-in admins can access this
public class DashboardController : Controller
```

### What `[Authorize]` does:
- If someone tries to visit `/Dashboard` without being logged in, they get redirected to `/Account/Login`
- This attribute protects the entire controller

```csharp
public async Task<IActionResult> Index()
{
    var today = DateOnly.FromDateTime(DateTime.Now);
    var weekStart = DateOnly.FromDateTime(DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek));
    // weekStart = the Sunday of this week

    // Get all check-ins from today (with student info)
    var todayLogs = await _db.AttendanceLogs
        .Include(a => a.Student)                 // JOIN with Students table
        .Where(a => a.Date == today)             // Filter: only today
        .OrderByDescending(a => a.CheckInTime)   // Sort: newest first
        .ToListAsync();

    // Count total logs this week
    var weekLogs = await _db.AttendanceLogs
        .Where(a => a.Date >= weekStart)
        .CountAsync();                           // SQL: SELECT COUNT(*)

    // Build the view model
    var model = new DashboardViewModel
    {
        TotalStudents = await _db.Students.CountAsync(),
        CheckedInToday = todayLogs.Count,
        OnTimeToday = todayLogs.Count(l => l.Status == AttendanceStatus.OnTime),
        LateToday = todayLogs.Count(l => l.Status == AttendanceStatus.Late),
        TotalLogsThisWeek = weekLogs,
        RecentActivity = recentActivity
    };

    return View(model);   // Pass the data to the Dashboard view
}
```

### Key EF Core Methods:
| Method | SQL Equivalent | Purpose |
|---|---|---|
| `.Include()` | `JOIN` | Load related data from another table |
| `.Where()` | `WHERE` | Filter rows |
| `.OrderByDescending()` | `ORDER BY ... DESC` | Sort newest first |
| `.CountAsync()` | `SELECT COUNT(*)` | Count matching rows |
| `.ToListAsync()` | Execute query | Run the query and get results as a list |

---

## 11. StudentsController.cs — Student Management

### Index Action (List Students):
```csharp
public async Task<IActionResult> Index(string? search)
{
    var query = _db.Students.AsQueryable();    // Start building a query

    if (!string.IsNullOrWhiteSpace(search))   // If search term provided
    {
        query = query.Where(s =>
            s.FullName.Contains(search) ||     // Search by name
            s.SchoolId.Contains(search));      // OR search by school ID
    }

    var students = await query.OrderBy(s => s.FullName).ToListAsync();
```

### What is `.AsQueryable()`?
It creates a **query builder** — the SQL is NOT executed yet. You can keep adding `.Where()`, `.OrderBy()`, etc. The query only runs when you call `.ToListAsync()`.

### Create Action (Add Student):
```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateStudentViewModel model)
{
    // Check for duplicate School ID
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
        CreatedAt = DateTime.Now
    };

    _db.Students.Add(student);          // Track the new student
    await _db.SaveChangesAsync();       // INSERT INTO Students ...

    TempData["Success"] = $"Student '{student.FullName}' added successfully.";
    return RedirectToAction("Index");   // Refresh the page
}
```

### What is `TempData`?
- `TempData` stores a message that survives **one redirect**
- After adding a student, we redirect back to the list page and show a success message
- The message disappears after being read once

### Delete Action:
```csharp
[HttpPost]
public async Task<IActionResult> Delete(int id)
{
    var student = await _db.Students.FindAsync(id);    // Find by primary key
    if (student != null)
    {
        var logs = _db.AttendanceLogs.Where(a => a.StudentId == id);
        _db.AttendanceLogs.RemoveRange(logs);   // Delete their attendance logs first
        _db.Students.Remove(student);            // Then delete the student
        await _db.SaveChangesAsync();            // Execute both DELETE operations
    }
    return RedirectToAction("Index");
}
```

### Why delete logs first?
The AttendanceLogs table has a **foreign key** to Students. If we try to delete a student while their logs still reference them, the database will throw an error. So we delete the logs first.

---

## 12. SchedulesController.cs — Schedule Management

### Create Action:
```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateScheduleViewModel model)
{
    // Convert string time to TimeSpan
    if (!TimeSpan.TryParse(model.StartTime, out var startTime) ||
        !TimeSpan.TryParse(model.EndTime, out var endTime))
    {
        TempData["Error"] = "Invalid time format.";
        return RedirectToAction("Index");
    }

    var schedule = new ClassSchedule
    {
        SubjectName = model.SubjectName.Trim(),
        DayOfWeek = model.DayOfWeek,        // e.g., DayOfWeek.Monday
        StartTime = startTime,               // e.g., 08:00
        EndTime = endTime,                   // e.g., 09:30
        GracePeriodMinutes = model.GracePeriodMinutes   // e.g., 15
    };

    _db.ClassSchedules.Add(schedule);
    await _db.SaveChangesAsync();
}
```

### What is `TimeSpan.TryParse`?
- HTML time inputs send time as a **string** like `"08:00"`
- `TryParse` tries to convert `"08:00"` → `TimeSpan(8, 0, 0)`
- Returns `true` if successful, `false` if the format is invalid
- The `out var startTime` stores the converted value

---

## 13. HistoryController.cs — Attendance Logs & Export

### Filtering:
```csharp
public async Task<IActionResult> Index(string? status, string? date)
{
    var query = _db.AttendanceLogs
        .Include(a => a.Student)     // JOIN Students table
        .Include(a => a.Schedule)    // JOIN ClassSchedules table
        .AsQueryable();

    // Apply status filter (if provided)
    if (!string.IsNullOrWhiteSpace(status) && status != "All")
    {
        if (Enum.TryParse<AttendanceStatus>(status, out var statusEnum))
        {
            query = query.Where(a => a.Status == statusEnum);
        }
    }

    // Apply date filter (if provided)
    if (!string.IsNullOrWhiteSpace(date) && DateOnly.TryParse(date, out var filterDate))
    {
        query = query.Where(a => a.Date == filterDate);
    }

    var logs = await query.OrderByDescending(a => a.CheckInTime).ToListAsync();
```

### CSV Export:
```csharp
public async Task<IActionResult> Export()
{
    var logs = await _db.AttendanceLogs
        .Include(a => a.Student)
        .Include(a => a.Schedule)
        .OrderByDescending(a => a.CheckInTime)
        .ToListAsync();

    var sb = new StringBuilder();
    // Write CSV header
    sb.AppendLine("Student Name,School ID,Action,Subject,Date,Time,Status,Notes");

    // Write each row
    foreach (var log in logs)
    {
        sb.AppendLine($"\"{log.Student?.FullName}\",...");  // Quoted for CSV safety
    }

    // Return as a downloadable file
    var bytes = Encoding.UTF8.GetBytes(sb.ToString());
    return File(bytes, "text/csv", $"attendance_logs_{DateTime.Now:yyyyMMdd}.csv");
    //                 ↑ MIME type   ↑ Filename: attendance_logs_20260404.csv
}
```

### What is `StringBuilder`?
- More efficient than concatenating strings with `+`
- `.AppendLine()` adds a new line each time
- Perfect for building CSV files row by row

---

## 14. Key C# Concepts Used

### `async` / `await`
```csharp
// Without async (blocks the thread — bad for web servers):
var students = _db.Students.ToList();

// With async (non-blocking — allows server to handle other requests while waiting):
var students = await _db.Students.ToListAsync();
```
**Why?** Web servers handle many users at once. `await` lets the server work on other requests while waiting for the database.

### LINQ (Language Integrated Query)
```csharp
// LINQ = writing database queries in C# instead of SQL
var lateStudents = await _db.AttendanceLogs
    .Where(a => a.Status == AttendanceStatus.Late)     // WHERE Status = 1
    .Include(a => a.Student)                            // JOIN Students
    .OrderByDescending(a => a.CheckInTime)              // ORDER BY CheckInTime DESC
    .ToListAsync();                                     // Execute the query
```

### Lambda Expressions (`=>`)
```csharp
// Lambda = a short inline function
s => s.FullName         // "Given s, return s.FullName"
s => s.Grade == "10"    // "Given s, return true if Grade equals 10"

// Equivalent to:
string GetFullName(Student s) { return s.FullName; }
bool IsGrade10(Student s) { return s.Grade == "10"; }
```

### Null Safety (`?` and `??`)
```csharp
student?.FullName          // If student is null, return null instead of crashing
matchedSchedule?.Id        // If matchedSchedule is null, return null

student?.FullName ?? "Unknown"    // If null, use "Unknown" as default
```

### `ViewBag` and `TempData`
```csharp
// ViewBag — pass data from Controller to View (same request only)
ViewBag.Error = "Something went wrong";    // In Controller
@ViewBag.Error                              // In View (Razor)

// TempData — pass data that survives one redirect
TempData["Success"] = "Student added!";     // In Controller (before redirect)
@TempData["Success"]                        // In View (after redirect)
```

---

> **Student Attendance Logging System** — Cebu Eastern College Final Project  
> Built with C# ASP.NET MVC + SQLite

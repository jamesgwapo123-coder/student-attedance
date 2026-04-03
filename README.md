# 📋 Student Attendance Logging System

A web-based attendance tracking system built with **C# ASP.NET MVC** and **SQLite** for Cebu Eastern College.

Students check in using their School ID, and the system automatically determines if they are **On Time** or **Late** based on pre-defined class schedules.

---

## ✨ Features

- **Student Check-In** — Students enter their School ID; time is captured automatically
- **On Time / Late Detection** — Compares check-in time against class schedules with configurable grace periods
- **Admin Dashboard** — Real-time stats: total students, checked in today, on time, late
- **Student Management** — Add, search, and delete student records
- **Schedule Management** — Define class subjects, days, times, and grace periods
- **Attendance History** — Filterable logs with CSV export
- **Secure Admin Login** — Cookie-based authentication with SHA-256 password hashing

---

## 🛠 Tech Stack

| Technology | Purpose |
|---|---|
| C# / .NET 8.0 | Backend framework |
| ASP.NET MVC | Web application pattern |
| Entity Framework Core | Database ORM |
| SQLite | Lightweight file-based database |
| Razor Views | Server-side HTML templates |
| Lucide Icons | Modern SVG icon library |

---

## 🚀 Quick Start

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

### Run

```bash
cd AttendanceSystem
dotnet restore
dotnet run --urls "http://localhost:5000"
```

Open `http://localhost:5000` in your browser.

### Default Admin Login

| Field | Value |
|---|---|
| Username | `admin` |
| Password | `admin123` |

---

## 📁 Project Structure

```
AttendanceSystem/
├── Controllers/          ← Request handlers (MVC logic)
│   ├── HomeController.cs         # Student check-in
│   ├── AccountController.cs      # Admin login/logout
│   ├── DashboardController.cs    # Stats overview
│   ├── StudentsController.cs     # Student CRUD
│   ├── SchedulesController.cs    # Schedule CRUD
│   └── HistoryController.cs      # Logs + CSV export
├── Models/               ← Database entities
├── Views/                ← Razor HTML templates
├── Data/AppDbContext.cs  ← EF Core + SQLite config
├── wwwroot/css/site.css  ← Stylesheet (white theme)
└── Program.cs            ← App entry point
```

---

## 📖 Documentation

| Document | Description |
|---|---|
| [DOCUMENTATION.md](AttendanceSystem/DOCUMENTATION.md) | Technical guide — installation, architecture, Mermaid diagrams, database schema |
| [USER_MANUAL.md](AttendanceSystem/USER_MANUAL.md) | Non-technical guide — step-by-step usage for students and admins |
| [CODE_EXPLANATION.md](AttendanceSystem/CODE_EXPLANATION.md) | Line-by-line code breakdown of all 13 source files |

---

## 📊 How It Works

```
Student enters School ID
        ↓
System looks up student in database
        ↓
Checks if already checked in today
        ↓
Finds today's class schedule
        ↓
Compares check-in time vs. (Start Time + Grace Period)
        ↓
   ✅ On Time  or  ⚠️ Late
        ↓
Saves attendance log to database
```

---

## 👥 Team

**Cebu Eastern College** — BS Information Technology Final Project

---

> Built with ❤️ using C# ASP.NET MVC + SQLite

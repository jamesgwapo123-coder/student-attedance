# 📖 User Manual — Student Attendance Logging System

### Cebu Eastern College — Final Project

---

## Table of Contents

1. [Getting Started](#getting-started)
2. [For Students — How to Check In](#for-students--how-to-check-in)
3. [For Admin — Logging In](#for-admin--logging-in)
4. [Admin — Dashboard](#admin--dashboard)
5. [Admin — Managing Students](#admin--managing-students)
6. [Admin — Managing Class Schedules](#admin--managing-class-schedules)
7. [Admin — Viewing Attendance History](#admin--viewing-attendance-history)
8. [Admin — Exporting Records](#admin--exporting-records)
9. [Admin — Logging Out](#admin--logging-out)
10. [Frequently Asked Questions (FAQ)](#frequently-asked-questions-faq)

---

## Getting Started

### Starting the System

1. Open a **terminal** (Command Prompt on Windows)
2. Navigate to the project folder:
   ```
   cd AttendanceSystem
   ```
3. Run the server:
   ```
   dotnet run --urls "http://localhost:5000"
   ```
4. Open your web browser and go to:
   ```
   http://localhost:5000
   ```

> The system is now running. Students can check in from any device connected to the same network.

---

## For Students — How to Check In

### Step 1: Open the Check-In Page

Open your browser and navigate to `http://localhost:5000`. You will see the **Student Attendance** check-in page.

### Step 2: Enter Your School ID

Type your School ID into the text field (e.g., `STU-001`).

> ⚠️ **Note:** Your School ID must be registered by the admin first. If you see "Student ID not found," ask your teacher to register you.

### Step 3: Click "Check In"

Press the **Check In** button. The system will:

1. **Look up your name** in the database
2. **Check the current time** against today's class schedule
3. **Determine your status:**
   - ✅ **On Time** — You checked in within the allowed grace period
   - ⚠️ **Late** — You checked in after the grace period expired

### Step 4: View Your Result

A confirmation card will appear showing:

| Field | Description |
|---|---|
| **Name** | Your registered full name |
| **ID** | Your School ID |
| **Time** | The exact time you checked in |
| **Subject** | The current class (e.g., "Mathematics") |
| **Status** | On Time or Late |

### Important Rules for Students

- You can only check in **once per day**
- You do **not** need to create an account — just use your School ID
- If there is no class scheduled at the time you check in, your attendance will still be recorded as a general check-in

---

## For Admin — Logging In

### Step 1: Go to the Login Page

From the student check-in page, click **"Admin Login →"** at the bottom of the page.

Or go directly to: `http://localhost:5000/Account/Login`

### Step 2: Enter Your Credentials

| Field | Default Value |
|---|---|
| Username | `admin` |
| Password | `admin123` |

### Step 3: Click "Sign In"

You will be redirected to the **Dashboard**.

---

## Admin — Dashboard

The Dashboard is the first page you see after logging in. It provides a real-time overview of today's attendance.

### Stat Cards

| Card | What It Shows |
|---|---|
| **Total Students** | Number of registered students in the system |
| **Checked In Today** | How many students have checked in today |
| **On Time** | Number of students who checked in on time today |
| **Late** | Number of students who checked in late today |

### Recent Activity

Below the stat cards, you'll see the **Recent Activity** section — a list of today's check-ins showing the student name, time, and status.

---

## Admin — Managing Students

### Accessing the Students Page

Click **"Students"** in the left sidebar.

### Viewing Students

You will see a table with all registered students:

| Column | Description |
|---|---|
| Student Name | Full name of the student |
| School ID | Their unique identifier |
| Grade | Grade level or section |
| Last Check-In | When they last checked in |
| Status | Their last recorded status |
| Actions | Delete button |

### Searching for a Student

Use the **search bar** at the top of the page. Type a name or School ID and click **"Search"**.

### Adding a New Student

1. Click the **"+ Add Student"** button (top-right corner)
2. A modal window will appear
3. Fill in the details:

| Field | Required | Example |
|---|---|---|
| School ID | ✅ Yes | `STU-001` |
| Full Name | ✅ Yes | `Juan Dela Cruz` |
| Grade / Section | No | `Grade 10 - A` |

4. Click **"Add Student"**
5. The student will appear in the table

> ⚠️ Each School ID must be **unique**. You cannot add two students with the same ID.

### Deleting a Student

Click the **"Delete"** button next to the student you want to remove. This action is **permanent** and will also delete all their attendance records.

---

## Admin — Managing Class Schedules

### Accessing the Schedules Page

Click **"Schedules"** in the left sidebar.

### Why Schedules Matter

The system uses class schedules to automatically determine if a student is **On Time** or **Late**. Without schedules, the system cannot compare the check-in time against anything.

### Viewing Schedules

You will see a table showing all defined class schedules:

| Column | Description |
|---|---|
| Subject | Name of the class (e.g., "Mathematics") |
| Day | Day of the week |
| Start Time | When the class begins |
| End Time | When the class ends |
| Grace Period | Minutes allowed after start time to still be "On Time" |
| Actions | Delete button |

### Adding a New Schedule

1. Click the **"+ Add Schedule"** button
2. Fill in the form:

| Field | Required | Example |
|---|---|---|
| Subject Name | ✅ Yes | `Mathematics` |
| Day of Week | ✅ Yes | `Monday` |
| Start Time | ✅ Yes | `08:00` |
| End Time | ✅ Yes | `09:30` |
| Grace Period (minutes) | ✅ Yes | `15` |

3. Click **"Add Schedule"**

### Understanding the Grace Period

The grace period determines the cutoff between "On Time" and "Late":

```
Example:
  Class Start:   8:00 AM
  Grace Period:   15 minutes
  Cutoff Time:   8:15 AM

  ✅ Check in at 8:10 AM → ON TIME
  ⚠️ Check in at 8:20 AM → LATE
```

### Tips for Setting Up Schedules

- Create a schedule for **each class period** on **each day**
- Example: If "Mathematics" happens Monday and Wednesday, create **two separate schedules**
- Set the grace period based on your school's policy (common values: 10 or 15 minutes)

### Deleting a Schedule

Click the **"Delete"** button next to the schedule to remove it.

---

## Admin — Viewing Attendance History

### Accessing the History Page

Click **"History"** in the left sidebar.

### Viewing Records

The history page shows a table of **all attendance records** with:

| Column | Description |
|---|---|
| Student Name | Who checked in |
| School ID | Their identifier |
| Subject | Which class they checked into |
| Status | On Time or Late |
| Date | The date of check-in |
| Time | The exact time of check-in |

### Filtering Records

Use the filter buttons at the top:

| Filter | What It Shows |
|---|---|
| **All** | Every attendance record |
| **On Time** | Only records with "On Time" status |
| **Late** | Only records with "Late" status |

You can also filter by **date** using the date picker.

### Summary Bar

At the bottom of the table, a summary bar shows:
- **Total records** currently displayed
- **Number of On Time** records
- **Number of Late** records

---

## Admin — Exporting Records

### How to Export Attendance as CSV

1. Go to the **History** page
2. Click the **"Export CSV"** button (top-right corner)
3. A `.csv` file will be downloaded to your computer

### What's in the CSV File?

The exported file contains these columns:

| Column | Example |
|---|---|
| Student Name | Juan Dela Cruz |
| School ID | STU-001 |
| Subject | Mathematics |
| Status | OnTime |
| Date | 2026-04-04 |
| Check-In Time | 07:55 AM |

### Opening the CSV File

- **Microsoft Excel** — Double-click the file
- **Google Sheets** — Upload the file to Google Drive → Open with Google Sheets
- **LibreOffice Calc** — Right-click → Open with LibreOffice Calc

---

## Admin — Logging Out

Click the **"Logout"** button at the bottom of the left sidebar. You will be redirected back to the student check-in page.

---

## Frequently Asked Questions (FAQ)

### Q: A student says their ID was not found? 
**A:** The student must be registered first. Go to **Students** → **+ Add Student** and enter their School ID and name.

### Q: A student was marked "Late" but arrived on time?
**A:** Check the class schedule in **Schedules**. Make sure:
- The correct **day of the week** is selected
- The **start time** is accurate
- The **grace period** is set properly (try increasing it)

### Q: A student checked in but there's no status / it says "General"?
**A:** There is no class schedule defined for the current day and time. Go to **Schedules** and add the correct class schedule.

### Q: Can a student check in twice in one day?
**A:** No. The system only allows **one check-in per student per day**. If they try again, it will show "You have already checked in today!"

### Q: How do I reset all data and start fresh?
**A:** 
1. Stop the server (press `Ctrl + C` in the terminal)
2. Delete the `attendance.db` file in the AttendanceSystem folder
3. Start the server again — a fresh database will be created automatically

### Q: Can I change the admin password?
**A:** Currently, the default admin password is hardcoded during database creation. To change it, you would need to modify the seed data in `Data/AppDbContext.cs` and recreate the database.

### Q: Can multiple people use the system at the same time?
**A:** Yes! The system supports multiple users simultaneously. Multiple students can check in at the same time, and the admin can manage the system while students are checking in.

### Q: Does the system work on phones?
**A:** Yes! The interface is responsive. Students can open the check-in page on their phone's browser — just make sure they're connected to the same network as the server.

---

> **Student Attendance Logging System** — Cebu Eastern College Final Project  
> Built with C# ASP.NET MVC + SQLite

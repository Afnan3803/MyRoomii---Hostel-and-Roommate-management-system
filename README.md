# MyRoomii — Hostel & Roommate Management System

A full-stack database-driven platform for university students to find hostels and compatible roommates. Built with C# (.NET), Microsoft SQL Server, and Entity Framework Core as a 3-phase academic project at LUMS.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | C# · .NET Framework |
| ORM | Entity Framework Core · LINQ |
| Database | Microsoft SQL Server (SSMS) |
| Design Pattern | Factory Pattern |

---

## Features

- **Roommate Matching** — compatibility scoring algorithm based on budget, cleanliness level, noise tolerance, sleep schedule, and lifestyle preferences
- **Dual BLL** — Business Logic Layer switchable at runtime between LINQ/EF and Stored Procedure implementations via a shared `IRoommateService` interface and the Factory Design Pattern
- **Hostel Listings** — landlords can create and manage room listings with amenities (AC, WiFi, parking, pets, etc.)
- **Messaging** — chat system between matched users
- **Reviews** — students can rate and review landlords and listings
- **User Verification** — verification status tracking per user

---

## Database Highlights

- 1M+ rows distributed across 15+ relational tables
- Fully normalized to 3NF with enforced primary/foreign keys and constraints
- Single-script zero-touch deployment — run `group59_p2.sql` on any SQL Server instance

### SQL Server Features Implemented

| Feature | Usage |
|---|---|
| Stored Procedures | Core CRUD and business operations (SP-based BLL) |
| User-Defined Functions | Compatibility score calculation, utility helpers |
| Triggers (AFTER) | Auto-update landlord rating on new review |
| Triggers (INSTEAD OF) | Safe deletion with cascading logic |
| CTEs | Roommate matching pipeline, ranked results |
| Views | Aggregated listing summaries, active user views |
| Indexes | Composite indexes on frequently queried columns |
| Table Partitioning | Partitioned across high-volume tables |

---

## Project Structure

    MyRoomii/
    ├── Phase3/
    │   ├── Entities.cs          # EF entity classes
    │   ├── MyRoomiiContext.cs   # DbContext configuration
    │   ├── Services.cs          # IRoommateService + LINQ & SP implementations
    │   ├── DbHelper.cs          # Raw SQL / SP helper methods
    │   └── Program.cs           # Entry point + Factory pattern instantiation
    ├── group59_p2.sql           # Full DB creation & population script
    └── dbproj.sln

---

## Getting Started

### Prerequisites
- Visual Studio 2022+
- Microsoft SQL Server + SSMS
- .NET 8 SDK

### Setup

1. **Clone the repo**
```bash
   git clone https://github.com/Afnan3803/MyRoomii---Hostel-and-Roommate-management-system.git
   cd MyRoomii---Hostel-and-Roommate-management-system
```

2. **Set up the database**
   - Open SSMS and connect to your SQL Server instance
   - Run `group59_p2.sql` — this creates and populates the entire database from scratch

3. **Configure connection string**
   - Open `MyRoomiiContext.cs`
   - Update the connection string with your server name and credentials

4. **Run the application**
   - Open `dbproj.sln` in Visual Studio
   - Build and run `Phase3`

---

## Authors

Built as a Database Systems course project at LUMS.

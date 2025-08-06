# ElevateHR Solutions 🚀

> **Where People Rise** - A comprehensive Human Capital Management (HCM) platform built with ASP.NET Core 8, designed to streamline employee management, payroll tracking, and organizational operations.

![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core%208.0-green)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-purple)
![License](https://img.shields.io/badge/License-MIT-yellow)

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Screenshots](#screenshots)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Detailed Setup](#detailed-setup)
- [Default Credentials](#default-credentials)
- [Project Structure](#project-structure)
- [User Roles & Permissions](#user-roles--permissions)
- [API Documentation](#api-documentation)
- [Technologies Used](#technologies-used)
- [Contributing](#contributing)
- [License](#license)
- [Support](#support)

## 🎯 Overview

ElevateHR Solutions is a modern, web-based Human Capital Management system that provides organizations with the tools they need to manage their workforce effectively. Built with security, scalability, and user experience in mind, it offers role-based access control and comprehensive employee management capabilities.

### Key Highlights
- 🔐 **Secure Authentication** - ASP.NET Core Identity with role-based authorization
- 📊 **Comprehensive Reporting** - Employee statistics, salary analytics, and department reports
- 🎨 **Modern UI** - Glass morphism design with responsive Bootstrap 5 layout
- 🔄 **Real-time Updates** - Live salary history tracking and audit trails
- 🏥 **Health Monitoring** - Built-in health checks and monitoring endpoints
- 📱 **Responsive Design** - Works seamlessly on desktop, tablet, and mobile devices

## ✨ Features

### 👥 Employee Management
- **Employee Directory** - Comprehensive employee profiles with search and filtering
- **Role-based Access** - Employees, Managers, and HR Admins with different permission levels
- **Personal Profiles** - Secure access to personal information and employment details
- **Department Assignment** - Organize employees into departments with manager oversight

### 💰 Compensation Management
- **Salary Tracking** - Complete salary history with change tracking
- **Audit Trails** - Track who made salary changes and when
- **Analytics** - Department-wise salary analytics and reporting
- **History Views** - Personal salary history for employees

### 🏢 Department Management
- **Department Creation** - Create and manage organizational departments
- **Employee Assignment** - Move employees between departments
- **Manager Oversight** - Department managers can manage their team members
- **Statistics & Reports** - Department performance and employee statistics

### 📊 Reporting & Analytics
- **Employee Statistics** - Comprehensive workforce analytics
- **Salary Analytics** - Compensation analysis across departments
- **Department Reports** - Detailed department performance reports
- **Export Capabilities** - Data export for external analysis

### 🔒 Security Features
- **Authentication** - Secure login with ASP.NET Core Identity
- **Authorization** - Role-based access control (RBAC)
- **Data Protection** - Employees can only access their own data
- **Audit Logging** - Complete audit trails for all sensitive operations

## 📸 Screenshots

> Note: Add screenshots of your application here to showcase the UI

## 🛠 Prerequisites

Before running ElevateHR Solutions, ensure you have the following installed:

- **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** (8.0 or later)
- **SQL Server** (LocalDB, Express, or Full version)
  - SQL Server LocalDB comes with Visual Studio
  - Or download [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- **IDE** (recommended):
  - [Visual Studio 2022](https://visualstudio.microsoft.com/) (Community, Professional, or Enterprise)
  - Or [Visual Studio Code](https://code.visualstudio.com/) with C# extension

## 🚀 Quick Start

Get up and running in under 5 minutes:

```bash
# 1. Clone the repository
git clone <your-repository-url>
cd HCM_D

# 2. Restore NuGet packages
dotnet restore

# 3. Update database (creates database and applies migrations)
dotnet ef database update

# 4. Run the application
dotnet run

# 5. Open your browser and navigate to:
# https://localhost:7213 (or the URL shown in your terminal)
```

## 📖 Detailed Setup

### Step 1: Clone and Navigate
```bash
git clone <your-repository-url>
cd HCM_D
```

### Step 2: Configure Database (Optional)
The application uses SQL Server LocalDB by default. If you want to use a different database:

1. Open `appsettings.json`
2. Update the connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your-Connection-String-Here"
  }
}
```

### Step 3: Install Dependencies
```bash
dotnet restore
```

### Step 4: Database Setup
```bash
# Apply migrations and create database
dotnet ef database update

# Optional: If you need to create a new migration
dotnet ef migrations add YourMigrationName
```

### Step 5: Run the Application
```bash
# Development mode
dotnet run

# Or with specific environment
dotnet run --environment Development
```

### Step 6: Access the Application
- **HTTPS**: https://localhost:7213
- **HTTP**: http://localhost:5279
- **Swagger API Docs**: https://localhost:7213/swagger
- **Health Checks**: https://localhost:7213/health

## 🔑 Default Credentials

The application automatically creates a default admin user:

| Role | Email | Password | Description |
|------|-------|----------|-------------|
| HR Admin | `admin@hr.com` | `Admin@123` | Full system access |

**⚠️ Important**: Change the default password after first login for security.

### Creating Additional Users
1. Register new users through the registration page
2. Assign roles via the admin interface or database
3. Available roles: `HR Admin`, `Manager`, `Employee`

## 📁 Project Structure

```
HCM_D/
├── Areas/                          # ASP.NET Core Areas
│   └── Identity/                   # Authentication pages
├── Controllers/                    # API Controllers
│   └── Api/                       # REST API endpoints
├── Data/                          # Database context and migrations
├── HealthChecks/                  # Health monitoring
├── Middleware/                    # Custom middleware
├── Models/                        # Data models
├── Pages/                         # Razor Pages
│   ├── Departments/              # Department management
│   ├── Employees/                # Employee management
│   ├── SalaryHistories/          # Salary tracking
│   └── Shared/                   # Shared layouts and partials
├── Services/                      # Business logic services
├── wwwroot/                       # Static files (CSS, JS, images)
└── Program.cs                     # Application entry point
```

## 👤 User Roles & Permissions

### 🔴 HR Admin
- **Full Access**: Complete system administration
- **Employee Management**: Create, read, update, delete all employees
- **Department Management**: Full department control
- **Salary Management**: Modify salaries and view all history
- **Reports**: Access to all analytics and reports

### 🟡 Manager
- **Department Access**: Manage employees in their assigned department
- **Employee Management**: Edit employees in their department
- **Salary Management**: Modify salaries for their team members
- **Reports**: View department-specific analytics

### 🟢 Employee
- **Personal Access**: View own profile and information
- **Salary History**: View personal salary change history
- **Restricted Access**: Cannot access other employees' data

## 📚 API Documentation

ElevateHR Solutions includes a comprehensive REST API with Swagger documentation.

### Accessing API Documentation
- **Swagger UI**: https://localhost:7213/swagger
- **OpenAPI Spec**: https://localhost:7213/swagger/v1/swagger.json

### Key API Endpoints

| Endpoint | Method | Description | Authorization |
|----------|--------|-------------|---------------|
| `/api/employees` | GET | Get all employees | HR Admin, Manager |
| `/api/employees/{id}` | GET | Get employee by ID | HR Admin, Manager |
| `/api/employees` | POST | Create new employee | HR Admin |
| `/api/employees/{id}` | PUT | Update employee | HR Admin, Manager |
| `/api/departments` | GET | Get all departments | HR Admin, Manager |
| `/api/departments/{id}` | GET | Get department details | HR Admin, Manager |

### Authentication
The API uses cookie-based authentication. Authenticate through the web interface to access API endpoints.

## 🛠 Technologies Used

### Backend
- **Framework**: ASP.NET Core 8.0
- **ORM**: Entity Framework Core 8.0
- **Database**: SQL Server
- **Authentication**: ASP.NET Core Identity
- **API Documentation**: Swagger/OpenAPI

### Frontend
- **UI Framework**: Bootstrap 5.3
- **Icons**: Bootstrap Icons
- **Styling**: Custom CSS with Glass Morphism design
- **JavaScript**: Vanilla JavaScript with Bootstrap JS

### Development & Deployment
- **Package Manager**: NuGet
- **Health Checks**: ASP.NET Core Health Checks
- **Logging**: Built-in ASP.NET Core logging
- **Error Handling**: Global exception middleware

## 🔧 Configuration

### Environment Variables
Create an `appsettings.Production.json` for production settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your-production-connection-string"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

### Health Checks
Monitor application health at `/health`:
- Database connectivity
- Employee data integrity
- Entity Framework Core status

## 🤝 Contributing

We welcome contributions! Please follow these steps:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/AmazingFeature`)
3. **Commit** your changes (`git commit -m 'Add some AmazingFeature'`)
4. **Push** to the branch (`git push origin feature/AmazingFeature`)
5. **Open** a Pull Request

### Development Guidelines
- Follow C# coding conventions
- Add unit tests for new features
- Update documentation as needed
- Ensure all health checks pass

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📧 Support

### Getting Help
- 🐛 **Bug Reports**: [Create an issue](../../issues)
- 💡 **Feature Requests**: [Create an issue](../../issues)
- 📖 **Documentation**: Check this README and inline code comments

### Contact Information
- **Support Email**: support@elevatehr.com
- **Privacy Policy**: Available at `/Privacy` when running the application

### Common Issues

**Q: Database connection failed**
A: Ensure SQL Server is running and the connection string is correct.

**Q: Unable to login with default credentials**
A: Check that database migrations have been applied (`dotnet ef database update`).

**Q: API returns 401 Unauthorized**
A: Ensure you're authenticated through the web interface first.

---

<div align="center">

**Built with ❤️ by the ElevateHR Solutions Team**

[Report Bug](../../issues) · [Request Feature](../../issues) · [Documentation](README.md)

</div>
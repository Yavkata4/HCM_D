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
Admin Page screenshots -
<img width="1817" height="934" alt="image8" src="https://github.com/user-attachments/assets/1701ed58-62a1-4f21-9f9a-0e577793edd4" />
<img width="1817" height="930" alt="image7" src="https://github.com/user-attachments/assets/8540773b-04fd-4b91-8d84-b27290275679" />
<img width="1815" height="933" alt="image6" src="https://github.com/user-attachments/assets/d2c3347c-b7dd-4b14-814b-36ba0c198f0f" />
<img width="1815" height="932" alt="image5" src="https://github.com/user-attachments/assets/39c42d7c-53c3-47ea-a020-0400c6c26256" />
<img width="1812" height="930" alt="image4" src="https://github.com/user-attachments/assets/4ed6be7a-5505-4e74-bba3-448d11827a75" />
<img width="1820" height="929" alt="image3" src="https://github.com/user-attachments/assets/609d2dbd-0dcd-4dff-8dde-2a98adc4c71a" />
<img width="1810" height="930" alt="image 2png" src="https://github.com/user-attachments/assets/d0e45821-6fc5-44a1-80b0-8e6563b98a86" />
<img width="1809" height="935" alt="image" src="https://github.com/user-attachments/assets/5f4f8df6-3f5d-4bf4-996a-4f974a716e6c" />

Employee Login screenshots-
<img width="1810" height="934" alt="image3" src="https://github.com/user-attachments/assets/c5be9b40-9813-46be-a964-22aa435f6c73" />
<img width="1815" height="932" alt="image2" src="https://github.com/user-attachments/assets/96791544-2590-4a0c-a046-13083f4d8f5c" />
<img width="1816" height="933" alt="image" src="https://github.com/user-attachments/assets/241dd65c-1226-4be6-98b2-897514677095" />

Manager Login screenshots-
<img width="1819" height="931" alt="image7" src="https://github.com/user-attachments/assets/8a8333ac-84b6-43bd-aa14-1026307df0d5" />
<img width="1819" height="928" alt="image6" src="https://github.com/user-attachments/assets/fae4816c-430d-40ad-902f-2f7f1e7adaea" />
<img width="1815" height="937" alt="image5" src="https://github.com/user-attachments/assets/ad539f7f-a49f-4606-b176-e8eb48ada773" />
<img width="1802" height="931" alt="image4" src="https://github.com/user-attachments/assets/fa662634-8902-4160-843e-012f1108e5fa" />
<img width="1813" height="937" alt="image3" src="https://github.com/user-attachments/assets/b9b00edd-638e-47d5-97c6-e91206addbee" />
<img width="1799" height="926" alt="image2" src="https://github.com/user-attachments/assets/4dbf0cdc-d7e7-4b35-93fb-b782515947e3" />
<img width="1821" height="937" alt="image" src="https://github.com/user-attachments/assets/f6912081-2adc-4033-a9e2-064983b8ac2b" />

Other screenshots -
Logged out view-
<img width="1811" height="937" alt="logged out" src="https://github.com/user-attachments/assets/cb6d6795-dc9b-40ff-bb14-af5990644d40" />

Privacy Policy -
<img width="1819" height="933" alt="Privacy Policy" src="https://github.com/user-attachments/assets/7d700624-40e2-43c4-8dd4-7efbbe83e7c9" />
olicy

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
 Manager    manager@company.com  Manager@123
 Employee   employee@company.com Employee@123


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

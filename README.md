# BarbershopServer
This repository contains the full source code for the Barbershop application, including the API layer, business logic, data access layer, and shared entity classes. This project handles client requests, processes business rules, and interacts with the database to manage appointments and users in a barbershop setting.



## Overview

The API consists of several controllers that handle various operations related to appointments and users. The following sections describe the available endpoints and their functionalities.
### Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [Visual Studio](https://visualstudio.microsoft.com/)

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/saraKenzi/BarbershopProject.git
   ```
2. Navigate to the project directory:
   ```bash
   cd BarbershopProject
   ```
3. Restore the project dependencies:
   ```bash
   dotnet restore
   ```
4. Update the appsettings.json file with your database connection string and other configurations if necessary.
  
## Running the Project
  Build and run the project:
  ```bash
  dotnet run
   ```


## AppointmentController

### Endpoints

| CRUD   | Response              | Explain                             | Method | Send in Body            | URL                                           | Query Params       |
|--------|-----------------------|-------------------------------------|--------|-------------------------|-----------------------------------------------|--------------------|
| Create | StatusCode, Message, MyObject | Create a new appointment            | POST   | AppointmentToAddDTO     | `/api/appointment/CreateNewAppointment`       | ----               |
| Read   | StatusCode, MyObject  | Get all appointments                | GET    | ----                    | `/api/appointment/GetAllAppointments`         | `page`, `perPage`  |
| Update | StatusCode, Message, MyObject | Update an existing appointment      | PUT    | AppointmentToAddDTO     | `/api/appointment/UpdateAppointmentDate/{appointmentId}` | ----               |
| Delete | StatusCode, Message, MyObject | Delete an appointment               | DELETE | ----                    | `/api/appointment/DeleteAppointment/{appointmentId}` | ----               |
| Read   | StatusCode, MyObject  | Get future appointments count       | GET    | ----                    | `/api/appointment/GetFutureAppointmentsCount` | ----               |

## UserController

### Endpoints

| CRUD   | Response              | Explain                 | Method | Send in Body | URL                  | Query Params     |
|--------|-----------------------|-------------------------|--------|--------------|----------------------|------------------|
| Create | StatusCode, Message, MyObject | Add a new user            | POST   | UserToAddDTO | `/api/user/AddUser`          | ----             |
| Read   | StatusCode, MyObject  | User login              | POST   | UserLoginDTO | `/api/user/Login`            | ----             |
| Read   | StatusCode, MyObject  | Get user details by ID  | GET    | ----         | `/api/user/GetUserDetailsByUserId/{userId}` | ----             |
| Read   | StatusCode, MyObject  | Get user ID by first name | GET    | ----         | `/api/user/GetUserIdByFirstName/{userName}` | ----             |
| Delete | StatusCode, Message   | User logout             | GET    | ----         | `/api/user/Logout`           | ----             |

## Configuration

The application configuration is managed through the `appsettings.json` file. This includes settings for the database connection, JWT authentication, and logging.

Example `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Barbershop": "Data Source=DESKTOP-A8CKV4D\\SQLEXPRESS; Initial Catalog=BarbershopDB; Integrated Security=True; Trusted_Connection=True; TrustServerCertificate=True;"
  },
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Error",
        "System": "Error"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/log.txt",
          "rollingInterval": "Day"
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ]
  },
  "Jwt": {
    "SecretKey": "saraarassaraarassaraarassaraarassaraarassaraarassaraarassaraarassaraarassaraarassaraaras",
    "Issuer": "http://localhost:44369",
    "Audience": "http://localhost:44369",
    "ExpireMinutes": 10
  }
}
```
## Logging
Logging is configured using Serilog. Logs are written to the console and to a file with a rolling interval of one day.

## Authentication
JWT (JSON Web Token) is used for authenticating requests. The token is generated when a user logs in and is required for accessing protected endpoints


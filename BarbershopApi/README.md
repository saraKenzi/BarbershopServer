# Barbershop API

This project is a Barbershop API that allows users to manage appointments and user information. The API is built using ASP.NET Core.

## Controllers

### AppointmentController

- **GetAllAppointments**: Retrieves all existing appointments.
- **CreateNewAppointment**: Schedules a new appointment.
- **UpdateAppointmentDate**: Updates an existing appointment.
- **DeleteAppointment**: Deletes an existing appointment.
- **GetFutureAppointmentsCount**: Retrieves the count of future appointments.

### UserController

- **AddUser**: Adds a new user.
- **Login**: Logs in a user.
- **Logout**: Logs out a user.
- **GetUserDetailsByUserId**: Retrieves user details by user ID.
- **GetUserIdByFirstName**: Retrieves user ID by first name.

## Configuration

The project configuration is stored in the `appsettings.json` file. Below is an example of the configuration:

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

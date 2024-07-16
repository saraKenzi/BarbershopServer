create table Appointment
(AppointmentID int primary key identity (1,1) not null,
UserID int foreign key references Users(UserID),
AppointmentDate dateTime not null,
CreatedDate dateTime default getdate())
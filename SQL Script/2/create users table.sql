create table Users
(UserID int primary key identity (1,1),
FirstName varchar(50),
LastName varchar (50),
Phone varchar (15),
UserName varchar (50) unique not null,
[Password] varchar (100) not null

)


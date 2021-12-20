use sit_connect
go

create table Audits
(
    LogId bigint not null,
    ActorId bigint,
    LogType nvarchar(max) not null,
    Timestamp datetime not null,
    IpAddress nvarchar(max)
)
    go

create unique index Audits_LogId_uindex
    on Audits (LogId)
    go

create table OtpCodes
(
    Code int not null
        constraint OtpCodes_pk
            primary key nonclustered,
    UserId bigint not null,
    Timestamp datetime not null
)
    go

create unique index OtpCodes_Code_uindex
    on OtpCodes (Code)
    go

create table Users
(
    Id bigint not null
        constraint Users_pk
            primary key nonclustered,
    Email nvarchar(max) not null,
    Password nvarchar(max) not null,
    FirstName nvarchar(max) not null,
    LastName nvarchar(max) not null,
    BillingCardNo nvarchar(max) not null,
    DateOfBirth date not null,
    Photo varbinary(max)
)
    go

create unique index User_Id_uindex
    on Users (Id)
    go


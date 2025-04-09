dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design

dotnet tool install --global dotnet-ef

dotnet ef migrations add InitialCreate
dotnet ef database update

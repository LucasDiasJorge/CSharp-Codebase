dotnet add package MySql.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=meubanco;User=root;Password=minhasenha;"
  }
}

dotnet ef migrations add Inicial
dotnet ef database update
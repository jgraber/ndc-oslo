# ndc-oslo
Workshop designing APIs
-Have sql management studio/preferred db access tool with the local db instance
- Updated prefered IDE
- Docker installed
   - RabbitMQ
     ```
     docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.13-management
     ```

   - Optional: SQL Server installed
     ```
      docker pull mcr.microsoft.com/mssql/server:2022-latest
      docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Password" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
     ```

Connection string for SQL Server running in Docker
```
"DefaultConnection": "Server=localhost,1433;Database=ProductDB;User=sa;Password=YourStrong@Password;TrustServerCertificate=True;"
```

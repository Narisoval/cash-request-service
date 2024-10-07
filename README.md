# 🏦 Cash Request Service - Test Assignment

Welcome to the **Cash Request Service**! This service is designed to handle the registration of cash withdrawal requests at bank branches and is fully prepared to run in Docker using `docker-compose`. The system includes an ASP.NET Core Web API for handling requests, a backend service for processing these requests, and a database setup with either PostgreSQL or MSSQL.

---

## 📋 Requirements

### Functional Requirements

1. **API Endpoint** for creating a new cash withdrawal request:
   - Accepts the following JSON request:
     ```json
     {
       "client_id": "<some client id>",
       "department_address": "<address>",
       "amount": 1000.00,
       "currency": "UAH(or else)"
     }
     ```
   - Logs the request and sends it to RabbitMQ for further processing.
   - Performs validation: 
     - Amount should be between 100 and 100,000.
   
2. **API Endpoint** for retrieving the status of a cash request by `request_id`:
   ```json
   {
     "request_id": 1
   }
   ```

3. **API Endpoint** for retrieving the status of cash requests by `client_id` and `department_address`:
   ```json
   {
     "client_id": "<some client id>",
     "department_address": "<address>"
   }
   ```

4. **Backend Processor**:
   - Reads requests from RabbitMQ.
   - Saves the request to the database (PostgreSQL or MSSQL).
   - Returns the `id` of the saved request.
   - Responds with the request's details (amount, currency, status) based on `request_id` or a combination of `client_id` and `department_address`.

5. **Database Implementation**:
   - Interact with the database using **stored procedures** and **Dapper**.
   - A script for creating tables and stored procedures is provided in a `.sql` file.

6. **Testing**:
   - A comprehensive test project is included to validate the logic of the service.
   - Tests are executed in an isolated environment using **xUnit** and **TestContainers** to spin up services like the API, backend, database, and RabbitMQ.

---
## 🛠️ Setup Instructions

You can run the project using **Docker Compose** (recommended) or **locally**. Follow the instructions below based on your preferred setup.

### Option 1: Running with Docker Compose (Recommended)

1. **Clone the Repository**:
   ```bash
   git clone <repository-url>
   cd cash-request-service
   ```

2. **Run the Application Using Docker Compose**:
   To spin up the services (API, backend, PostgreSQL, RabbitMQ) using Docker Compose, run:
   ```bash
   docker-compose up --build
   ```

   This will start:
   - **API** service on port `5000`
   - **Backend** service on port `5001`
   - **PostgreSQL** on port `5234`
   - **RabbitMQ** on port `5672` (and the management UI on `15672`)

4. **Accessing the Services**:
   - API: `http://localhost:5000`
   - Backend: `http://localhost:5001`
   - RabbitMQ Management: `http://localhost:15672` (default credentials: guest/guest)

---

### Option 2: Running Locally

1. **Clone the Repository**:
   ```bash
   git clone <repository-url>
   cd cash-request-service
   ```

2. **Install Dependencies**:
   Make sure you have the following installed on your local machine:
   - .NET 8 SDK
   - PostgreSQL or MSSQL
   - RabbitMQ

3. **Add Database Connection String to User Secrets** (Backend Only):
   To keep the database connection string safe, use **.NET Secret Manager**. Run the following commands for the **backend project**:

   ```bash
   cd CashRequestService.Backend
   dotnet user-secrets init
   dotnet user-secrets set "RepositorySettings:ConnectionString" "Host=your_host;Database=your_db;Username=your_user;Password=your_password"
   ```

4. **Configure RabbitMQ**:
   Ensure RabbitMQ is running on your machine. You can download RabbitMQ [here](https://www.rabbitmq.com/download.html) and start it locally. By default, RabbitMQ listens on `localhost:5672`, with the management UI at `localhost:15672`.

5. **Run the Application**:
   Open separate terminal windows and run both the API and backend services:

   - **API Service**:
     ```bash
     cd CashRequestService.Api
     dotnet run
     ```

   - **Backend Service**:
     ```bash
     cd ../CashRequestService.Backend
     dotnet run
     ```

   By default, the API service will be available at `http://localhost:5000` and the backend at `http://localhost:5001`.

---

## 🔧 Configuration

The application configuration is managed through **appsettings.json** files. In the **backend project**, you can use the `DbType` field to switch between PostgreSQL and MSSQL by specifying the appropriate Unit of Work implementation.

### Example Configuration in `appsettings.json` (Backend Project):

```json
{
  "UnitOfWork": "PgSqlUnitOfWork",  // Use "MsSqlUnitOfWork" for MSSQL
  "MessageBroker": {
    "Host": "rabbitmq://localhost/",
    "Username": "guest",
    "Password": "guest"
  }
}
```

By default, the application is configured to use PostgreSQL. You can change the `"UnitOfWork"` value to `"MsSqlUnitOfWork"` if you want to use MSSQL instead. Other settings are set to their default values, but you can override them in the `appsettings.json` file or via environment variables as needed.


---

## 🧪 Running Tests

The test project is configured to use **xUnit** and **TestContainers** to spin up containers for PostgreSQL, RabbitMQ, and the services (API and backend). 

1. Add desirable db type(PGSQL/MSSQL) in appsettings.json

2. To run the tests:
   ```bash
   dotnet test
   ```
---


## 🚀 Technologies Used

- **ASP.NET Core 8** for Web API and backend services.
- **RabbitMQ** for messaging between services.
- **PostgreSQL / MSSQL** for the database.
- **Dapper** for data access with stored procedures.
- **xUnit** for testing.
- **TestContainers** for spinning up isolated test environments.
- **Docker** for containerization.

---

## ⚠️ Notes

- Ensure that all settings are set up correctly in your `docker-compose.yml` or `appsettings.json` files.
- Database initialization scripts are located in the `pgsql-db-init` and `mssql-db-init` folders, depending on your selected DB.

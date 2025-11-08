# Hotel API

A RESTful API for hotel management system built with ASP.NET Core. This project provides endpoints for user authentication, post management, comments, likes, and media handling, designed to integrate with a Flutter mobile application and a web frontend.

## Features

- **User Authentication**: JWT-based authentication with registration and login
- **Post Management**: Create, read, update, and delete posts with media support
- **Comments & Likes**: Full CRUD operations for comments and like functionality
- **Media URL Handling**: Automatic conversion of relative paths to full URLs for seamless integration with Hotel_Web project
- **Database Integration**: Entity Framework Core with SQL Server
- **API Testing**: Comprehensive integration tests with xUnit

## Technologies Used

- **Framework**: ASP.NET Core (.NET 9.0)
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: JWT (JSON Web Tokens)
- **Testing**: xUnit
- **Documentation**: Swagger/OpenAPI (via Swashbuckle)

## Prerequisites

- .NET 9.0 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code with C# extensions

## Installation

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd Hotel_API
   ```

2. **Restore packages**:
   ```bash
   dotnet restore
   ```

3. **Update database connection**:
   - Open `appsettings.json` and `appsettings.Development.json`
   - Update the `ConnectionStrings:DefaultConnection` with your SQL Server connection string

4. **Run database migrations** (if needed):
   ```bash
   dotnet ef database update
   ```

5. **Configure Hotel_Web URL**:
   - In `appsettings.json`, set `HotelWebUrl` to your Hotel_Web project's base URL (e.g., `https://localhost:5001`)

## Running the Application

1. **Development mode**:
   ```bash
   dotnet run --environment Development
   ```

2. **Production mode**:
   ```bash
   dotnet run --environment Production
   ```

The API will be available at `https://localhost:5001` (or configured port).

## Testing

Run the integration tests:
```bash
cd Hotel_API.Tests
dotnet test
```

All 27 tests should pass, covering authentication, post operations, comments, likes, and media URL handling.

## API Documentation

Comprehensive API documentation is available in the `Doc/` folder:

- [API Development Guide](Doc/API_DEVELOPMENT_GUIDE.md)
- [API Documentation](Doc/API_DOCUMENTATION.md)
- [API Quick Reference](Doc/API_QUICK_REFERENCE.md)
- [API Testing Guide](Doc/API_TESTING_GUIDE.md)
- [Media URL Service](Doc/MEDIA_URL_SERVICE.md)
- [Models Reference](Doc/MODELS_REFERENCE.md)

## Project Structure

```
Hotel_API/
├── Controllers/          # API controllers
├── Models/              # Entity models and ViewModels
├── Data/                # Database context
├── Services/            # Business logic services
├── Properties/          # Launch settings
├── Doc/                 # Documentation
├── Hotel_API.Tests/     # Integration tests
├── appsettings.json     # Configuration
└── Program.cs           # Application entry point
```

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is part of a graduation thesis. Please contact the author for licensing information.

## Contact

For questions or support, please refer to the documentation in the `Doc/` folder or contact the development team.
# Loyalty Rewards Platform API

A comprehensive .NET 9 API for managing loyalty rewards programs with user management, points tracking, and redemption features.

## 🚀 Features

- **User Management**: Registration, authentication, and profile management
- **JWT Authentication**: Secure token-based authentication
- **Points System**: Track points earned and redeemed
- **Rewards Catalog**: Manage available rewards
- **Third-party Integration**: Support for external applications
- **Audit Logging**: Track all user activities
- **RESTful API**: Clean, well-documented endpoints
- **Swagger Documentation**: Interactive API documentation

## 🛠️ Technology Stack

- **.NET 9** - Latest .NET framework
- **Entity Framework Core** - ORM for database operations
- **SQLite** - Lightweight database for development
- **JWT Bearer Authentication** - Secure authentication
- **BCrypt** - Password hashing
- **Swagger/OpenAPI** - API documentation
- **Docker** - Containerization support

## 📋 Prerequisites

- .NET 9 SDK
- Visual Studio 2022 or VS Code
- Git

## 🏃‍♂️ Quick Start

### 1. Clone the Repository
```bash
git clone <repository-url>
cd LoyaltyRewardsApi
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Update Database
```bash
dotnet ef database update
```

### 4. Run the Application
```bash
dotnet run
```

The API will be available at:
- **Local**: `http://localhost:5026`
- **Swagger**: `http://localhost:5026/swagger`

## 📚 API Endpoints

### Authentication
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/logout` - Logout (requires auth)
- `POST /api/auth/change-password` - Change password (requires auth)
- `POST /api/auth/logout-all` - Logout from all sessions (requires auth)

### User Management
- `GET /api/user` - Get all users (requires auth)
- `GET /api/user/{id}` - Get user by ID (requires auth)
- `PUT /api/user/{id}` - Update user (requires auth)
- `DELETE /api/user/{id}` - Delete user (admin only)

### Health Check
- `GET /health` - API health status

## 🔧 Configuration

### JWT Settings (appsettings.json)
```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key-here",
    "Issuer": "loyalty-rewards-api",
    "Audience": "loyalty-rewards-app",
    "ExpiryMinutes": 1440
  }
}
```

### Database Connection
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=loyalty_rewards.db"
  }
}
```

## 🐳 Docker Deployment

### Build Docker Image
```bash
docker build -t loyalty-rewards-api .
```

### Run Container
```bash
docker run -p 8080:10000 loyalty-rewards-api
```

## ☁️ Cloud Deployment

The application is ready for deployment to:
- Azure App Service
- Railway
- Render
- Heroku
- AWS

Deployment files included:
- `Dockerfile` - Container deployment
- `render.yaml` - Render.com deployment
- `railway.toml` - Railway deployment
- `web.config` - IIS/Azure deployment

## 🔐 Security Features

- **Password Hashing**: BCrypt with salt
- **JWT Tokens**: Secure, stateless authentication
- **Token Revocation**: Database-tracked token validity
- **Input Validation**: Comprehensive request validation
- **CORS Support**: Configurable cross-origin requests

## 📝 Database Schema

### Core Tables
- **Users**: User accounts and profiles
- **UserWallets**: Points balance tracking
- **PointTransactions**: Points earning/spending history
- **RewardsCatalog**: Available rewards
- **Redemptions**: Reward redemption records
- **UserSessions**: Active login sessions
- **ThirdPartyApps**: External application integrations
- **AuditLogs**: Activity tracking

## 🧪 Testing

### Test User Registration
```bash
curl -X POST http://localhost:5026/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "Test123!",
    "firstName": "Test",
    "lastName": "User"
  }'
```

### Test Login
```bash
curl -X POST http://localhost:5026/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!"
  }'
```

## 📈 Performance

- **Database Indexing**: Optimized queries with proper indexes
- **Caching**: Ready for Redis integration
- **Async/Await**: Non-blocking database operations
- **Connection Pooling**: Efficient database connections

## 🔍 Monitoring

- **Health Checks**: Built-in health endpoint
- **Logging**: Structured logging with Serilog-ready
- **Metrics**: Application performance tracking
- **Audit Trail**: Complete user activity logs

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 📄 License

This project is licensed under the MIT License.

## 📞 Support

For support, please contact [your-email@example.com] or create an issue in the repository.

---

**Built with ❤️ using .NET 9**

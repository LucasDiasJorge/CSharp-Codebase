# Cache Increment Project

This project demonstrates a high-performance caching pattern using **Redis for fast atomic increments** combined with **periodic persistence to MySQL** for durability. This is a common pattern used by large-scale systems like Facebook, YouTube, and Twitter.

## 🏗️ Architecture Overview

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Client API    │───▶│      Redis      │    │      MySQL      │
│   Requests      │    │  (Fast Counter) │◀──▶│  (Persistence)  │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                              │                         ▲
                              │                         │
                              └─── Background Service ──┘
                                   (Periodic Sync)
```

### Key Components:

1. **Redis**: Handles fast atomic increments (`INCR` operations)
2. **MySQL**: Provides durable storage and historical data
3. **Background Service**: Automatically syncs Redis counters to MySQL every N minutes
4. **REST API**: Provides endpoints for counter operations

## 🚀 Features

- ✅ **Ultra-fast increments** using Redis atomic operations
- ✅ **Automatic periodic sync** to MySQL for durability
- ✅ **Manual sync triggers** for immediate persistence
- ✅ **Sync status monitoring** to check Redis vs MySQL state
- ✅ **RESTful API** with comprehensive endpoints
- ✅ **Swagger documentation** for easy testing
- ✅ **Configurable sync intervals**
- ✅ **Error handling and logging**
- ✅ **Health check endpoint**

## 📋 Prerequisites

1. **.NET 8.0** or later
2. **MySQL Server** (running on localhost:3306)
3. **Redis Server** (running on localhost:6379)

### Installing Dependencies

**MySQL:**
```bash
# Windows (using Chocolatey)
choco install mysql

# Or download from: https://dev.mysql.com/downloads/mysql/
```

**Redis:**
```bash
# Windows (using Chocolatey)
choco install redis-64

# Or download from: https://github.com/MicrosoftArchive/redis/releases
```

## 🛠️ Setup Instructions

### 1. Configure Database Connection

Update `appsettings.json` with your MySQL credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CacheIncrementDb;User=root;Password=your_password;",
    "Redis": "localhost:6379"
  }
}
```

### 2. Create MySQL Database

```sql
CREATE DATABASE CacheIncrementDb;
```

### 3. Install NuGet Packages

```bash
dotnet restore
```

### 4. Run the Application

```bash
dotnet run
```

The API will be available at:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`
- **Swagger UI**: `http://localhost:5000` (in development)

## 📚 API Endpoints

### Counter Operations

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/counter/{counterId}/increment?incrementBy=1` | Increment counter |
| `GET` | `/api/counter/{counterId}?forceFromDatabase=false` | Get counter value |
| `PUT` | `/api/counter/{counterId}` | Set counter to specific value |
| `GET` | `/api/counter/{counterId}/sync-status` | Check sync status |
| `POST` | `/api/counter/{counterId}/sync` | Manual sync to MySQL |
| `GET` | `/api/counter/mysql/all` | Get all counters from MySQL |
| `GET` | `/api/counter/health` | Health check |

### Example Usage

**Increment a counter:**
```bash
curl -X POST "http://localhost:5000/api/counter/page_views/increment"
```

**Get counter value:**
```bash
curl -X GET "http://localhost:5000/api/counter/page_views"
```

**Set counter value:**
```bash
curl -X PUT "http://localhost:5000/api/counter/page_views" \
     -H "Content-Type: application/json" \
     -d '{"value": 1000}'
```

**Check sync status:**
```bash
curl -X GET "http://localhost:5000/api/counter/page_views/sync-status"
```

## ⚙️ Configuration

### Sync Interval

Modify sync frequency in `appsettings.json`:

```json
{
  "CounterSync": {
    "IntervalMinutes": 1
  }
}
```

### Redis Connection

Configure Redis connection:

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  }
}
```

## 🏃‍♂️ Testing the System

### 1. Start the Application
```bash
dotnet run
```

### 2. Test Fast Increments
```bash
# Increment multiple times rapidly
for /L %i in (1,1,100) do curl -X POST "http://localhost:5000/api/counter/test_counter/increment"
```

### 3. Check Values
```bash
# Get from Redis (fast)
curl "http://localhost:5000/api/counter/test_counter"

# Get from MySQL (durable)
curl "http://localhost:5000/api/counter/test_counter?forceFromDatabase=true"

# Check sync status
curl "http://localhost:5000/api/counter/test_counter/sync-status"
```

### 4. Manual Sync
```bash
curl -X POST "http://localhost:5000/api/counter/test_counter/sync"
```

## 📊 Performance Benefits

**Redis Increments:**
- ⚡ **Sub-millisecond** response times
- 🔄 **Atomic operations** (thread-safe)
- 📈 **High throughput** (100k+ ops/sec)

**MySQL Persistence:**
- 💾 **Durable storage** (survives restarts)
- 📜 **Historical data** (with timestamps)
- 🔄 **ACID compliance** (backup/restore)

## 🔧 Troubleshooting

### MySQL Connection Issues
```bash
# Check if MySQL is running
net start mysql80

# Test connection
mysql -u root -p
```

### Redis Connection Issues
```bash
# Check if Redis is running
redis-cli ping
# Should return: PONG
```

### Application Logs
Check console output for connection status and sync logs.

## 🎯 Use Cases

This pattern is ideal for:

- **Page view counters**
- **Like/upvote systems**
- **API rate limiting**
- **Real-time analytics**
- **User activity tracking**
- **Gaming leaderboards**

## 📈 Scaling Considerations

- **Redis Clustering**: For multi-node Redis setup
- **MySQL Read Replicas**: For read-heavy workloads
- **Horizontal Scaling**: Multiple app instances with shared Redis/MySQL
- **Monitoring**: Add metrics collection (Prometheus, etc.)

## 🔒 Production Recommendations

1. **Connection Pooling**: Configure Redis and MySQL connection pools
2. **Error Handling**: Implement circuit breakers and retry policies
3. **Monitoring**: Add health checks and alerting
4. **Security**: Use authentication and SSL/TLS
5. **Backup Strategy**: Regular MySQL backups and Redis persistence
6. **Load Testing**: Validate performance under expected load

---

This implementation provides a solid foundation for high-performance counting systems with the reliability of persistent storage! 🚀

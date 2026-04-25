# Cache Increment Project Setup Script
# Run this script to set up the development environment

Write-Host "🚀 Setting up Cache Increment Project..." -ForegroundColor Green

# Check if Docker is installed
if (Get-Command docker -ErrorAction SilentlyContinue) {
    Write-Host "✅ Docker found" -ForegroundColor Green
    
    Write-Host "🐋 Starting Redis and MySQL containers..." -ForegroundColor Yellow
    docker-compose up -d
    
    Write-Host "⏳ Waiting for services to be ready..." -ForegroundColor Yellow
    Start-Sleep -Seconds 10
    
    # Test Redis connection
    try {
        docker exec cache-increment-redis redis-cli ping
        Write-Host "✅ Redis is ready" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Redis connection failed" -ForegroundColor Red
    }
    
    # Test MySQL connection
    try {
        docker exec cache-increment-mysql mysqladmin ping -h localhost -u root -ppassword
        Write-Host "✅ MySQL is ready" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ MySQL connection failed" -ForegroundColor Red
    }
}
else {
    Write-Host "⚠️  Docker not found. Please install Redis and MySQL manually:" -ForegroundColor Yellow
    Write-Host "   - Redis: https://redis.io/download" -ForegroundColor Cyan
    Write-Host "   - MySQL: https://dev.mysql.com/downloads/mysql/" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore

Write-Host ""
Write-Host "🏗️  Building project..." -ForegroundColor Yellow
dotnet build

Write-Host ""
Write-Host "✅ Setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "🚀 To start the application:" -ForegroundColor Cyan
Write-Host "   dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "📖 API Documentation:" -ForegroundColor Cyan
Write-Host "   http://localhost:5000 (Swagger UI)" -ForegroundColor White
Write-Host ""
Write-Host "🧪 Test endpoints using:" -ForegroundColor Cyan
Write-Host "   - CacheIncrement.http file in VS Code" -ForegroundColor White
Write-Host "   - Swagger UI at http://localhost:5000" -ForegroundColor White
Write-Host ""
Write-Host "📊 Monitor containers:" -ForegroundColor Cyan
Write-Host "   docker-compose ps" -ForegroundColor White
Write-Host ""
Write-Host "🛑 Stop containers:" -ForegroundColor Cyan
Write-Host "   docker-compose down" -ForegroundColor White

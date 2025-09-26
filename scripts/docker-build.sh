#!/bin/bash

# SS Tracker Docker Build Script

echo "🚀 Building SS Tracker application with React frontend..."

# Stop and remove existing container if running
echo "📦 Stopping existing containers..."
docker-compose down

# Remove old images (optional - comment out if you want to keep them)
echo "🧹 Cleaning up old images..."
docker image prune -f

# Build the application
echo "🔨 Building Docker image..."
docker-compose build --no-cache

# Start the application
echo "🚀 Starting the application..."
docker-compose up -d

echo "✅ Build complete!"
echo ""
echo "📍 Available endpoints:"
echo "   🌐 Main API: http://localhost:5007/api/v1/schedule"
echo "   📊 Swagger: http://localhost:5007/swagger/index.html"
echo "   📅 Calendar: http://localhost:5007/calendar"
echo "   ❤️  Health: http://localhost:5007/api/v1/health"
echo "   👥 Available Advisors: http://localhost:5007/api/v1/schedule/advisors/available"
echo ""
echo "📝 View logs with: docker-compose logs -f"
echo "🛑 Stop with: docker-compose down"

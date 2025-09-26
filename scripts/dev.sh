#!/bin/bash

# SS Tracker Unified Development Script
# This script provides complete hot-reload for both React and .NET using Docker

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_header() {
    echo -e "${PURPLE}$1${NC}"
}

# Function to stop development environment
stop_dev() {
    print_status "Stopping development environment..."
    docker-compose -f docker-compose.dev.yml down 2>/dev/null || true
    # Kill any background React watch processes
    pkill -f "fswatch.*ClientApp/src" 2>/dev/null || true
    pkill -f "inotifywait.*ClientApp/src" 2>/dev/null || true
    print_success "Development environment stopped"
}

# Function to build React
build_react() {
    print_status "Building React application..."
    cd ClientApp && npm run build && cd ..
    print_status "Copying React build to wwwroot..."
    mkdir -p wwwroot
    cp -r ClientApp/build/* wwwroot/
    print_success "React build completed"
}

# Function to watch React files and auto-rebuild
watch_react() {
    print_status "Starting React file watcher..."
    
    # Function to handle React rebuilds
    rebuild_react() {
        echo ""
        print_status "🔄 React files changed! Rebuilding..."
        if build_react; then
            print_success "✅ React rebuild completed - $(date '+%H:%M:%S')"
            print_warning "🌐 Refresh your browser at http://localhost:5007/calendar to see changes"
        else
            print_error "❌ React rebuild failed"
        fi
        echo ""
    }

    # Choose the appropriate file watcher for the OS
    if command -v fswatch &> /dev/null; then
        print_success "Using fswatch for file monitoring (macOS)"
        fswatch -o ClientApp/src/ | while read f; do rebuild_react; done &
    elif command -v inotifywait &> /dev/null; then
        print_success "Using inotifywait for file monitoring (Linux)"
        while inotifywait -r -e modify,create,delete ClientApp/src/ &>/dev/null; do rebuild_react; done &
    else
        print_warning "No file watcher available. Install fswatch (macOS) or inotify-tools (Linux)"
        print_warning "Manual mode: Run './build-react.sh' when you make React changes"
        return 1
    fi
    
    # Store the PID of the background process
    WATCH_PID=$!
    echo $WATCH_PID > .react-watch.pid
    print_success "React file watcher started (PID: $WATCH_PID)"
}

# Function to check if required tools are installed
check_dependencies() {
    print_status "Checking dependencies..."
    
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed or not in PATH"
        exit 1
    fi
    
    if ! command -v docker-compose &> /dev/null; then
        print_error "Docker Compose is not installed or not in PATH"
        exit 1
    fi
    
    if [ ! -f "ClientApp/package.json" ]; then
        print_error "ClientApp/package.json not found. Are you in the correct directory?"
        exit 1
    fi
    
    print_success "All dependencies check passed"
}

# Function to start the development environment
start_dev() {
    print_header "🚀 Starting SS Tracker Development Environment"
    echo ""
    
    # Check dependencies
    check_dependencies
    
    # Stop any existing environment
    stop_dev
    
    # Initial React build
    build_react
    
    # Start Docker development environment
    print_status "Starting Docker development environment..."
    docker-compose -f docker-compose.dev.yml up -d --build
    
    # Wait for the container to be ready
    print_status "Waiting for services to be ready..."
    sleep 8
    
    # Check if container is running
    if ! docker-compose -f docker-compose.dev.yml ps | grep -q "Up"; then
        print_error "Failed to start development environment"
        docker-compose -f docker-compose.dev.yml logs
        exit 1
    fi
    
    # Start React file watcher
    if watch_react; then
        print_success "React hot-reload enabled"
    else
        print_warning "React hot-reload not available - manual rebuild required"
    fi
    
    echo ""
    print_header "✅ Development Environment Ready!"
    echo ""
    print_success "🌐 Application: http://localhost:5007/calendar"
    print_success "📊 API Docs: http://localhost:5007/swagger/index.html"
    print_success "🔧 API Health: http://localhost:5007/api/v1/health"
    echo ""
    print_header "🔥 Hot Reload Status:"
    print_success "  ✅ .NET/C#: Automatic (dotnet watch) ~2-3 seconds"
    if [ -f ".react-watch.pid" ]; then
        print_success "  ✅ React/TypeScript: Automatic file watching enabled"
    else
        print_warning "  ⚠️  React/TypeScript: Manual - run './build-react.sh' after changes"
    fi
    print_success "  ✅ Assets/CSV: Auto-synced via Docker volumes"
    echo ""
    print_header "📝 Development Commands:"
    echo "  📊 View logs:     docker-compose -f docker-compose.dev.yml logs -f"
    echo "  🔄 Restart:       docker-compose -f docker-compose.dev.yml restart"
    echo "  🛑 Stop:          $0 stop"
    echo "  🔨 Manual build:  ./build-react.sh"
    echo ""
    print_warning "Press Ctrl+C to stop the development environment"
    echo ""
}

# Function to show logs
show_logs() {
    print_status "Showing development logs (Ctrl+C to exit)..."
    docker-compose -f docker-compose.dev.yml logs -f
}

# Function to restart services
restart_dev() {
    print_status "Restarting development environment..."
    docker-compose -f docker-compose.dev.yml restart
    print_success "Development environment restarted"
}

# Function to show status
show_status() {
    print_header "🔍 SS Tracker Development Status"
    echo ""
    
    print_status "Docker Services:"
    docker-compose -f docker-compose.dev.yml ps
    echo ""
    
    if [ -f ".react-watch.pid" ]; then
        WATCH_PID=$(cat .react-watch.pid)
        if kill -0 $WATCH_PID 2>/dev/null; then
            print_success "React file watcher: Running (PID: $WATCH_PID)"
        else
            print_warning "React file watcher: Not running"
            rm -f .react-watch.pid
        fi
    else
        print_warning "React file watcher: Not started"
    fi
    echo ""
    
    # Check if endpoints are responding
    if curl -s http://localhost:5007/api/v1/health &>/dev/null; then
        print_success "API Health: Responding ✅"
    else
        print_warning "API Health: Not responding ❌"
    fi
}

# Function to cleanup
cleanup() {
    print_status "Cleaning up..."
    stop_dev
    rm -f .react-watch.pid
}

# Trap to cleanup on script exit
trap cleanup EXIT INT TERM

# Main script logic
case "${1:-start}" in
    "start")
        start_dev
        # Keep script running to maintain file watcher
        while true; do
            sleep 1
        done
        ;;
    "stop")
        stop_dev
        exit 0
        ;;
    "restart")
        restart_dev
        ;;
    "logs")
        show_logs
        ;;
    "status")
        show_status
        ;;
    "build")
        build_react
        print_success "Manual React build completed"
        print_warning "💡 Tip: Use './dev.sh start' for automatic hot-reload instead"
        ;;
    *)
        print_header "SS Tracker Development Script"
        echo ""
        echo "Usage: $0 [command]"
        echo ""
        echo "Commands:"
        echo "  start    Start development environment with hot-reload (default)"
        echo "  stop     Stop development environment"
        echo "  restart  Restart Docker services"
        echo "  logs     Show development logs"
        echo "  status   Show current status"
        echo "  build    Manual React build"
        echo ""
        echo "Examples:"
        echo "  $0           # Start development"
        echo "  $0 start     # Start development"
        echo "  $0 logs      # View logs"
        echo "  $0 stop      # Stop everything"
        ;;
esac

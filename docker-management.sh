#!/bin/bash

# Parcel Delivery System - Docker Management Scripts

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Functions
print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_success() {
    echo -e "${BLUE}[SUCCESS]${NC} $1"
}

# Build and start the application
start() {
    print_info "Starting Parcel Delivery System..."
    docker-compose up --build -d
    
    print_info "Waiting for application to start..."
    sleep 5
    
    # Check if application is running
    if curl -s http://localhost:5227/health > /dev/null 2>&1; then
        print_success "Application started successfully!"
        echo ""
        print_info "Access URLs:"
        echo "   API Documentation (Swagger): http://localhost:5227"
        echo "   Health Check: http://localhost:5227/health"
        echo "   Departments API: http://localhost:5227/api/Departments"
        echo "   Containers API: http://localhost:5227/api/ShippingContainers"
        echo ""
        print_info "Test the XML import:"
        echo "   curl -X POST http://localhost:5227/api/ShippingContainers/import \\"
        echo "        -H \"Content-Type: multipart/form-data\" \\"
        echo "        -F \"file=@Container_2-MSX.xml\""
    else
        print_warning "Application may still be starting. Check logs with: ./docker-management.sh logs"
    fi
}

# Stop the application
stop() {
    print_info "Stopping Parcel Delivery System..."
    docker-compose down
    print_success "Application stopped successfully!"
}

# Stop and remove everything (including volumes)
clean() {
    print_warning "This will remove all containers, images, and volumes. Are you sure? (y/N)"
    read -r response
    if [[ "$response" =~ ^([yY][eE][sS]|[yY])$ ]]; then
        print_info "Cleaning up all Docker resources..."
        docker-compose down -v --rmi all
        docker system prune -f
        print_success "Cleanup completed!"
    else
        print_info "Cleanup cancelled."
    fi
}

# View logs
logs() {
    print_info "Showing application logs (press Ctrl+C to exit)..."
    docker-compose logs -f parcel-delivery-api
}

# Check application status
status() {
    print_info "Checking application status..."
    
    # Check if docker-compose is running
    if ! docker-compose ps --services --filter "status=running" | grep -q "parcel-delivery-api"; then
        print_error "Application container is not running"
        echo ""
        print_info "Container status:"
        docker-compose ps
        echo ""
        print_info "To start the application, run: ./docker-management.sh start"
        return 1
    fi
    
    # Show container status
    docker-compose ps
    echo ""
    
    # Check health endpoint
    print_info "Health check:"
    if curl -s http://localhost:5227/health > /dev/null 2>&1; then
        print_success "Application is healthy!"
        echo "   Response: $(curl -s http://localhost:5227/health)"
    else
        print_warning "Application container is running but health check failed"
        print_info "This might mean the application is still starting up"
        print_info "Try: ./docker-management.sh logs"
    fi
}

# Run tests
test() {
    print_info "Running tests in Docker..."
    print_info "Building test image..."
    # Build test image
    if ! docker build --target test -t parcel-delivery-tests . > /tmp/test-build.log 2>&1; then
        print_error "Failed to build test image"
        cat /tmp/test-build.log
        return 1
    fi
    
    print_info "Test image built successfully, running tests..."
    
    # Run tests and capture output (allow non-zero exit codes)
    docker run --rm parcel-delivery-tests > /tmp/test-results.log 2>&1
    local test_exit_code=$?
    
    # Extract test summary from the output
    local total_tests=$(grep "Total tests:" /tmp/test-results.log | sed 's/.*Total tests: \([0-9]*\).*/\1/')
    local passed_tests=$(grep "Passed:" /tmp/test-results.log | sed 's/.*Passed: \([0-9]*\).*/\1/')
    local failed_tests=$(grep "Failed:" /tmp/test-results.log | sed 's/.*Failed: \([0-9]*\).*/\1/')
    local test_time=$(grep "Total time:" /tmp/test-results.log | sed 's/.*Total time: \([0-9.]*\) Seconds.*/\1/')
    
    echo ""
    print_info "Test Results Summary:"
    echo "   Total Tests: ${total_tests:-Unknown}"
    echo "   Passed: ${passed_tests:-Unknown}"
    echo "   Failed: ${failed_tests:-0}"
    echo "   Duration: ${test_time:-Unknown}s"
    
    if [[ "${failed_tests:-0}" == "0" ]] && [[ -n "$total_tests" && "$total_tests" != "0" ]]; then
        print_success "All tests passed!"
        echo ""
        print_info "Test coverage breakdown:"
        
        # Count tests by category
        local domain_tests=$(grep -c "Tests\.Domain\." /tmp/test-results.log || echo "0")
        local application_tests=$(grep -c "Tests\.Application\." /tmp/test-results.log || echo "0")
        local api_tests=$(grep -c "Tests\.Api\." /tmp/test-results.log || echo "0")
        
        echo "   Domain Layer: ${domain_tests} tests"
        echo "   Application Layer: ${application_tests} tests"
        echo "   API Layer: ${api_tests} tests"
        
    elif [[ "${failed_tests:-0}" != "0" ]]; then
        print_warning "${failed_tests} test(s) failed"
        echo ""
        print_info "Failed tests:"
        
        # Show failed test names
        grep -E "\[FAIL\]" /tmp/test-results.log | sed 's/.*Tests\./  • /' | head -10
        
        if [[ $(grep -c "\[FAIL\]" /tmp/test-results.log) -gt 10 ]]; then
            echo "  ... and $(($(grep -c "\[FAIL\]" /tmp/test-results.log) - 10)) more"
        fi
        
        echo ""
        print_info "For detailed error information, check the full output above"
        
    else
        print_error "Unable to determine test results"
        echo ""
        print_info "Raw test output:"
        cat /tmp/test-results.log | head -50
        return 1
    fi
    
    # Cleanup temp files
    rm -f /tmp/test-build.log /tmp/test-results.log
    
    # Return appropriate exit code
    if [[ "${failed_tests:-0}" == "0" ]] && [[ -n "$total_tests" && "$total_tests" != "0" ]]; then
        return 0
    else
        return 1
    fi
}

# Quick demo of the system
demo() {
    print_info "Running quick demo..."
    
    # Check if app is running
    if ! curl -s http://localhost:5227/health > /dev/null 2>&1; then
        print_error "Application is not running. Start it first with: ./docker-management.sh start"
        return 1
    fi
    
    echo ""
    print_info "=== Demo: Parcel Delivery System ==="
    echo ""
    
    # Option to clear existing data for a fresh demo
    print_info "0. Demo Setup:"
    echo "   Would you like to clear existing data for a fresh demo? (y/N)"
    read -r -t 10 clear_response || clear_response="n"
    
    if [[ "$clear_response" =~ ^([yY][eE][sS]|[yY])$ ]]; then
        print_info "Clearing existing data..."
        docker-compose restart parcel-delivery-api > /dev/null 2>&1
        sleep 3
        print_success "Data cleared! Starting fresh demo..."
    else
        print_info "Using existing data (may show duplicate detection behavior)"
    fi
    
    echo ""
    
    # 1. Check departments
    print_info "1. Available Departments:"
    local dept_response=$(curl -s http://localhost:5227/api/Departments)
    
    if command -v jq > /dev/null 2>&1; then
        # Use jq if available for better formatting
        echo "$dept_response" | jq -r '.[] | "   • \(.name): \(.description) [\(.isActive | if . then "Active" else "Inactive" end)]"'
    else
        # Fallback to basic parsing if jq is not available
        echo "$dept_response" | sed 's/\[//g' | sed 's/\]//g' | sed 's/},{/\n/g' | sed 's/[{}]//g' | \
        while IFS= read -r line; do
            name=$(echo "$line" | sed -n 's/.*"name":"\([^"]*\)".*/\1/p')
            desc=$(echo "$line" | sed -n 's/.*"description":"\([^"]*\)".*/\1/p')
            if [[ -n "$name" && -n "$desc" ]]; then
                echo "   • $name: $desc"
            fi
        done
    fi
    
    echo ""
    
    # 2. Test XML import
    print_info "2. Testing XML Import:"
    echo "   Importing Container_2-MSX.xml..."
    
    local import_response=$(curl -s -X POST http://localhost:5227/api/ShippingContainers/import \
         -H "Content-Type: multipart/form-data" \
         -F "file=@Container_2-MSX.xml" 2>/dev/null)
    
    # Check if import was successful
    if echo "$import_response" | grep -q '"error"'; then
        # Check if it's a duplicate container issue
        if echo "$import_response" | grep -q "already exists"; then
            print_success "Duplicate Detection Working!"
            echo "   The system correctly detected that this container was already imported."
            echo "   Status: Container already exists with validated data integrity"
        else
            print_warning "Import failed:"
            if command -v jq > /dev/null 2>&1; then
                local message=$(echo "$import_response" | jq -r '.message // .error')
                # Show only first part of error message if it's too long
                if [[ ${#message} -gt 100 ]]; then
                    echo "   $(echo "$message" | cut -c1-100)..."
                else
                    echo "   $message"
                fi
            else
                echo "   $(echo "$import_response" | sed -n 's/.*"message":"\([^"]*\)".*/\1/p' | cut -c1-100)"
            fi
        fi
    else
        print_success "Import successful!"
        if command -v jq > /dev/null 2>&1; then
            local container_id=$(echo "$import_response" | jq -r '.containerId')
            local parcel_count=$(echo "$import_response" | jq -r '.totalParcels')
            echo "   Container ID: $container_id"
            echo "   Parcels imported: $parcel_count (duplicates automatically removed)"
        else
            echo "   Container imported successfully with duplicate removal"
        fi
    fi
    
    echo ""
    
    # 3. Check container status
    print_info "3. Container Overview:"
    local containers_response=$(curl -s http://localhost:5227/api/ShippingContainers)
    
    if command -v jq > /dev/null 2>&1; then
        local container_count=$(echo "$containers_response" | jq '. | length')
        echo "   Total containers: $container_count"
        
        if [[ "$container_count" -gt 0 ]]; then
            echo "   Latest containers:"
            echo "$containers_response" | jq -r '.[:3] | .[] | "   • Container \(.containerId): \(.totalParcels) parcels, \(.totalWeight)kg, €\(.totalValue)"'
        fi
    else
        local container_count=$(echo "$containers_response" | grep -o '"containerId"' | wc -l)
        echo "   Total containers: $container_count"
    fi
    
    echo ""
    
    # 4. Show system capabilities
    print_info "4. System Capabilities Demonstrated:"
    echo "   • XML Import & Parsing"
    echo "   • Department Assignment (Weight-based rules)"
    echo "   • Insurance Department (Value-based rules)"
    echo "   • Duplicate Detection"
    echo "   • Data Integrity Validation"
    echo ""
    
    print_success "Demo completed!"
    echo ""
    print_info "Next steps:"
    echo "   • Open http://localhost:5227 to explore the Swagger documentation"
    echo "   • Try importing your own XML files"
    echo "   • Use the API endpoints to manage parcels and departments"
}

# Show help
help() {
    echo "Parcel Delivery System - Docker Management"
    echo ""
    echo "Usage: $0 {start|stop|clean|logs|status|test|demo|help}"
    echo ""
    echo "Commands:"
    echo "  start   - Build and start the application (START HERE)"
    echo "  stop    - Stop the application"
    echo "  clean   - Stop and remove all Docker resources"
    echo "  logs    - View application logs"
    echo "  status  - Check application status"
    echo "  test    - Run tests in Docker"
    echo "  demo    - Quick demo of system functionality"
    echo "  help    - Show this help message"
    echo ""
    echo "Quick Start for Reviewers:"
    echo "  1. ./docker-management.sh start"
    echo "  2. Open http://localhost:5227 for Swagger"
    echo "  3. ./docker-management.sh demo (optional)"
}

# Main script logic
case "${1:-help}" in
    start)
        start
        ;;
    stop)
        stop
        ;;
    clean)
        clean
        ;;
    logs)
        logs
        ;;
    status)
        status
        ;;
    test)
        test
        ;;
    demo)
        demo
        ;;
    help|*)
        help
        ;;
esac

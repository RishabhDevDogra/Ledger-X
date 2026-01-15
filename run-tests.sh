#!/usr/bin/env bash

# LedgerX Test Runner Script
# Comprehensive testing for the LedgerX application

echo "╔════════════════════════════════════════════════════════════════╗"
echo "║          LedgerX Unit Test Suite Runner                       ║"
echo "╚════════════════════════════════════════════════════════════════╝"
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Test counters
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0

# Function to print section headers
print_header() {
    echo ""
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo ""
}

# Function to run tests
run_tests() {
    local test_filter="$1"
    local description="$2"
    
    echo -e "${YELLOW}Running: $description${NC}"
    
    if [ -z "$test_filter" ]; then
        dotnet test --logger "console;verbosity=minimal"
    else
        dotnet test --filter "$test_filter" --logger "console;verbosity=minimal"
    fi
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ $description passed${NC}"
        ((PASSED_TESTS++))
    else
        echo -e "${RED}✗ $description failed${NC}"
        ((FAILED_TESTS++))
    fi
    
    ((TOTAL_TESTS++))
}

# Main menu
show_menu() {
    echo -e "${BLUE}Select test suite to run:${NC}"
    echo ""
    echo "  1) Run All Tests"
    echo "  2) Service Layer Tests Only"
    echo "  3) Controller Tests Only"
    echo "  4) Repository Tests Only"
    echo "  5) Run Specific Test Class"
    echo "  6) Run with Code Coverage"
    echo "  7) Run in Watch Mode"
    echo "  8) Generate Detailed Report"
    echo "  9) Exit"
    echo ""
    read -p "Enter choice [1-9]: " choice
}

# Parse command line argument if provided
if [ ! -z "$1" ]; then
    case "$1" in
        all)
            choice=1
            ;;
        services)
            choice=2
            ;;
        controllers)
            choice=3
            ;;
        repositories)
            choice=4
            ;;
        coverage)
            choice=6
            ;;
        watch)
            choice=7
            ;;
        report)
            choice=8
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
else
    show_menu
fi

case $choice in
    1)
        print_header "Running All Unit Tests"
        dotnet test
        ;;
    2)
        print_header "Running Service Layer Tests"
        dotnet test --filter "LedgerX.Tests.Services"
        ;;
    3)
        print_header "Running Controller Tests"
        dotnet test --filter "LedgerX.Tests.Controllers"
        ;;
    4)
        print_header "Running Repository Tests"
        dotnet test --filter "LedgerX.Tests.Repositories"
        ;;
    5)
        print_header "Run Specific Test Class"
        echo -e "${YELLOW}Available test classes:${NC}"
        echo "  • AccountServiceTests"
        echo "  • JournalEntryServiceTests"
        echo "  • ReportServiceTests"
        echo "  • LedgerKeyServiceTests"
        echo "  • AccountsControllerTests"
        echo "  • JournalEntriesControllerTests"
        echo "  • ReportsControllerTests"
        echo "  • LedgerKeysControllerTests"
        echo "  • AccountRepositoryTests"
        echo "  • JournalEntryRepositoryTests"
        echo "  • LedgerKeyRepositoryTests"
        echo ""
        read -p "Enter test class name (or partial name): " test_class
        dotnet test --filter "ClassName~$test_class"
        ;;
    6)
        print_header "Running Tests with Code Coverage"
        dotnet test \
            /p:CollectCoverage=true \
            /p:CoverageFormat=opencover \
            /p:CoverageFileName=coverage.xml \
            --logger "console;verbosity=normal"
        
        if [ -f "coverage.xml" ]; then
            echo -e "${GREEN}✓ Coverage report generated: coverage.xml${NC}"
        fi
        ;;
    7)
        print_header "Running Tests in Watch Mode"
        echo -e "${YELLOW}Tests will run automatically when files change...${NC}"
        dotnet watch test --project LedgerX.Tests/LedgerX.Tests.csproj
        ;;
    8)
        print_header "Generating Detailed Test Report"
        dotnet test \
            --logger "trx;logfilename=test-results.trx" \
            --logger "console;verbosity=detailed"
        
        if [ -f "test-results.trx" ]; then
            echo -e "${GREEN}✓ Test results saved to test-results.trx${NC}"
        fi
        ;;
    9)
        echo -e "${YELLOW}Exiting...${NC}"
        exit 0
        ;;
    *)
        echo -e "${RED}Invalid choice. Please try again.${NC}"
        exit 1
        ;;
esac

# Summary
if [ $? -eq 0 ]; then
    echo ""
    echo -e "${GREEN}╔════════════════════════════════════════════════════════════════╗${NC}"
    echo -e "${GREEN}║              Tests Completed Successfully!                     ║${NC}"
    echo -e "${GREEN}╚════════════════════════════════════════════════════════════════╝${NC}"
else
    echo ""
    echo -e "${RED}╔════════════════════════════════════════════════════════════════╗${NC}"
    echo -e "${RED}║              Some Tests Failed                                 ║${NC}"
    echo -e "${RED}╚════════════════════════════════════════════════════════════════╝${NC}"
fi

echo ""

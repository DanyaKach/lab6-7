#!/bin/bash

# Simple script to run K6 smoke tests locally
# Make sure the API is running on localhost:5000 before running tests

set -e

BASE_URL="${BASE_URL:-http://localhost:5000}"
TEST_TYPE="${1:-smoke}"

echo "🚀 Running K6 $TEST_TYPE test against $BASE_URL"
echo "📊 Make sure your API is running and accessible"
echo ""

case $TEST_TYPE in
    smoke)
        k6 run -e BASE_URL="$BASE_URL" scripts/smoke-test.js
        ;;
    *)
        echo "❌ Only smoke tests are supported"
        echo "📝 Usage: ./run-tests.sh [smoke]"
        exit 1
        ;;
esac

echo ""
echo "✅ Test completed! Check results above."
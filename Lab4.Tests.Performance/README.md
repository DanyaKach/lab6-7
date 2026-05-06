# K6 Smoke Tests

This directory contains K6 smoke test scripts for the Student API.

## Test Type

### Smoke Test (`smoke-test.js`)
- **Purpose**: Basic functionality verification
- **Load**: 1 virtual user for 30 seconds
- **Checks**: Basic API responsiveness and functionality
- **Use case**: Quick health check, CI/CD pipeline

## Running Tests

### Prerequisites
- K6 installed
- API running on the target environment

### Local Development
```bash
# Smoke test
k6 run scripts/smoke-test.js

# With npm
npm run smoke:staged
```

### With Custom Base URL
```bash
k6 run -e BASE_URL=http://your-api-url scripts/smoke-test.js
```

### GitHub Actions
Tests are automatically run via GitHub Actions workflow `k6-performance.yml`:
- On pull requests affecting API code
- Manual trigger

## Test Results

Results are exported to `results.json` and uploaded as artifacts in CI/CD.

### Key Metrics
- **http_req_duration**: Response time percentiles
- **http_req_failed**: Error rate
- **vus**: Virtual users over time
- **http_reqs**: Request rate

## Thresholds

Smoke test has defined performance thresholds:
- Response time limits (< 500ms for GET, < 1000ms for POST)
- Error rate limits (< 10%)

## API Endpoints Tested

- `GET /api/student` - List students
- `POST /api/student` - Create student

## Data Generation

Tests generate unique test data to avoid conflicts:
- Unique emails and names per test run
- Timestamp-based identifiers
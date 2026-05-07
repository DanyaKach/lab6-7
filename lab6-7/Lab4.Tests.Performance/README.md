# K6 Performance Tests

This directory contains K6 performance test scripts for the Student API.

## Test Types

### Smoke Test (`smoke-test.js`)
- **Purpose**: Basic functionality verification
- **Load**: 1 virtual user for 30 seconds
- **Checks**: Basic API responsiveness and functionality
- **Use case**: Quick health check, CI/CD pipeline

### Load Test (`load-test.js`)
- **Purpose**: Performance testing under normal and peak loads
- **Load**: Gradual ramp-up from 10 to 100 users over 16 minutes
- **Checks**: Sustained performance, response times under load
- **Use case**: Capacity planning, performance monitoring

### Stress Test (`stress-test.js`)
- **Purpose**: Finding system breaking points
- **Load**: Aggressive ramp-up to 300 users over 8 minutes
- **Checks**: System stability under extreme load
- **Use case**: Breaking point analysis, scalability limits

## Running Tests

### Prerequisites
- K6 installed
- API running on the target environment

### Local Development
```bash
# Smoke test
npm run smoke:staged

# Load test
npm run load

# Stress test
npm run stress

# With result export
npm run load:export
npm run stress:export
```

### With Custom Base URL
```bash
k6 run -e BASE_URL=http://your-api-url scripts/smoke-test.js
k6 run -e BASE_URL=http://your-api-url scripts/load-test.js
k6 run -e BASE_URL=http://your-api-url scripts/stress-test.js
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

### Smoke Test
- Response time (95%): < 500ms
- Error rate: < 10%

### Load Test
- Response time (95%): < 1000ms
- Error rate: < 5%

### Stress Test
- Response time (95%): < 2000ms
- Error rate: < 10%
- Response time limits (< 500ms for GET, < 1000ms for POST)
- Error rate limits (< 10%)

## API Endpoints Tested

- `GET /api/student` - List students
- `POST /api/student` - Create student

## Data Generation

Tests generate unique test data to avoid conflicts:
- Unique emails and names per test run
- Timestamp-based identifiers
import http from 'k6/http'
import { check, sleep } from 'k6'

export let options = {
  vus: 1, // 1 virtual user
  duration: '30s', // Test duration

  thresholds: {
    http_req_duration: ['p(95)<500'], // 95% of requests should be below 500ms
    http_req_failed: ['rate<0.1'], // Error rate should be below 10%
  },
}

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000'

export default function () {
  // Smoke test - basic functionality check
  let response = http.get(`${BASE_URL}/api/student`)

  check(response, {
    'GET /api/student status is 200 or 404': r => r.status === 200 || r.status === 404,
    'response time < 500ms': r => r.timings.duration < 500,
  })

  // Create student test
  let studentData = {
    fullName: `Smoke Test User ${Date.now()}`,
    email: `smoke${Date.now()}@test.com`,
    enrollmentDate: new Date().toISOString(),
  }

  response = http.post(`${BASE_URL}/api/student`, JSON.stringify(studentData), {
    headers: {
      'Content-Type': 'application/json',
    },
  })

  check(response, {
    'POST /api/student status is 201': r => r.status === 201,
    'response time < 1000ms': r => r.timings.duration < 1000,
  })

  sleep(1) // Wait 1 second between iterations
}

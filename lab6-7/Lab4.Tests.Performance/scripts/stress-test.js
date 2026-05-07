import http from 'k6/http'
import { check, sleep } from 'k6'

export let options = {
  stages: [
    { duration: '1m', target: 50 }, // Quick ramp up to 50 users
    { duration: '2m', target: 100 }, // Ramp up to 100 users
    { duration: '2m', target: 200 }, // Ramp up to 200 users
    { duration: '2m', target: 300 }, // Ramp up to 300 users
    { duration: '3m', target: 300 }, // Stay at 300 users to stress the system
    { duration: '1m', target: 0 }, // Quick ramp down
  ],

  thresholds: {
    http_req_duration: ['p(95)<2000'], // Allow higher latency under stress
    http_req_failed: ['rate<0.1'], // Error rate should be below 10% under stress
  },
}

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000'

export default function () {
  // High-frequency GET requests
  let response = http.get(`${BASE_URL}/api/student`)

  check(response, {
    'GET /api/student status is 200 or 404': r => r.status === 200 || r.status === 404,
    'GET response time < 2000ms': r => r.timings.duration < 2000,
  })

  // Create student under stress (higher frequency)
  if (Math.random() < 0.5) { // 50% of iterations create students
    let studentData = {
      fullName: `Stress Test User ${Date.now()}-${__VU}-${__ITER}`,
      email: `stress${Date.now()}-${__VU}-${__ITER}@test.com`,
      enrollmentDate: new Date().toISOString(),
    }

    response = http.post(`${BASE_URL}/api/student`, JSON.stringify(studentData), {
      headers: {
        'Content-Type': 'application/json',
      },
    })

    check(response, {
      'POST /api/student status is 201': r => r.status === 201,
      'POST response time < 3000ms': r => r.timings.duration < 3000,
    })
  }

  // Additional GET to increase load
  if (Math.random() < 0.3) { // 30% chance for extra request
    http.get(`${BASE_URL}/api/student`)
  }

  sleep(Math.random() * 0.5 + 0.1) // Shorter sleep 0.1-0.6 seconds for higher load
}
import http from 'k6/http'
import { check, sleep } from 'k6'

export let options = {
  stages: [
    { duration: '2m', target: 10 }, // Ramp up to 10 users over 2 minutes
    { duration: '5m', target: 10 }, // Stay at 10 users for 5 minutes
    { duration: '2m', target: 50 }, // Ramp up to 50 users over 2 minutes
    { duration: '5m', target: 50 }, // Stay at 50 users for 5 minutes
    { duration: '2m', target: 100 }, // Ramp up to 100 users over 2 minutes
    { duration: '5m', target: 100 }, // Stay at 100 users for 5 minutes
    { duration: '2m', target: 0 }, // Ramp down to 0 users over 2 minutes
  ],

  thresholds: {
    http_req_duration: ['p(95)<1000'], // 95% of requests should be below 1000ms
    http_req_failed: ['rate<0.05'], // Error rate should be below 5%
  },
}

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000'

export default function () {
  // GET all students
  let response = http.get(`${BASE_URL}/api/student`)

  check(response, {
    'GET /api/student status is 200 or 404': r => r.status === 200 || r.status === 404,
    'GET response time < 1000ms': r => r.timings.duration < 1000,
  })

  // Create student (only some users to avoid too much data creation)
  if (Math.random() < 0.3) { // 30% of iterations create students
    let studentData = {
      fullName: `Load Test User ${Date.now()}-${__VU}`,
      email: `load${Date.now()}-${__VU}@test.com`,
      enrollmentDate: new Date().toISOString(),
    }

    response = http.post(`${BASE_URL}/api/student`, JSON.stringify(studentData), {
      headers: {
        'Content-Type': 'application/json',
      },
    })

    check(response, {
      'POST /api/student status is 201': r => r.status === 201,
      'POST response time < 2000ms': r => r.timings.duration < 2000,
    })
  }

  sleep(Math.random() * 2 + 1) // Random sleep between 1-3 seconds
}
import http from 'k6/http';
import { check } from 'k6';
import { REST_URL, httpOptions, randomDevice } from './lib/config.js';

export const options = httpOptions;

export default function () {
  const res = http.get(
    `${REST_URL}/api/readings/aggregates?deviceId=${randomDevice()}&from=2024-01-01T00:00:00Z&to=2024-12-31T23:59:59Z`
  );
  check(res, {
    'status 200': (x) => x.status === 200,
    'is array': (x) => {
      try { return Array.isArray(JSON.parse(x.body)); } catch { return false; }
    },
  });
}

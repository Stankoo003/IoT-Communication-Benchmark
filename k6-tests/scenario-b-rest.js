import http from 'k6/http';
import { check } from 'k6';
import { REST_URL, httpOptions, randomDevice } from './lib/config.js';

export const options = httpOptions;

export default function () {
  const res = http.get(`${REST_URL}/api/readings?deviceId=${randomDevice()}&limit=10`);
  check(res, {
    'status 200': (x) => x.status === 200,
    'is array': (x) => Array.isArray(JSON.parse(x.body)),
  });
}

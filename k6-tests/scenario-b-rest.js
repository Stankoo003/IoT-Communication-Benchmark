import http from 'k6/http';
import { check } from 'k6';
import { REST_URL, makeHttpOptions, randomDevice } from './lib/config.js';

export const options = makeHttpOptions('rest', 'b');

export default function () {
  const res = http.get(`${REST_URL}/api/readings/selective/${randomDevice()}`);
  check(res, {
    'status 200': (x) => x.status === 200,
    'is array': (x) => Array.isArray(JSON.parse(x.body)),
  });
}

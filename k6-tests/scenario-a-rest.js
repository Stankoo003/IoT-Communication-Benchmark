import http from 'k6/http';
import { check } from 'k6';
import { REST_URL, makeHttpOptions, randomReading } from './lib/config.js';

export const options = makeHttpOptions('rest', 'a');

export default function () {
  const r = randomReading();
  const res = http.post(`${REST_URL}/api/readings`, JSON.stringify(r), {
    headers: { 'Content-Type': 'application/json' },
  });
  check(res, { 'status 201': (x) => x.status === 201 });
}

import http from 'k6/http';
import { check } from 'k6';
import { GRAPHQL_URL, httpOptions, randomDevice } from './lib/config.js';

export const options = httpOptions;

const QUERY = `query Readings($d: String!, $l: Int) {
  readings(deviceId: $d, limit: $l) { timestamp temperature humidity }
}`;

export default function () {
  const body = JSON.stringify({
    query: QUERY,
    variables: { d: randomDevice(), l: 10 },
  });
  const res = http.post(GRAPHQL_URL, body, {
    headers: { 'Content-Type': 'application/json' },
  });
  check(res, {
    'status 200': (x) => x.status === 200,
    'has data': (x) => JSON.parse(x.body).data?.readings != null,
  });
}

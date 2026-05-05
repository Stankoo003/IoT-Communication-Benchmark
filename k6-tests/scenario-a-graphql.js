import http from 'k6/http';
import { check } from 'k6';
import { GRAPHQL_URL, httpOptions, randomReading } from './lib/config.js';

export const options = httpOptions;

const MUTATION = `mutation Ingest($d: String!, $t: Float, $h: Float, $p: Float, $l: Int, $s: Int, $m: Int, $b: Float, $loc: String) {
  ingestReading(deviceId: $d, temperature: $t, humidity: $h, pressure: $p, light: $l, sound: $s, motion: $m, battery: $b, location: $loc) { id }
}`;

export default function () {
  const r = randomReading();
  const body = JSON.stringify({
    query: MUTATION,
    variables: {
      d: r.deviceId, t: r.temperature, h: r.humidity, p: r.pressure,
      l: r.light, s: r.sound, m: r.motion, b: r.battery, loc: r.location,
    },
  });
  const res = http.post(GRAPHQL_URL, body, {
    headers: { 'Content-Type': 'application/json' },
  });
  check(res, {
    'status 200': (x) => x.status === 200,
    'no errors': (x) => !JSON.parse(x.body).errors,
  });
}

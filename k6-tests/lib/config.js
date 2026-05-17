export const REST_URL = __ENV.REST_URL || 'http://localhost:5001';
export const GRPC_HOST = __ENV.GRPC_HOST || 'localhost:5002';
export const GRAPHQL_URL = __ENV.GRAPHQL_URL || 'http://localhost:5003/graphql';

export const DEVICES = Array.from({ length: 50 }, (_, i) => `Device_${i + 1}`);
export const LOCATIONS = ['Lab', 'Outside', 'Room A', 'Room B', 'Room C'];

const VUS = __ENV.VUS ? parseInt(__ENV.VUS) : 10;
const DURATION = __ENV.DURATION || '30s';

export function makeHttpOptions(protocol, scenario) {
  return {
    vus: VUS,
    duration: DURATION,
    tags: { protocol, scenario },
    thresholds: {
      http_req_failed: ['rate<0.01'],
      http_req_duration: ['p(95)<2000'],
      checks: ['rate>0.99'],
    },
  };
}

export function makeGrpcOptions(protocol, scenario) {
  return {
    vus: VUS,
    duration: DURATION,
    tags: { protocol, scenario },
    thresholds: {
      grpc_req_duration: ['p(95)<2000'],
      checks: ['rate>0.99'],
    },
  };
}

export const httpOptions = makeHttpOptions('unknown', 'unknown');
export const grpcOptions = makeGrpcOptions('grpc', 'unknown');

export function randomDevice() {
  return DEVICES[Math.floor(Math.random() * DEVICES.length)];
}

export function randomReading(deviceId) {
  return {
    deviceId: deviceId || randomDevice(),
    timestamp: new Date().toISOString(),
    temperature: 18 + Math.random() * 15,
    humidity: 30 + Math.random() * 60,
    pressure: 990 + Math.random() * 30,
    light: Math.floor(Math.random() * 1000),
    sound: Math.floor(Math.random() * 80),
    motion: Math.random() > 0.7 ? 1 : 0,
    battery: 20 + Math.random() * 80,
    location: LOCATIONS[Math.floor(Math.random() * LOCATIONS.length)],
  };
}

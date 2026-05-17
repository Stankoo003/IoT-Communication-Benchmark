import grpc from 'k6/net/grpc';
import { check } from 'k6';
import { GRPC_HOST, makeGrpcOptions, randomReading } from './lib/config.js';

const client = new grpc.Client();
client.load(['../grpc-service/Protos'], 'iot_sensor.proto');

export const options = makeGrpcOptions('grpc', 'a');

export default function () {
  if (__ITER === 0) {
    client.connect(GRPC_HOST, { plaintext: true });
  }
  const r = randomReading();
  const data = {
    timestamp: r.timestamp,   // ISO string → protobuf JSON encoding za Timestamp
    device_id: r.deviceId,
    temperature: r.temperature, // direktan broj → DoubleValue JSON encoding
    humidity: r.humidity,
    pressure: r.pressure,
    light: r.light,             // Int32Value JSON encoding
    sound: r.sound,
    motion: r.motion,
    battery: r.battery,
    location: r.location,
  };
  const res = client.invoke('iot.SensorService/IngestReading', data);
  check(res, { 'status OK': (x) => x && x.status === grpc.StatusOK });
}

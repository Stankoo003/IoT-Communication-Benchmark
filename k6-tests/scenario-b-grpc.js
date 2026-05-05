import grpc from 'k6/net/grpc';
import { check } from 'k6';
import { GRPC_HOST, grpcOptions, randomDevice } from './lib/config.js';

const client = new grpc.Client();
client.load(['../grpc-service/Protos'], 'iot_sensor.proto');

export const options = grpcOptions;

export default function () {
  if (__ITER === 0) {
    client.connect(GRPC_HOST, { plaintext: true });
  }
  const res = client.invoke('iot.SensorService/GetReadings', {
    device_id: randomDevice(),
    limit: 10,
  });
  check(res, { 'status OK': (x) => x && x.status === grpc.StatusOK });
}

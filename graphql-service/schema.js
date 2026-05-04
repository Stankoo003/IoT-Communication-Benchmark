export const typeDefs = `#graphql

  type SensorReading {
    id:          ID!
    timestamp:   String!
    deviceId:    String!
    temperature: Float
    humidity:    Float
    pressure:    Float
    light:       Int
    sound:       Int
    motion:      Int
    battery:     Float
    location:    String
  }

  type HourlyAggregate {
    hour:              String!
    deviceId:          String!
    location:          String
    avgTemperature:    Float
    avgHumidity:       Float
    avgPressure:       Float
    avgLight:          Float
    avgSound:          Float
    totalMotionEvents: Int
    minBattery:        Float
    readingCount:      Int
  }

  type Device {
    deviceId:  String!
    location:  String
    createdAt: String
  }

  type IngestResult {
    id: ID!
  }

  type Query {
    # Scenario B – klijent bira tacno koja polja zeli (anti over-fetching)
    readings(
      deviceId: String
      from:     String
      to:       String
      limit:    Int
    ): [SensorReading!]!

    latestReading(deviceId: String!): SensorReading

    # Scenario C – agregacije nad velikim opsegom
    aggregates(
      deviceId: String
      from:     String
      to:       String
    ): [HourlyAggregate!]!

    devices: [Device!]!
  }

  type Mutation {
    # Scenario A – upis novog ocitavanja
    ingestReading(
      deviceId:    String!
      timestamp:   String
      temperature: Float
      humidity:    Float
      pressure:    Float
      light:       Int
      sound:       Int
      motion:      Int
      battery:     Float
      location:    String
    ): IngestResult!
  }
`;

export const resolvers = {
  Query: {
    // Scenario B – Selective Monitoring
    readings: async (_, { deviceId, from, to, limit = 100 }, { db }) => {
      const conditions = [];
      const params = [];

      if (deviceId) { params.push(deviceId); conditions.push(`device_id = $${params.length}`); }
      if (from)     { params.push(from);     conditions.push(`timestamp >= $${params.length}::timestamptz`); }
      if (to)       { params.push(to);       conditions.push(`timestamp <= $${params.length}::timestamptz`); }

      params.push(limit);
      const where = conditions.length ? `WHERE ${conditions.join(' AND ')}` : '';
      const sql = `
        SELECT id, timestamp, device_id, temperature, humidity, pressure, light, sound, motion, battery, location
        FROM sensor_readings
        ${where}
        ORDER BY timestamp DESC
        LIMIT $${params.length}
      `;

      const { rows } = await db.query(sql, params);
      return rows.map(mapReading);
    },

    // Scenario C – Heavy Querying
    aggregates: async (_, { deviceId, from, to }, { db }) => {
      const conditions = [];
      const params = [];

      if (deviceId) { params.push(deviceId); conditions.push(`device_id = $${params.length}`); }
      if (from)     { params.push(from);     conditions.push(`hour >= $${params.length}::timestamptz`); }
      if (to)       { params.push(to);       conditions.push(`hour <= $${params.length}::timestamptz`); }

      const where = conditions.length ? `WHERE ${conditions.join(' AND ')}` : '';
      const sql = `
        SELECT hour, device_id, location,
               avg_temperature, avg_humidity, avg_pressure,
               avg_light, avg_sound, total_motion_events,
               min_battery, reading_count
        FROM hourly_aggregates
        ${where}
        ORDER BY hour DESC
        LIMIT 1000
      `;

      const { rows } = await db.query(sql, params);
      return rows.map(mapAggregate);
    },

    devices: async (_, __, { db }) => {
      const { rows } = await db.query('SELECT device_id, location, created_at FROM devices ORDER BY device_id');
      return rows.map(r => ({
        deviceId:  r.device_id,
        location:  r.location,
        createdAt: r.created_at?.toISOString(),
      }));
    },
  },

  Mutation: {
    // Scenario A – High-Frequency Ingestion
    ingestReading: async (_, args, { db }) => {
      const { deviceId, timestamp, temperature, humidity, pressure, light, sound, motion, battery, location } = args;
      const { rows } = await db.query(
        `INSERT INTO sensor_readings
           (timestamp, device_id, temperature, humidity, pressure, light, sound, motion, battery, location)
         VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10)
         RETURNING id`,
        [timestamp ?? new Date().toISOString(), deviceId, temperature, humidity, pressure, light, sound, motion ?? 0, battery, location]
      );
      return { id: rows[0].id.toString() };
    },
  },
};

function mapReading(r) {
  return {
    id:          r.id.toString(),
    timestamp:   r.timestamp instanceof Date ? r.timestamp.toISOString() : r.timestamp,
    deviceId:    r.device_id,
    temperature: r.temperature !== null ? parseFloat(r.temperature) : null,
    humidity:    r.humidity    !== null ? parseFloat(r.humidity)    : null,
    pressure:    r.pressure    !== null ? parseFloat(r.pressure)    : null,
    light:       r.light,
    sound:       r.sound,
    motion:      r.motion,
    battery:     r.battery     !== null ? parseFloat(r.battery)     : null,
    location:    r.location,
  };
}

function mapAggregate(r) {
  return {
    hour:              r.hour instanceof Date ? r.hour.toISOString() : r.hour,
    deviceId:          r.device_id,
    location:          r.location,
    avgTemperature:    r.avg_temperature    !== null ? parseFloat(r.avg_temperature)    : null,
    avgHumidity:       r.avg_humidity       !== null ? parseFloat(r.avg_humidity)       : null,
    avgPressure:       r.avg_pressure       !== null ? parseFloat(r.avg_pressure)       : null,
    avgLight:          r.avg_light          !== null ? parseFloat(r.avg_light)          : null,
    avgSound:          r.avg_sound          !== null ? parseFloat(r.avg_sound)          : null,
    totalMotionEvents: r.total_motion_events !== null ? parseInt(r.total_motion_events) : null,
    minBattery:        r.min_battery        !== null ? parseFloat(r.min_battery)        : null,
    readingCount:      r.reading_count      !== null ? parseInt(r.reading_count)        : null,
  };
}

CREATE TABLE IF NOT EXISTS devices (
    device_id   VARCHAR(50) PRIMARY KEY,
    location    VARCHAR(100),
    created_at  TIMESTAMPTZ DEFAULT NOW()
);


CREATE TABLE IF NOT EXISTS sensor_readings (
    id          BIGSERIAL,
    timestamp   TIMESTAMPTZ         NOT NULL,
    device_id   VARCHAR(50)         NOT NULL REFERENCES devices(device_id),
    temperature NUMERIC(6,2),       -- °C
    humidity    NUMERIC(6,2),       -- %
    pressure    NUMERIC(8,2),       -- hPa
    light       INTEGER,            -- lux
    sound       INTEGER,            -- dB
    motion      SMALLINT,           -- 0 or 1 (boolean)
    battery     NUMERIC(5,2),       -- %
    location    VARCHAR(100),

    PRIMARY KEY (id, timestamp)
);


CREATE INDEX IF NOT EXISTS idx_sensor_timestamp
    ON sensor_readings (timestamp DESC);

CREATE INDEX IF NOT EXISTS idx_sensor_device_id
    ON sensor_readings (device_id);

CREATE INDEX IF NOT EXISTS idx_sensor_device_time
    ON sensor_readings (device_id, timestamp DESC);

CREATE INDEX IF NOT EXISTS idx_sensor_location
    ON sensor_readings (location);


CREATE OR REPLACE VIEW hourly_aggregates AS
SELECT
    date_trunc('hour', timestamp) AS hour,
    device_id,
    location,
    ROUND(AVG(temperature)::numeric, 2)  AS avg_temperature,
    ROUND(AVG(humidity)::numeric, 2)     AS avg_humidity,
    ROUND(AVG(pressure)::numeric, 2)     AS avg_pressure,
    ROUND(AVG(light)::numeric, 0)        AS avg_light,
    ROUND(AVG(sound)::numeric, 0)        AS avg_sound,
    SUM(motion)                          AS total_motion_events,
    ROUND(MIN(battery)::numeric, 2)      AS min_battery,
    COUNT(*)                             AS reading_count
FROM sensor_readings
GROUP BY date_trunc('hour', timestamp), device_id, location
ORDER BY hour DESC, device_id;

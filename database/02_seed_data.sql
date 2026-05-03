-- ============================================
-- IoT Communication Benchmark
-- 02_seed_data.sql
-- ============================================

-- Insert all unique devices (Device_1 to Device_50)
INSERT INTO devices (device_id, location) VALUES
('Device_1',  'Mixed'),('Device_2',  'Mixed'),('Device_3',  'Mixed'),
('Device_4',  'Mixed'),('Device_5',  'Mixed'),('Device_6',  'Mixed'),
('Device_7',  'Mixed'),('Device_8',  'Mixed'),('Device_9',  'Mixed'),
('Device_10', 'Mixed'),('Device_11', 'Mixed'),('Device_12', 'Mixed'),
('Device_13', 'Mixed'),('Device_14', 'Mixed'),('Device_15', 'Mixed'),
('Device_16', 'Mixed'),('Device_17', 'Mixed'),('Device_18', 'Mixed'),
('Device_19', 'Mixed'),('Device_20', 'Mixed'),('Device_21', 'Mixed'),
('Device_22', 'Mixed'),('Device_23', 'Mixed'),('Device_24', 'Mixed'),
('Device_25', 'Mixed'),('Device_26', 'Mixed'),('Device_27', 'Mixed'),
('Device_28', 'Mixed'),('Device_29', 'Mixed'),('Device_30', 'Mixed'),
('Device_31', 'Mixed'),('Device_32', 'Mixed'),('Device_33', 'Mixed'),
('Device_34', 'Mixed'),('Device_35', 'Mixed'),('Device_36', 'Mixed'),
('Device_37', 'Mixed'),('Device_38', 'Mixed'),('Device_39', 'Mixed'),
('Device_40', 'Mixed'),('Device_41', 'Mixed'),('Device_42', 'Mixed'),
('Device_43', 'Mixed'),('Device_44', 'Mixed'),('Device_45', 'Mixed'),
('Device_46', 'Mixed'),('Device_47', 'Mixed'),('Device_48', 'Mixed'),
('Device_49', 'Mixed'),('Device_50', 'Mixed')
ON CONFLICT (device_id) DO NOTHING;

-- ============================================
-- Load full CSV data using PostgreSQL COPY
-- The CSV file must be placed in /database/
-- and mounted via Docker volume (see docker-compose.yml)
-- ============================================
COPY sensor_readings(timestamp, device_id, temperature, humidity, pressure, light, sound, motion, battery, location)
FROM '/docker-entrypoint-initdb.d/real_time_data.csv'
DELIMITER ','
CSV HEADER;

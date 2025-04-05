import json
import random
import pyodbc
import re
import platform
from datetime import datetime, timedelta

# Load appsettings.json
with open('../FlowGuardMonitoring.WebHost/appsettings.json') as f:
    config = json.load(f)

connection_string = config["ConnectionStrings"]["DefaultConnection"]
matches = re.findall(r'(\w+)=([^;]+)', connection_string)
connection_params = {key.lower(): value for key, value in matches}

server = connection_params["server"]
database = connection_params["database"]
is_windows = platform.system() == "Windows"

if is_windows:
    connection_string = (
        f'DRIVER={{ODBC Driver 18 for SQL Server}};'
        f'SERVER={server};'
        f'DATABASE={database};'
        f'Trusted_Connection=Yes;'
        f'TrustServerCertificate=Yes;'
    )
else:
    connection_string = (
        f'DRIVER={{ODBC Driver 18 for SQL Server}};'
        f'SERVER={server};'
        f'DATABASE={database};'
        f'UID={connection_params["id"]};'
        f'PWD={connection_params["password"]};'
        f'TrustServerCertificate=YES;'
    )

conn = pyodbc.connect(connection_string)
cursor = conn.cursor()

# Mapping
SENSOR_TYPE_MAPPING = {
    1: "Ph",
    2: "Level",
    3: "Temperature",
    4: "Quality",
    5: "Contaminants"
}

def get_active_sensors():
    cursor.execute("""
        SELECT s.SensorId, s.Type, st.Latitude, st.Longitude
        FROM Sensors s
        JOIN Sites st ON s.SiteId = st.SiteId
        WHERE s.IsActive = 1
    """)
    sensors = []
    for row in cursor.fetchall():
        sensor_type = SENSOR_TYPE_MAPPING.get(row.Type, "Unknown")
        sensor = {
            "id": row.SensorId,
            "type": sensor_type,
            "location": {
                "lat": row.Latitude,
                "lon": row.Longitude,
                "depth": 10 if sensor_type == "Level" else None
            }
        }
        sensors.append(sensor)
    return sensors

def generate_measurement(sensor, timestamp):
    data = {
        "timestamp": timestamp,
        "sensorId": sensor["id"],
        "value": None,
        "rawValue": 0,
    }
    sensor_type = sensor["type"]

    if sensor_type == "Ph":
        data["rawValue"] = round(random.uniform(5.5, 8.5), 2)
        data["value"] = str(data["rawValue"])
    elif sensor_type == "Level":
        depth = sensor["location"]["depth"] or 10
        data["rawValue"] = round(random.uniform(0, depth), 2)
        data["value"] = f"{data['rawValue']}m"
    elif sensor_type == "Temperature":
        data["rawValue"] = round(random.uniform(-10, 40), 2)
        data["value"] = f"{data['rawValue']}°C"
    elif sensor_type == "Quality":
        data["rawValue"] = random.randint(0, 100)
        data["value"] = f"{data['rawValue']}%"
    elif sensor_type == "Contaminants":
        contaminants = ["Lead", "Mercury", "Arsenic", "Nitrates", "Pesticides", "Cadmium"]
        selected = random.sample(contaminants, random.randint(1, 3))
        data["value"] = ", ".join(selected)
        data["rawValue"] = 0  # can be adjusted if needed
    return data

def insert_measurement(measurement):
    cursor.execute("""
        INSERT INTO FlowGuardMonitoringDB.dbo.Measurements (Timestamp, SensorId, Value, RawValue)
        VALUES (?, ?, ?, ?)
    """, measurement["timestamp"], measurement["sensorId"], measurement["value"], measurement["rawValue"])

def main():
    sensors = get_active_sensors()
    now = datetime.now()
    start = now - timedelta(days=14)

    print(f"Inserting measurements from {start} to {now}...")

    total = 0
    for sensor in sensors:
        time = start
        while time < now:
            measurement = generate_measurement(sensor, time)
            insert_measurement(measurement)
            total += 1
            time += timedelta(hours=1)

    conn.commit()
    print(f"Inserted {total} measurements for {len(sensors)} sensors.")

if __name__ == "__main__":
    main()
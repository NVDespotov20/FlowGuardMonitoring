import json
import random
import pyodbc
import re
from flask import Flask, jsonify
from datetime import datetime

app = Flask(__name__)

# ------------------------------------------------------------------------------
# 1. Load Database Configuration from appsettings.json
# ------------------------------------------------------------------------------
with open('../FlowGuardMonitoring.WebHost/appsettings.json') as f:
    config = json.load(f)

# Retrieve the connection string from the configuration file.
connection_string = config["ConnectionStrings"]["DefaultConnection"]

matches = re.findall(r'(\w+)=([^;]+)', connection_string)

connection_params = {key.lower(): value for key, value in matches}

connection_string = (
    f'DRIVER={{ODBC Driver 18 for SQL Server}};'
    f'SERVER={connection_params["server"]};'
    f'DATABASE={connection_params["database"]};'
    f'UID={connection_params["id"]};'
    f'PWD={connection_params["password"]};'
    f'TrustServerCertificate={connection_params["trustservercertificate"]}'
)

connection_string = connection_string.replace("TrustServerCertificate=True", "TrustServerCertificate=YES")

odbc_driver = "{ODBC Driver 17 for SQL Server}"
if "DRIVER=" not in connection_string.upper():
    connection_string = f"DRIVER={odbc_driver};{connection_string}"
conn = pyodbc.connect(connection_string)

# ------------------------------------------------------------------------------
# 3. Sensor Type Mapping
# ------------------------------------------------------------------------------
SENSOR_TYPE_MAPPING = {
    1: "Ph",
    2: "Level",
    3: "Temperature",
    4: "Quality",
    5: "Contaminants"
}

# ------------------------------------------------------------------------------
# 4. Helper Function: Retrieve Sensor Information from the Database
# ------------------------------------------------------------------------------
def get_sensor(sensor_id):
    cursor = conn.cursor()
    query = """
        SELECT 
            s.SensorId, 
            s.Name AS SensorName, 
            s.Type, 
            s.InstallationDate, 
            s.IsActive, 
            s.SiteId,
            st.Latitude, 
            st.Longitude
        FROM Sensors s
        JOIN Sites st ON s.SiteId = st.SiteId
        WHERE s.SensorId = ?
    """
    cursor.execute(query, sensor_id)
    row = cursor.fetchone()

    if row:
        sensor_type = SENSOR_TYPE_MAPPING.get(row.Type, "Unknown")

        sensor = {
            "id": row.SensorId,
            "name": row.SensorName,
            "type": sensor_type,
            "installation_date": row.InstallationDate,
            "is_active": row.IsActive,
            "site_id": row.SiteId,
            "location": {
                "lat": row.Latitude,
                "lon": row.Longitude
            }
        }
        if sensor_type == "Level":
            sensor["location"]["depth"] = 10  # default depth in meters;
        return sensor
    else:
        return None

# ------------------------------------------------------------------------------
# 5. Helper Function: Generate a Measurement Based on Sensor Type
# ------------------------------------------------------------------------------
def generate_measurement(sensor):
    # Default values for the measurement
    data = {
        "timeStamp": datetime.now().isoformat(),
        "value": None,
        "rawValue": 0,
    }

    sensor_type = sensor.get("type")

    if sensor_type == "Ph":
        data["rawValue"] = round(random.uniform(5.5, 8.5), 2)
        data["value"] = str(data["rawValue"])

    elif sensor_type == "Level":
        depth = sensor.get("location", {}).get("depth", 10)
        data["rawValue"] = round(random.uniform(0, depth), 2)
        data["value"] = str(data["rawValue"]) + "m"

    elif sensor_type == "Temperature":
        data["rawValue"] = round(random.uniform(-10, 40), 2)
        data["value"] = str(data["rawValue"]) + "°C"

    elif sensor_type == "Quality":
        data["rawValue"] = random.randint(0, 100)
        data["value"] = str(data["rawValue"]) + "%"

    elif sensor_type == "Contaminants":
        possible_contaminants = ["Lead", "Mercury", "Arsenic", "Nitrates", "Pesticides", "Cadmium"]
        num = random.randint(1, len(possible_contaminants))
        contaminants = random.sample(possible_contaminants, num)
        data["value"] = ", ".join(contaminants)

    return data

# ------------------------------------------------------------------------------
# 6. Define the REST Endpoint
# ------------------------------------------------------------------------------
@app.route('/sensor/<sensor_id>/measurement', methods=['GET'])
def sensor_measurement(sensor_id):
    # Retrieve sensor details from the database.
    sensor = get_sensor(sensor_id)
    if not sensor:
        return jsonify({"error": "Sensor not found"}), 404

    # Optionally, check if the sensor is active.
    if not sensor.get("is_active"):
        return jsonify({"error": "Sensor is not active"}), 400

    # Generate measurement data based on the sensor type.
    data = generate_measurement(sensor)

    return jsonify(data)

# ------------------------------------------------------------------------------
# 7. Run the Flask Application
# ------------------------------------------------------------------------------
if __name__ == '__main__':
    app.run(debug=True)

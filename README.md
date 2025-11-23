## Work flow
1. Device connects via MQTT over WSS

2. Device sends JWT in WebSocket upgrade header

3. YARP middleware intercepts header, validates JWT, extracts tenantId/deviceId

4. Optionally, rewrite topic: "device/data" → "company/{tenantId}/device/{deviceId}/data"

5. Forward WebSocket connection to Mosquitto

6. Mosquitto only routes messages — no auth plugin required

<img width="200" height="400" alt="PiHR (2)" src="https://github.com/user-attachments/assets/90ccd95b-b9d9-4e2a-b01f-47c10cee763b" />

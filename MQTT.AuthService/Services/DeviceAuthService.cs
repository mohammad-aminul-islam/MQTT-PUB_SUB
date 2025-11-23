namespace MQTT.AuthService.Services;

public class DeviceAuthService
{
    public static List<string> validDevices = new List<string>
    {
        "device101",
        "device102",
        "device201"
    };
    public bool IsValidToken(string token, out string tenantId, out string deviceId)
    {
        tenantId = "";
        deviceId = "";
        if (string.IsNullOrEmpty(token))
            return false;

        // Decode JWT or API key and extract tenantId/deviceId
        // For demo: accept token = "tenant1-device101"
        var parts = token.Split(" ");
        if (parts.Length == 2 && validDevices.Any(x => x.Equals(parts.Last())))
        {
            tenantId = parts[0];
            deviceId = parts[1];
            return true;
        }

        return false;
    }
}

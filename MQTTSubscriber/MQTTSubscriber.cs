using MQTTnet;
using System.Text;
using System.Text.Json;

namespace MQTT.Demo
{
    public class MQTTSubscriber
    {
        private IMqttClient _client;
        public static List<DeviceAttendanceDto> deviceAttendances = new List<DeviceAttendanceDto>();
        public async Task ConnectAsync()
        {
            try
            {
                var factory = new MqttClientFactory();
                _client = factory.CreateMqttClient();

                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer("localhost", 1883)   // Mosquitto broker
                    .WithClientId("BackendSubscriber")
                    .WithCleanSession(false)
                    .Build();

                _client.ConnectedAsync += async e =>
                {
                    Console.WriteLine("Connected to MQTT Broker!");

                    // Subscribe to all device temperature topics using wildcard
                    await _client.SubscribeAsync("devices/+", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
                    Console.WriteLine("Subscribed to all device temperature topics");
                };

                _client.DisconnectedAsync += async e =>
                {
                    Console.WriteLine("Disconnected. Reconnecting in 2 seconds...");
                    await Task.Delay(2000);
                    await _client.ConnectAsync(options);
                };

                _client.ApplicationMessageReceivedAsync += e =>
                {
                    string topic = e.ApplicationMessage.Topic;
                    string other =JsonSerializer.Serialize( e.ApplicationMessage.UserProperties);
                    string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    var model = JsonSerializer.Deserialize<DeviceAttendanceDto>(payload);
                    Console.WriteLine($"Subscriber received message from '{topic}': {payload} {other}");
                    deviceAttendances.Add(model);
                    Console.WriteLine($"Data count:{MQTTSubscriber.deviceAttendances.Count}");
                    // TODO: Process message (e.g., store in database)
                    return Task.CompletedTask;
                };

                await _client.ConnectAsync(options);

                Console.WriteLine("Subscriber is running. Press Ctrl+C to exit.");

                // Keep the subscriber alive
                await Task.Delay(-1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message} {ex.StackTrace}");
            }
        }
    }
}
public class DeviceAttendanceDto
{
    public int rf_id { get; set; }
    public DateTime attendance_time { get; set; }
}
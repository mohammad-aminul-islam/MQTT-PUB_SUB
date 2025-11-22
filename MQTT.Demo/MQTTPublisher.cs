using MQTTnet;
using System.Text;
using System.Text.Json;

namespace MQTT.Demo;

public class MQTTPublisher
{
    private IMqttClient _client;

    public async Task ConnectAndPublishAsync(string device)
    {
        var factory = new MqttClientFactory();
        _client = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("localhost", 1883)
            .WithClientId(device)
            .WithCleanSession(false) // Publisher should use clean session
            .Build();

        _client.ConnectedAsync += async e =>
        {
            Console.WriteLine($"Connected to MQTT Broker! ResultCode: {e.ConnectResult.ResultCode} MaximumQoS:{e.ConnectResult.MaximumQoS}");
            Console.WriteLine($"Connected to MQTT Broker! MaximumPacketSize: {e.ConnectResult.MaximumPacketSize} AuthenticationData:{e.ConnectResult.AuthenticationData}");
            Console.WriteLine($"Connected to MQTT Broker! TopicAliasMaximum: {e.ConnectResult.TopicAliasMaximum} RetainAvailable:{e.ConnectResult.RetainAvailable}");
        };

        _client.DisconnectedAsync += async e =>
        {
            Console.WriteLine("Disconnected. Reconnecting in 2 seconds...");
            await Task.Delay(2000);
            try
            {
                await _client.ConnectAsync(options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reconnection failed: {ex.Message}");
            }
        };

        await _client.ConnectAsync(options);
        Console.WriteLine("Start publishing messages...");

        var random = new Random();
        while (true)
        {
            try
            {
                // Simulated sensor data
                var payload = new
                {
                    rf_id = random.Next(1, 9999),
                    attendance_time = DateTime.UtcNow.AddMinutes(360)
                };
                var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic($"devices/{device}")
                    .WithPayload(payloadJson)
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce) // QoS 1 for persistence
                    .WithRetainFlag(false) // Set to true only if you want to retain the LAST message
                    .Build();

                var result = await _client.PublishAsync(message);

                if (result.ReasonCode == MqttClientPublishReasonCode.Success)
                {
                    Console.WriteLine($"Published: {payload}");
                }
                else
                {
                    Console.WriteLine($"Publish failed: {JsonSerializer.Serialize(result)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing: {ex.Message}");
            }

            await Task.Delay(5000); // Send every 5 seconds
        }
    }
}
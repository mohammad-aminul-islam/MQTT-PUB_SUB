using MQTTnet;
using System.Text;

namespace MQTT.Demo;

public class MQTTPublisher
{
    private IMqttClient _client;

    public async Task ConnectAndPublishAsync()
    {
        var factory = new MqttClientFactory();
        _client = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("localhost", 1883)
            .WithClientId("IoTDevice001")
            .WithCleanSession(true) // Publisher should use clean session
            .Build();

        _client.ConnectedAsync += async e =>
        {
            Console.WriteLine("Connected to MQTT Broker!");
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
                string payload = $"Temperature:{random.Next(20, 30)}°C at {DateTime.Now:HH:mm:ss}";

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic("device/IoTDevice001/temperature")
                    .WithPayload(payload)
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
                    Console.WriteLine($"Publish failed: {result.ReasonCode}");
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
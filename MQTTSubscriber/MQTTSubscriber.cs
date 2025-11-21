using MQTTnet;
using System.Text;

namespace MQTT.Demo
{
    public class MQTTSubscriber
    {
        private IMqttClient _client;

        public async Task ConnectAsync()
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
                await _client.SubscribeAsync("device/+/temperature", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
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
                string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                Console.WriteLine($"Subscriber received message from '{topic}': {payload}");

                // TODO: Process message (e.g., store in database)
                return Task.CompletedTask;
            };

            await _client.ConnectAsync(options);

            Console.WriteLine("Subscriber is running. Press Ctrl+C to exit.");

            // Keep the subscriber alive
            await Task.Delay(-1);
        }
    }
}

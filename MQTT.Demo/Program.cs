using MQTT.Demo;
Console.WriteLine("Enter device no");
string device = Console.ReadLine();
new MQTTPublisher().ConnectAndPublishAsync(device);
Console.ReadLine();

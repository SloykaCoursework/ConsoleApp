using ConsoleApp;
using CourseWorkLibrary;
using CourseWorkLibrary.DataApi;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Threading.Tasks;
using System.Text;

const string address = "192.168.1.20";
const string port = "5555";

while (true)
{

    var command = PrintMenu();
    if (command is null) return;

    var serverAdress = new Uri($"tcp://{address}:{port}");
    var client = new NetTcpClient(serverAdress);
    Console.WriteLine($"Connect to server at {serverAdress}");
    await client.ConnectAsync();

    var userApi = new DataApiClient(client);
    await ManageResult(userApi, command ?? CommandCode.CreateData);

    Console.ReadKey();
    client.Dispose();

}

static async Task ManageResult(IDataApi dataApi, CommandCode command)
{
    
    var result = await dataApi.SendCommand(command);

    Console.WriteLine($"token = {result}");

    var factory = new ConnectionFactory { HostName = "192.168.1.35" };
    using var connection = await factory.CreateConnectionAsync();
    using var channel = await connection.CreateChannelAsync();

    await channel.QueueDeclareAsync(queue: result, durable: true, exclusive: false, autoDelete: false, arguments: null);

    bool isRecive = false;

    var consumer = new AsyncEventingBasicConsumer(channel);
    consumer.ReceivedAsync += (model, ea) =>
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        Console.WriteLine(message);
        isRecive = true;
        return Task.CompletedTask;
    };

    while (!isRecive)
    {
        await channel.BasicConsumeAsync(result, autoAck: true, consumer: consumer);
    }

}

static CommandCode? PrintMenu()
{

    while (true)
    {

        Console.Clear();
        Console.Write($"=== Commands List ===\n" +
            $"{((int)CommandCode.CreateData)} - {CommandCode.CreateData}\n" +
            $"{((int)CommandCode.FindArithmeticMeanValues)} - {CommandCode.FindArithmeticMeanValues}\n" +
            $"{((int)CommandCode.CountAllGroups)} - {CommandCode.CountAllGroups}\n" +
            $"{((int)CommandCode.ChangeCourseToAll)} - {CommandCode.ChangeCourseToAll}\n" +
            $"{((int)CommandCode.FindOldestStudent)} - {CommandCode.FindOldestStudent}\n" +
            $"{((int)CommandCode.FindYoungerInstructor)} - {CommandCode.FindYoungerInstructor}\n" +
            $"0 - Exit\n\n" +
            $"Chouse command: ");

        if (int.TryParse(Console.ReadLine(), out int res))
        {

            if (res == 0) return null;

            if (Enum.IsDefined<CommandCode>((CommandCode)res))
                return (CommandCode)res;

            Console.WriteLine("[ERROR] Wrong command\n" +
                "Press any key to continue...");
            Console.ReadKey();
            continue;

        }

        Console.WriteLine("[ERROR] Command read error\n" +
                "Press any key to continue...");
        Console.ReadKey();

    }

}
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;

class SimpleTCPServer
{
    static void Main(string[] args)
    {
        IPAddress ip = IPAddress.Parse("127.0.0.1");
        TcpListener serverSocket = new TcpListener(ip, 14000);
        serverSocket.Start();
        Console.WriteLine("Server is ready...");

        while (true)
        {
            TcpClient connectionSocket = serverSocket.AcceptTcpClient();
            Console.WriteLine("Client connected.");
            EchoService service = new EchoService(connectionSocket);
            Task.Run(() => service.DoItAgain());
        }
    }
}

internal class EchoService
{
    private TcpClient connectionSocket;

    public EchoService(TcpClient connectionSocket)
    {
        this.connectionSocket = connectionSocket;
    }

    internal void DoIt()
    {
        try
        {
            using (Stream ns = connectionSocket.GetStream())
            using (StreamReader sr = new StreamReader(ns))
            using (StreamWriter sw = new StreamWriter(ns) { AutoFlush = true })
            {
                sr.ReadLine(); // Ignoring the first command, because funny stuff happens where uhh... well it tries to match it with a case as normal, but we dont want that just want to see funnniiiii commands... but it doesnt like it when it receives no commandies, so we start with this so it can be happy c: And so I can be happy at not having to see this shit anymore. Been a buncha asking chatbots "wtf is wrong with this pile of garbage" :c
                sw.WriteLine("Available commands: Random, Add, Subtract, Exit");

                bool waitingForNumbers = false;
                string command = null;

                while (true)
                {
                    if (!waitingForNumbers)
                    {
                        Console.WriteLine("Waiting for a command...");
                        command = sr.ReadLine()?.Trim().ToLower();

                        if (string.IsNullOrEmpty(command) || command == "exit")
                        {
                            sw.WriteLine("Goodbye!");
                            break;
                        }

                        if (command == "random" || command == "add" || command == "subtract")
                        {
                            sw.WriteLine("Please enter two numbers separated by a space:");
                            waitingForNumbers = true;
                        }
                        else
                        {
                            sw.WriteLine("Invalid command. Use: Random, Add, Subtract, Exit");
                        }
                    }
                    else
                    {
                        string numbers = sr.ReadLine();

                        if (string.IsNullOrEmpty(numbers)) return;

                        string[] parts = numbers.Split(' ');

                        if (parts.Length == 2
                            && int.TryParse(parts[0], out int num1)
                            && int.TryParse(parts[1], out int num2))
                        {
                            switch (command)
                            {
                                case "random":
                                    if (num1 <= num2)
                                    {
                                        Random random = new Random();
                                        int result = random.Next(num1, num2 + 1);
                                        sw.WriteLine($"Here's a random number between {num1} and {num2}: {result}");
                                    }
                                    else
                                    {
                                        sw.WriteLine("Oops! The first number should be less than or equal to the second.");
                                    }
                                    break;

                                case "add":
                                    sw.WriteLine($"The sum of {num1} and {num2} is: {num1 + num2}");
                                    break;

                                case "subtract":
                                    sw.WriteLine($"Subtracting gives us: {num1} minus {num2} equals {num1 - num2}");
                                    break;
                            }
                        }
                        else
                        {
                            sw.WriteLine("Invalid input. Make sure to enter two integers.");
                        }

                        waitingForNumbers = false;
                        command = null; // yayness
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred in EchoService: " + ex.Message);
        }
        finally
        {
            connectionSocket.Close();
        }
    }


    public void DoItAgain()
    {
        try
        {
            using (Stream ns = connectionSocket.GetStream())
            using (StreamReader sr = new StreamReader(ns))
            using (StreamWriter sw = new StreamWriter(ns) { AutoFlush = true })
            {
                // Read and parse the incoming JSON request
                string jsonRequest = sr.ReadLine();
                var request = JsonSerializer.Deserialize<Request>(jsonRequest);

                // Validate and process the request
                if (request != null && (request.Method == "random" || request.Method == "add" || request.Method == "subtract"))
                {
                    int result = 0;
                    string response = "";

                    switch (request.Method)
                    {
                        case "random":
                            if (request.Tal1 <= request.Tal2)
                            {
                                Random random = new Random();
                                result = random.Next(request.Tal1, request.Tal2 + 1);
                                response = $"Random number between {request.Tal1} and {request.Tal2}: {result}";
                            }
                            else
                            {
                                response = "Invalid input. Tal1 must be less than or equal to Tal2.";
                            }
                            break;

                        case "add":
                            result = request.Tal1 + request.Tal2;
                            response = $"Sum of {request.Tal1} and {request.Tal2}: {result}";
                            break;

                        case "subtract":
                            result = request.Tal1 - request.Tal2;
                            response = $"Difference of {request.Tal1} and {request.Tal2}: {result}";
                            break;
                    }

                    // I think this assignmnet sucks ass, nooooo idea wtf im doing
                    var jsonResponse = JsonSerializer.Serialize(new { result = response });
                    sw.WriteLine(jsonResponse);
                }
                else
                {
                    var errorResponse = JsonSerializer.Serialize(new { error = "Invalid method or malformed JSON" });
                    sw.WriteLine(errorResponse);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error in JsonTcpServer: " + ex.Message);
        }
        finally
        {
            connectionSocket.Close();
            //Think we are a doneso... I recommend whoever looks at this pile of garbage to play this Discography to figure out my mental state of making this https://www.youtube.com/watch?v=fvt4lZl1pv0&t=2s I do think he unironically has some staight bangers, but thats mostly becaue he ripped the rhythm/melody from others the "With Reshiram" @1:08:27 slaps, Bonus songs sucks balls tho, like Viktor Orban
        }
    }

    private class Request
    {
        public string Method { get; set; }
        public int Tal1 { get; set; }
        public int Tal2 { get; set; }
    }
}

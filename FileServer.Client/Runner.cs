using System;
using System.Collections.Generic;
using System.Linq;

namespace FileServer.Client
{
    public class Runner
    {
        public static void Main()
        {
            Console.Title = "File Server Client v1.0";
            Console.WriteLine("***FILE-SERVER CLIENT v1.0 IS RUNNING***");
            Console.WriteLine();
            FileServerClient client = null;

            var validServerDetails = false;
            while (validServerDetails == false)
            {
                try
                {
                    Console.WriteLine("PLEASE ENTER THE SERVER HOSTNAME/IP:");
                    var ip = Console.ReadLine();
                    Console.WriteLine("PLEASE ENTER THE SERVER PORT:");
                    var port = int.Parse(Console.ReadLine());

                    client = new FileServerClient(ip, port);
                    validServerDetails = true;
                }
                catch
                {
                    Console.WriteLine("INVALID HOSTNAME OR PORT SUPPLIED");
                    Console.WriteLine();
                    validServerDetails = false;
                }
            }


            var isConnected = false;
            while (isConnected == false)
            {
                Console.WriteLine("PLEASE ENTER YOUR USERNAME:");
                var userName = Console.ReadLine();
                Console.WriteLine();
                Console.WriteLine("PLEASE ENTER YOUR PASSWORD:");
                var password = Console.ReadLine();
                Console.WriteLine();

                isConnected = client.Connect(userName, password);
                Console.WriteLine("PLEASE WAIT WHILE CONNECTING AND AUTHENTICATING..");

                if (isConnected == false)
                {
                    Console.WriteLine("INCORRECT USERNAME OR PASSWORD SUPPLIED");
                    Console.WriteLine();
                }
            }

            Console.WriteLine();
            Console.WriteLine("CONNECTED TO SERVER. YOU CAN USE THE FOLLOWING COMMANDS:");
            Console.WriteLine("1. LIST - IN ORDER TO SEE ALL FILES ON THE SERVER");
            Console.WriteLine("2. GET <file names list> - IN ORDER TO DOWNLOAD THE REQUESTED FILES FROM THE SERVER");
            Console.WriteLine("3. PUT <file names list> - IN ORDER TO UPLOAD THE REQUESTED FILES TO THE SERVER");
            Console.WriteLine("4. EXIT - IN ORDER TO LOG OFF FROM THE SERVER AND CLOSE THE CLIENT");
            Console.WriteLine();

            var shouldExit = false;
            while (shouldExit == false)
            {
                try
                {
                    Console.WriteLine("ENTER REQUESTED COMMAND:");
                    var userCommand = Console.ReadLine();

                    var commandAndParams = userCommand.Split(new[] { ' ' });
                    var command = commandAndParams.FirstOrDefault();

                    IEnumerable<string> commandParams = new List<string>();
                    if (commandAndParams.Length > 1)
                    {
                        commandParams = commandAndParams.Skip(1);
                    }

                    Console.WriteLine();

                    switch (command.ToLower())
                    {
                        case "list":
                            Console.WriteLine("LIST ALL FILES ON THE SERVER:");
                            var allFiles = client.List();
                            Console.WriteLine(allFiles);
                            Console.WriteLine();
                            break;

                        case "get":
                            Console.WriteLine($"DOWNLOADING FILES: {string.Join(",", commandParams.ToArray())}");
                            var result = client.GetFiles(commandParams.ToArray());
                            Console.WriteLine(result);
                            Console.WriteLine();
                            break;
                        case "put":
                            Console.WriteLine($"UPLOADING FILES: {string.Join(",", commandParams.ToArray())}");
                            var uploadResult = client.SendFiles(commandParams.ToArray());
                            Console.WriteLine(uploadResult);
                            break;
                        case "exit":
                            client.Disconnect();
                            shouldExit = true;
                            break;
                        default:
                            Console.WriteLine("PLEASE ENTER VALID COMMAND");
                            Console.WriteLine();
                            break;
                    }
                }
                catch
                {
                    Console.WriteLine("PLEASE ENTER VALID COMMAND");
                    Console.WriteLine();
                }

            }
        }
    }
}
/*
 * Project: Assignment 3
 * File: ManualOperations.cs
 * Author: Darsh Patel(8870657) and Bhumitkumar Patel(8847159)
 * Description: This file contains the functions for manual testing
 * Date: 2024-02-24
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public class ManualOperations
    {
        // login username
        private string username;

        /*
         * Function: SendLogMessage()
         * Parameters: ClientWebSocket clientWebSocket: login client socket
         *             string username: username of the client
         *             string level: level of the message to be sent
         *             string message: message value to be sent
         * Description: This function sends any logs to the server in specified format.
         * Return values: bool: true if positive server response
         */
        public static async Task<bool> SendLogMessage(ClientWebSocket clientWebSocket, string username, string level = "", string message = "")
        {
            // required variables
            string format = File.ReadAllText("../../config.txt").Replace("TextFormat: ", "").ToLower();
            string data = "";
            string timestamp = DateTime.UtcNow.ToString();

            // making message based on the format
            switch (format)
            {
                case "json":
                    data = $"{{\"username\":\"{username}\",\"level\":\"{level}\",\"message\":\"{message}\",\"timestamp\":\"{timestamp}\"}}";
                    break;
                case "xml":
                    data = $"<username>{username}</username><level>{level}</level><message>{message}</message><timestamp>{timestamp}</timestamp>";
                    break;
                default:
                    data = $"{username}|{level}|{message}|{timestamp}";
                    break;
            }

            try
            {
                // Convert the JSON message to a byte array
                byte[] messageBytes = Encoding.UTF8.GetBytes(data);

                // Send the JSON message to the server
                await clientWebSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending log message: {ex.Message}");
                return false;
            }

            string stringResponse = "";
            try
            {
                // getting the response from server
                var buffer = new byte[1024 * 4];
                WebSocketReceiveResult result;
                var response = new ArraySegment<byte>(buffer);
                result = await clientWebSocket.ReceiveAsync(response, CancellationToken.None);
                stringResponse = Encoding.UTF8.GetString(response.Array, 0, result.Count);

            }
            catch
            {
                stringResponse = "";
            }
            Console.WriteLine($"Response from server: {stringResponse}");
            if (stringResponse.Contains("Error"))
            {
                return false;
            }

            return true;
        }

        /*
         * Function: Login()
         * Parameters: Uri serverUri: server url
         *             bool manual: manual flag
         * Description: This function starts the login process for any user and provides response from server appropriately
         * Return values: clientWebSocket: login client socket value
         */
        public async Task<ClientWebSocket> Login(Uri serverUri, bool manual)
        {
            Console.WriteLine("------Login Test Started------");
            // creating a new client
            ClientWebSocket client = new ClientWebSocket();

            // connecting to server
            try
            {
                await client.ConnectAsync(serverUri, CancellationToken.None);

                // taking input of username

                if (manual)
                {
                    Console.Write("Enter your username: ");
                    username = Console.ReadLine();
                }

                // sending username to verify the user
                if (!await SendLogMessage(client, username, "REQ", "login"))
                {
                    Console.WriteLine("------Login Test Failed------\n");
                    return null;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not connect to server: {e.Message}");
                Console.WriteLine("------Login Test Failed------\n");
                return null;
            }
            Console.WriteLine("------Login Test Successful------\n");
            return client;
        }
        /*
         * Function: SendLog()
         * Parameters: ClientWebSocket clientWebSocket: login client socket
         * Description: This function allows the user to manually create a log and send it for desired number of times
         * Return values: void
         */
        public async Task SendLog(ClientWebSocket client)
        {
            Console.WriteLine("------Send Log Test Started------");
            Console.Write("How many logs you would like to send:");

            // required varaibles
            int logs = 0;
            int failed = 0;
            int success = 0;

            // take input of number of logs
            try
            {
                logs = int.Parse(Console.ReadLine());
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error:{e.Message}");
            }

            // take input of log parameters and send it
            for (int i = 0; i < logs; i++)
            {
                Console.WriteLine((i + 1) + ":");
                Console.WriteLine("Enter the following parameters for first test case");
                Console.Write("Level: ");
                string level = Console.ReadLine();
                Console.Write("Message: ");
                string message = Console.ReadLine();

                // sending the log
                if (await SendLogMessage(client, username, level, message))
                {
                    success++;
                }
                else
                {
                    failed++;
                }
            }

            // Final Result
            if (success + failed == logs)
            {
                Console.WriteLine($"------Send Log Test Success: {failed} failed & {success} successful------");
            }
            else
            {
                Console.WriteLine($"------Send Log Test Unsuccessful: {failed} failed & {success} successful------");
            }
            Console.WriteLine();
        }

        /*
         * Function: MultipleClient()
         * Parameters: Uri serverUri: server url
         * Desccription: This function tests the logging system for multiple clients manually
         * Return values: void
         */
        public async Task MultipleClient(Uri serverUri)
        {
            Console.WriteLine("------Multiple Client Test Started------");
            Console.WriteLine("Attempting to connect to server...");

            List<ClientWebSocket> connectedClients = new List<ClientWebSocket>();

            // required variables
            int clients = 0;
            int success = 0;
            int failed = 0;

            // taking input of client numbers
            Console.Write("Enter the number of clients: ");
            try
            {
                clients = int.Parse(Console.ReadLine());
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error:{e.Message}");
            }

            try
            {
                for (int i = 0; i < clients; i++)
                {
                    // Create a new client
                    ClientWebSocket client = new ClientWebSocket();

                    // Connect to the server
                    await client.ConnectAsync(serverUri, CancellationToken.None);

                    Console.Write($"Enter the username of user {i + 1}: ");
                    string user = Console.ReadLine();

                    // Send username to verify the user
                    if (await SendLogMessage(client, user, "REQ", "login"))
                    {
                        success++;
                    }
                    else
                    {
                        failed++;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error connecting to server: {e.Message}");
            }

            // Final result
            if (success + failed == clients)
            {
                Console.WriteLine($"------Multiple Client Test Success: {failed} fail & {success} successful------\n");
            }
            else
            {
                Console.WriteLine($"------Multiple Client Test Failed: {failed} fail & {success} successful------\n");
            }

        }

        /*
        * Function: StressTest()
        * Parameters: Uri serverUri: server url
        * Desccription: This function tests the logging system for stress testing manually
        * Return values: void
        */
        public async Task StressTest(Uri serverUri)
        {
            Console.WriteLine("------Stress Test Started------");
            Console.WriteLine("Attempting to connect to server...");


            Console.Write("Enter the number of clients: ");
            int clientsNum = 0;

            try
            {
                clientsNum = int.Parse(Console.ReadLine());
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error:{e.Message}");
            }

            List<ClientWebSocket> clients = new List<ClientWebSocket>();
            Dictionary<ClientWebSocket, string> clientUsernames = new Dictionary<ClientWebSocket, string>();

            int receivedCounts = 0;

            try
            {
                Console.WriteLine("------Login Process Started------");
                // Connect all clients
                for (int i = 0; i < clientsNum; i++)
                {

                    string user = "testUser" + i; // Generate username
                    ClientWebSocket client = new ClientWebSocket();
                    await client.ConnectAsync(serverUri, CancellationToken.None);
                    Console.WriteLine($"Client {i + 1} connected to the server");

                    clients.Add(client);
                    clientUsernames.Add(client, user); // Store the username for this client

                    // Send login message with the same username used for connecting
                    await SendLogMessage(client, user, "REQ", "login");

                    Console.WriteLine($"Login message sent for client {i + 1}");
                }
                Console.WriteLine("------Login Process completed------");
                Console.WriteLine();

                // Wait for 2 seconds after all clients have connected
                await Task.Delay(2000);

                Console.WriteLine("------Sending Log Messages------");
                // Send test log message for all clients
                foreach (var kvp in clientUsernames)
                {
                    ClientWebSocket client = kvp.Key;
                    string username = kvp.Value;

                    if (await SendLogMessage(client, username, "INFO", "This is a test log"))
                    {
                        receivedCounts++;
                    }
                    Console.WriteLine($"Test log message sent for client with username: {username}");
                }
                Console.WriteLine("------Log Messages Sent------");
                Console.WriteLine();
                // Wait for 2 seconds after all clients have sent test log messages
                await Task.Delay(2000);

                Console.WriteLine("------Log out all users------");
                foreach (var kvp in clientUsernames)
                {
                    ClientWebSocket client = kvp.Key;
                    string username = kvp.Value;

                    // sending log message
                    await SendLogMessage(client, username, "REQ", "logout");
                    Console.WriteLine($"Test log message sent for client with username: {username}");
                }
                Console.WriteLine("------Log out process completed------");
            }
            catch (Exception e)
            {
                Console.WriteLine($"------Stress Test failed: {e.Message}------");
                return;
            }

            Console.WriteLine();
            if (receivedCounts == clients.Count)
            {
                Console.WriteLine($"------Stress Test Success------");
            }
            else
            {
                Console.WriteLine("------Stress Test Failed------");
            }
            Console.WriteLine();
        }

        /*
         * Function: StressTest()
         * Parameters: Uri serverUri: server url
         * Desccription: This function tests the logging system for abuse testing manually
         * Return values: void
         */
        public async Task AbuseTest(Uri serverUri)
        {
            try
            {
                Console.WriteLine("------ Abuse Test 1 started: Anonymous user trying login repeatedly ------");

                // required variables
                string user;

                // name input
                Console.Write("Enter the username: ");
                try
                {
                    user = Console.ReadLine();
                }
                catch
                {
                    user = "ManualAbuser";
                }

                List<ClientWebSocket> clients = new List<ClientWebSocket>();

                try
                {
                    Console.WriteLine("------Login Process Started------");
                    // Connect all clients
                    for (int i = 0; i < 10; i++)
                    {
                        ClientWebSocket client = new ClientWebSocket();
                        await client.ConnectAsync(serverUri, CancellationToken.None);
                        Console.WriteLine($"Client connected to the server");

                        clients.Add(client);

                        // Send login message with the same username used for connecting
                        await SendLogMessage(client, user, "REQ", "login");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }

                Console.WriteLine($"------ {user} has been blocked ------\n");

                // Abuse test 2
                Console.WriteLine("------ Abuse Test 2 started: Anonymous user sending wrong logs repeatedly ------");

                ClientWebSocket anonymousClient = new ClientWebSocket();
                await anonymousClient.ConnectAsync(serverUri, CancellationToken.None);

                // name input
                Console.Write("Enter the username: ");
                try
                {
                    user = Console.ReadLine();
                }
                catch
                {
                    user = "ManualAbuser";
                }

                while (!await SendLogMessage(anonymousClient, user, "REQ", "login"))
                {
                    Console.WriteLine("Invalid user, Try again.\n");
                    anonymousClient.Dispose();
                    anonymousClient = new ClientWebSocket();
                    await anonymousClient.ConnectAsync(serverUri, CancellationToken.None);
                    Console.Write("Enter the username: ");
                    user = Console.ReadLine();
                }

                for (int i = 0; i < 10; i++)
                {
                    // sending log
                    await SendLogMessage(anonymousClient, user, "INVALID", "This is a noisy abuser");
                }

                if (anonymousClient.State == WebSocketState.Open)
                {
                    await anonymousClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                }
                anonymousClient.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}

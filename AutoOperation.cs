/*
 * Project: Assignment 3
 * File: AutoOperations.cs
 * Author: Darsh Patel(8870657) and Bhumitkumar Patel(8847159)
 * Description: This file contains the functions for auto testing
 * Date: 2024-02-24
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public class AutoOperation
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
        public async Task<ClientWebSocket> Login(Uri serverUri)
        {
            Console.WriteLine("------Login Test Started------");
            // creating a new client
            ClientWebSocket client = new ClientWebSocket();

            // connecting to server
            try
            {
                await client.ConnectAsync(serverUri, CancellationToken.None);

                // taking input of username
                username = "TestUser";

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
         * Description: This function automatically sends 50 random log messages
         * Return values: void
         */
        public async Task SendLog(ClientWebSocket client)
        {
            Console.WriteLine("------Send Log Test Started------");

            // required varaibles
            int logs = 50;
            int failed = 0;
            int success = 0;

            // sending all logs 
            for (int i = 0; i < logs; i++)
            {
                Console.Write((i + 1) + ": ");
                string[] level = { "INFO", "ERROR", "WARN" };
                string[] message = { "Some information recieved", "Some error occured", "Some warnings appeared" };

                int j = new Random().Next(3);
                // sending the log
                if (await SendLogMessage(client, username, level[j], message[j]))
                {
                    success++;
                }
                else
                {
                    failed++;
                }
            }

            // Final result
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
         * Desccription: This function tests the logging system for multiple clients automatically
         * Return values: void
         */
        public async Task MultipleClient(Uri serverUri, List<string> usernames)
        {
            Console.WriteLine("------Multiple Client Test Started------");
            Console.WriteLine("Attempting to connect to server...");

            // required variables
            int success = 0;
            int failed = 0;

            try
            {
                for (int i = 0; i < usernames.Count; i++)
                {
                    // Create a new client
                    ClientWebSocket client = new ClientWebSocket();

                    // Connect to the server
                    await client.ConnectAsync(serverUri, CancellationToken.None);

                    // Send username to verify the user
                    if (await SendLogMessage(client, usernames[i], "REQ", "login"))
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

            // Final Result
            if (success + failed == usernames.Count)
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
         * Desccription: This function tests the logging system for stress testing automatically
         * Return values: void
         */
        public async Task<List<ClientWebSocket>> StressTest(Uri serverUri)
        {
            Console.WriteLine("------Stress Test Started------");
            Console.WriteLine("Attempting to connect to server...");

            // required variables
            List<ClientWebSocket> clients = new List<ClientWebSocket>();
            Dictionary<ClientWebSocket, string> clientUsernames = new Dictionary<ClientWebSocket, string>();
            int receivedCounts = 0;

            try
            {
                Console.WriteLine("------Login Process Started------");
                // Connect all clients
                for (int i = 0; i < 50; i++)
                {
                    string username = "testUser" + i; // Generate username
                    ClientWebSocket client = new ClientWebSocket();
                    await client.ConnectAsync(serverUri, CancellationToken.None);
                    Console.WriteLine($"Client {i + 1} connected to the server");

                    clients.Add(client);
                    clientUsernames.Add(client, username); // Store the username for this client

                    // Send login message with the same username used for connecting
                    await SendLogMessage(client, username, "REQ", "login");

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

                    await SendLogMessage(client, username, "INFO", "This is a test log");
                    Console.WriteLine($"Test log message sent for client with username: {username}");

                    if (await SendLogMessage(client, username, "INFO", "This is a test log"))
                    {
                        receivedCounts++;
                    }

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
                return null;
            }

            if (receivedCounts == clients.Count)
            {
                Console.WriteLine($"------Stress Test Success------");
            }
            else
            {
                Console.WriteLine("------Stress Test Failed------");
            }
            Console.WriteLine();
            return clients;
        }

        /*
         * Function: StressTest()
         * Parameters: Uri serverUri: server url
         * Desccription: This function tests the logging system for abuse testing automatically
         * Return values: void
         */
        public async Task AbuseTest(Uri serverUri)
        {
            Console.WriteLine("------Abuse Test 1 started: Anonymous user trying login repeatedly------");

            // required variables
            string user = "AutoAbuser";
            List<ClientWebSocket> clients = new List<ClientWebSocket>();

            for (int i = 0; i < 10; i++)
            {
                ClientWebSocket client = new ClientWebSocket();
                await client.ConnectAsync(serverUri, CancellationToken.None);
                Console.WriteLine($"Client connected to the server");

                clients.Add(client);

                // Send login message with the same username used for connecting
                await SendLogMessage(client, user, "REQ", "login");
            }
            Console.WriteLine($"------{user} has been blocked------\n");

            // Abuse Test 2
            Console.WriteLine("------Abuse Test 2 started: Anonymous user sending wrong logs repeatedly------");

            ClientWebSocket anonymousClient = new ClientWebSocket();
            await anonymousClient.ConnectAsync(serverUri, CancellationToken.None);

            user = "AutoAbuser=2";
            await SendLogMessage(anonymousClient, user, "REQ", "login");

            for (int i = 0; i < 10; i++)
            {
                // sending log 
                await SendLogMessage(anonymousClient, user, "INVALID", "This is a noisy abuser");
            }
        }

    }
}
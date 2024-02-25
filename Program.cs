/*
 * Project: Assignment 3
 * File: Program.cs
 * Author: Darsh Patel(8870657) and Bhumitkumar Patel(8847159)
 * Description: This file contains the main for the test client from where the auto or manual test starts
 * Date: 2024-02-24
 */
using Client;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;



namespace Client
{
    public class Program
    {
        /*
         * Function: parseArguments()
         * Parameters: string[] args: command line arguments
         *             ref string serverAddress: server IP address which needs to be extracted from command line arguments
         *             ref int portNo: port number of server which needs to be extracted from command line arguments
         *             ref bool isAutoMode: true if auto mode
         *             ref bool isManualMode: true if manual mode
         * Description: This function parse the given command line arguments into specific variables and flags.
         * Return values: bool: true if single flag on or false
         */
        private static bool parseArguments(string[] args, ref string serverAddress, ref int portNo, ref bool isAutoMode, ref bool isManualMode)
        {
            // Check the number of command line arguments
            if (args.Length < 2)
            {
                return false;
            }
            // extract server address and port number
            serverAddress = args[0];
            if (!int.TryParse(args[1], out portNo))
            {
                return false;
            }

            // Check for optional parameters
            for (int i = 2; i < args.Length; i++)
            {
                if (args[i] == "-auto")
                {
                    isAutoMode = true;
                }
                else if (args[i] == "-manual")
                {
                    isManualMode = true;
                }
                else
                {
                    return false;
                }
            }

            // Ensure only one mode is specified
            if (isAutoMode == isManualMode)
            {
                return false;
            }

            return true;
        }

        /*
         * Function: initiateOperation()
         * Parameters: Uri serverUri: server URL
         *             bool manual: manual flag
         * Description: This function initiates the manual/auto operation based on the flag
         * Return values: void
         */
        private static async Task initiateOperation(Uri serverUri, bool manual)
        {
            ClientWebSocket client = null;
            // running auto or manual based on the flags
            if (manual)
            {
                ManualOperations manualOp = new ManualOperations();

                //Login
                client = await manualOp.Login(serverUri, manual);
                if (client == null)
                {
                    return;
                }

                // Sending manual logs
                await manualOp.SendLog(client);

                // Checking multiple client login
                await manualOp.MultipleClient(serverUri);

                // checking stress testing by sending multiple logs from different clients at the same time
                await manualOp.StressTest(serverUri);

                // checking abuse test
                await manualOp.AbuseTest(serverUri);

            }
            else
            {
                AutoOperation autoOperation = new AutoOperation();
                client = await autoOperation.Login(serverUri);
                if (client == null)
                {
                    return;
                }
                await Task.Delay(2000);

                // Sending auto logs
                await autoOperation.SendLog(client);
                await Task.Delay(2000);

                // Checking multiple client login
                List<string> usernames = new List<string> { "user1", "user2", "user3" };
                await Task.Delay(2000);

                // test the stress mode
                await autoOperation.StressTest(serverUri);
                await Task.Delay(2000);

                // checking abuse test
                await autoOperation.AbuseTest(serverUri);

            }

        }

        static async Task Main(string[] args)
        {
            // variables
            string serverAddress = "";
            int portNo = 0;
            bool auto = false;
            bool manual = false;

            // parsed command line arguments
            if (!parseArguments(args, ref serverAddress, ref portNo, ref auto, ref manual))
            {
                Console.WriteLine("Usage: Program.exe <IP Address> <Port No.> <-auto | -manual>");
                Console.WriteLine("<IP Address>: Server's IP Address");
                Console.WriteLine("<Port No.>: Server's Port Number");
                Console.WriteLine("<-auto | -manual>: Flags for test operation (only one at a time)");
                return;
            }
            // created a url to connect to server
            Uri serverUri = new Uri($"ws://{serverAddress}:{portNo}");

            await initiateOperation(serverUri, manual);
        }
    }
}

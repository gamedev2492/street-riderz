﻿using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

using Aws.GameLift.Server;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.CognitoIdentity;
using Amazon;

namespace Supragma
{

    [Serializable]
    public class SocketObj
    {
        public string ipAddress;
        public string port;
    }

    [Serializable]
    public class SessionConnection
    {
        public SocketObj sessionConnectionInfo;
    }

    //This script will handle Sessions and connecting players by matchmaking
    public class SessionManager : MonoBehaviour
    {
        public RaceManager raceManager;

        private bool isHeadlessServer = false;
        private bool isGameliftServer = false;
        private System.Timers.Timer timer = new System.Timers.Timer(120000);

        private static int ListenPort = 7777;

        private static int MaxPlayers = 80;

        void Start()
        {
            // tells client/server we want this many players

            //todo set max players
            //maxConnections = MaxPlayers;

            // detect headless server mode
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Debug.Log("Server");
                isHeadlessServer = true;
                SetupServerAndGamelift();
            }
            else
            {
                SetupClient();
            }
        }

        void Update()
        {
        }

        private void OnApplicationQuit()
        {
            if (isHeadlessServer)
            {
                TerminateSession();
            }
        }

        //todo
        // This should called on the server when a client disconnects
        public void OnServerDisconnect()
        {
            // remove all players
            //gameManager.RemovePlayer();
            CheckPlayers();
        }

        private void CheckPlayers()
        {
            // if no players are playing the game now terminate the server process
            int numPlayers = 0; //todo total players
            if (numPlayers <= 0 && isHeadlessServer)
            {
                TerminateSession();
            }
        }

        // when gameover or time out
        public void TerminateSession()
        {
            Debug.Log("** TerminateSession Requested **");
            if (isGameliftServer)
            {
                GameLiftServerAPI.TerminateGameSession();
                GameLiftServerAPI.ProcessEnding();
            }
            Debug.Log("** Process Exit **");
            Application.Quit();
        }

        private void SetupClient()
        {
            // in debug mode don't attempt to match with GameLift
            if (PlayerPrefs.GetInt("ShowUnityHUD", 1) == 0)
            {
                // TODO update UI 
                FindMatch();
            }
        }

        async void FindMatch()
        {
            Debug.Log("Reaching out to client service Lambda function");

            AWSConfigs.AWSRegion = "us-east-1"; // Your region here

            // paste this in from the Amazon Cognito Identity Pool console
            CognitoAWSCredentials credentials = new CognitoAWSCredentials(
                "us-east-1:a70f5010-a4c4-45a7-ba01-8e3d0cc08a9d", // Your identity pool ID here
                RegionEndpoint.USEast1 // Your region here
            );

            //todo json change later
            string matchParams = "{\"latencyMap\":{\"us-east-1\":60}, \"playerSkill\":10}";

            AmazonLambdaClient client = new AmazonLambdaClient(credentials, RegionEndpoint.USEast1);
            InvokeRequest request = new InvokeRequest
            {
                FunctionName = "ConnectStreetRiderzClient",
                InvocationType = InvocationType.RequestResponse,
                Payload = matchParams
            };

            //todo show UI
            //SetStatusText("Finding match, please wait");

            InvokeResponse response;
            response = await client.InvokeAsync(request).ConfigureAwait(false);

            if (response != null)
            {
                if (response.StatusCode == 200)
                {
                    var payload = Encoding.ASCII.GetString(response.Payload.ToArray()) + "\n";
                    var connectionObj = JsonUtility.FromJson<SessionConnection>(payload);

                    if (connectionObj.sessionConnectionInfo.port == null)
                    {
                        Debug.Log($"Error in Lambda assume matchmaking failed: {payload}");

                        //todo show UI
                        //SetStatusText("Matchmaking failed");
                    }
                    else
                    {
                        //todo Hide UI
                        //HideStatusText();

                        Debug.Log($"Connecting! IP Address: {connectionObj.sessionConnectionInfo.ipAddress} Port: {connectionObj.sessionConnectionInfo.port}");

                        //todo Set IP and Port for client and start it
                        //networkAddress = connectionObj.GameSessionConnectionInfo.IpAddress;
                        //networkPort = Int32.Parse(connectionObj.GameSessionConnectionInfo.Port);
                        //StartClient();
                    }
                }
            }
            else
            {
                Debug.LogError(response.FunctionError);

                //todo show UI
                //SetStatusText($"Client service failed: {response.FunctionError}");
            }
        }

        private void CheckForPlayers(System.Object source, System.Timers.ElapsedEventArgs e)
        {
            CheckPlayers();
        }

        private void SetupServerAndGamelift()
        {
            // start the server

            //todo 
            //networkPort = ListenPort;
            //StartServer();
            //Debug.Log($"Server listening on port {networkPort}");

            // initialize GameLift
            print("Starting GameLift initialization.");
            var initSDKOutcome = GameLiftServerAPI.InitSDK();
            if (initSDKOutcome.Success)
            {
                isGameliftServer = true;
                var processParams = new ProcessParameters(
                    (gameSession) =>
                    {
                    // onStartGameSession callback
                    GameLiftServerAPI.ActivateGameSession();
                    // quit if no player joined within two minutes
                    timer.Elapsed += CheckForPlayers;
                        timer.AutoReset = false;
                        timer.Start();
                    },
                    (updateGameSession) =>
                    {

                    },
                    () =>
                    {
                    // onProcessTerminate callback
                    TerminateSession();
                    },
                    () =>
                    {
                    // healthCheck callback
                    return true;
                    },
                    ListenPort,
                    new LogParameters(new List<string>()
                    {
                    "/local/game/logs/myserver.log"
                    })
                );
                var processReadyOutcome = GameLiftServerAPI.ProcessReady(processParams);
                if (processReadyOutcome.Success)
                {
                    print("GameLift process ready.");
                }
                else
                {
                    print($"GameLift: Process ready failure - {processReadyOutcome.Error}.");
                }
            }
            else
            {
                print($"GameLift: InitSDK failure - {initSDKOutcome.Error}.");
            }
        }
    }
}
// WebSocketServerRequest.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Bootstrap;
using Features.Lobby.Integration;
using UnityEngine;

namespace Teenpatti
{
    public class WebSocketServerRequest : MonoBehaviour
    {
        public static WebSocketServerRequest Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }



        [System.Serializable]
        public class JoinApiResponse
        {
            public string session_id;
            public string user_id;
            public string table_id;
            public string status; // "ACTIVE", "RECONNECTABLE", "SETTLING", "NONE"
            public string table_status;
            public string ws_url;
            public string ws_token;
            public bool was_reconnect;
            public string reservation_id;
            public int reconnect_window_seconds;
        }

        [System.Serializable]
        public class RejoinApiResponse
        {
            public string status; // "ACTIVE", "RECONNECTABLE", "SETTLING", "NONE"
            public string ws_url;
            public string ws_token;
            public string tableID;
        }



        public async Task CreatePrivateRoom(int bootAmount)
        {
            ProfileImage profileImage = new ProfileImage();
            profileImage.index = UserDetail.profileImageIndex;
            profileImage.url = "";
            var msg = new WSMessage<JoinRequest>
            {
                type = "createPrivateRoom",
                data = new JoinRequest
                {
                    userID = BootstrapLobbyAdapter.GetUserId(),
                    username = UserDetail.UserName,
                    bootAmount = bootAmount,
                    profileImage = profileImage,
                    chips = (int)BootstrapService.Instance.Wallet.deposit_balance / 100f + (int)BootstrapService.Instance.Wallet.win_balance / 100f,
                    isPrivateCreate = true,
                }
            };
            string json = JsonUtility.ToJson(msg);
            await WebSocketClient.Instance.Send(json);
        }

        public async Task JoinPrivateRoom(string privateCode)
        {
            ProfileImage profileImage = new ProfileImage();
            profileImage.index = UserDetail.profileImageIndex;
            profileImage.url = "";
            var msg = new WSMessage<JoinRequest>
            {
                type = "joinPrivateRoom",
                data = new JoinRequest
                {
                    userID = BootstrapLobbyAdapter.GetUserId(),
                    username = UserDetail.UserName,
                    profileImage = profileImage,
                    privateCode = privateCode,
                    chips = (int)BootstrapService.Instance.Wallet.deposit_balance / 100f + (int)BootstrapService.Instance.Wallet.win_balance / 100f,
                    isPrivateJoin = true,
                }
            };
            string json = JsonUtility.ToJson(msg);
            await WebSocketClient.Instance.Send(json);
        }

        public async Task JoinGame(int bootAmount, string privateCode = "")
        {
            ProfileImage profileImage = new ProfileImage();
            profileImage.index = UserDetail.profileImageIndex;
            profileImage.url = "";
            var msg = new WSMessage<JoinRequest>
            {
                type = "join",
                data = new JoinRequest
                {
                    userID = BootstrapLobbyAdapter.GetUserId(),
                    username = UserDetail.UserName,
                    bootAmount = bootAmount,
                    profileImage = profileImage,
                    privateCode = privateCode,
                    chips = privateCode == "" ? (int)BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f : (int)BootstrapService.Instance.Wallet.deposit_balance / 100f + (int)BootstrapService.Instance.Wallet.win_balance / 100f,
                }
            };
            Debug.Log("chips to play with: " + msg.data.chips);
            Debug.Log("is private code null or empty: " + string.IsNullOrEmpty(privateCode));
            Debug.Log("Wallet info: " + BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f + " " + BootstrapService.Instance.Wallet.deposit_balance / 100f);
            string json = JsonUtility.ToJson(msg);
            await WebSocketClient.Instance.Send(json);
        }


        // public async Task RejoinGame()
        // {
        //     var msg = new WSMessage<RejoinRequest>
        //     {
        //         type = "rejoin",
        //         data = new RejoinRequest
        //         {
        //             userID = BootstrapLobbyAdapter.GetUserId()
        //         }
        //     };

        //     string json = JsonUtility.ToJson(msg);
        //     if (WebSocketClient.Instance != null)
        //     {
        //         await WebSocketClient.Instance.Send(json);
        //     }
        // }


        public async Task RejoinGame(string tableID)
        {
            var msg = new WSMessage<RejoinRequest>
            {
                type = "join",
                data = new RejoinRequest
                {
                    userID = BootstrapLobbyAdapter.GetUserId(),
                    tableID = tableID
                }
            };

            string json = JsonUtility.ToJson(msg);
            await WebSocketClient.Instance.Send(json);
        }

        public async Task SendAction(string action, int amount = 0, string targetID = null, bool accept = false)
        {
            var msg = new WSMessage<ActionRequest>
            {
                type = "action",
                data = new ActionRequest
                {
                    tableID = GameLiveData.instance.tableId,
                    userID = BootstrapLobbyAdapter.GetUserId(),
                    action = action,
                    amount = amount,
                    targetID = targetID,
                    accept = accept
                }
            };


            string json = JsonUtility.ToJson(msg);
            if (WebSocketClient.Instance != null)
            {
                await WebSocketClient.Instance.Send(json);
            }
        }

        public async Task StartPrivateTableGame()
        {
            Debug.Log("starting private table game");
            var msg = new WSMessage<ActionRequest>
            {
                type = "startPrivateTable",
                data = new ActionRequest
                {
                    userID = BootstrapLobbyAdapter.GetUserId(),
                    tableID = GameLiveData.instance.tableId,
                }
            };


            string json = JsonUtility.ToJson(msg);
            if (WebSocketClient.Instance != null)
            {
                await WebSocketClient.Instance.Send(json);
            }
        }

        public async Task LeaveTable()
        {
            var request = new WSMessage<LeaveRequest>
            {
                type = "leave",
                data = new LeaveRequest
                {
                    userID = BootstrapLobbyAdapter.GetUserId(),
                    tableID = GameLiveData.instance.tableId
                }
            };

            string json = JsonUtility.ToJson(request);
            if (WebSocketClient.Instance != null)
            {
                await WebSocketClient.Instance.Send(json);
            }
        }


        public async Task SendPing()
        {
            var request = new WSMessage<Dictionary<string, object>>
            {
                type = "ping",
                data = new Dictionary<string, object>
                {
                    { "type", "ping" }
                }
            };

            string json = JsonUtility.ToJson(request);
            if (WebSocketClient.Instance != null)
            {
                await WebSocketClient.Instance.Send(json);
            }
        }



        [System.Serializable]
        public class WSMessage<T>
        {
            public string type;   // "join"
            public T data;
        }


        [System.Serializable]
        public class JoinRequest
        {
            public string userID;
            public string username;
            public int bootAmount;
            public string privateCode;
            public float chips;
            public bool isPrivateJoin;
            public bool isPrivateCreate;
            public ProfileImage profileImage;
        }


        [System.Serializable]
        public class RejoinRequest
        {
            public string userID;
            internal string tableID;
        }

        [System.Serializable]
        public class LeaveRequest
        {
            public string userID;
            public string tableID;
        }

        [System.Serializable]
        private class Wrapper
        {
            public Dictionary<string, object> data;
        }

        [System.Serializable]
        public class ActionRequest
        {
            public string tableID;
            public string userID;
            public string action;
            public int amount;
            public string targetID;
            public bool accept;
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.IO;
using GameServer.SharedData;
using GameServer.Network;

namespace GameServer
{
    public class DemoTcpClient : IGameServer
    {
        Action<GameMessage> _msgHander;

        INetworkClient client;

        void Connect(string endpoint){
            client = NetworkFactory.CreateNetworkClient<TcpNetworkClient> ();
            //client = NetworkFactory.CreateNetworkClient<RoomClientBridge> ();
            client.MessageHandler += (msg) => {
                RecieveMessage (msg);
            };
			client.Connect (endpoint);
        }

        void RecieveMessage(GameMessage msg){
            try{
                if (_msgHander != null) {
                    _msgHander (msg);
                }
            }catch(Exception e){
                Debug.LogError (e.ToString ());
            }
        }

        void Send(GameMessage msg){
            if(client.Connected){
                client.Send (msg);
            }
        }


        //新玩家, msgHandler待集成的时候适配
        public int NewPlayer(string id, float posX, float posY,int function, Action<GameMessage> msgHandler)
        {
            _msgHander = msgHandler;
            Connect (GameManager.ServerEndpoint);
            Send(new ClientJoinMessage() {
                id = id,
                posX = posX,
                posY = posY,
            });
            return 0;
        }

        public int Function(string id, int function)
        {
            Send (new FunctionMessage(){
                function = function,
            });  
            return 0;
        }
        

        //移动
        public int OnMoveTowards (string id, float dx, float dy){
            Send (new ClientMoveMessage(){
                dx = dx,
                dy = dy
            });  
            return 0;
        }

        public int OnBoxsPos(List<GameBox> boxs)
        {
            Send (new BoxMessage(){
                boxs = boxs,
            });  
            return 0;
        }
        //退出
        public int Quit(string id){
            Send (new GameMessage (){ 
                stateCode = MsgCode.CLIENT_QUIT
            });
            client.Close();
            return 0;
        }

        public int LaunchDir(float angle)
        {
            Send (new LaunchDirMessage() { 
                angle = angle,
            });
            
            return 0;
        }

        public int PlaySkill(string id, int skillId)
        {
            Send(new PlayerSkillMessage()
            {
                playerId = id,
                skillId = skillId,
            });
            return 0;
        }

        //获得人物数量
        public int GetPlayersCount(){
            return 0;
        }
    }
}


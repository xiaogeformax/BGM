using System;
using System.Collections.Generic;
using ProtoBuf;
using GameServer.SharedData;

namespace GameServer
{
    public interface IGameServer
    {
        //新玩家, msgHandler待集成的时候适配
        int NewPlayer(string id,float posX,float posY,int function, Action<GameMessage> msgHandler);

        //移动
        int OnMoveTowards (string id, float dx, float dy);

        int OnBoxsPos(List<GameBox> boxs);
        
        int  LaunchDir(float angle);
        
        int Function(string id,int function);
        //退出
        int Quit(string id);

        int PlaySkill(string id, int skillId);
        //开枪 
        //int Fire(string id);

        //获得子弹数量
        // int GetBulletsCount ();

        //获得人物数量
        int GetPlayersCount();
    }     
}


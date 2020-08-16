using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using GameServer;
using UnityEngine.SceneManagement;
using GameServer.SharedData;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {

    public static string MyId = ConstSettings.DefaultName;
    public static string ServerEndpoint = ConstSettings.DefaultServerEndPoint;

    //public Gamer zhujue;
    public MarioCharacterController zhujue;
    public MarioCharacterController gamerPrefab;


    public GameObject box;
    //play start pos
    public static float posX;
    public static float posY;

    
    

    public static bool isConnect;
   

    public IGameServer server;

    Dictionary<string, MarioCharacterController> _gamers = new Dictionary<string, MarioCharacterController>();
    Dictionary<string, NPCSimulator> _npcClients = new Dictionary<string, NPCSimulator>();

    public  List<GameBox> boxs = new List<GameBox>();

    public  Dictionary<int,GameObject> boxsDic = new Dictionary<int, GameObject>();
    public RoleType roleType = RoleType.none;


    public GameObject arrow;

    public GameObject startPanel;
    public enum RoleType
    {
        none = 0,
        launch =1,
        move=2,
    }
 

    [HideInInspector]
    public int TimeLeft;

    private static GameManager _instance = null;
    public static GameManager Instance
    {
        get { return _instance; }
    }

    void Awake()
    {
        _instance = this;
        
        server = new DemoTcpClient();
        MyId += Random.Range(1, 20000).ToString();
    }

    public void Init()
    {
        zhujue.client.Function(MyId,(int) roleType);
    }

	// Use this for initialization
	void Start () {

        //是否关闭调试面板
        if (ConstSettings.DisableConsolePanelInRuntime) {
            GameObject.Find ("debugpanel").SetActive (Application.isEditor);
        }

        zhujue.client = new RoomClientSimulator (server, MyId);
        zhujue.client.Connect (posX, posY,(int)roleType);

     

        //for test
        //SpawnClientTest(10);
	}

    public void SpawnClientTest(int count)
    {
        for (int i = 0; i < count; ++i)
        {
            var testId = "NPCPlayer:" + System.Guid.NewGuid().ToString().Substring(0, 8);
            var newclient = new NPCSimulator(new DemoTcpClient(), testId, this);
            newclient.Connect(posX, posY, (int) roleType);

            _npcClients.Add(testId, newclient);
        }
    }

    public void KillClientTest(int count){

        List<string> killList = new List<string> ();

        foreach (var kv in _gamers) {
            if (kv.Value != zhujue) {
                var find = kv.Key;
                killList.Add (find);;
                if (killList.Count >= count)
                    break;
            }
        }

        foreach (var kill in killList) {
            if (_npcClients.ContainsKey (kill)) {
                var client = _npcClients [kill];
                if (client != null) {
                    client.Quit ();
                    _npcClients.Remove (kill);
                }
            }
        }
    }

    void AddPlayer(GamePlayer user){

        //不重复加s
        if (_gamers.ContainsKey(user.id))
            return;

        //自己
        if (user.id == MyId)
        {
            _gamers.Add(user.id, zhujue);
            
            foreach (var kv in _gamers)
            {
                if (kv.Value != zhujue) 
                {
                    if (user.function == ((int) RoleType.launch))
                    {
                        zhujue.transform.SetParent( kv.Value.gameObject.transform);
                        zhujue.transform.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                        zhujue.transform.localPosition = new Vector3(0.05f,0.1f,0);
                        zhujue.transform.gameObject.name = "launch";
                        kv.Value.gameObject.name = "move";
                    }
                    else if (user.function == ((int) RoleType.move))
                    {
                        kv.Value.gameObject.transform.SetParent(zhujue.transform);
                        kv.Value.gameObject.transform.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                        kv.Value.gameObject.transform.localPosition = new Vector3(0.05f,0.1f,0);
                        zhujue.transform.gameObject.name = "move";
                        kv.Value.gameObject.name = "launch";
                    }
                }
            }
        }
        else
        {
            //其他人加入
            var newGamerObj = Instantiate(gamerPrefab.gameObject);


            // newGamerObj.transform.parent = root.transform;
            var newGamer = newGamerObj.GetComponent<MarioCharacterController>();

            newGamer.SetPosition(user.x, user.y);

            _gamers.Add(user.id, newGamer);
            
            foreach (var kv in _gamers)
            {
                if (kv.Value != zhujue) 
                {
                    if (user.function == ((int) RoleType.move))
                    {
                        zhujue.transform.SetParent( kv.Value.gameObject.transform);
                        zhujue.transform.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                        zhujue.transform.localPosition = new Vector3(0.05f,0.1f,0);
                        
                        zhujue.transform.gameObject.name = "launch";
                        kv.Value.gameObject.name = "move";
                    
                    }
                    else if (user.function == ((int) RoleType.launch))
                    {
                        kv.Value.gameObject.transform.SetParent(zhujue.transform);
                        kv.Value.gameObject.transform.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                        kv.Value.gameObject.transform.localPosition = new Vector3(0.05f,0.1f,0);
                        
                        zhujue.transform.gameObject.name = "move";
                        kv.Value.gameObject.name = "launch";
                    }
                }
            }
        }

       
        
        var startPanelCom = startPanel.GetComponent<StartPanel>();
        if (startPanelCom != null && user.function == ((int) RoleType.launch))
        {
            startPanelCom.SetLaunchPlayerBlack();
        }

        if (startPanelCom != null && user.function == ((int) RoleType.move))
        {
            startPanelCom.SetMoveBlack();
        }

        
        
       
    }
    Dictionary<string,Color> __colorMap = new Dictionary<string, Color>();
    Color GetColor(GamePlayer p){
        if (!__colorMap.ContainsKey (p.id)) {
            var color = new Color (p.r, p.g, p.b);
            __colorMap.Add (p.id, color);
        }

        return __colorMap [p.id];
    }

    void FixedUpdate(){
        //处理消息队列
        lock (zhujue.client.msgQueue) {
            while (zhujue.client.msgQueue.Count > 0) {
                var msg = zhujue.client.msgQueue.Dequeue ();
                OnRecieveMsg (msg);
            }
        }
         
        
    }

    //收到服务器的消息
    void OnRecieveMsg(GameMessage basemsg)
    {
        if (basemsg == null)
            return;


        //TODO
        int code = basemsg.stateCode;

        if (code == MsgCode.USER_JOIN)
        {
            var msg = (GamePlayerMessage)basemsg;
            var user = msg.player;
            Debug.Log("on user join, id=" + user.id);
            AddInfo(string.Format("{0} joined game", user.id));
            AddPlayer(user);
            RefreshPlayerText();
            var curId = user.id;
            if (_gamers.ContainsKey(curId))
            {
                var gamer = _gamers[curId];
                gamer.SetPosition(user.x, user.y);
            }

            var startPanelCom = startPanel.GetComponent<StartPanel>();
            if (startPanelCom != null && user.function == ((int) RoleType.launch))
            {
                startPanelCom.SetLaunchPlayerBlack();
            }

            if (startPanelCom != null && user.function == ((int) RoleType.move))
            {
                startPanelCom.SetMoveBlack();
            }

            if (user.function == ((int) RoleType.launch))
            {
                if (_gamers.ContainsKey(curId))
                {
                    _gamers[curId].gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                }
            }
        }
        else if (code == MsgCode.USER_STATE_UPDATE)
        {
            var msg = (GamePlayerMessage)basemsg;
            var user = msg.player;
            if (user == null)
                return;

            var curId = user.id;
            if (_gamers.ContainsKey(curId))
            {
                var gamer = _gamers[curId];
                gamer.SetPosition(user.x, user.y);
                //gamer.SetRotation(user.dx, user.dy);
            }

        }
        else if (code == MsgCode.USER_QUIT)
        {
            var msg = (GamePlayerMessage)basemsg;
            var user = msg.player;

            var curId = user.id;
            Debug.Log("on player quit, id=" + user.id);
            AddInfo(string.Format("{0} quit game", user.id));
            if (_gamers.ContainsKey(curId))
            {
                var gamer = _gamers[curId];
                Destroy(gamer.gameObject);
                _gamers.Remove(curId);
                RefreshPlayerText();
            }
        }
        else if (code == MsgCode.MOVE_AND_FIRE)
        {
            var msg = (MoveAndFireMessage)basemsg;
            var user = msg.player;
            var b = msg.bullet;

            var curId = user.id;
            if (_gamers.ContainsKey(curId))
            {
                //角色移动
                var gamer = _gamers[curId];
                gamer.SetPosition(user.x, user.y);
                
            }
        }
        else if (code == MsgCode.HIT)
        {
            //var msg = (HitMessage)basemsg;
            //var target = msg.target;
            //var bullet = msg.bullet;

            //if (_gamers.ContainsKey (target.id)) {
            //    var gamer = _gamers [target.id];
            //    gamer.OnHit ();
            //    gamer.SetHp (target.hp);
            //    var bulletId = bullet.id;
            //    if (_bullets.ContainsKey (bulletId)) {
            //        var b = _bullets [bulletId];
            //        b.Kill ();
            //    }
            //}
        }
        else if (code == MsgCode.DIE)
        {
            var msg = (DieMessage)basemsg;
            var user = msg.player;

            SyncKillAndDead(msg.player);
            SyncKillAndDead(msg.killer);

            AddInfo(string.Format("{0} has been killed by {1}", user.id, msg.bullet.ownerId));

            if (_gamers.ContainsKey(user.id))
            {

                var gamer = _gamers[user.id];

                //同步位置，重新出生
                gamer.SetPosition(user.x, user.y);
                //gamer.SetRotation(user.dx, user.dy);
                // gamer.SetHp (user.hp);
            }

            RefreshPlayerText();

        }
        else if (code == MsgCode.SYNC)
        {

            Debug.Log("on sync command");
            //全场同步
            var msg = (SyncMessage)basemsg;
            if (msg.players != null)
            {
                foreach (var p in msg.players)
                {
                    AddPlayer(p);
                }
            }
            //// 注释掉子弹
            //if (msg.bullets != null) {
            //    foreach (var b in msg.bullets) {
            //        AddBullet (b);
            //    }
            //}
            TimeLeft = msg.timeleft;
            RefreshPlayerText();
            UpdateTimeText();
        }
        else if (code == MsgCode.SYNC_TIME)
        {
            var msg = (TimeMessage)basemsg;
            TimeLeft = msg.timeleft;
            UpdateTimeText();
        }
        else if (code == MsgCode.NEW_ROUND)
        {
            AddInfo("new round start..");
            //清空所有信息
            foreach (var p in _gamers)
            {
                if (p.Value != zhujue)
                {
                    Destroy(p.Value.gameObject);
                }
            }
            _gamers.Clear();
            
            //zhujue.killed = 0;
            //zhujue.dead = 0;
        }
        else if (code == MsgCode.PLAY_SKILL)
        {
            var msg = (PlayerSkillMessage)basemsg;
            if (_gamers.ContainsKey(msg.playerId))
            {
                var player = _gamers[msg.playerId];
                
            }
        }
        else if (code == MsgCode.BOX_POS)
        {
            if (roleType == RoleType.move)
            {
                var msg = (BoxMessage) basemsg;
                for (int i = 0; i < msg.boxs.Count; i++)
                {
                    var curBox = msg.boxs[i];
                    GameObject box1;
                   if( GameManager.Instance.boxsDic.TryGetValue(curBox.id, out box1))
                    {
                        box1.transform.position = new Vector3(curBox.x, curBox.y, 0);
                    }
                   else
                   {
                       var curBox1 = Instantiate(box) as GameObject;
                       curBox1.transform.position = new Vector3(curBox.x,curBox.y,0); 
                       
                       GameManager.Instance.boxs.Add(curBox);
                       GameManager.Instance.boxsDic.Add(curBox.id, curBox1);
                   }
                  
                }
            }
            
           
            
            
        }
        else if (code == MsgCode.LAUNCH_DIR)
        {
            if (roleType == RoleType.move)
            {
                var msg = (LaunchDirMessage) basemsg;
                GameManager.Instance.arrow.transform.rotation = Quaternion.Euler (0f, 0f, msg.angle);
            }
            
           
        } else if (code == MsgCode.FUNCTION)
        {
            var msg = (FunctionMessage) basemsg;
            var startPanelCom = startPanel.GetComponent<StartPanel>();
            if (startPanelCom != null && msg.function == ((int) RoleType.launch))
            {
                startPanelCom.SetLaunchPlayerBlack();
            }

            if (startPanelCom != null && msg.function == ((int) RoleType.move))
            {
                startPanelCom.SetMoveBlack();
            }

            if (MyId == msg.id)
            {
                foreach (var kv in _gamers)
                {
                    if (kv.Value != zhujue) 
                    {
                        if (msg.function == ((int) RoleType.launch))
                        {
                            zhujue.transform.SetParent( kv.Value.gameObject.transform);
                            //zhujue.transform.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                            zhujue.transform.localPosition = new Vector3(0.05f,0.1f,0);
                            zhujue.transform.gameObject.name = "launch";
                            kv.Value.gameObject.name = "move";
                        }
                        else if (msg.function == ((int) RoleType.move))
                        {
                            kv.Value.gameObject.transform.SetParent(zhujue.transform);
                            //kv.Value.gameObject.transform.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                            kv.Value.gameObject.transform.localPosition = new Vector3(0.05f,0.1f,0);
                            zhujue.transform.gameObject.name = "move";
                            kv.Value.gameObject.name = "launch";
                        }
                    }
                }
            }
            else
            {
                foreach (var kv in _gamers)
                {
                    if (kv.Value != zhujue) 
                    {
                        if (msg.function == ((int) RoleType.move))
                        {
                            zhujue.transform.SetParent( kv.Value.gameObject.transform);
                            //zhujue.transform.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                            zhujue.transform.localPosition = new Vector3(0.05f,0.1f,0);
                        
                            zhujue.transform.gameObject.name = "launch";
                            kv.Value.gameObject.name = "move";
                    
                        }
                        else if (msg.function == ((int) RoleType.launch))
                        {
                            kv.Value.gameObject.transform.SetParent(zhujue.transform);
                           // kv.Value.gameObject.transform.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                            kv.Value.gameObject.transform.localPosition = new Vector3(0.05f,0.1f,0);
                        
                            zhujue.transform.gameObject.name = "move";
                            kv.Value.gameObject.name = "launch";
                        }
                    }
                }
            }
           
        }
    }

    
    void SyncKillAndDead(GamePlayer player){
        if (_gamers.ContainsKey (player.id)) {
            var p = _gamers [player.id];
            //p.killed = player.killed;
            //p.dead = player.dead;
        }
    }


    Vector3 tempTransPos = new Vector3(0, 0, 0);
 
    public void Quit(){
        zhujue.client.Quit ();
        Application.Quit ();
    }

    public void PlaySkill(int skillId)
    {
        zhujue.client.PlaySkill(skillId);
    }
        
    
    public void LaunchDir(float angle)
    {
        zhujue.client.LaunchDir( angle);
    }


    void OnApplicationQuit(){
        zhujue.client.Quit ();
    }

    void RefreshPlayerText(){
        string txt = "";
        foreach (var p in _gamers) {
            //txt += string.Format("{0} KILL:<color=yellow>{1}</color> DEAD:<color=red>{2}</color>" , p.Key, p.Value.killed, p.Value.dead) + "\n";
        }

       
    }


    public void AddInfo(string info){
        
    }

    void UpdateTimeText(){
        
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using GameServer;
using GameServer.SharedData;
using UnityEngine;

public class MarioCharacterController : MonoBehaviour {

    public float motionSpeed;
    public float jumpForce;

    bool isMoving;


    public GameObject box;
    
    
    public GameObject curBox;

    private bool isBoxMoving;
    
    public RoomClientSimulator client;

    public bool isMainRole;
    
    public Vector2 launchDir = Vector2.right;

    private float input_V;
    private float input_H;
    public float angle =0; 
    
    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

        if (isMainRole == false)
        {
            return ;
        }
        isMoving = false;

        // Horizontal Motion

        if (GameManager.Instance.roleType == GameManager.RoleType.launch)
        {
            if (Input.GetKey(KeyCode.RightArrow)||Input.GetKey(KeyCode.LeftArrow))
            {
                if (Input.GetKey(KeyCode.RightArrow))
                {
                    angle -= 1f;
                }

                 if (Input.GetKey(KeyCode.LeftArrow))
                 {
                     angle += 1f;
                 }
                
                 if (angle >180)
                     angle = angle-360;
                 
                 if(angle<-180)
                     angle = 360 + angle;
                 //Vector3 targetDir = launchDir ;
                 //float angle = Vector3.Angle( targetDir, transform.forward );
                 GameManager.Instance.arrow.transform.rotation = Quaternion.Euler (0f, 0f, angle);
            }
            
        }

        if (GameManager.Instance.roleType == GameManager.RoleType.move)
        {
            if(Input.GetKey(KeyCode.RightArrow)) {
                isMoving = true;
                this.transform.Translate(Vector3.right * motionSpeed);
                this.GetComponent<SpriteRenderer>().flipX = true;
            }

            if(Input.GetKey(KeyCode.LeftArrow)) {
                isMoving = true;
                this.transform.Translate(Vector3.left * motionSpeed);
                this.GetComponent<SpriteRenderer>().flipX = false;
            }

            this.GetComponent<Animator>().SetBool("MarioIsMoving", isMoving);
            // Jump
            if(Input.GetKeyDown(KeyCode.Space)) {
                this.GetComponent<Rigidbody2D>().AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                this.GetComponent<Animator>().SetBool("MarioIsOnFloor", false);
            }
        }
       
       


        if (GameManager.Instance.roleType == GameManager.RoleType.launch)
        {
            // 抛掷
            if (Input.GetKeyUp(KeyCode.A))
            {
                if (isBoxMoving == false)
                {
                    curBox = Instantiate(box) as GameObject;
                    curBox.transform.localPosition =
                        new Vector3(transform.position.x, transform.position.y, transform.position.y);
                    curBox.GetComponent<Rigidbody2D>().AddForce(new Vector2((float)Math.Sin(angle), (float)Math.Cos(angle)) * 7, ForceMode2D.Impulse);
                    var boxPos = new GameBox();
                    boxPos.id = GameManager.Instance.boxs.Count;
                    boxPos.x = curBox.transform.position.x;
                    boxPos.y = curBox.transform.position.y;
                    GameManager.Instance.boxs.Add(boxPos);
                    GameManager.Instance.boxsDic.Add(  boxPos.id,curBox);
                    isBoxMoving = true;
                }else if(isBoxMoving == true)
                {
                    curBox.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                    isBoxMoving = false;
                }
            }
            
            if (GameManager.Instance.boxs != null )
            {
                for (int i = 0; i <  GameManager.Instance.boxs.Count ; i++)
                {
                    var curBox = GameManager.Instance.boxs[i];
                    GameObject curBoxObj;
                    if (GameManager.Instance.boxsDic.TryGetValue(curBox.id, out curBoxObj))
                    {
                        curBox.x = curBoxObj.transform.position.x;
                        curBox.y = curBoxObj.transform.position.y;

                    }
                }
            }
        }
        
        
        
       

        //client.SetRotation (input.x, input.y);
        //移动指令
        if (client != null)
        {
            if (GameManager.Instance.roleType == GameManager.RoleType.move)
            {
                client.Move(transform.position.x, transform.position.y);
            }

            if (GameManager.Instance.roleType == GameManager.RoleType.launch)
            {
                if (GameManager.Instance.boxs != null&&GameManager.Instance.boxs.Count>0)
                {
                    client.OnBoxsPos(GameManager.Instance.boxs);
                   
                }
                client.LaunchDir(angle);
                
            }
        }

        if (GameManager.Instance.roleType == GameManager.RoleType.move)
        {
            if (transform.childCount > 0)
            {
                var child = transform.GetChild(0);
                //child.transform.localPosition = new Vector3(0.05f, 0.1f, 0);
            }
        }
       
        
        
        //CubeController.Instance.UpdatePosition(transform.position.x,transform.position.y,transform.position.z) ;
    }

    public void SetPosition(float x, float y)
    {
        if(GameManager.Instance.roleType!= GameManager.RoleType.move)
        {
            transform.position = new Vector3(x, y, transform.position.z);
        }
    }
    void OnCollisionEnter2D(Collision2D col) {
        this.GetComponent<Animator>().SetBool("MarioIsOnFloor", true);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Floor")
        {
             Debug.LogError("  Floor        ");
             isBoxMoving = false;
        }
    }
}

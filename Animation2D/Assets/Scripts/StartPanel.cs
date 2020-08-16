using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartPanel : MonoBehaviour
{
	public Button Btn_Launch;
	public Button Btn_Move;
	public Button Btn_Start;

	public Text Txt_Status;
	// Use this for initialization
	void Start () {
		Btn_Launch.onClick.AddListener(LaunchPlayer);	
		Btn_Move.onClick.AddListener(MovePlayer);	
		Btn_Start.onClick.AddListener(StartGame);
	}

	void LaunchPlayer()
	{
		GameManager.Instance.roleType = GameManager.RoleType.launch;
		Txt_Status.text = "Launch";
	}

	void MovePlayer()
	{
		GameManager.Instance.roleType = GameManager.RoleType.move;
		Txt_Status.text = "Move";
	}

	void StartGame()
	{
		GameManager.Instance.Init();
		this.transform.gameObject.SetActive(false);
	}


	public void SetLaunchPlayerBlack()
	{
		Btn_Launch.GetComponent<Button>().interactable =false; 
	}
	
	public void SetMoveBlack()
	{
		Btn_Move.GetComponent<Button>().interactable =false;
	}
	
}

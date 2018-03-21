using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using ParkHopper.Events;


public class RollDiceView : MonoBehaviour {
	[SerializeField]
	private TextMeshProUGUI text;
	[SerializeField]
	private Button button;
	[SerializeField]
	private GameObject panel;

	// Use this for initialization
	void Start () 
	{
		button.onClick.AddListener (onRollButtonClick);	
		EventDispatcher.AddListener<GameStartEvent> (OnGameStart);
		EventDispatcher.AddListener<TurnEndEvent> (OnTurnEnd);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	private void UpdateText(int player)
	{
		text.SetText("Your up Player " + player + "!");
	}
	private void OnTurnEnd(IEvent e)
	{
		TurnEndEvent evt = (TurnEndEvent)e;
		UpdateText (evt.currentPlayer);
		ShowUI();
	}
	private void OnGameStart(IEvent e)
	{
		UpdateText (1);
		ShowUI();
	}
	private void ShowUI()
	{
		panel.SetActive (true);
	}
	private void HideUI ()
	{
		panel.SetActive (false);
	}
	private void onRollButtonClick()
	{
		EventDispatcher.DispatchEvent (new DiceRollBeginEvent());
		HideUI ();
	}
}

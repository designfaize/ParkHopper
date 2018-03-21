using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using ParkHopper.Events;
using System;


public class RollDiceView : MonoBehaviour {
	[SerializeField]
	private TextMeshProUGUI text;
	[SerializeField]
	private Button button;
	[SerializeField]
	private GameObject panel;
	[SerializeField]
	private bool cheatMode = true;
	[SerializeField]
	private InputField cheatInput;

	private int _player;
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
	private void UpdateText()
	{
		text.SetText("Your up Player " + _player.ToString() + "!");
	}
	private void OnTurnEnd(IEvent e)
	{
		TurnEndEvent evt = (TurnEndEvent)e;
		_player = evt.currentPlayer;
		UpdateText ();
		ShowUI();
	}
	private void OnGameStart(IEvent e)
	{
		_player = 1;
		UpdateText ();
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
		if (cheatMode && !string.IsNullOrEmpty(cheatInput.text))
			EventDispatcher.DispatchEvent (new DiceRollCompleteEvent (Int32.Parse(cheatInput.text), _player));
		else 
			EventDispatcher.DispatchEvent (new DiceRollBeginEvent());
		HideUI ();
	}
}

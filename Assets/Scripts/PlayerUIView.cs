using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkHopper.Events;
using TMPro;

public class PlayerUIView : MonoBehaviour {

	[SerializeField]
	private int playerNumber = 1;
	[SerializeField]
	private TextMeshProUGUI fastpasses;
	[SerializeField]
	private TextMeshProUGUI cash;
	// Use this for initialization
	void Start () {
		EventDispatcher.AddListener<PlayerBalanceUpdateEvent> (onBalanceUpdateEvent);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	private void onBalanceUpdateEvent(IEvent e)
	{
		PlayerBalanceUpdateEvent evt = (PlayerBalanceUpdateEvent)e;
		if (playerNumber == evt.playerNumber) 
		{
			fastpasses.SetText( evt.fastPasses.ToString());
			cash.SetText ("$" + evt.cash.ToString());
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkHopper.Events;
public class GameController : MonoBehaviour {
	private Rect rectModeSelect;
	[SerializeField]
	private GameObject spawnPoint;
	[SerializeField]
	private int numberOfDie = 1;
	[SerializeField]
	private int numberOfPlayers = 2;
	[SerializeField]
	private Camera mainCamera;
	private int _numberOfDiesAtRest = 0;
	private int _totalDiceValueThisRoll = 0;
	[SerializeField]
	private Player[] players;
	[SerializeField]
	private int startingCash;
	[SerializeField]
	private int startingFastpasses;
	private Player currentPlayer;

	private PlayerLandedOnTileEvent _currentPayToRideEvent;
	// Use this for initialization
	void Start () {
		ParkHopperEvents.RegisterEvents ();
		Debug.Log ("Reg Events");
		EventDispatcher.AddListener<DieAtRestEvent>(onDiceRollComplete);
		EventDispatcher.AddListener<PlayerLandedOnTileEvent> (onPlayerLandedOnTileEvent);
		EventDispatcher.AddListener<PayToRideEvent> (onPlayerPaidToRide);
		EventDispatcher.AddListener<PayToRideSkipEvent> (onPlayerPaidToRideSkip);
		rectModeSelect =  new Rect(10,10,180,80);
		setupPlayers ();
	}
	private void setupPlayers()
	{
		currentPlayer = players [0];
		foreach (Player player in players) 
		{
			player.addCash (startingCash);
			for (int i = 0; i < startingFastpasses; i++) 
			{
				player.addFastPass ();
			}
		}
	}
	// If the user was presented the option to pay to ride and accepted.
	// put them on the ride.
	private void onPlayerPaidToRide(IEvent e)
	{
		currentPlayer.removeCash (_currentPayToRideEvent.value);
		_currentPayToRideEvent = null;
	}
	// If the user was presented the option to pay to ride and skipped, end the turn;
	private void onPlayerPaidToRideSkip(IEvent e)
	{
		endTurn ();
		_currentPayToRideEvent = null;
	}
	private void onPlayerLandedOnTileEvent(IEvent e)
	{
		PlayerLandedOnTileEvent evt = (PlayerLandedOnTileEvent) e;
		Debug.Log (evt.tileType.ToString () + " " + evt.value);
		switch (evt.tileType) 
		{
			case TileBehavior.TileType.FastPass:
				currentPlayer.addFastPass ();
				endTurn ();
				break;
			case TileBehavior.TileType.Pay:
				if (currentPlayer.getCashBalance() < evt.value)
					endTurn ();
				else 
				{
					_currentPayToRideEvent = evt;
					Debug.LogWarning ("TODO:  Add dynamic Ride getting");
					EventDispatcher.DispatchEvent (new AskPayToRideEvent (ParkHopper.Rides.Ride.ThunderMountain, evt.value));
				}
				break;
			case TileBehavior.TileType.Get:
				currentPlayer.addCash(evt.value);
				endTurn ();
				break;
			case TileBehavior.TileType.Lose:
				currentPlayer.removeCash(evt.value);
				endTurn ();
				break;
			case TileBehavior.TileType.ForceMove:
				// If the turn is a force move, simulate another dice roll for the same player.  Do not end the turn.
				// We will get another onPlayerLandedOnTileEvent once the move is complete and we can re-eval the players status.
				EventDispatcher.DispatchEvent (new DiceRollCompleteEvent (evt.value, currentPlayer.playerNumber));
				break;
			default:
				endTurn ();
				break;
		}
	}
	private void endTurn()
	{
		if (currentPlayer.playerNumber == players.Length) {
			currentPlayer = players [0];
		}
		else
			currentPlayer = players [currentPlayer.playerNumber];
		mainCamera.transform.SetParent( currentPlayer.gameObject.transform);
		mainCamera.transform.position = new Vector3(currentPlayer.gameObject.transform.position.x +3f,currentPlayer.gameObject.transform.position.y +3f, currentPlayer.gameObject.transform.position.z -4f);
		EventDispatcher.DispatchEvent (new TurnEndEvent (currentPlayer.playerNumber));
	}
	private void onDiceRollComplete(IEvent e)
	{
		DieAtRestEvent evt = (DieAtRestEvent)e;
		_totalDiceValueThisRoll += evt.value;
		_numberOfDiesAtRest++;
		if (_numberOfDiesAtRest == numberOfDie) 
		{
			EventDispatcher.DispatchEvent (new DiceRollCompleteEvent (_totalDiceValueThisRoll, currentPlayer.playerNumber));
			ResetDice ();
		}
	}
	private void ResetDice()
	{
		_numberOfDiesAtRest = 0;
		_totalDiceValueThisRoll = 0;
	}
	private Vector3 Force()
	{
		Vector3 rollTarget = Vector3.zero + new Vector3(2 + 7 * Random.value, .5F + 4 * Random.value, -2 - 3 * Random.value);
		return Vector3.Lerp(spawnPoint.transform.position, rollTarget, 1).normalized * (-35 - Random.value * 20);
	}
	// handle GUI
	void OnGUI()
	{
		// Make mode selection box
		GUI.Box (rectModeSelect, "Dice Demo");

		if (GUI.Button (new Rect (20,60,160,20), "Roll Dice")) 
		{
			Dice.Clear();
			spawnPoint = GameObject.Find("spawnPoint");
			for (int i = 0; i < numberOfDie; i++)
				Dice.Roll("1d6", "d6-" + "blue", spawnPoint.transform.position, Force());
		}
	}
}

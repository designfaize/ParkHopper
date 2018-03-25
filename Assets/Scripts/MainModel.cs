using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkHopper.Events;
namespace ParkHopper
{
	public class MainModel : MonoBehaviour {
		[SerializeField] 
		private Camera playerCamera;
		[SerializeField]
		private Camera diceCamera;

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
		protected Player[] players;
		[SerializeField]
		private int startingCash;
		[SerializeField]
		private int startingFastpasses;
		[SerializeField]
		private GameObject diceSpawn;
		private Player currentPlayer;

		private const string DIE_TYPE = "1d6";
		private const string DIE_COLOR = "blue";
		private const string DIE_MATERIAL = "d6-";

		private const float ROLL_AGAIN_DELAY = 1f;
		private const float FORCE_MOVE_DELAY = 2f;
		private PlayerLandedOnTileEvent _currentPayToRideEvent;
		private PlayerLandedOnTileEvent _previousPlayerLandedOnTileEvent;

		private bool _rideDiceRoll = false;
		// Use this for initialization
		void Start () {
			ParkHopperEvents.RegisterEvents ();
			Debug.Log ("Reg Events");
			EventDispatcher.AddListener<DieAtRestEvent>(onDiceRollComplete);
			EventDispatcher.AddListener<PlayerLandedOnTileEvent> (onPlayerLandedOnTileEvent);
			EventDispatcher.AddListener<PayToRideEvent> (onPlayerPaidToRide);
			EventDispatcher.AddListener<PayToRideSkipEvent> (onPlayerPaidToRideSkip);
			EventDispatcher.AddListener<DiceRollBeginEvent> (onDiceRollBeginEvent);
			EventDispatcher.AddListener<BuySorcererCardEvent> (onBuySorcererCardEvent);
			EventDispatcher.AddListener<SkipBuySorcererCardEvent> (onSkipBuySorcererCardEvent);
			EventDispatcher.AddListener<UseSorcererCardEvent> (onUseSorcererCardEvent);
			EventDispatcher.AddListener<SkipUseSorcererCardEvent> (onSkipUseSorcererCardEvent);
			EventDispatcher.AddListener<UseFastpassEvent> (onUseFastpassEvent);


			setupPlayers ();
			EventDispatcher.DispatchEvent (new GameStartEvent ());
		}
		private void onDiceRollBeginEvent(IEvent e)
		{
			diceCamera.gameObject.SetActive (true);
			playerCamera.gameObject.SetActive (false);
			Dice.Clear();
			for (int i = 0; i < numberOfDie; i++)
				Dice.Roll(DIE_TYPE, DIE_MATERIAL + DIE_COLOR, diceSpawn.transform.position, Force());
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
			currentPlayer.removeCash (_previousPlayerLandedOnTileEvent.value);
			SendRideEvent ();
		}
		private void onUseFastpassEvent (IEvent e)
		{
			currentPlayer.useFastPass ();
			SendRideEvent ();
		}
		private void SendRideEvent()
		{
			// We have a strange ride behavior as designed by an 8 year old.
			// Space mountain requires a dice roll.  Greater than a 3 you ride path 2
			// Less than a 3, ride path 1.
			if (_previousPlayerLandedOnTileEvent.ride == TileBehavior.Ride.SpaceMountain) 
			{
				_rideDiceRoll = true;
				EventDispatcher.DispatchEvent(new ShowUIMessageEvent("Roll 3 or greater for Left Side. Less than 3 for Right."));
				StartCoroutine(SendEventAfterDelay(new DiceRollBeginEvent (), ROLL_AGAIN_DELAY));
				return;
			}
			EventDispatcher.DispatchEvent (new BeginRideEvent (_previousPlayerLandedOnTileEvent.ride));
			_currentPayToRideEvent = null;
		}
		// If the user was presented the option to pay to ride and skipped, end the turn;
		private void onPlayerPaidToRideSkip(IEvent e)
		{
			_currentPayToRideEvent = null;
			EndTurn ();
		}
		private void onPlayerLandedOnTileEvent(IEvent e)
		{
			PlayerLandedOnTileEvent evt = (PlayerLandedOnTileEvent) e;
			_previousPlayerLandedOnTileEvent = evt;
			Debug.Log (evt.tileType.ToString () + " " + evt.value);
			switch (evt.tileType) 
			{
				case TileBehavior.TileType.FastPass:
					currentPlayer.addFastPass ();
					EventDispatcher.DispatchEvent(new ShowUIMessageEvent("Player " + currentPlayer.playerNumber +" Got a FastPass"));
					EndTurn ();
					break;
				case TileBehavior.TileType.Pay:
				if (currentPlayer.getCashBalance () < evt.value && currentPlayer.getfastPassCount() == 0) {
						EventDispatcher.DispatchEvent (new ShowUIMessageEvent ("Not enough Cash to ride " + TileBehavior.rideLookupDictionary [evt.ride]));
						EndTurn ();
					}
					else 
					{
						_currentPayToRideEvent = evt;
						bool canUseFastpass = false;
						if (currentPlayer.getfastPassCount () > 0)
							canUseFastpass = true;
						bool canUseCash = false;
						if (currentPlayer.getCashBalance () >= evt.value)
							canUseCash = true;
						EventDispatcher.DispatchEvent (new AskPayToRideEvent (evt.ride, evt.value, canUseFastpass, canUseCash));
					}
					break;
				case TileBehavior.TileType.BuySorcererCard:
					HandleShowUIForSorcererCard (evt.value);
					break;
				case TileBehavior.TileType.Get:
					currentPlayer.addCash(evt.value);
					EventDispatcher.DispatchEvent(new ShowUIMessageEvent("Player " + currentPlayer.playerNumber +" Got $"+evt.value.ToString()));
					EndTurn ();
					break;
				case TileBehavior.TileType.Lose:
					currentPlayer.removeCash(evt.value);
					EventDispatcher.DispatchEvent(new ShowUIMessageEvent("Player " + currentPlayer.playerNumber +" Lost $"+evt.value.ToString()));
					EndTurn ();
					break;
				case TileBehavior.TileType.ForceMove:
					// If the turn is a force move, simulate another dice roll for the same player.  Do not end the turn.
					// We will get another onPlayerLandedOnTileEvent once the move is complete and we can re-eval the players status.
					ShowUIMessageEvent uiMessage;
					if (evt.value > 0) 
					uiMessage = new ShowUIMessageEvent("Move Forward " + evt.value.ToString() + " Spaces");
					else 
						uiMessage = new ShowUIMessageEvent("Move Back " + evt.value.ToString() + " Spaces");
					EventDispatcher.DispatchEvent(uiMessage);
					StartCoroutine(SendEventAfterDelay(new DiceRollCompleteEvent (evt.value, currentPlayer.playerNumber),FORCE_MOVE_DELAY));
					break;
				case TileBehavior.TileType.RollAgain:
					EventDispatcher.DispatchEvent(new ShowUIMessageEvent("Player " + currentPlayer.playerNumber +" Roll Again!"));
					StartCoroutine(SendEventAfterDelay(new DiceRollBeginEvent (), ROLL_AGAIN_DELAY));
					break;
				case TileBehavior.TileType.VilliansMove:
					if (currentPlayer.getSorcererCards () > 0)
						EventDispatcher.DispatchEvent (new AskUseSorcererCardEvent ());
					else 
					{
						EventDispatcher.DispatchEvent (new ShowUIMessageEvent ("No Sorcerer Cards.  Move Back " + evt.value.ToString () + " Spaces"));
						StartCoroutine (SendEventAfterDelay (new DiceRollCompleteEvent (evt.value, currentPlayer.playerNumber), FORCE_MOVE_DELAY));
					}
					break;
				case TileBehavior.TileType.VilliansLoseCash:
					if (currentPlayer.getSorcererCards () > 0)
						EventDispatcher.DispatchEvent (new AskUseSorcererCardEvent ());
					else 
					{
						currentPlayer.removeCash(evt.value);
						EventDispatcher.DispatchEvent(new ShowUIMessageEvent("No Sorcerer Cards.  Player " + currentPlayer.playerNumber +" Lost $"+evt.value.ToString()));
						EndTurn ();
					}
					break;
				default:
					EndTurn ();
					break;
			}
		}
		private void onBuySorcererCardEvent(IEvent e)
		{
			currentPlayer.removeCash (((BuySorcererCardEvent)e).cost);
			currentPlayer.addSorcererCard ();
			EndTurn ();
		}
		private void onUseSorcererCardEvent(IEvent e)
		{
			currentPlayer.useSorcererCard ();
			EndTurn ();
		}
		private void onSkipUseSorcererCardEvent(IEvent e)
		{
			if (_previousPlayerLandedOnTileEvent.tileType == TileBehavior.TileType.VilliansMove) 
			{
				EventDispatcher.DispatchEvent(new PlayerLandedOnTileEvent(TileBehavior.TileType.ForceMove,_previousPlayerLandedOnTileEvent.value,TileBehavior.Ride.Empty));
			} 
			else 
			{
				EventDispatcher.DispatchEvent(new PlayerLandedOnTileEvent(TileBehavior.TileType.Lose,_previousPlayerLandedOnTileEvent.value,TileBehavior.Ride.Empty));
			}
		}
		private void onSkipBuySorcererCardEvent(IEvent e)
		{
			EndTurn ();
		}
		public void HandleShowUIForSorcererCard(int cost)
		{
			if (currentPlayer.getCashBalance() >= cost)
				EventDispatcher.DispatchEvent (new AskBuySorcererCardEvent (cost));
			else 
			{
				EventDispatcher.DispatchEvent(new ShowUIMessageEvent("Player " + currentPlayer.playerNumber +" Doesn't have enough cash to buy a Sorcerer Card."));
				EndTurn ();
			}
		}
		private IEnumerator SendEventAfterDelay(IEvent e, float delay)
		{
			yield return new WaitForSeconds(delay);
			EventDispatcher.DispatchEvent (e);
		}
		protected void EndTurn()
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
			// Ride dice roll happens for space mountain.
			// If this is a non special dice roll, do the normal stuff.
			if (!_rideDiceRoll) {
				_totalDiceValueThisRoll += evt.value;
				_numberOfDiesAtRest++;
				if (_numberOfDiesAtRest == numberOfDie) {
					EventDispatcher.DispatchEvent (new DiceRollCompleteEvent (_totalDiceValueThisRoll, currentPlayer.playerNumber));
					ResetDice ();
					diceCamera.gameObject.SetActive (false);
					playerCamera.gameObject.SetActive (true);
				}
			}
			// If we are in our 1 off space mountain mode, do things a little different.
			else 
			{
				HandleSpaceMountainMode (evt.value);
			}
		}
		// Handle Space Mountain Mode
		private void HandleSpaceMountainMode(int dice)
		{
			_rideDiceRoll = false;
			ResetDice ();
			diceCamera.gameObject.SetActive (false);
			playerCamera.gameObject.SetActive (true);
			if (dice >= 3)
				EventDispatcher.DispatchEvent (new BeginRideEvent (TileBehavior.Ride.SpaceMountain2));
			else 
				EventDispatcher.DispatchEvent (new BeginRideEvent (TileBehavior.Ride.SpaceMountain1));
			_currentPayToRideEvent = null;
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

		 
	}
}
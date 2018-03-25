using System;
using ParkHopper.Events;
using System.Collections.Generic;

namespace ParkHopper.Events
{
	public static class ParkHopperEvents
	{
		public static void RegisterEvents ()
		{
			EventDispatcher.CreateScope (
				EventDispatcher.DEFAULT_SCOPE_ID,
				typeof (GameStartEvent),
				typeof (ShowUIMessageEvent),
				typeof (PlayerBalanceUpdateEvent),
				typeof (DiceRollBeginEvent),
				typeof (DieAtRestEvent),
				typeof (DiceRollCompleteEvent),
				typeof (PlayerLandedOnTileEvent),
				typeof (TurnEndEvent),

				// Ride Events
				typeof (AskPayToRideEvent),
				typeof (BeginRideEvent),
				typeof (PayToRideSkipEvent),
				typeof (UseFastpassEvent),
				typeof (PayToRideEvent),

				// SorcererCard Events
				typeof (AskBuySorcererCardEvent),
				typeof (BuySorcererCardEvent),
				typeof (SkipBuySorcererCardEvent),
				typeof (UseSorcererCardEvent),
				typeof (SkipUseSorcererCardEvent),
				typeof (AskUseSorcererCardEvent),


				// Dummy event, so you don't have to remember about adding an extra
				// comma before your new event.
				typeof (DummyEvent)
			);
		}
	}
 
	public class DummyEvent : IEvent { }
	public class GameStartEvent : IEvent { }
	public class ShowUIMessageEvent : IEvent 
	{
		public string message;
		public ShowUIMessageEvent (string message)
		{
			this.message = message;
		}
	}
	public class PlayerBalanceUpdateEvent : IEvent 
	{
		public int cash;
		public int fastPasses;
		public int playerNumber;
		public int sorcererCards;
		public PlayerBalanceUpdateEvent (int cash, int fastPasses, int playerNumber, int sorcererCards)
		{
			this.cash = cash;
			this.fastPasses = fastPasses;
			this.playerNumber = playerNumber;
			this.sorcererCards = sorcererCards;
		}
	}
	public class PayToRideEvent : IEvent{}
	public class DiceRollBeginEvent : IEvent{}
	public class DieAtRestEvent : IEvent
	{
		public int value;
		public DieAtRestEvent(int value)
		{
			this.value = value;
		}
	}
	public class DiceRollCompleteEvent : IEvent
	{
		public int value;
		public int player;
		public DiceRollCompleteEvent(int value, int player)
		{
			this.value = value;
			this.player = player;
		}
	}
	public class PlayerLandedOnTileEvent : IEvent
	{
		public TileBehavior.TileType tileType;
		public int value;
		public TileBehavior.Ride ride;
		public PlayerLandedOnTileEvent(TileBehavior.TileType tileType, int value, TileBehavior.Ride ride)
		{
			this.tileType = tileType;
			this.value = value;
			this.ride = ride;
		}
	}
	public class UseFastpassEvent : IEvent {}
	public class TurnEndEvent : IEvent
	{
		public int currentPlayer;
		public TurnEndEvent (int currentPlayer)
		{
			this.currentPlayer = currentPlayer;
		}
	}

	public class AskPayToRideEvent : IEvent 
	{
		public TileBehavior.Ride ride;
		public string rideName;
		public int cost;
		public bool canUseFastPass;
		public bool canUseCash;
		public AskPayToRideEvent(TileBehavior.Ride ride, int cost, bool canUseFastPass, bool canUseCash)
		{
			this.ride = ride;
			this.cost = cost;
			this.rideName = TileBehavior.rideLookupDictionary [ride];
			this.canUseFastPass = canUseFastPass;
			this.canUseCash = canUseCash;
		}
	}
	#region sorcerer card events
	public class AskBuySorcererCardEvent : IEvent
	{
		public int cost;
		public AskBuySorcererCardEvent(int cost)
		{
			this.cost = cost;
		}
	}
	public class BuySorcererCardEvent : IEvent
	{
		public int cost;
		public BuySorcererCardEvent(int cost)
		{
			this.cost = cost;
		}
	}
	public class SkipBuySorcererCardEvent : IEvent {}

	public class UseSorcererCardEvent : IEvent {}
	public class SkipUseSorcererCardEvent : IEvent {}
	public class AskUseSorcererCardEvent : IEvent {}
	#endregion
	// Handle the UI Interaction when deciding to pay to ride or skip
	public class BeginRideEvent : IEvent
	{
		public TileBehavior.Ride ride;
		public BeginRideEvent(TileBehavior.Ride ride)
		{
			this.ride = ride;
		}
	}
	public class PayToRideSkipEvent : IEvent{}
}

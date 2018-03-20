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
				typeof (PlayerBalanceUpdateEvent),
				typeof (DiceRollBeginEvent),
				typeof (DieAtRestEvent),
				typeof (DiceRollCompleteEvent),
				typeof (PlayerLandedOnTileEvent),
				typeof (TurnEndEvent),
				typeof (AskPayToRideEvent),
				typeof (PayToRideEvent),
				typeof (PayToRideSkipEvent),


				// Dummy event, so you don't have to remember about adding an extra
				// comma before your new event.
				typeof (DummyEvent)
			);
		}
	}
 
	public class DummyEvent : IEvent { }
	public class PlayerBalanceUpdateEvent : IEvent 
	{
		public int cash;
		public int fastPasses;
		public int playerNumber;
		public PlayerBalanceUpdateEvent (int cash, int fastPasses, int playerNumber)
		{
			this.cash = cash;
			this.fastPasses = fastPasses;
			this.playerNumber = playerNumber;
		}
	}
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
		public PlayerLandedOnTileEvent(TileBehavior.TileType tileType, int value)
		{
			this.tileType = tileType;
			this.value = value;
		}
	}
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
		public Rides.Ride ride;
		public string rideName;
		public int cost;
		public AskPayToRideEvent(Rides.Ride ride, int cost)
		{
			this.ride = ride;
			this.cost = cost;
			this.rideName = Rides.rideLookupDictionary [ride];
		}
	}
	// Handle the UI Interaction when deciding to pay to ride or skip
	public class PayToRideEvent : IEvent
	{
		public Rides.Ride ride;
		public PayToRideEvent(Rides.Ride ride)
		{
			this.ride = ride;
		}
	}
	public class PayToRideSkipEvent : IEvent{}
}

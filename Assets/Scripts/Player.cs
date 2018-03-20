using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkHopper.Events;

public class Player : MonoBehaviour 
{
	public int playerNumber = 1;
	[SerializeField]
	private int fastPasses = 0;
	private int cash = 0;

	public int currentPoint;
	public int rollDestination = 0;
	public Vector3 nextDestination;
	public Vector3 midwayToNextDestination;
	public float nextDistance;

	public int getCashBalance()
	{
		return cash;
	}
	public void addFastPass()
	{
		fastPasses++;
		sendPlayerBalanceUpdateEvent ();
	}
	public void useFastPass()
	{
		fastPasses--;
		sendPlayerBalanceUpdateEvent ();
	}
	public void addCash(int amount)
	{
		cash += amount;
		sendPlayerBalanceUpdateEvent ();
	}
	public void removeCash(int amount)
	{
		cash -= amount;
		if (cash < 0)
			cash = 0;
		sendPlayerBalanceUpdateEvent ();
	}
	private void sendPlayerBalanceUpdateEvent()
	{
		EventDispatcher.DispatchEvent (new PlayerBalanceUpdateEvent (cash, fastPasses, playerNumber));
	}
}

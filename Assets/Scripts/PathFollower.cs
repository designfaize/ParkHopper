using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkHopper.Events;
using ParkHopper;

public class PathFollower : MonoBehaviour {
	public GameObject[] pathObjects;
	private List<Transform> path = new List<Transform>();
	[SerializeField]
	private float speed = 3.0f;
	[SerializeField]
	private float reachDistance = 1.0f;
	[SerializeField]
	private Player[] players;
	[SerializeField]
	private Transform bigThunderMountain;
	[SerializeField]
	private Transform bigThunderSeat;
	[SerializeField]
	private int bigThunderExitTileIndex;

	private Vector3 nextBigThunderLocation;
	private int bigThunderPosition = 0;
	public List<Transform> thunderMountainPath = new List<Transform>();

	private bool moveComplete = true;
	private float c;
	private const float heightToAddToDest = .6f;
	private const float heightToAddInBetweenPieces = 1f;
	private Player _currentPlayer;

	private Rides.Ride _currentlyRiding= Rides.Ride.Empty;
	private Rides.Ride _currentlyBoarding= Rides.Ride.Empty;
	private Rides.Ride _currentlyReturning = Rides.Ride.Empty;
	// Use this for initialization
	void Awake () 
	{
		foreach (GameObject obj in pathObjects)
			path.Add (obj.transform);
		_currentPlayer = players[0];
		_currentPlayer.nextDestination = new Vector3 (path [_currentPlayer.currentPoint].position.x, path [_currentPlayer.currentPoint].position.y + heightToAddToDest, path [_currentPlayer.currentPoint].position.z);
		ParkHopperEvents.RegisterEvents ();
		EventDispatcher.AddListener<DiceRollCompleteEvent> (onDiceRollComplete);
		EventDispatcher.AddListener<PayToRideEvent> (onPayToRideEvent);
		EventDispatcher.AddListener<TurnEndEvent> (onTurnEndEvent);
	}
	private void onTurnEndEvent(IEvent e)
	{
		TurnEndEvent evt = (TurnEndEvent)e;
		_currentPlayer = players [evt.currentPlayer - 1];
		_currentPlayer.nextDestination = new Vector3 (path [_currentPlayer.currentPoint].position.x, path [_currentPlayer.currentPoint].position.y + heightToAddToDest, path [_currentPlayer.currentPoint].position.z);
	}
	void onDiceRollComplete(IEvent e)
	{
		DiceRollCompleteEvent evt = (DiceRollCompleteEvent)e;
		// If it was this players roll, lets move.
		if (evt.player == _currentPlayer.playerNumber) 
		{
			_currentPlayer.rollDestination = _currentPlayer.currentPoint + evt.value;
			if (_currentPlayer.rollDestination >= path.Count)
				_currentPlayer.rollDestination = path.Count -1;
		}
	}
	// Update is called once per frame
	void Update()
	{
		if (_currentPlayer.currentPoint != _currentPlayer.rollDestination) {
			moveComplete = false;
			float dist;
			// If moving backwards
			if (_currentPlayer.currentPoint > _currentPlayer.rollDestination) 
			{
				dist = Vector3.Distance (path [_currentPlayer.rollDestination-1].position, _currentPlayer.transform.position);
				_currentPlayer.transform.position = Vector3.LerpUnclamped (_currentPlayer.transform.position, path [_currentPlayer.rollDestination-1].position, Time.deltaTime * speed);
				if (dist <= reachDistance) {
					_currentPlayer.currentPoint = _currentPlayer.rollDestination;
					_currentPlayer.nextDestination = new Vector3 (path [_currentPlayer.currentPoint].position.x, path [_currentPlayer.currentPoint].position.y + heightToAddToDest, path [_currentPlayer.currentPoint].position.z);
				}
			}
			else 
			{
				// Calculate distance between where ware and where we need to be.
				dist = Vector3.Distance (_currentPlayer.nextDestination, _currentPlayer.transform.position);
				_currentPlayer.transform.position = Vector3.LerpUnclamped (_currentPlayer.transform.position, _currentPlayer.nextDestination, Time.deltaTime * speed);
				if (dist <= reachDistance) {
					_currentPlayer.currentPoint++;
					if (_currentPlayer.currentPoint >= path.Count)
						_currentPlayer.currentPoint = 0;
					_currentPlayer.nextDestination = new Vector3 (path [_currentPlayer.currentPoint].position.x, path [_currentPlayer.currentPoint].position.y + heightToAddToDest, path [_currentPlayer.currentPoint].position.z);
				}
			}

		} else if (_currentlyBoarding == Rides.Ride.ThunderMountain) {
			float dist = Vector3.Distance (bigThunderSeat.position, _currentPlayer.transform.position);
			_currentPlayer.transform.position = Vector3.LerpUnclamped (_currentPlayer.transform.position, bigThunderSeat.position, Time.deltaTime * speed);
			if (dist <= reachDistance / 2) {
				_currentlyBoarding = Rides.Ride.Empty;
				_currentlyRiding = Rides.Ride.ThunderMountain;
			}
		} else if (_currentlyRiding == Rides.Ride.ThunderMountain) {
			float dist = Vector3.Distance (bigThunderMountain.position, nextBigThunderLocation);
			bigThunderMountain.position = Vector3.MoveTowards (bigThunderMountain.position, nextBigThunderLocation, Time.deltaTime * speed);
			_currentPlayer.transform.position = Vector3.MoveTowards (_currentPlayer.transform.position, bigThunderSeat.position, Time.deltaTime * speed);
			if (dist <= reachDistance) {
				bigThunderPosition++;
				if (bigThunderPosition >= thunderMountainPath.Count) {
					_currentlyRiding = Rides.Ride.Empty;
					_currentlyReturning = Rides.Ride.ThunderMountain;
					_currentPlayer.rollDestination = bigThunderExitTileIndex;
					_currentPlayer.nextDestination = path [bigThunderExitTileIndex].position;
					_currentPlayer.currentPoint = bigThunderExitTileIndex-1;
				} else {
					nextBigThunderLocation = thunderMountainPath [bigThunderPosition].position;
				}
			}
		}
		else if (_currentlyReturning == Rides.Ride.ThunderMountain) 
		{
			nextBigThunderLocation = thunderMountainPath [0].position;
			bigThunderMountain.position = Vector3.MoveTowards (bigThunderMountain.position, nextBigThunderLocation, Time.deltaTime * speed);
			_currentPlayer.transform.position = Vector3.MoveTowards (_currentPlayer.transform.position, _currentPlayer.nextDestination, Time.deltaTime * speed);
			float dist = Vector3.Distance (bigThunderMountain.position, nextBigThunderLocation);
			if (dist <= reachDistance) 
			{
				_currentlyReturning = Rides.Ride.Empty;
			}
		}
		else 
		{
			if (!moveComplete) {
				moveComplete = true;
				EventDispatcher.DispatchEvent (GetCurrentTileEventData ());
			}
		}
	}
	private void onPayToRideEvent(IEvent e)
	{
		PayToRideEvent evt = (PayToRideEvent)e;
		switch (evt.ride) 
		{
		case Rides.Ride.ThunderMountain:
			_currentlyBoarding = evt.ride;
			nextBigThunderLocation = thunderMountainPath [0].position;
			break;
		default:
			break;
		}
	}
	private PlayerLandedOnTileEvent GetCurrentTileEventData()
	{
		TileBehavior tileData = pathObjects [_currentPlayer.currentPoint-1].GetComponent<TileBehavior>();
		return new PlayerLandedOnTileEvent (tileData.tile, tileData.value);
	}
	void OnDrawGizmos()
	{
		if (thunderMountainPath.Count > 0) 
		{
			for (int i = 0; i < thunderMountainPath.Count; i++) 
			{
				if (thunderMountainPath [i] != null) 
				{
						Gizmos.DrawSphere (thunderMountainPath [i].position, reachDistance);
				}
			}
		}
	}
}

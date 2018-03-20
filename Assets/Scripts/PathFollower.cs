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

	public List<Transform> thunderMountainPath = new List<Transform>();

	private bool moveComplete = true;
	private float c;
	private const float heightToAddToDest = .6f;
	private const float heightToAddInBetweenPieces = 1f;
	private Player _currentPlayer;

	private Rides.Ride _currentlyRiding= Rides.Ride.Empty;
	// Use this for initialization
	void Awake () 
	{
		foreach (GameObject obj in pathObjects)
			path.Add (obj.transform);
		_currentPlayer = players[0];
		_currentPlayer.nextDestination = new Vector3 (path [_currentPlayer.currentPoint].position.x, path [_currentPlayer.currentPoint].position.y + heightToAddToDest, path [_currentPlayer.currentPoint].position.z);
		_currentPlayer.midwayToNextDestination = new Vector3 (path [_currentPlayer.currentPoint].position.x, path [_currentPlayer.currentPoint].position.y + heightToAddInBetweenPieces, path [_currentPlayer.currentPoint].position.z);
		_currentPlayer.nextDistance= Vector3.Distance(_currentPlayer.nextDestination, _currentPlayer.transform.position);
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
		_currentPlayer.nextDistance= Vector3.Distance(_currentPlayer.nextDestination, path[_currentPlayer.currentPoint].position);
		_currentPlayer.midwayToNextDestination = new Vector3 (path [_currentPlayer.currentPoint].position.x, path [_currentPlayer.currentPoint].position.y + heightToAddInBetweenPieces, path [_currentPlayer.currentPoint].position.z);
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
			// Calculate distance between where ware and where we need to be.
			float dist = Vector3.Distance (_currentPlayer.nextDestination, _currentPlayer.transform.position);
			// If we _currentPlayer.midwayToNextDestinatione inbetween moves
			Debug.Log("Distance : "+ dist);
			float halfway = _currentPlayer.nextDistance / 2;
			Debug.Log("halfway : "+ halfway);
			//if (dist > halfway)
			//	_currentPlayer.transform.position = Vector3.LerpUnclamped (_currentPlayer.transform.position, _currentPlayer.midwayToNextDestination, Time.deltaTime * speed);
			//else
				_currentPlayer.transform.position = Vector3.LerpUnclamped (_currentPlayer.transform.position, _currentPlayer.nextDestination, Time.deltaTime * speed);

			if (dist <= reachDistance) {
				_currentPlayer.currentPoint++;
				if (_currentPlayer.currentPoint >= path.Count)
					_currentPlayer.currentPoint = 0;
				_currentPlayer.midwayToNextDestination = new Vector3 (path [_currentPlayer.currentPoint].position.x, path [_currentPlayer.currentPoint].position.y + heightToAddInBetweenPieces, path [_currentPlayer.currentPoint].position.z);
				_currentPlayer.nextDistance = Vector3.Distance (_currentPlayer.nextDestination, _currentPlayer.transform.position);;
				_currentPlayer.nextDestination = new Vector3 (path [_currentPlayer.currentPoint].position.x, path [_currentPlayer.currentPoint].position.y + heightToAddToDest, path [_currentPlayer.currentPoint].position.z);
			}
		}
		else if (_currentlyRiding == Rides.Ride.ThunderMountain) 
		{
			float dist = Vector3.Distance (_currentPlayer.nextDestination, _currentPlayer.transform.position);
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
			_currentlyRiding = evt.ride;
			//_nextRideDestination = thunderMountainPath [0].position;
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

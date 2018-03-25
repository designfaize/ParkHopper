using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkHopper.Events;
using ParkHopper;

public class PathFollower : MonoBehaviour 
{
	public GameObject pathObjects;
	private List<Transform> path = new List<Transform>();
	[SerializeField]
	private float speed = 3.0f;
	[SerializeField]
	private float reachDistance = 1.0f;
	[SerializeField]
	private Player[] players;

	#region bigthunder vars
	[SerializeField]
	private Transform bigThunderMountain;
	[SerializeField]
	private Transform bigThunderSeat;
	[SerializeField]
	private int bigThunderExitTileIndex;
	private Vector3 nextBigThunderLocation;
	private int bigThunderPosition = 0;
	public GameObject thunderMountainPathContainer;
	private List<Transform> thunderMountainPath = new List<Transform>();
	#endregion

	#region splash vars
	// Splash Mountain
	private int splashPosition = 0;
	public GameObject splashMountainPathContainer;
	private List<Transform> splashMountainPath = new List<Transform>();
	private Vector3 nextSplashLocation;
	[SerializeField]
	private int splashExitTileIndex =26;
	#endregion

	#region test track vars
	// Test Track
	private int testTrackPosition = 0;
	public GameObject testTrackPathContainer;
	private List<Transform> testTrackPath = new List<Transform>();
	private Vector3 nexttestTrackLocation;
	[SerializeField]
	private int testTrackExitTileIndex;
	#endregion

	#region Figment vars
	// Figment
	private int figmentPosition = 0;
	public GameObject figmentPathContainer;
	private List<Transform> figmentPath = new List<Transform>();
	private Vector3 nextFigmentLocation;
	[SerializeField]
	private int figmentExitTileIndex;
	#endregion

	#region space vars
	private int space1Position = 0;
	private int space2Position = 0;
	public GameObject spaceMountainPathContainer1;
	public GameObject spaceMountainPathContainer2;
	private List<Transform> space1MountainPath = new List<Transform>();
	private List<Transform> space2MountainPath = new List<Transform>();
	private Vector3 nextSpace1Location;
	private Vector3 nextSpace2Location;
	[SerializeField]
	private int space1ExitTileIndex = 33;
	[SerializeField]
	private int space2ExitTileIndex = 36;

	#endregion

	private bool moveComplete = true;
	private float c;
	private const float heightToAddToDest = .6f;
	private const float heightToAddInBetweenPieces = 1f;
	private Player _currentPlayer;

	private TileBehavior.Ride _currentlyRiding= TileBehavior.Ride.Empty;
	private TileBehavior.Ride _currentlyBoarding= TileBehavior.Ride.Empty;
	private TileBehavior.Ride _currentlyReturning = TileBehavior.Ride.Empty;

	private bool _justRoadARide=false;
	// Use this for initialization
	void Awake () 
	{
		foreach (Transform obj in pathObjects.transform)
			path.Add (obj.transform);
		_currentPlayer = players[0];
		_currentPlayer.nextDestination = new Vector3 (path [_currentPlayer.currentPoint].position.x, path [_currentPlayer.currentPoint].position.y + heightToAddToDest, path [_currentPlayer.currentPoint].position.z);
		ParkHopperEvents.RegisterEvents ();
		EventDispatcher.AddListener<DiceRollCompleteEvent> (onDiceRollComplete);
		EventDispatcher.AddListener<BeginRideEvent> (onBeginRideEvent);
		EventDispatcher.AddListener<TurnEndEvent> (onTurnEndEvent);

		foreach (Transform child in splashMountainPathContainer.transform)
		{
			splashMountainPath.Add (child);
		}
		foreach (Transform child in thunderMountainPathContainer.transform)
		{
			thunderMountainPath.Add (child);
		}
		foreach (Transform child in spaceMountainPathContainer1.transform)
		{
			space1MountainPath.Add (child);
		}
		foreach (Transform child in spaceMountainPathContainer2.transform)
		{
			space2MountainPath.Add (child);
		}
		foreach (Transform child in testTrackPathContainer.transform)
		{
			testTrackPath.Add (child);
		}
		foreach (Transform child in figmentPathContainer.transform)
		{
			figmentPath.Add (child);
		}
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
			if (_currentPlayer.currentPoint > _currentPlayer.rollDestination) {
				dist = Vector3.Distance (path [_currentPlayer.rollDestination - 1].position, _currentPlayer.transform.position);
				_currentPlayer.transform.position = Vector3.LerpUnclamped (_currentPlayer.transform.position, path [_currentPlayer.rollDestination - 1].position, Time.deltaTime * speed);
				if (dist <= reachDistance) {
					_currentPlayer.currentPoint = _currentPlayer.rollDestination;
					_currentPlayer.nextDestination = new Vector3 (path [_currentPlayer.currentPoint].position.x, path [_currentPlayer.currentPoint].position.y + heightToAddToDest, path [_currentPlayer.currentPoint].position.z);
				}
			} else {
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

		} else if (_currentlyBoarding == TileBehavior.Ride.ThunderMountain) {
			float dist = Vector3.Distance (bigThunderSeat.position, _currentPlayer.transform.position);
			_currentPlayer.transform.position = Vector3.LerpUnclamped (_currentPlayer.transform.position, bigThunderSeat.position, Time.deltaTime * speed);
			if (dist <= reachDistance / 2) {
				_currentlyBoarding = TileBehavior.Ride.Empty;
				_currentlyRiding = TileBehavior.Ride.ThunderMountain;
			}
		} else if (_currentlyRiding == TileBehavior.Ride.ThunderMountain) {
			float dist = Vector3.Distance (bigThunderMountain.position, nextBigThunderLocation);
			bigThunderMountain.position = Vector3.MoveTowards (bigThunderMountain.position, nextBigThunderLocation, Time.deltaTime * speed);
			_currentPlayer.transform.position = Vector3.MoveTowards (_currentPlayer.transform.position, bigThunderSeat.position, Time.deltaTime * speed);
			if (dist <= reachDistance) {
				bigThunderPosition++;
				if (bigThunderPosition >= thunderMountainPath.Count) {
					_currentlyRiding = TileBehavior.Ride.Empty;
					_currentlyReturning = TileBehavior.Ride.ThunderMountain;
					_currentPlayer.rollDestination = bigThunderExitTileIndex;
					_currentPlayer.nextDestination = path [bigThunderExitTileIndex].position;
					_currentPlayer.currentPoint = bigThunderExitTileIndex - 1;
					_justRoadARide = true;
				} else {
					nextBigThunderLocation = thunderMountainPath [bigThunderPosition].position;
				}
			}
		} else if (_currentlyReturning == TileBehavior.Ride.ThunderMountain) {
			nextBigThunderLocation = thunderMountainPath [0].position;
			bigThunderMountain.position = Vector3.MoveTowards (bigThunderMountain.position, nextBigThunderLocation, Time.deltaTime * speed);
			_currentPlayer.transform.position = Vector3.MoveTowards (_currentPlayer.transform.position, _currentPlayer.nextDestination, Time.deltaTime * speed);
			float dist = Vector3.Distance (bigThunderMountain.position, nextBigThunderLocation);
			if (dist <= reachDistance) {
				_currentlyReturning = TileBehavior.Ride.Empty;
			}
		} 
		else if (_currentlyRiding == TileBehavior.Ride.SplashMountain) 
			RideNonSpecialRide (ref nextSplashLocation, ref splashPosition, ref splashMountainPath, splashExitTileIndex);
		else if (_currentlyRiding == TileBehavior.Ride.SpaceMountain1) 
			RideNonSpecialRide (ref nextSpace1Location, ref space1Position, ref space1MountainPath, space1ExitTileIndex);
		else if (_currentlyRiding == TileBehavior.Ride.SpaceMountain2) 
			RideNonSpecialRide (ref nextSpace2Location, ref space2Position, ref space2MountainPath, space2ExitTileIndex);
		else if (_currentlyRiding == TileBehavior.Ride.TestTrack) 
			RideNonSpecialRide (ref nexttestTrackLocation, ref testTrackPosition, ref testTrackPath, testTrackExitTileIndex);
		else if (_currentlyRiding == TileBehavior.Ride.Figment) 
			RideNonSpecialRide (ref nextFigmentLocation, ref figmentPosition, ref figmentPath, figmentExitTileIndex);
		else 
		{
			if (!moveComplete) {
				moveComplete = true;
				EventDispatcher.DispatchEvent (GetCurrentTileEventData ());
			}
		}
	}
	private void RideNonSpecialRide(ref Vector3 nextRideLocation, ref int nextPosition, ref List<Transform> list, int exitTileIndex)
	{
		float dist = Vector3.Distance (_currentPlayer.transform.position, nextRideLocation);
		_currentPlayer.transform.position = Vector3.MoveTowards (_currentPlayer.transform.position, nextRideLocation, Time.deltaTime * speed);
		if (dist <= reachDistance) {
			nextPosition++;
			if (nextPosition >= list.Count) {
				_currentlyRiding = TileBehavior.Ride.Empty;
				nextPosition = 0;
				_currentPlayer.rollDestination = exitTileIndex;
				_currentPlayer.nextDestination = path [exitTileIndex].position;
				_currentPlayer.currentPoint = exitTileIndex - 1;
				_justRoadARide = true;
			} else {
				nextRideLocation = list [nextPosition].position;
			}
		}
	}
	private void onBeginRideEvent(IEvent e)
	{
		BeginRideEvent evt = (BeginRideEvent)e;
		_currentlyRiding = evt.ride;
		switch (evt.ride) 
		{
			case TileBehavior.Ride.ThunderMountain:
				nextBigThunderLocation = thunderMountainPath [0].position;
				break;
			case TileBehavior.Ride.SplashMountain:
				nextSplashLocation = splashMountainPath [0].position;
				break;
			case TileBehavior.Ride.SpaceMountain1:
				nextSpace1Location = space1MountainPath [0].position;
				break;
			case TileBehavior.Ride.SpaceMountain2:
				nextSpace2Location = space2MountainPath [0].position;
				break;
			case TileBehavior.Ride.TestTrack:
				nexttestTrackLocation = testTrackPath [0].position;
				break;
			case TileBehavior.Ride.Figment:
				nextFigmentLocation = figmentPath [0].position;
				break;
			default:
				break;
		}
	}
	private PlayerLandedOnTileEvent GetCurrentTileEventData()
	{
		TileBehavior tileData;
		if (_justRoadARide) 
		{
			_justRoadARide = false;
			// Weird hack for stopping the ride on the wrong tile.  Fix later.
			tileData = path [_currentPlayer.currentPoint].GetComponent<TileBehavior>();
			_currentPlayer.currentPoint++;
			_currentPlayer.rollDestination++;
		}
		else
			tileData = path [_currentPlayer.currentPoint-1].GetComponent<TileBehavior>();
		return new PlayerLandedOnTileEvent (tileData.tile, tileData.value,tileData.ride);
	}
	void OnDrawGizmos()
	{
		foreach (Transform child in thunderMountainPathContainer.transform)
		{
			Gizmos.DrawSphere (child.position, reachDistance);
		}
		foreach (Transform child in splashMountainPathContainer.transform)
		{
			Gizmos.DrawSphere (child.position, reachDistance);
		}
		foreach (Transform child in spaceMountainPathContainer1.transform)
		{
			Gizmos.DrawSphere (child.position, reachDistance);
		}
		foreach (Transform child in spaceMountainPathContainer2.transform)
		{
			Gizmos.DrawSphere (child.position, reachDistance);
		}
		foreach (Transform child in testTrackPathContainer.transform)
		{
			Gizmos.DrawSphere (child.position, reachDistance);
		}
		foreach (Transform child in figmentPathContainer.transform)
		{
			Gizmos.DrawSphere (child.position, reachDistance);
		}
	}
}

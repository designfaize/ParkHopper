using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ParkHopper.Events;
using ParkHopper;

public class PayToRideView : MonoBehaviour {
	[SerializeField]
	private Button rideButton;
	[SerializeField]
	private Button skipButton;
	[SerializeField]
	private GameObject payToRidePanel;

	private TileBehavior.Ride _ride;
	void Start()
	{
		EventDispatcher.AddListener<AskPayToRideEvent>(onAskPayToRideEvent);
	}
	void Awake()
	{
		rideButton.onClick.AddListener(onRideButtonClick);
		skipButton.onClick.AddListener(onSkipButtonClick);
	}
	void Destroy()
	{
		rideButton.onClick.RemoveListener(onRideButtonClick);
		skipButton.onClick.RemoveListener(onSkipButtonClick);
	}
	private void onRideButtonClick()
	{
		EventDispatcher.DispatchEvent (new PayToRideEvent (_ride));
		Hide ();
	}
	private void onSkipButtonClick()
	{
		EventDispatcher.DispatchEvent (new PayToRideSkipEvent ());
		Hide ();
	}
	private void onAskPayToRideEvent(IEvent e)
	{
		payToRidePanel.gameObject.SetActive (true);
		_ride = ((AskPayToRideEvent)e).ride;
	}
	private void Hide()
	{
		payToRidePanel.gameObject.SetActive (false);
	}
}

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
	[SerializeField]
	private Button fastPassButton;

	private TileBehavior.Ride _ride;
	void Start()
	{
		EventDispatcher.AddListener<AskPayToRideEvent>(onAskPayToRideEvent);
	}
	void Awake()
	{
		rideButton.onClick.AddListener(onRideButtonClick);
		skipButton.onClick.AddListener(onSkipButtonClick);
		fastPassButton.onClick.AddListener(onFPButtonClick);
	}
	void Destroy()
	{
		rideButton.onClick.RemoveListener(onRideButtonClick);
		skipButton.onClick.RemoveListener(onSkipButtonClick);
	}
	private void onFPButtonClick()
	{
		EventDispatcher.DispatchEvent (new UseFastpassEvent ());
		Hide ();
	}
	private void onRideButtonClick()
	{
		EventDispatcher.DispatchEvent (new PayToRideEvent ());
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
		if (((AskPayToRideEvent)e).canUseFastPass)
			fastPassButton.gameObject.SetActive (true);
		else
			fastPassButton.gameObject.SetActive (false);
		if (((AskPayToRideEvent)e).canUseCash)
			rideButton.gameObject.SetActive (true);
		else
			rideButton.gameObject.SetActive (false);
	}
	private void Hide()
	{
		payToRidePanel.gameObject.SetActive (false);
	}
}

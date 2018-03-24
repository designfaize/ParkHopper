using UnityEngine;
using UnityEngine.UI;
using ParkHopper.Events;
using TMPro;

public class UseSorcererCardView : MonoBehaviour {
	[SerializeField]
	private GameObject panel;
	[SerializeField]
	private Button useButton;
	[SerializeField]
	private Button declineButton;

	// Use this for initialization
	void Start () 
	{
		useButton.onClick.AddListener (onUseButtonClick);
		declineButton.onClick.AddListener (onSkipButtonClick);	
		EventDispatcher.AddListener<AskUseSorcererCardEvent> (onAskUseSorcererCardEvent);
	}
	private void onAskUseSorcererCardEvent(IEvent e)
	{
		panel.SetActive (true);
	}
	private void onUseButtonClick()
	{
		EventDispatcher.DispatchEvent (new UseSorcererCardEvent ());
		Hide ();
	}
	private void onSkipButtonClick()
	{
		EventDispatcher.DispatchEvent (new SkipUseSorcererCardEvent ());
		Hide ();
	}
	private void Hide()
	{
		panel.SetActive (false);
	}
}

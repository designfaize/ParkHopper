using UnityEngine;
using UnityEngine.UI;
using ParkHopper.Events;
using TMPro;
public class BuySorcererCardUI : MonoBehaviour {
	[SerializeField]
	private GameObject panel;
	[SerializeField]
	private Button buyButton;
	[SerializeField]
	private Button SkipButton;

	private int _cost;
	void Start () 
	{
		buyButton.onClick.AddListener (onBuyButtonClick);
		SkipButton.onClick.AddListener (onSkipButtonClick);
		EventDispatcher.AddListener<AskBuySorcererCardEvent> (onAskBuySorcererCardEvent);
	}
	private void onAskBuySorcererCardEvent(IEvent e)
	{
		panel.SetActive (true);
		_cost = ((AskBuySorcererCardEvent)e).cost;
	}
	 
	private void onBuyButtonClick()
	{
		EventDispatcher.DispatchEvent (new BuySorcererCardEvent (_cost));
		Hide ();
	}
	private void onSkipButtonClick()
	{
		EventDispatcher.DispatchEvent (new SkipBuySorcererCardEvent ());
		Hide ();
	}
	private void Hide()
	{
		panel.SetActive (false);
	}
}

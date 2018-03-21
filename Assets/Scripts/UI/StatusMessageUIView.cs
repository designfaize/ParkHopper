using UnityEngine;
using ParkHopper.Events;
using System.Collections;
using TMPro;

public class StatusMessageUIView : MonoBehaviour {
	[SerializeField]
	TextMeshProUGUI statusText;
	[SerializeField]
	private const float TIME_TO_SHOW_MESSAGE = 2f;

	// Use this for initialization
	void Start () {
		EventDispatcher.AddListener<ShowUIMessageEvent> (onShowUIMessage);
	}
	
	private void onShowUIMessage(IEvent e)
	{
		string message = ((ShowUIMessageEvent)e).message;
		StartCoroutine(ShowObject(TIME_TO_SHOW_MESSAGE, message));
	}

	private IEnumerator ShowObject(float timeInSeconds, string message)
	{
		statusText.SetText (message);
		yield return new WaitForSeconds(timeInSeconds);
		statusText.SetText (string.Empty);
	}
}

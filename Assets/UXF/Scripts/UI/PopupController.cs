using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UXF.UI
{
	public class PopupController : MonoBehaviour {

		public Text popupTitle;
		public Text popupMessage;

		Action nextAction;
		Action nextTrialContinue;
		Action nextBlockContinue;

		public void DisplayPopup(Popup popup)
		{
			popupTitle.text = popup.messageType.ToString();
			popupMessage.text = popup.message;
			nextAction = popup.onOK;
			nextTrialContinue = popup.onTrialContinue;
			nextBlockContinue = popup.onBlockContinue;
			gameObject.SetActive(true);
			transform.SetAsLastSibling();
		}

		public void OkPress()
		{
			gameObject.SetActive(false);
			nextAction.Invoke();
		}

		public void CancelPress()
		{
			gameObject.SetActive(false);
		}

		public void TrialContinue()
		{
			nextAction = nextTrialContinue;
			gameObject.SetActive(false);
			nextAction.Invoke();
		}

		public void BlockContinue()
		{
			nextAction = nextBlockContinue;
			gameObject.SetActive(false);
			nextAction.Invoke();
		}

		[ContextMenu("Test popup")]
		public void PopupTest()
		{
			Popup popup = new Popup();
			popup.messageType = MessageType.Attention;
			popup.message = "Testing popup!";
			popup.onOK = new Action(() => {});	
			DisplayPopup(popup);
		}

	}

	public struct Popup
	{
		public MessageType messageType;
		public string message;
		public Action onOK;
		public Action onTrialContinue;
		public Action onBlockContinue;
	}

	public enum MessageType
	{
		Attention, Warning, Error
	}

}
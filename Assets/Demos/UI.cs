using System.Collections;
using System.Collections.Generic;
using Engine;
using Engine.UI.Message;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Demos
{
	public class UI : MonoBehaviour
	{
		public MessageWindow MsgWindow;

		public void ShowMessage()
		{
			MessageWindow.Show(MsgWindow,
							   "Change BG color",
							   "Do you want to set the background color to blue?",
							   MessageType.Question, MessageButtons.YesNo,
							   yesAction: () => { Camera.main.backgroundColor = Color.blue; });
		}
	}
}

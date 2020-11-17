using System;

namespace Kit.UI.Message
{
	/// <summary>The <see cref="Window.Data" /> that gets passed to a <see cref="MessageWindow" />.</summary>
	public struct MessageInfo
	{
		/// <summary>Type of message to show – determines the icon.</summary>
		public MessageType Type;

		/// <summary>Title of the window.</summary>
		public string Title;

		/// <summary>Sub-title to go along with the title.</summary>
		public string Subtitle;

		/// <summary>The message to show.</summary>
		public string Message;

		/// <summary>Buttons to show with the message.</summary>
		public MessageButtons Buttons;

		/// <summary>What to do when the Okay button is pressed?</summary>
		public Action OkayAction;

		/// <summary>What to do when the Yes button is pressed?</summary>
		public Action YesAction;

		/// <summary>What to do when the No button is pressed?</summary>
		public Action NoAction;

		/// <summary>What to do when the Cancel button is pressed?</summary>
		public Action CancelAction;
	}

	/// <summary>Button configurations for a <see cref="MessageWindow" />.</summary>
	public enum MessageButtons
	{
		/// <summary>Just the OK button.</summary>
		OK,

		/// <summary>OK and Cancel buttons.</summary>
		OKCancel,

		/// <summary>Yes and No buttons.</summary>
		YesNo,

		/// <summary>Yes, No and Cancel buttons.</summary>
		YesNoCancel
	}

	/// <summary>The type of message in a <see cref="MessageWindow" />.</summary>
	public enum MessageType
	{
		/// <summary>
		/// The message is to alert the user.
		/// </summary>
		Alert,

		/// <summary>
		/// The message is to inform the user.
		/// </summary>
		Info,

		/// <summary>
		/// The message is to ask the user.
		/// </summary>
		Question
	}
}
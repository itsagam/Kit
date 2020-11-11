using System;

namespace Engine.UI.Message
{
	/// <summary>
	/// The <see cref="Window.Data"/> that gets passed to a <see cref="MessageWindow"/>.
	/// </summary>
	public struct MessageInfo
	{
		/// <summary>
		/// Type of message to show – determines the icon.
		/// </summary>
		public MessageType Type;

		/// <summary>
		/// Buttons to show with the message.
		/// </summary>
		public MessageButtons Buttons;

		/// <summary>
		/// Title of the window.
		/// </summary>
		public string Title;

		/// <summary>
		/// Sub-title to go along with the title.
		/// </summary>
		public string Subtitle;

		/// <summary>
		/// The message to show.
		/// </summary>
		public string Message;

		/// <summary>
		/// What to do when the Okay button is pressed?
		/// </summary>
		public Action OkayAction;

		/// <summary>
		/// What to do when the Yes button is pressed?
		/// </summary>
		public Action YesAction;

		/// <summary>
		/// What to do when the No button is pressed?
		/// </summary>
		public Action NoAction;

		/// <summary>
		/// What to do when the Cancel button is pressed?
		/// </summary>
		public Action CancelAction;
	}

	/// <summary>
	/// Button configurations for a <see cref="MessageWindow"/>.
	/// </summary>
	public enum MessageButtons
	{
		OK,
		OKCancel,
		YesNo,
		YesNoCancel
	}

	/// <summary>
	/// The type of message in a <see cref="MessageWindow"/>.
	/// </summary>
	public enum MessageType
	{
		Alert,
		Info,
		Question
	}
}
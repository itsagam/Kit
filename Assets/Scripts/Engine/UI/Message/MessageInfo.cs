using System;

public struct MessageInfo
{
	public MessageType Type;
	public MessageButtons Buttons;

	public string Title;
	public string Subtitle;
	public string Message;

	public Action OkayAction;
	public Action YesAction;
	public Action NoAction;
	public Action CancelAction;
}

public enum MessageButtons
{
	OK,
	OKCancel,
	YesNo,
	YesNoCancel
}

public enum MessageType
{
	Alert,
	Info,
	Question
}
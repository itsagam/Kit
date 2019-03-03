using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

public class MessageWindow : Window
{
	public Image IconImage;
	public Text TitleText;
	public GameObject SubtitleSeparator;
	public Text SubtitleText;
	public Text MessageText;
	public Button[] Buttons;
	public Text[] ButtonTexts;
	public Button CloseButton;
	public Sprite AlertSprite;
	public Sprite InfoSprite;
	public Sprite QuestionSprite;

	protected override void Awake()
	{
		base.Awake();
		Buttons[0].onClick.AddListener(OnButton1Clicked);
		Buttons[1].onClick.AddListener(OnButton2Clicked);
		Buttons[2].onClick.AddListener(OnButton3Clicked);
		CloseButton.onClick.AddListener(OnCloseClicked);
	}

	public override void Refresh()
	{
		if (data is string message)
			data = new MessageInfo() { Message = message };

		RefreshIcon();
		RefreshTexts();
		RefreshButtons();
	}

	protected void RefreshIcon()
	{
		switch (MessageInfo.Type)
		{
			case MessageType.Alert:
				IconImage.sprite = AlertSprite;
				break;

			case MessageType.Info:
				IconImage.sprite = InfoSprite;
				break;

			case MessageType.Question:
				IconImage.sprite = QuestionSprite;
				break;
		}
	}

	protected void RefreshTexts()
	{
		TitleText.text = MessageInfo.Title.IsNullOrWhiteSpace() ? Application.productName : MessageInfo.Title;
		SubtitleSeparator.gameObject.SetActive(!MessageInfo.Subtitle.IsNullOrWhiteSpace());
		SubtitleText.text = MessageInfo.Subtitle;
		MessageText.text = MessageInfo.Message;
	}

	protected void RefreshButtons()
	{
		Buttons[1].gameObject.SetActive(MessageInfo.Buttons != MessageButtons.OK);
		Buttons[2].gameObject.SetActive(MessageInfo.Buttons == MessageButtons.YesNoCancel);
		switch (MessageInfo.Buttons)
		{
			case MessageButtons.OK:
				ButtonTexts[0].text = "OK";
				break;

			case MessageButtons.OKCancel:
				ButtonTexts[0].text = "OK";
				ButtonTexts[1].text = "Cancel";
				break;

			case MessageButtons.YesNo:
				ButtonTexts[0].text = "Yes";
				ButtonTexts[1].text = "No";
				break;

			case MessageButtons.YesNoCancel:
				ButtonTexts[0].text = "Yes";
				ButtonTexts[1].text = "No";
				ButtonTexts[2].text = "Cancel";
				break;
		}
	}

	protected void OnButton1Clicked()
	{
		if (MessageInfo.Buttons == MessageButtons.OK || MessageInfo.Buttons == MessageButtons.OKCancel)
			MessageInfo.OkayAction?.Invoke();
		else
			MessageInfo.YesAction?.Invoke();
	}

	protected void OnButton2Clicked()
	{
		if (MessageInfo.Buttons == MessageButtons.OKCancel)
			MessageInfo.CancelAction?.Invoke();
		else
			MessageInfo.NoAction?.Invoke();
	}

	protected void OnButton3Clicked()
	{
		MessageInfo.CancelAction?.Invoke();
	}

	protected void OnCloseClicked()
	{
		MessageInfo.CancelAction?.Invoke();
	}

	public MessageInfo MessageInfo
	{
		get
		{
			return (MessageInfo) Data;
		}
	}
}
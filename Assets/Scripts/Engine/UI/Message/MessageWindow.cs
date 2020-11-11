using UnityEngine;
using UnityEngine.UI;

namespace Engine.UI.Message
{
	/// <summary>
	/// A pre-made Window for showing general game messages.
	/// </summary>
	public class MessageWindow: Window
	{
		/// <summary>
		/// The Image to use for showing the icon.
		/// </summary>
		[Tooltip("The Image to use for showing the icon.")]
		public Image IconImage;

		/// <summary>
		/// The Text to use for showing title.
		/// </summary>
		[Tooltip("The Text to use for showing title.")]
		public Text TitleText;

		/// <summary>
		/// The game object associated with sub-title (optional). Gets hidden if there isn't a subtitle in the message.
		/// </summary>
		[Tooltip("The game object associated with sub-title (optional). Gets hidden if there isn't a subtitle in the message.")]
		public GameObject SubtitleSeparator;

		/// <summary>
		/// The Text to use for showing sub-title (optional).
		/// </summary>
		[Tooltip("The Text to use for showing sub-title (optional).")]
		public Text SubtitleText;

		/// <summary>
		/// The Text to use for showing the message.
		/// </summary>
		[Tooltip("The Text to use for showing the message.")]
		public Text MessageText;

		/// <summary>
		/// References to the three buttons to use for message options.
		/// </summary>
		[Tooltip("References to the three buttons to use for message options.")]
		public Button[] Buttons;

		/// <summary>
		/// References to the three texts to that go with the buttons.
		/// </summary>
		[Tooltip("References to the three texts to that go with the buttons.")]
		public Text[] ButtonTexts;

		/// <summary>
		/// Reference to the button that closes the message window.
		/// </summary>
		[Tooltip("Reference to the button that closes the message window.")]
		public Button CloseButton;

		/// <summary>
		/// The sprite to use for alerts.
		/// </summary>
		[Tooltip("The sprite to use for alerts.")]
		public Sprite AlertSprite;

		/// <summary>
		/// The sprite to use for info-boxes.
		/// </summary>
		[Tooltip("The sprite to use for info-boxes.")]
		public Sprite InfoSprite;

		/// <summary>
		/// The sprite to use for questions.
		/// </summary>
		[Tooltip("The sprite to use for questions.")]
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

			if (SubtitleSeparator != null)
				SubtitleSeparator.gameObject.SetActive(!MessageInfo.Subtitle.IsNullOrWhiteSpace());

			if (SubtitleText != null)
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
			get => (MessageInfo) Data;
			set => Data = value;
		}

		public override object Data
		{
			get => data;
			set
			{
				switch (value)
				{
					case string message:
						data = new MessageInfo { Message = message };
						Refresh();
						break;

					case MessageInfo info:
						data = info;
						Refresh();
						break;

					default:
						data = null;
						break;
				}
			}
		}
	}
}
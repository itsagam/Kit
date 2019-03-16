using UnityEngine;
using UnityEngine.UI;

namespace Engine.UI.Widgets
{
	[RequireComponent(typeof(Button))]
	public class ToggleButton : MonoBehaviour
	{
		public Sprite OnSprite;
		public Sprite OffSprite;

		public Button Button { get; protected set; }

		[SerializeField]
		protected bool isOn;

		public Toggle.ToggleEvent ValueChanged;

		protected void Awake()
		{
			Button = GetComponent<Button>();
			Button.onClick.AddListener(Toggle);
			SetImage();
		}

		protected void Toggle()
		{
			IsOn = !IsOn;
		}

		protected void SetImage()
		{
			if (Button != null)
				Button.image.sprite = isOn ? OnSprite : OffSprite;
		}

		public bool IsOn
		{
			get => isOn;
			set
			{
				isOn = value;
				SetImage();
				ValueChanged.Invoke(isOn);
			}
		}
	}
}

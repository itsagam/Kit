using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Button))]
public class ToggleButton : MonoBehaviour
{
	public Sprite OnSprite;
	public Sprite OffSprite;

	public Button Button { get; protected set; }

	[SerializeField]
	protected bool isOn;

	public Toggle.ToggleEvent OnValueChanged;

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
		Button.image.sprite = isOn ? OnSprite : OffSprite;
	}
	
	public bool IsOn
	{
		get
		{
			return isOn;
		}
		set
		{
			isOn = value;
			SetImage();
			OnValueChanged.Invoke(isOn);
		}
	}
}

using UnityEngine;
using UnityEngine.U2D;
using Sirenix.OdinInspector;

namespace Cards
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class Card2D : MonoBehaviour
	{
		[OnValueChanged("UpdateFace")]
		public SpriteAtlas Atlas;

		[SerializeField]
		[HideInInspector]
		protected Card card;

		protected SpriteRenderer spriteRenderer;

		protected void Awake()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
			UpdateFace();
		}

		protected void UpdateFace()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			spriteRenderer.sprite = Atlas.GetSprite($"{card.Rank}_{card.Suit}");
		}
		
		public Card Card
		{
			get => card;
			set
			{
				card = value;
				UpdateFace();
			}
		}
		
		[ShowInInspector]
		public Suit Suit
		{
			get => card.Suit;
			set
			{
				card.Suit = value;
				UpdateFace();
			}
		}

		[ShowInInspector]
		public Rank Rank
		{
			get => card.Rank;
			set
			{
				card.Rank = value;
				UpdateFace();
			}
		}
	}
}
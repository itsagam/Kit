using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Cards
{
	[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
	public class Card3D : MonoBehaviour
	{
		public const float TileWidth = 0.7145f;
		public const float TileHeight = 1.00f;
		public const int TileFaceUVStart = 103;
		public const int TileFaceUVEnd = 135;
		protected static Vector2[] originalUV;

		[SerializeField]
		[HideInInspector]
		protected Card card;

		protected Mesh mesh;

		protected void Awake()
		{
			mesh = GetComponent<MeshFilter>().mesh;
			if (originalUV == null)
				originalUV = mesh.uv;
			UpdateFace();
		}
		
		protected void UpdateFace()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;				
#endif

			int suit = (int) card.Suit;
			int rank = (int) card.Rank;
			int column = (suit - 1) + (rank % 2 > 0 ? 4 : 0);
			int row = (rank -1) / 2;		
			float xPos = column * TileWidth;
			float yPos = row * TileHeight;

			Vector2[] uvs = mesh.uv;
			for (int i = TileFaceUVStart; i <= TileFaceUVEnd; i++)
			{
				uvs[i].x = originalUV[i].x + xPos;
				uvs[i].y = originalUV[i].y + yPos;
			}
			mesh.uv = uvs;
		}

		public Card Card
		{
			get
			{
				return card;
			}
			set
			{
				card = value;
				UpdateFace();
			}
		}

		[ShowInInspector]
		public Suit Suit
		{
			get
			{
				return card.Suit;
			}
			set
			{
				card.Suit = value;
				UpdateFace();
			}
		}

		[ShowInInspector]
		public Rank Rank
		{
			get
			{
				return card.Rank;
			}
			set
			{
				card.Rank = value;
				UpdateFace();
			}
		}
	}
}
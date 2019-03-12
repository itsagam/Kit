using System;
using UnityEngine;

namespace Engine.Algorithms
{
	public struct SphericalCoordinates: IEquatable<SphericalCoordinates>
	{
		private float radius, polar, elevation;
		private float minRadius, maxRadius;
		private float minPolar, maxPolar;
		private float minElevation, maxElevation;
		private bool loopPolar, loopElevation;

		public SphericalCoordinates(float radius, float polar, float elevation,
			float minRadius = 0, float maxRadius = float.PositiveInfinity,
			float minPolar = 0, float maxPolar = Mathf.PI * 2f,
			float minElevation = 0, float maxElevation = Mathf.PI * 2f,
			bool loopPolar = true, bool loopElevation = true)
		{
			this.radius = this.polar = this.elevation = 0;
			this.minRadius = minRadius;
			this.maxRadius = maxRadius;
			this.minPolar = minPolar;
			this.maxPolar = maxPolar;
			this.minElevation = minElevation;
			this.maxElevation = maxElevation;
			this.loopPolar = loopPolar;
			this.loopElevation = loopElevation;

			Radius = radius;
			Polar = polar;
			Elevation = elevation;
		}

		public SphericalCoordinates(Vector3 cartesian,
			float minRadius = 0, float maxRadius = float.PositiveInfinity,
			float minPolar = 0, float maxPolar = Mathf.PI * 2f,
			float minElevation = 0, float maxElevation = Mathf.PI * 2f,
			bool loopPolar = true, bool loopElevation = true)
		{
			radius = polar = elevation = 0;
			this.minRadius = minRadius;
			this.maxRadius = maxRadius;
			this.minPolar = minPolar;
			this.maxPolar = maxPolar;
			this.minElevation = minElevation;
			this.maxElevation = maxElevation;
			this.loopPolar = loopPolar;
			this.loopElevation = loopElevation;

			if (cartesian.x == 0)
				cartesian.x = Mathf.Epsilon;
			Radius = cartesian.magnitude;

			Polar = Mathf.Atan(cartesian.z / cartesian.x);

			if (cartesian.x < 0f)
				Polar += Mathf.PI;
			Elevation = Mathf.Asin(cartesian.y / Radius);
		}

		public float DistanceTo(SphericalCoordinates other)
		{
			return DistanceTo(other, Radius);
		}

		// Haversine Formula
		public float DistanceTo(SphericalCoordinates other, float radius)
		{
			float dLat = other.Elevation - Elevation;
			float dLon =  other.Polar - Polar;
			float a = Mathf.Pow(Mathf.Sin(dLat / 2), 2) +
				Mathf.Cos(Elevation) * Mathf.Cos(other.Elevation) * Mathf.Pow(Mathf.Sin(dLon / 2), 2);
			float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
			return radius * c;
		}

		public SphericalCoordinates Translate(float radius)
		{
			Radius += radius;
			return this;
		}

		public SphericalCoordinates Rotate(float polar, float elevation)
		{
			Polar += polar;
			Elevation += elevation;
			return this;
		}

		public Vector3 ToCartesian
		{
			get
			{
				float a = Radius * Mathf.Cos(Elevation);
				return new Vector3 (a * Mathf.Cos(Polar), Radius * Mathf.Sin(Elevation), a * Mathf.Sin(Polar));
			}
		}

		public float Radius
		{
			get => radius;
			set => radius = Mathf.Clamp(value, MinRadius, MaxRadius);
		}

		public float Polar
		{
			get => polar;
			set => polar = LoopPolar ? Mathf.Repeat(value, MaxPolar - MinPolar)
							   : Mathf.Clamp(value, MinPolar, MaxPolar);
		}

		public float Elevation
		{
			get => elevation;
			set => elevation = LoopElevation ? Mathf.Repeat(value, MaxElevation - MinElevation)
								   : Mathf.Clamp(value, MinElevation, MaxElevation);
		}

		public float MinRadius
		{
			get => minRadius;
			set
			{
				minRadius = value;
				Radius = Radius;
			}
		}

		public float MaxRadius
		{
			get => maxRadius;
			set
			{
				maxRadius = value;
				Radius = Radius;
			}
		}

		public float MinPolar
		{
			get => minPolar;
			set
			{
				minPolar = value;
				Polar = Polar;
			}
		}

		public float MaxPolar
		{
			get => maxPolar;
			set
			{
				maxPolar = value;
				Polar = Polar;
			}
		}

		public float MinElevation
		{
			get => minElevation;
			set
			{
				minElevation = value;
				Elevation = Elevation;
			}
		}

		public float MaxElevation
		{
			get => maxElevation;
			set
			{
				maxElevation = value;
				Elevation = Elevation;
			}
		}

		public bool LoopPolar
		{
			get => loopPolar;
			set
			{
				loopPolar = value;
				Polar = Polar;
			}
		}

		public bool LoopElevation
		{
			get => loopElevation;
			set
			{
				loopElevation = value;
				Elevation = Elevation;
			}
		}

		public static bool operator ==(SphericalCoordinates a, SphericalCoordinates b)
		{
			return a.Radius == b.Radius && a.Polar == b.Polar && a.Elevation == b.Elevation;
		}

		public static bool operator !=(SphericalCoordinates a, SphericalCoordinates b)
		{
			return a.Radius != b.Radius || a.Polar != b.Polar || a.Elevation == b.Elevation;
		}

		public override bool Equals(object obj)
		{
			return obj is SphericalCoordinates coord && this == coord;
		}

		public bool Equals(SphericalCoordinates other)
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			return Radius.GetHashCode() ^ (Polar.GetHashCode() << 2) ^ (Elevation.GetHashCode() >> 2);
		}

		public override string ToString()
		{
			return $"(R: {Radius}, P: {Polar}, E: {Elevation})";
		}
	}
}
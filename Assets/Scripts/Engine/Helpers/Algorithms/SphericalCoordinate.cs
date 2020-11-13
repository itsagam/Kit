using System;
using UnityEngine;

namespace Engine.Algorithms
{
	/// <summary>
	/// Represents a point on a sphere, and allows to do math on it.
	/// </summary>
	public struct SphericalCoordinate: IEquatable<SphericalCoordinate>
	{
		private float radius, polar, elevation;
		private float minRadius, maxRadius;
		private float minPolar, maxPolar;
		private float minElevation, maxElevation;
		private bool loopPolar, loopElevation;

		/// <summary>
		/// Create a spherical coordinate with the given radius, polar and elevation angles.
		/// </summary>
		public SphericalCoordinate(float radius, float polar, float elevation,
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

		/// <summary>
		/// Convert a cartesian-coordinate to spherical-coordinates.
		/// </summary>
		public SphericalCoordinate(Vector3 cartesian,
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

			Radius = cartesian.magnitude;
			Polar = Mathf.Atan(cartesian.z / cartesian.x);

			if (cartesian.x < 0f)
				Polar += Mathf.PI;
			Elevation = Mathf.Asin(cartesian.y / Radius);
		}

		/// <summary>
		/// Calculates the distance from a point on a sphere to another point on a sphere.
		/// </summary>
		/// <param name="other">The other point.</param>
		/// <returns>Distance between this and the other point.</returns>
		public float DistanceTo(SphericalCoordinate other)
		{
			return DistanceTo(other, Radius);
		}

		/// <summary>
		/// Calculates the distance from a point on a sphere to another point at a given radius.
		/// </summary>
		/// <remarks>Uses <see href="https://en.wikipedia.org/wiki/Haversine_formula">Haversine Formula</see>.</remarks>
		/// <param name="other">The other point.</param>
		/// <returns>Distance between this and the other point at the radius specified.</returns>
		public float DistanceTo(SphericalCoordinate other, float atRadius)
		{
			float dLat = other.Elevation - Elevation;
			float dLon =  other.Polar - Polar;
			float a = Mathf.Pow(Mathf.Sin(dLat / 2), 2) +
				Mathf.Cos(Elevation) * Mathf.Cos(other.Elevation) * Mathf.Pow(Mathf.Sin(dLon / 2), 2);
			float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
			return atRadius * c;
		}

		/// <summary>
		/// Moves the point inward or outward by a given radius.
		/// </summary>
		public SphericalCoordinate Translate(float byRadius)
		{
			Radius += byRadius;
			return this;
		}

		/// <summary>
		/// Moves the point in a given direction.
		/// </summary>
		public SphericalCoordinate Rotate(float byPolar /*haha!*/, float byElevation)
		{
			Polar += byPolar;
			Elevation += byElevation;
			return this;
		}

		/// <summary>
		/// Converts the spherical coordinates to cartesian coordinates.
		/// </summary>
		public Vector3 ToCartesian
		{
			get
			{
				float a = Radius * Mathf.Cos(Elevation);
				return new Vector3 (a * Mathf.Cos(Polar), Radius * Mathf.Sin(Elevation), a * Mathf.Sin(Polar));
			}
		}

		/// <summary>
		/// The radius of the coordinates (how much inward or outward are the coordinates from origin).
		/// </summary>
		public float Radius
		{
			get => radius;
			set => radius = Mathf.Clamp(value, MinRadius, MaxRadius);
		}

		/// <summary>
		/// The vertical angle of the coordinates (at what latitude do the coordinates lie on a sphere).
		/// </summary>
		public float Polar
		{
			get => polar;
			set => polar = LoopPolar ? Mathf.Repeat(value, MaxPolar - MinPolar)
							   : Mathf.Clamp(value, MinPolar, MaxPolar);
		}

		/// <summary>
		/// The horizontal angle of the coordinates (at what longitude do the coordinates lie on a sphere).
		/// </summary>
		public float Elevation
		{
			get => elevation;
			set => elevation = LoopElevation ? Mathf.Repeat(value, MaxElevation - MinElevation)
								   : Mathf.Clamp(value, MinElevation, MaxElevation);
		}

		/// <summary>
		/// Minimum radius.
		/// </summary>
		public float MinRadius
		{
			get => minRadius;
			set
			{
				minRadius = value;
				Radius = Radius;
			}
		}

		/// <summary>
		/// Maximum radius.
		/// </summary>
		public float MaxRadius
		{
			get => maxRadius;
			set
			{
				maxRadius = value;
				Radius = Radius;
			}
		}

		/// <summary>
		/// Minimum polar angle.
		/// </summary>
		/// <remarks>0 by default.</remarks>
		public float MinPolar
		{
			get => minPolar;
			set
			{
				minPolar = value;
				Polar = Polar;
			}
		}

		/// <summary>
		/// Maximum polar angle.
		/// </summary>
		/// <remarks>2π/360° by default.</remarks>
		public float MaxPolar
		{
			get => maxPolar;
			set
			{
				maxPolar = value;
				Polar = Polar;
			}
		}

		/// <summary>
		/// Minimum elevation angle.
		/// </summary>
		/// <remarks>0 by default.</remarks>
		public float MinElevation
		{
			get => minElevation;
			set
			{
				minElevation = value;
				Elevation = Elevation;
			}
		}

		/// <summary>
		/// Maximum elevation angle.
		/// </summary>
		/// <remarks>2π/360° by default.</remarks>
		public float MaxElevation
		{
			get => maxElevation;
			set
			{
				maxElevation = value;
				Elevation = Elevation;
			}
		}

		/// <summary>
		/// Should loop around the polar angle if it goes out of range?
		/// </summary>
		/// <remarks>True by default.</remarks>
		public bool LoopPolar
		{
			get => loopPolar;
			set
			{
				loopPolar = value;
				Polar = Polar;
			}
		}

		/// <summary>
		/// Should loop around the elevation angle if it goes out of range?
		/// </summary>
		/// <remarks>True by default.</remarks>
		public bool LoopElevation
		{
			get => loopElevation;
			set
			{
				loopElevation = value;
				Elevation = Elevation;
			}
		}

		public static bool operator ==(SphericalCoordinate a, SphericalCoordinate b)
		{
			return a.Radius == b.Radius && a.Polar == b.Polar && a.Elevation == b.Elevation;
		}

		public static bool operator !=(SphericalCoordinate a, SphericalCoordinate b)
		{
			return a.Radius != b.Radius || a.Polar != b.Polar || a.Elevation == b.Elevation;
		}

		public override bool Equals(object obj)
		{
			return obj is SphericalCoordinate coord && this == coord;
		}

		public bool Equals(SphericalCoordinate other)
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kit.Algorithms
{
	/// <summary>A generic implementation of the A-Star algorithm.</summary>
	public class AStar
	{
		/// <summary>Find the shortest path from start to destination.</summary>
		/// <param name="start">The initial node.</param>
		/// <param name="destination">The final node.</param>
		/// <param name="distance">A function that should return the distance between two nodes.</param>
		/// <param name="estimate">A function that should return an estimate between a node and the destination.</param>
		/// <param name="links">A function that should return all the nodes linked with a given one.</param>
		/// <typeparam name="T">The type of a node.</typeparam>
		public static Path<T> FindPath<T>(T start,
										  T destination,
										  Func<T, T, int> distance,
										  Func<T, int> estimate,
										  Func<T, IEnumerable<T>> links)
		{
			var closed = new HashSet<T>();
			var queue = new PriorityQueue<Path<T>>();
			queue.Enqueue(0, new Path<T>(start));
			while (!queue.IsEmpty)
			{
				var path = queue.Dequeue();
				if (closed.Contains(path.LastStep))
					continue;
				if (path.LastStep.Equals(destination))
					return path;
				closed.Add(path.LastStep);
				foreach (T n in links(path.LastStep))
				{
					int d = distance(path.LastStep, n);
					if (d >= int.MaxValue)
						continue;
					var newPath = path.AddStep(n, d);
					queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
				}
			}

			return null;
		}
	}

	/// <summary>
	///     <para>Represents a linked-list of nodes and a running total of cost.</para>
	///     <para>Result spited out by the <see cref="AStar" /> algorithm.</para>
	/// </summary>
	/// <typeparam name="T">The type of a node.</typeparam>
	public class Path<T>: IEnumerable<T>
	{
		/// <summary>Reference to the last node.</summary>
		public T LastStep { get; }

		/// <summary>A <see cref="Path{T}" /> to all the previous nodes.</summary>
		public Path<T> PreviousSteps { get; }

		/// <summary>Total cost of this <see cref="Path{T}" />.</summary>
		public int TotalCost { get; }

		private Path(T lastStep, Path<T> previousSteps, int totalCost)
		{
			LastStep = lastStep;
			PreviousSteps = previousSteps;
			TotalCost = totalCost;
		}

		public Path(T start): this(start, null, 0)
		{
		}

		public Path<T> AddStep(T step, int stepCost)
		{
			return new Path<T>(step, this, TotalCost + stepCost);
		}

		public IEnumerator<T> GetEnumerator()
		{
			for (var p = this; p != null; p = p.PreviousSteps)
				yield return p.LastStep;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public class PriorityQueue<T>
	{
		private SortedDictionary<int, Queue<T>> list = new SortedDictionary<int, Queue<T>>();

		public void Enqueue(int priority, T value)
		{
			if (!list.TryGetValue(priority, out var q))
			{
				q = new Queue<T>();
				list.Add(priority, q);
			}

			q.Enqueue(value);
		}

		public T Dequeue()
		{
			(int key, var value) = list.First();
			T v = value.Dequeue();
			if (value.Count == 0)
				list.Remove(key);
			return v;
		}

		public bool IsEmpty => !list.Any();
	}
}
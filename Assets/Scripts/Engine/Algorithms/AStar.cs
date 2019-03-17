using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Algorithms
{
	public class Path<T> : IEnumerable<T>
	{
		public T LastStep { get; }
		public Path<T> PreviousSteps { get; }
		public int TotalCost { get; }

		private Path(T lastStep, Path<T> previousSteps, int totalCost)
		{
			LastStep = lastStep;
			PreviousSteps = previousSteps;
			TotalCost = totalCost;
		}
		public Path(T start) : this(start, null, 0)
		{
		}
		public Path<T> AddStep(T step, int stepCost)
		{
			return new Path<T>(step, this, TotalCost + stepCost);
		}
		public IEnumerator<T> GetEnumerator()
		{
			for (Path<T> p = this; p != null; p = p.PreviousSteps)
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
			// will throw if there isn’t any first element!
			var pair = list.First();
			T v = pair.Value.Dequeue();
			if (pair.Value.Count == 0) // nothing left of the top priority.
				list.Remove(pair.Key);
			return v;
		}
		public bool IsEmpty => !list.Any();
	}

	public class AStar
	{
		public static Path<T> FindPath<T>(T start, T destination, Func<T, T, int> distance, Func<T, int> estimate, Func<T, IEnumerable<T>> links)
		{
			var closed = new HashSet<T>();
			var queue = new PriorityQueue<Path<T>>();
			queue.Enqueue(0, new Path<T>(start));
			while (!queue.IsEmpty)
			{
				Path<T> path = queue.Dequeue();
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
					Path<T> newPath = path.AddStep(n, d);
					queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
				}
			}
			return null;
		}
	}
}
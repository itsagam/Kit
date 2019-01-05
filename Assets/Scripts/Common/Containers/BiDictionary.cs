using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BiDictionary<TFirst, TSecond>
{
	protected Dictionary<TFirst, TSecond> firstToSecond = new Dictionary<TFirst, TSecond>();
	protected Dictionary<TSecond, TFirst> secondToFirst = new Dictionary<TSecond, TFirst>();

	public void Add(TFirst first, TSecond second)
	{
		firstToSecond[first] = second;
		secondToFirst[second] = first;
	}

	public void Remove(TFirst first)
	{
		firstToSecond.Remove(first);
		secondToFirst.Remove(secondToFirst.First(kvp => kvp.Value.Equals(first)).Key);
	}
		
	public void Remove(TSecond second)
	{
		secondToFirst.Remove(second);
		firstToSecond.Remove(firstToSecond.First(kvp => kvp.Value.Equals(second)).Key);
	}

	public TSecond this[TFirst first]
	{
		get
		{
			return firstToSecond[first];
		}
		set
		{
			Add(first, value);
		}
	}

	public TFirst this[TSecond second]
	{
		get
		{
			return secondToFirst[second];
		}
		set
		{
			Add(value, second);
		}
	}

	public override string ToString()
	{
		string ret = "";
		foreach (var item in firstToSecond)
			ret += item.ToString() + "\n";
		return ret;
	}
}
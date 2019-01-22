using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HTTP
{
	public class Request
	{
		public string Action;
		public Dictionary<string, string> Parameters;

		protected Action<Reply> onReply;
		protected Action onFailed;

		public Request(string action, Dictionary<string, string> parameters = null)
		{
			Action = action;
			Parameters = parameters;
		}

		public virtual Request OnReply(Action<Reply> onReply)
		{
			this.onReply = onReply;
			return this;
		}

		public virtual Request OnFailed(Action onFailed)
		{
			this.onFailed = onFailed;
			return this;
		}

		public void Succeed(Reply reply)
		{
			onReply?.Invoke(reply);
		}

		public void Fail()
		{
			onFailed?.Invoke();
		}

		public override string ToString()
		{
			string output = "{";
			
			if (Action != null)
				output += $"Action: {Action}";
			
			if (Parameters != null)
			{
				output += ", Parameters: [";
				foreach(KeyValuePair<string, string> pair in Parameters)
					output += $"{pair.Key}: {pair.Value}, ";
				output += "]";
			}

			output += "}";
			return output;
		}
	}
}
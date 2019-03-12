using System.Net;
using UnityEngine;

namespace Engine
{
	public class NetworkHelper
	{
		public static bool IsConnectedToInternet(bool quick = true)
		{
			if (quick)
				return Application.internetReachability != NetworkReachability.NotReachable;

			try
			{
				using (var client = new WebClient())
				using (client.OpenRead("http://www.google.com"))
					return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
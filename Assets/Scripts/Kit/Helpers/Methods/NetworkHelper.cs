using System.Net;
using UnityEngine;

namespace Kit
{
	/// <summary>Helper functions for connectivity.</summary>
	public class NetworkHelper
	{
		/// <summary>Returns whether the device is connected to the internet.</summary>
		/// <param name="quick">Should do a quick check or a thorough one?</param>
		public static bool IsConnectedToInternet(bool quick = true)
		{
			if (quick)
				return Application.internetReachability != NetworkReachability.NotReachable;

			try
			{
				using (WebClient client = new WebClient())
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
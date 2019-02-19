using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetworkHelper
{
	public static bool IsConnectedToInternet(bool quick = true)
	{
		if (quick)
		{
			return Application.internetReachability != NetworkReachability.NotReachable;
		}
		else
		{
			try
			{
				using (var client = new WebClient())
					using (var stream = client.OpenRead("http://www.google.com"))
						return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
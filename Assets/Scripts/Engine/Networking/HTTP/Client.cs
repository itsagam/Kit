using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UniRx.Async;

namespace HTTP
{
	public class Client : MonoBehaviour
	{
		public Uri BaseURI { get; protected set; }

		public void Configure(Settings settings)
		{
			Protocol protocol = settings.protocol != null ? settings.protocol.Value : Protocol.HTTP;
			UriBuilder builder = new UriBuilder(protocol.ToString().ToLower(), settings.hostName);
			if (settings.port != null)
				builder.Port = settings.port.Value;
			BaseURI = builder.Uri;
			Log("Configured", settings.ToString() + " - " + BaseURI.ToString());
		}

		public void Send(Request request)
		{
			SendAsync(request).Wait();
		}

		public async Task SendAsync(Request request)
		{
			if (BaseURI == null)
			{
				request.Fail();
				Log(new HTTP.Exception(HTTP.Exception.Type.NotConfigured, request.ToString()));
				return;
			}

			UriBuilder builder = new UriBuilder(BaseURI);
			builder.Path = request.Action.ToLower();
			string url = builder.ToString();

			UnityWebRequest uwr;
			if (request.Parameters != null)
			{
				WWWForm form = new WWWForm();
				foreach(KeyValuePair<string, string> pair in request.Parameters)
					form.AddField(pair.Key, pair.Value);
				uwr = UnityWebRequest.Post(url, form);
			}
			else
			{
				uwr = new UnityWebRequest(url);
			}
			await uwr.SendWebRequest();
			if (uwr.isHttpError || uwr.isNetworkError)
			{
				Reply reply = new Reply(uwr.downloadHandler.data);
				request.Succeed(reply);
				Log("Sent/Received", $"Request: {request.ToString()}\nReply: {reply.ToString()}"); 
			}
			else
			{
				request.Fail();
				Log(new HTTP.Exception(HTTP.Exception.Type.CouldNotSend, $"{uwr.error}\n{request.ToString()}"));
			}
		}

		public static void Log(HTTP.Exception exception)
		{
			Log(exception.ToString());
		}
		
		public static void Log(string tag, string data)
		{
			Log(tag + ":\n" + data);
		}
		
		public static void Log(string message)
		{
			Debug.Log("[HTTP] " + message + "\n");
		}

		public static bool IsConnectedToInternet(bool quick = false)
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
					{
						using (var stream = client.OpenRead("http://www.google.com"))
						{
							return true;
						}
					}
				}
				catch
				{
					return false;
				}
			}
		}
	}
}
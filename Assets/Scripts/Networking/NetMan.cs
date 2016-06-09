using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class NetMan : MonoBehaviour {

	private int myReliableChannelId;
	private string strSocketConnOpen = "Socket open. SocketID: ";
	private string strConnToServ = "Connected to server. ConnectionID: ";
	private string msg = "hello";

	public int maxConnections = 10;
	public int socketId;
	public int socketPort = 8888;
	public string ipAddress = "192.168.137.1";
	public int connectionId;
	public int bufferSize = 1024;

	

	// Use this for initialization
	void Start () 
	{
		NetworkTransport.Init ();
		ConnectionConfig config = new ConnectionConfig ();
		myReliableChannelId = config.AddChannel (QosType.Reliable);
		HostTopology topology = new HostTopology (config, maxConnections);
		socketId = NetworkTransport.AddHost (topology, socketPort);
		Debug.Log (strSocketConnOpen + socketId.ToString());
	}

	public void Connect() 
	{
		byte error;
		connectionId = NetworkTransport.Connect (socketId, ipAddress, socketPort, 0, out error);
		Debug.Log (strConnToServ + connectionId);
	}
	
	public void OnGUI()
	{
		GUILayout.BeginHorizontal ();
		ipAddress = GUILayout.TextField (ipAddress);
		GUILayout.Label("IP Address");
		GUILayout.EndHorizontal ();
		
		GUILayout.BeginHorizontal ();
		string tempPort;
		tempPort = GUILayout.TextField (socketPort.ToString ());
		socketPort = int.Parse (tempPort);
		GUILayout.Label ("Port");
		GUILayout.EndHorizontal ();

		if (GUILayout.Button ("Connect")) 
		{
			Debug.Log("Connecting....");
			Connect();
		}

		GUILayout.BeginHorizontal ();
		string tempMsg;
		tempMsg = GUILayout.TextField("Enter message here");
		msg = tempMsg;
		GUILayout.Label("Message");
		GUILayout.EndHorizontal ();

		if (GUILayout.Button ("Send Message")) 
		{
			Debug.Log("Sending message...");
			SendSocketMessage(msg);
		}

	}

	public void SendSocketMessage(string msg) 
	{
		byte error;
		byte[] buffer = new byte[bufferSize];
		Stream stream = new MemoryStream (buffer);
		BinaryFormatter formatter = new BinaryFormatter ();
		formatter.Serialize (stream, msg);

		NetworkTransport.Send (socketId, 
		                       connectionId, 
		                       myReliableChannelId, 
		                       buffer, 
		                       bufferSize, 
		                       out error);

	}

	// Update is called once per frame
	void Update () 
	{
		int recHostId;
		int recConnectionId;
		int recChannelId;
		byte[] recBuffer = new byte[bufferSize];
		int dataSize;
		byte error;
		NetworkEventType recNetworkEvent = NetworkTransport.Receive (out recHostId, 
		                                                            out recConnectionId, 
		                                                            out recChannelId, 
		                                                            recBuffer, 
		                                                            bufferSize, 
		                                                            out dataSize,
		                                                            out error);

		switch (recNetworkEvent) 
		{
			case NetworkEventType.Nothing:
				break;
			case NetworkEventType.ConnectEvent:
				Debug.Log("Incoming conenction event received.");
				break;
			case NetworkEventType.DataEvent:
				Stream stream = new MemoryStream(recBuffer);
				BinaryFormatter formatter = new BinaryFormatter();
				string message = formatter.Deserialize(stream) as string;
				Debug.Log("Incoming message event received: " + message);
				break;
			case NetworkEventType.DisconnectEvent:
				Debug.Log("Remote client event disconnected");
				break;
		}

	}
}










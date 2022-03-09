using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;

public class Server : MonoBehaviour {
    public NetworkDriver _networkDriver;
    private NativeList<NetworkConnection> _networkConnections;
    private InputHandler _inputHandler;

    void Start() {
        SetInputHandler();
        _inputHandler.SetTitle("TitleServer", "SERVER");
        _inputHandler.SetButtonLabel("SendToClient");

        ListenToPort();
    }

    private void ListenToPort() {
        //Listen to Port 9000
        _networkDriver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = 9000;
        //Bind Portnumber and internet protocol version
        if (_networkDriver.Bind(endpoint) != 0)
            Debug.Log("Server: failed to bind to port " + endpoint.Port);
        else
            //Start Listening
            _networkDriver.Listen();

        _networkConnections = new NativeList<NetworkConnection>(16, Allocator.Persistent); //NativeList resizable list with specific size and memory allocation
    }

    public void OnDestroy() {
        _networkDriver.Dispose();
        _networkConnections.Dispose();
    }

    void Update() {
        _networkDriver.ScheduleUpdate().Complete();

        // CleanUpConnections to remove stale connections
        for (int i = 0; i < _networkConnections.Length; i++) {
            if (!_networkConnections[i].IsCreated) {
                _networkConnections.RemoveAtSwapBack(i);
                --i;
            }
        }

        // AcceptNewConnections
        NetworkConnection networkConnection;
        while ((networkConnection = _networkDriver.Accept()) != default(NetworkConnection)) {
            _networkConnections.Add(networkConnection);
            Debug.Log("Server: accepted a connection");
        }

        //StreamReader
        DataStreamReader _streamReader;
        //Iterate the connections
        for (int i = 0; i < _networkConnections.Length; i++) {

            //StreamWriter
            if (_inputHandler._IsClicked) {
                FixedString128Bytes _writeText = _inputHandler.GetMessage();
                _networkDriver.BeginSend(NetworkPipeline.Null, _networkConnections[i], out var _streamWriter);
                _streamWriter.WriteFixedString128(_writeText);
                _networkDriver.EndSend(_streamWriter);
                _inputHandler._inputField.text = "";
                _inputHandler._IsClicked = false;
            }

            //Iterate next event for connection
            NetworkEvent.Type _networkEventType;
            while ((_networkEventType = _networkDriver.PopEventForConnection(_networkConnections[i], out _streamReader)) != NetworkEvent.Type.Empty) {

                //Network Event Receive Data
                if (_networkEventType == NetworkEvent.Type.Data) {
                    FixedString128Bytes _readText = _streamReader.ReadFixedString128();
                    _inputHandler.SetOutputField(_readText);

                    //Network Event Disconnected
                } else if (_networkEventType == NetworkEvent.Type.Disconnect) {
                    Debug.Log("Server: client disconnected from server");
                    _networkConnections[i] = default(NetworkConnection);
                }
            }
        }
    }

    private void SetInputHandler() {
        _inputHandler = GameObject.Find("ServerInput").GetComponent<InputHandler>();
    }
}
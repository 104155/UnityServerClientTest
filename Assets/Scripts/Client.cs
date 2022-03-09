using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;

public class Client : MonoBehaviour {
    public NetworkDriver _networkDriver;
    public NetworkConnection _networkConnection;
    public bool _isDone;
    private InputHandler _inputHandler;

    void Start() {
        SetInputHandler();
        _inputHandler.SetTitle("TitleClient", "CLIENT");
        _inputHandler.SetButtonLabel("SendToServer");

        ListenToPort();
    }

    private void ListenToPort() {
        _networkDriver = NetworkDriver.Create();
        _networkConnection = default(NetworkConnection);

        var _endpoint = NetworkEndPoint.LoopbackIpv4;
        _endpoint.Port = 9000;
        _networkConnection = _networkDriver.Connect(_endpoint);
    }

    public void OnDestroy() {
        _networkDriver.Dispose();
    }

    void Update() {
        _networkDriver.ScheduleUpdate().Complete();

        if (!_networkConnection.IsCreated) {
            if (!_isDone)
                Debug.Log("Client: something went wrong during connect");
            return;
        }

        //StreamWriter
        if (_inputHandler._IsClicked) {
            FixedString128Bytes _writeText = _inputHandler.GetMessage();
            _networkDriver.BeginSend(_networkConnection, out var _streamWriter);
            _streamWriter.WriteFixedString128(_writeText);
            _networkDriver.EndSend(_streamWriter);
            _inputHandler._inputField.text = "";
            _inputHandler._IsClicked = false;
        }

        //Iterate next event for connection
        DataStreamReader _streamReader;
        NetworkEvent.Type _networkEventType;
        while ((_networkEventType = _networkConnection.PopEvent(_networkDriver, out _streamReader)) != NetworkEvent.Type.Empty) {

            //Network Event Connected
            if (_networkEventType == NetworkEvent.Type.Connect) {
                Debug.Log("Client: connected to the server");

                //Network Event Received Data
            } else if (_networkEventType == NetworkEvent.Type.Data) {
                FixedString128Bytes _text = _streamReader.ReadFixedString128();
                _inputHandler.SetOutputField(_text);
                _isDone = true;

                //Networkevent Disconnected
            } else if (_networkEventType == NetworkEvent.Type.Disconnect) {
                Debug.Log("Client: got disconnected from server");
                _networkConnection = default(NetworkConnection);
            }
        }
    }
    private void SetInputHandler() {
        _inputHandler = GameObject.Find("ClientInput").GetComponent<InputHandler>();
    }

}
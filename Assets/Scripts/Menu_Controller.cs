using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class Menu_Controller : MonoBehaviour
{
    public static Menu_Controller Instance;
    public Connection.ConnectionRole ActualMenuRole = Connection.ConnectionRole.Idle;
    public enum Card
    {
        SelectRole,
        Server,
        ServerSettings,
        Client,
        ClientSettings,
        Video
    }
    public Card ActualCard = Card.SelectRole;
    public List<GameObject> CardsGameobjects = new List<GameObject>();
    private void Change_Card(Card ChangeToThis)
    {
        
        ActualCard = ChangeToThis;
        for (int i= 0;i<CardsGameobjects.Count;i++)
        {
            if (i == (int)ChangeToThis)
            {
                CardsGameobjects[i].SetActive(true);
            }
            else { 
                CardsGameobjects[i].SetActive(false);
            }
        }
    }

    //Additional
    public TMP_InputField Client_Ip;    
    public TMP_InputField Client_Port;
    public TMP_InputField Server_link; 
    public TMP_InputField Server_Port;

    public TextMeshProUGUI Client_InfoIpPort;
    public TextMeshProUGUI Server_InfoPort;

    #region 1
    public void SelectRole_Button_Server()
    {
        Connection.Start_Connection(Connection.ConnectionRole.Server);
        Change_Card(Card.Server);
        ActualMenuRole = Connection.ConnectionRole.Server;
    }
    public void SelectRole_Button_Client()
    {
        Change_Card(Card.Client);
        ActualMenuRole = Connection.ConnectionRole.Client;
        Client_InfoIpPort.text = "Ip:port\r\n" + Connection.IpServ + ":" + Connection.Port;
    }
    #endregion 1

    #region 2 Serv
    public void Button_Back()
    {
        Connection.Stop_Connection();
        Change_Card(Card.SelectRole);
        ActualMenuRole = Connection.ConnectionRole.Idle;
    }
    public void Server_Button_Settings()
    {
        Change_Card(Card.ServerSettings);
        //Reading port
        Server_Port.text = Connection.Port.ToString();
    }
    public void Server_Button_Ok()
    {
        Change_Card(Card.Server);
        //Saving port
        int newPort;
        if (!Int32.TryParse(Server_Port.text, out newPort))
        { 
            Debug.LogWarning("Forbidden port. Setting port to 7777."); 
            newPort = 7777; 
        }
        Connection.Port = newPort;
        //Reading menu port
        Server_InfoPort.text = "Port: " + Server_Port.text;
    }
    public void Server_Button_Cancel()
    {
        Change_Card(Card.Server);
    }
    public void Server_Button_Start()
    {
        Change_Card(Card.Video);
        Interpreter.ReceiveBool(Interpreter.DataKeys.LinkRequest); //Setting message as received
        Video_Controller.VideoLink = Server_link.text;
        Interpreter.Send(Interpreter.DataKeys.LinkString, Server_link.text);
        Video_Controller.OpenVideoPlayer();
    }
    #endregion 2 Serv

    #region 2 Client
    public void Client_Button_Settings()
    {
        Change_Card(Card.ClientSettings);
        //Reading ip and port
        Client_Ip.text = Connection.IpServ;
        Client_Port.text = Connection.Port.ToString();
    }
    public void Client_Button_Ok()
    {
        Change_Card(Card.Client);
        //Saving ip and port
        Connection.IpServ = Client_Ip.text;
        int newPort;
        if (!Int32.TryParse(Client_Port.text, out newPort))
        {
            Debug.LogWarning("Forbidden port. Setting port to 7777.");
            newPort = 7777;
        }
        Connection.Port = newPort;
        //Reading menu ip and port
        Client_InfoIpPort.text = "Ip:port\r\n" + Client_Ip.text + ":" + Client_Port.text;
    }
    public void Client_Button_Cancel()
    {
        Change_Card(Card.Client);
    }
    public void Client_Button_Connect()
    {
        if (!Connection.isConnected)
        {
            Debug.Log("Connecting as client...");
            Connection.Start_Connection(Connection.ConnectionRole.Client);
        }
        _SendLinkRequest = true;
    }

    private bool _SendLinkRequest = false;
    private void FixedUpdate()
    {
        if (_SendLinkRequest&&Connection.isConnected)
        {
            Interpreter.Send(Interpreter.DataKeys.LinkRequest, true);
            _SendLinkRequest = false;
        }
        if(Interpreter.IsNewData(Interpreter.DataKeys.LinkString))
        {
            Change_Card(Card.Video);
            Video_Controller.VideoLink = Interpreter.ReceiveString(Interpreter.DataKeys.LinkString);
            Video_Controller.OpenVideoPlayer();
        }
    }
    #endregion 2 Client

    #region 4 Video
    public void Video_Button_Close()
    {
        if (ActualMenuRole == Connection.ConnectionRole.Server)
            Change_Card(Card.Server);
        else if (ActualMenuRole == Connection.ConnectionRole.Client) Change_Card(Card.Client);
    }
    #endregion 4 Video

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else Destroy(this);
    }

}

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; //pointer

public class Video_Controller : MonoBehaviour
{
    public static Video_Controller Instance;

    public VideoPlayer Player;
    public static string VideoLink = "";

    //Linked objects
    public GameObject ExBar;
    public TMP_InputField LinkText;
    public TextMeshProUGUI VideoTime;
    public TextMeshProUGUI OffsetTime;

    //Video loading
    #region Video Loading
    double _VideoDuraction = 0;
    ulong _VideoFramesCount = 0;
    public static ulong VideoFramesCount{
        get
        {
            return Instance._VideoFramesCount;
        }
    }
    string _VideoDuractionString = "";
    bool _isWaitingForVideo = false;
    public static void OpenVideoPlayer()
    {
        //Setting link
        Instance.Player.url = VideoLink;
        //Waiting for Video to be loaded
        Instance._isWaitingForVideo = true;
        Instance.Player.Play();
    }
    private void CheckForTheVideoLoaded()
    {
        if (_isWaitingForVideo == true && Player.frame > 10)
        {
            _isWaitingForVideo = false;
            //Getting video duraction
            _VideoFramesCount = Player.frameCount;
            _VideoDuraction = Player.length;
            _VideoDuractionString = _SecondsToString(_VideoDuraction);
            Debug.Log("Video duraction: " + _SecondsToString(_VideoDuraction));
        }
    }
    #endregion Video Loading

    //Video additional functions
    #region Video additional functions
    private void _UpdateVideoTime()
    {
        VideoTime.text = _SecondsToString(Player.time) + " / " + _VideoDuractionString;
    }
    private void _Keyboard_Buttons_Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Button_Pause_Resume();
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            Button_Minus_Ten();
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            Button_Plus_Ten();
        if (Input.GetKeyDown(KeyCode.R)) //Ping
            Interpreter.Send(Interpreter.DataKeys.MoveToFrame, (int)Player.frame);
        //Mouse
        if (Input.GetMouseButtonDown(0) && !_Is_Pointer_Over_UI())
            Button_Pause_Resume();
    }
    private float _OffsetCheckTimer = 0;
    private float _TimeToAutoSync = 0;
    private int _SavedOffset=0;
    private void _CheckForOffset()
    {
        _OffsetCheckTimer += Time.fixedDeltaTime;
        if (_OffsetCheckTimer > 1 && Player.isPrepared) //Every sec
        {
            _OffsetCheckTimer = 0;
            Interpreter.Send(Interpreter.DataKeys.OffsetPing, (int)Player.frame);
        }
    }
    private void _UpdateReceiveOffset()
    {
        if(Interpreter.IsNewData(Interpreter.DataKeys.OffsetPing))
            _SavedOffset = Interpreter.ReceiveInt(Interpreter.DataKeys.OffsetPing);
            if (Player.frameRate > 0)
            {
            int Diff = (int)Player.frame - _SavedOffset;
                float off = Diff / Player.frameRate;
                OffsetTime.text = "~" + string.Format("{0:0.0}", off);
                if (Connection.Role == Connection.ConnectionRole.Server && off > 0.5f)
                {
                    _TimeToAutoSync += Time.fixedDeltaTime;
                    if (_TimeToAutoSync > 2)
                    {
                        Debug.Log("Auto update");
                        _TimeToAutoSync = 0;
                        //Auto sync
                        Interpreter.Send(Interpreter.DataKeys.MoveToFrame, (int)Player.frame);
                    }
                }
                else _TimeToAutoSync = 0;
            }
        
    }
    #endregion Video additional functions

    //Ejection bar
    #region Ejection bar
    private readonly float[] _BarPosY = { 35, -36 }; // Ejected, Injected
    private bool _isEjected = false;
    private float _NotMovingTime = 0;
    private readonly float _MaxNotMovingTime = 1.2f;
    private bool _isEjecting = false;
    private Vector2 _MousePos = Vector2.zero;
    private void _ChangeEjectionOfTheBar(bool val)
    {
        if (!_isEjecting && _isEjected != val)
        {
            _isEjecting = true;
            Cursor.visible = !_isEjected;//makes cursor invisible
            _isEjected = val;
            LeanTween.value(_BarPosY[_isEjected ? 1 : 0], _BarPosY[_isEjected ? 0 : 1], 0.3f).setOnUpdate((float pos) =>
            {
                ExBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(ExBar.GetComponent<RectTransform>().anchoredPosition.x, pos);
            }).setOnComplete(() => { _isEjecting = false; }).setEaseInOutCubic();
        }

    }
    private void _UpdateMousePos()
    {
        if ((_MousePos != (Vector2)Input.mousePosition && _Is_Pointer_Over_Window()) || _Is_Pointer_Over_UI())
        {
            //Mouse was moved
            _ChangeEjectionOfTheBar(true);
            _NotMovingTime = 0;
        }
        else
        {
            //Mouse didn't move
            _NotMovingTime += Time.fixedDeltaTime;
            if (_NotMovingTime > _MaxNotMovingTime)
            {
                _ChangeEjectionOfTheBar(false);
            }
        }
        _MousePos = Input.mousePosition;
    }
    #endregion Ejection bar

    //Buttons
    #region Buttons
    //Pause, Resume
    public Sprite[] PauseResumeSprite = new Sprite[2]; //0 ->Pause  1 -> Resume
    public Image PauseResumeIconSprite;
    private readonly float _PauseResumeScale = 2f;
    public void Button_Pause_Resume()
    {
        if (Player.isPaused)//Resume
        {
            Interpreter.Send(Interpreter.DataKeys.Resume,(int)Player.frame);
            Player.Play();
        }
        else//Pause
        {
            Interpreter.Send(Interpreter.DataKeys.Pause, (int)Player.frame);
            Player.Pause();
        }
        _Pause_Resume_Animaton();
    }
    private void _Pause_Resume_Animaton()
    {
        LeanTween.cancel(PauseResumeIconSprite.gameObject);
        //Show icon
        PauseResumeIconSprite.gameObject.SetActive(true);
        PauseResumeIconSprite.sprite = PauseResumeSprite[Player.isPaused ? 0 : 1];
        PauseResumeIconSprite.GetComponent<RectTransform>().localScale = new Vector3(1, 1, PauseResumeIconSprite.GetComponent<RectTransform>().localScale.z);
        LeanTween.value(PauseResumeIconSprite.gameObject, 0.3f, 1, 0.6f).setOnUpdate((float val) =>
        {
            PauseResumeIconSprite.color = new Color(1, 1, 1, 1 - val);
            PauseResumeIconSprite.GetComponent<RectTransform>().localScale = new Vector3(1f + val * (_PauseResumeScale - 1), 1f + val * (_PauseResumeScale - 1), PauseResumeIconSprite.GetComponent<RectTransform>().localScale.z);
        }).setEaseInOutCubic().setOnComplete(() =>
        {
            PauseResumeIconSprite.gameObject.SetActive(false);
        });
    }
    public void Button_Plus_Ten()
    {
        //Calculating time
        int newFrame = (int)Player.frame + (int)(Player.frameRate * 10);
        Player.frame = newFrame;
        Interpreter.Send(Interpreter.DataKeys.MoveToFrame, newFrame);
        //After changed fix
        ExtendingBar.AfterChanged = 30;
    } //virtual
    public void Button_Minus_Ten()
    {
        //Calculating frame
        int newFrame = (int)Player.frame - (int)(Player.frameRate * 10);
        Player.frame = newFrame;
        Interpreter.Send(Interpreter.DataKeys.MoveToFrame, newFrame);
        //After changed fix
        ExtendingBar.AfterChanged = 30;
    } //virtual
    #endregion Buttons

    //Additional functions
    #region Additional functions
    private string _SecondsToString(double seconds)
    {
        TimeSpan t = TimeSpan.FromSeconds(seconds);
        if (t.Hours > 0)
            return string.Format("{0:D2}:{1:D2}:{2:D2}",
                            t.Hours,
                            t.Minutes,
                            t.Seconds);
        else
            return string.Format("{0:D2}:{1:D2}",
                            t.Minutes,
                            t.Seconds);
    }
    private bool _Is_Pointer_Over_UI()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
    private bool _Is_Pointer_Over_Window()
    {
        var view = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        return view.x > 0 && view.x < 1 && view.y > 0 && view.y < 1;
    }
    #endregion Additional functions

    private void _UpdateNewMessages()
    {
        if(Connection.Role == Connection.ConnectionRole.Server)
        {
            if(Interpreter.IsNewData(Interpreter.DataKeys.LinkRequest)&&Interpreter.ReceiveBool(Interpreter.DataKeys.LinkRequest))
            {
                //Sending Link
                Debug.Log("Client asked for link");
                Interpreter.Send(Interpreter.DataKeys.LinkString, VideoLink);
            }
        }
        if (Interpreter.IsNewData(Interpreter.DataKeys.MoveToFrame))
        {
            Player.frame = Player.frame = Interpreter.ReceiveInt(Interpreter.DataKeys.MoveToFrame);
        }
        if (Interpreter.IsNewData(Interpreter.DataKeys.Pause))
        {
            Player.frame = Interpreter.ReceiveInt(Interpreter.DataKeys.Pause);
            Player.Pause();
            _Pause_Resume_Animaton();
        }
        if (Interpreter.IsNewData(Interpreter.DataKeys.Resume))
        {
            Player.frame = Interpreter.ReceiveInt(Interpreter.DataKeys.Resume);
            Player.Play();
            _Pause_Resume_Animaton();
        }
    }

    private void FixedUpdate()
    {
        _UpdateNewMessages();
        CheckForTheVideoLoaded();
        _UpdateVideoTime();
        _UpdateMousePos();

        _CheckForOffset();
        _UpdateReceiveOffset();
    }
    private void Update()
    {
        _Keyboard_Buttons_Update();
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(this);
    }
}

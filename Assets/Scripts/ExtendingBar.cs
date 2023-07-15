using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExtendingBar : MonoBehaviour
{
    public static ExtendingBar Instance;

    //Volume
    public Sprite[] VolumeSprite = new Sprite[2]; // 0 -> Normal  1 -> Muted
    public Image VolumeIconSprite;
    //Slider
    public Slider VideoSlider;
    private int _AfterChanged = 0;
    public static int AfterChanged
    { 
        set { 
            Instance._AfterChanged = value; 
        } 
    }
    //Max Min
    public Sprite[] MaxMinSprite = new Sprite[2]; // 0 -> Max. 1 -> Min.
    public Image MaxMinButtonSprite;
    //Back to menu
    public GameObject BackToMenuPanel;

    //Volume slider
    #region Volume slider
    public void Slider_Volume(Slider slider)
    {
        if (slider.value < 0.01f)
        {
            //muted
            VolumeIconSprite.sprite = VolumeSprite[1];
            Video_Controller.Instance.Player.SetDirectAudioVolume(0, 0);
            return;
        }
        //Unmute
        Video_Controller.Instance.Player.SetDirectAudioMute(0, false); //Change state
        VolumeIconSprite.sprite = VolumeSprite[0];
        Video_Controller.Instance.Player.SetDirectAudioVolume(0, slider.value);
        return;
    }
    #endregion Volume slider

    //Video slider
    #region Video slider
    public void Slider_Video(Slider slider)
    {
        if (Input.GetMouseButton(0))
        {
            //After changed fix
            _AfterChanged = 100;
            //Calculating frame
            int newFrame = (int)(slider.value * Video_Controller.VideoFramesCount);
            Interpreter.Send(Interpreter.DataKeys.MoveToFrame, newFrame);
            Video_Controller.Instance.Player.frame = newFrame;
        }
    }
    private void _Update_Video_Slider()
    {
        if (!Input.GetMouseButton(0))
        {
            if (_AfterChanged <= 0 && Video_Controller.VideoFramesCount != 0)
                VideoSlider.value = (float)Video_Controller.Instance.Player.frame / (float)Video_Controller.VideoFramesCount;
            else _AfterChanged--;
        }
    }
    #endregion Video slider

    //Buttons
    #region Buttons
    public void Button_Back_To_Menu()
    {
        BackToMenuPanel.SetActive(!BackToMenuPanel.activeSelf);
    }
    public void Button_Back_From_Video() //Quiting videoplayer
    {
        Menu_Controller.Instance.Video_Button_Close();
        Video_Controller.Instance.LinkText.transform.parent.gameObject.SetActive(false);
        BackToMenuPanel.SetActive(false);
    }
    public void Button_Show_Link()
    {
        Video_Controller.Instance.LinkText.text = Video_Controller.VideoLink;
        Video_Controller.Instance.LinkText.transform.parent.gameObject.SetActive(!Video_Controller.Instance.LinkText.transform.parent.gameObject.activeSelf);
    }
    public void Button_Mute()
    {
        Video_Controller.Instance.Player.SetDirectAudioMute(0, !Video_Controller.Instance.Player.GetDirectAudioMute(0)); //Change state
        VolumeIconSprite.sprite = VolumeSprite[Video_Controller.Instance.Player.GetDirectAudioMute(0) || (Video_Controller.Instance.Player.GetDirectAudioVolume(0) < 0.01f) ? 1 : 0];
    }
    public void Button_Maximize_Minimize()
    {
        //Fullscreen
        System_Controller.FullScreenToggle();
        //Changing Icon
        MaxMinButtonSprite.sprite = MaxMinSprite[Screen.fullScreen ? 0 : 1];
    }
    public void Button_Apply_Link()
    {
        Video_Controller.VideoLink = Video_Controller.Instance.LinkText.text;
        Video_Controller.Instance.LinkText.transform.parent.gameObject.SetActive(false);
        //Reloading Video
        Video_Controller.OpenVideoPlayer();
    }
    #endregion Buttons

    private void FixedUpdate()
    {
        _Update_Video_Slider();
    }
    private void OnEnable()
    {
        MaxMinButtonSprite.sprite = MaxMinSprite[Screen.fullScreen ? 1 : 0];
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

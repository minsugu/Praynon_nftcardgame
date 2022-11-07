using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject player;

    /*
    public enum PlayerState
    {
        Idle,
        Playing,
        Die,
    }
    PlayerState _state = PlayerState.Idle;*/

    void Start()
    {
        /*
        Managers.Input.MouseAction -= OnMouseClicked;
        Managers.Input.MouseAction += OnMouseClicked;
        Managers.Input.KeyAction -= OnKeyboard;
        Managers.Input.KeyAction += OnKeyboard;*/
    }

    void Update()
    {/*
        switch (_state)
        {
            case PlayerState.Idle:
                UpdateIdle();
                break;
            case PlayerState.Playing:
                UpdatePlaying();
                break;
            case PlayerState.Die:
                UpdateDie();
                break;
        }*/

    }

    void UpdateDie()
    {

    }

    void UpdateIdle()
    {

    }

    void UpdatePlaying()
    {

    }

    void OnMouseClicked(Define.MouseEvent evt)
    {

    }

    void OnKeyboard()
    {

    }


}

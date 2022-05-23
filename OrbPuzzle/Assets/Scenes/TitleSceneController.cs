using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleSceneController : MonoBehaviour
{
    [SerializeField] FadeInOut _fade;
    [SerializeField] Text _start;
    [SerializeField] Text _exit;

    enum SELECTICON
    {
        START,
        EXIT
    }
    SELECTICON _selectIcon;

    // Start is called before the first frame update
    void Start()
    {
        _selectIcon = SELECTICON.START;
        _start.color = Color.red;
        _exit.color = Color.white;
    }

    public void SetSelectInput(int input)
    {
        if (input == 1) { _selectIcon = SELECTICON.START; }
        if (input == -1) { _selectIcon = SELECTICON.EXIT; }

        switch(_selectIcon)
        {
            case SELECTICON.START:
                _start.color = Color.red;
                _exit.color = Color.white;
                break;
            case SELECTICON.EXIT:
                _start.color = Color.white;
                _exit.color = Color.red;
                break;
        }
    }

    public void SetSubmitInput()
    {
        switch(_selectIcon)
        {
            case SELECTICON.START:
                _fade.StartFadeOut();
                break;
            case SELECTICON.EXIT:
                Application.Quit();
                break;
        }
    }
}

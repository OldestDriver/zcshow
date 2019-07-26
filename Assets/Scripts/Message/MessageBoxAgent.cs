using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageBoxAgent : MonoBehaviour
{
    [SerializeField] Text _message;
    [SerializeField] float durtime = 2f;

    private bool _show = false;
    private bool _showTemp = false;

    private bool showMessage = false;


    public void UpdateMessage(string message) {
        if (showMessage)
        {
            gameObject.SetActive(true);
            _message.text = message;
        }
    }

    public void UpdateMessageTemp(string message)
    {
        if (showMessage) {
            _showTemp = true;
            gameObject.SetActive(true);
            _message.text = message;

            StartCoroutine(ShowMessage());
        }
    }

    public void Close() {
        if (!_showTemp) {
            gameObject.SetActive(false);
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.M)) {
            showMessage = !showMessage;

            if (showMessage)
            {
                UpdateMessageTemp("消息管理器打开!");
            }
            else {
                gameObject.SetActive(false);
            }


        }
    }


    IEnumerator ShowMessage()
    {
        yield return new WaitForSeconds(durtime);
        _showTemp = false;
        gameObject.SetActive(false);
    }


}

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Message
{
    public string text;
    public float showMessageTime;

    public Message(string _text,float _showMessageTime)
    {
        text = _text;
        showMessageTime = _showMessageTime;
    }
}

public class MessageQueue : UI_Base
{
    public IEnumerator corShowMessage;

    volatile int _locked = 0;
    Queue<Message> messageQueue = new Queue<Message>();

    enum Texts
    {
        SignQueue
    }

    public override void Init()
    {
        Bind<Text>(typeof(Texts));
    }

    public void EnqueueMessage(Message message)
    {
        // _locked == 0   _locked 는 1로 바뀜 // 반환값은 이전 값 
        // _locked != 0   _locked 는 그대로 // 반환값은 그대로
        messageQueue.Enqueue(message);
        if (Interlocked.CompareExchange(ref _locked, 1, 0) == 0)
        {
            corShowMessage = ShowMessage();
            StartCoroutine(corShowMessage);
        }
    }

    IEnumerator ShowMessage()
    {
        while(messageQueue.Count > 0)
        {
            Managers.Sound.Play2D("SFX/Notification");
            Message message = messageQueue.Dequeue();
            Get<Text>((int)Texts.SignQueue).text = message.text;
            Get<Text>((int)Texts.SignQueue).color = new Color(50, 50, 50, 255);
            yield return new WaitForSeconds(message.showMessageTime);
        }

        Get<Text>((int)Texts.SignQueue).text = "";
        Get<Text>((int)Texts.SignQueue).color = new Color(50, 50, 50, 0);
        _locked = 0;
    }

    public void ClearMessage()
    {
        messageQueue.Clear();
        if (corShowMessage != null)
            StopCoroutine(corShowMessage);
        Get<Text>((int)Texts.SignQueue).text = "";
        Get<Text>((int)Texts.SignQueue).color = new Color(50, 50, 50, 0);
        _locked = 0;
    }
}

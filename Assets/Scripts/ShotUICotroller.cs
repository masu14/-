using UnityEngine;
using UniRx;
using UnityEngine.UI;
using Merusenne.Player;

/// <summary>
/// ゲーム画面左上のショットUIの制御クラス
/// プレイヤーの現在選択しているショットを明るめに、それ以外のショットを暗めに表示する
/// </summary>
public class ShotUICotroller : MonoBehaviour
{
    private PlayerShot _playerShot;                                 //プレイヤーの選択中のショットの色を取得するために使う

    private int _shotColor =0;                                      //選択中のショットの色番号(blue=0,green=1,red=2)
    private Color32 _onColor = new Color32(255, 255, 255, 255);     //明るめ
    private Color32 _offColor = new Color32(255, 255, 255, 100);    //暗め
    void Start()
    {
        
        _playerShot = GameObject.FindWithTag("Player").GetComponent<PlayerShot>();  //プレイヤーのショット制御取得

        //プレイヤーのショット切り替えを購読、ショットUI更新、OnDestroy時にDispose()されるように登録
        _playerShot.OnShotSwitch
            .Subscribe(x =>
            {
                _shotColor = x;
                UpdateColor();
            })
            .AddTo(this);
        
    }

    //プレイヤーが選択中のショットの色に応じてショットUIの明るさを変える
    private void UpdateColor()
    {
        string expectedTag = GetExpectedTag(_shotColor);                                //選択中のショットの色のタグ取得

        //選択中のショットの色を明るめに、それ以外のショットの色を暗めに
        if (!string.IsNullOrEmpty(expectedTag) && gameObject.CompareTag(expectedTag))
        {
            GetComponent<Image>().color = _onColor;     //明るめ
            //Debug.Log(expectedTag);
        }
        else
        {
            GetComponent<Image>().color = _offColor;    //暗め
        }
        
    }

    //ショットの色番号からタグを返す(blue=0,green=1,red=2)
    private string GetExpectedTag(int colorIndex)
    {
        switch (colorIndex)
        {
            case 0:                     
                return "Blue_shot_UI";  //青色
            case 1:                     
                return "Green_shot_UI"; //緑色
            case 2:                     
                return "Red_shot_UI";   //赤色
            default:
                return "";
        }
    }
}
        

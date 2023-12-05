using UnityEngine;

/// <summary>
/// tutorialオブジェクトに付けるスクリプトコンポーネント
/// PopUpScaleControllerのサイズ変更の処理をつかってOpen,Closeのパラメータをインスペクターで変更できる
/// ポップアップ処理はゲーム初プレイ時にGameManagerが，緑の床に触れたときTutorialSpotControllerがそれぞれ呼び出す
/// </summary>

public class PopUpController : MonoBehaviour
{
   
    public PopUpScaleController open, close;        //それぞれのパラメータを定義してPlay()でサイズ変更

    void Start()
    {
        open.Setup(gameObject);     //openにポップアップ画像のオブジェクトをセットする
        close.Setup(gameObject);    //closeにポップアップ画像のオブジェクトをセットする
    }

    //ポップアップが開く処理
    public void Open()
    {
        open.Play();
    }

    //ポップアップが閉じる処理
    public void Close()
    {
        close.Play();
    }

}
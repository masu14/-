using UnityEngine;
using UnityEngine.Rendering.Universal;
using UniRx;

/// <summary>
/// Barrierを制御するクラス
/// Barrierは親オブジェクトのGimmickObjectにショットが衝突したときその色に応じて明滅する
/// lightが点いているときコライダーがアクティブ、消えているとき非アクティブ状態になる
/// </summary>
public class BarrierController : MonoBehaviour
{
    private Light2D _barrierLight;                              //子オブジェクトのLight
    private BoxCollider2D _boxCollider2D;                        //アクティブ状態の管理に使用
    private GimmickController _grandGimmick;                     //祖父オブジェクトのGimmickController

    //パラメータ
    [SerializeField] private bool _barrierActive = false;       //Barrier明滅フラグ

    //light2Dの色
    private Color32 _blue = new Color32(127, 255, 255, 255);    //青色
    private Color32 _green = new Color32(56, 241, 104, 255);    //緑色
    private Color32 _red = new Color32(231, 69, 69, 255);       //赤色
    private Color32 _clear = Color.clear;                       //無色
   

    private void Start()
    {
        _barrierLight = transform.GetChild(0).gameObject.GetComponent<Light2D>();                                   //Barrierのlight取得
        _boxCollider2D = transform.GetComponent<BoxCollider2D>();                                                    //Barrierのコライダー取得
        _grandGimmick = transform.parent.gameObject.transform.parent.gameObject.GetComponent<GimmickController>();   //祖父のGimmick取得

        
        if (_grandGimmick != null)
        {
            //ショット衝突の購読
            _grandGimmick.OnCollisionObj.Subscribe(SwitchBarrierLight);      
        }

        //Barrierの初期化
        if (_barrierActive == false)
        {
            _boxCollider2D.enabled = false;
            _barrierLight.color = _clear;
        }
    }

    
    //プレイヤーのショット衝突時に呼び出す、Barrierと同じ色のショットが衝突すると色とコライダーが変化する
    private void SwitchBarrierLight(GameObject gameObject)
    {
        //ショットのタグ確認
        if (gameObject.CompareTag("Shot_blue") || gameObject.CompareTag("Shot_green") || gameObject.CompareTag("Shot_red"))
        {
            string barrierTag = $"Barrier_{gameObject.tag.Substring(5)}";   //"Shot_blue" => "Barrier_blue"

            //ショットの自身のタグから対応した色とコライダーを切り替える
            if (this.gameObject.CompareTag(barrierTag))
            {
                _barrierActive = !_barrierActive;
                _boxCollider2D.enabled = _barrierActive;
                _barrierLight.color = _barrierActive ? GetBarrierColor(gameObject.tag) : _clear;

            }
        }
    }
    //衝突したショットのタグから対応した色を返す
    private Color32 GetBarrierColor(string shotTag)
    {
        switch (shotTag)
        {
            case "Shot_blue":   //青色
                return _blue;   
            case "Shot_green":  //緑色
                return _green;
            case "Shot_red":    //赤色
                return _red;
            default:            //無色
                return _clear;
        }
    }

    
}

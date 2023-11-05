using UniRx;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// LuminaBoardの発光を制御するクラス
/// Scene上の二つ上の階層のGimmickObject(祖父オブジェクト)がショットと衝突すると発光する
/// </summary>
public class LuminaBoardController : MonoBehaviour 
{
    private GameObject _boardLight;                                 //子オブジェクトのLight
    private GimmickController _grandGimmick;                        //祖父オブジェクトのGimmickController

    Color32 _firstLight = new Color32(252, 252, 252, 252);          //白色

    void Start()
    {
        //初期は白に発光
        _boardLight = transform.GetChild(0).gameObject;
        _boardLight.GetComponent<Light2D>().color = _firstLight;

        _grandGimmick = transform.parent.gameObject.transform.parent.gameObject.GetComponent<GimmickController>();  //祖父オブジェクトのGimmickController取得

        //ショット衝突の購読
        if(_grandGimmick != null)
        {
            _grandGimmick.OnCollision.Subscribe(SetLumina);
        }
    }

    //ショットが衝突したらその色に発光
    private void SetLumina(Color32 color)
    {
        _boardLight.GetComponent<Light2D>().color = color;
    }

}

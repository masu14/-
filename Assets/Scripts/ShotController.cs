using Merusenne.Player;
using UnityEngine;
using UniRx;

/// <summary>
/// ショット本体の制御を行うクラス
/// ShotxDirプロパティはショット発射後に移動方向を変化させないために必要
/// </summary>
public class ShotController : MonoBehaviour
{
    private PlayerMove _playerMove;
    
    //パラメータ
    [SerializeField] private float shotSpeed = 0.02f;   //解除コードの速さ
    [SerializeField] private float shotTime = 1.0f;     //解除コードの消滅までの時間


    private bool _isWrite = true;                       //一度だけ書き換え可能なフラグ
    private bool _shotxDir;                             //ショットの発射向き

    //ショット発射時のプレイヤーの体の向きを取得する
    private bool ShotxDir
    {
        get { return _shotxDir; }
        set
        {
            if(_isWrite)
            {
                _shotxDir = value;
                _isWrite = false;   //一度書き換えたら以降は書き換え不可
            }
        }
    }

    private void Awake()
    {
        _playerMove = GameObject.FindWithTag("Player").GetComponent<PlayerMove>();       //プレイヤーの動き取得

        //プレイヤーの体の向きを一度だけ取得、OnDestroy時にDispose()されるように登録
        _playerMove.Observable.Subscribe(xDir => ShotxDir = xDir).AddTo(this);
    }

    private void FixedUpdate()
    {
        
        if (_shotxDir == true)                //スプライトが右向きのとき
        {
            ShotMove(shotSpeed);
        }
        else                                  //スプライトが左向きのとき
        {
            ShotMove(-shotSpeed);
        }

        //生成から一定時間経過後ショット消滅
        Destroy(gameObject, shotTime);        
    }

    //ショットの速度の更新
    private void ShotMove(float shotSpeed)                             
    {
        transform.Translate(shotSpeed, 0, 0);
    }

    //衝突すると消滅する
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
    }
}

using System;
using UniRx;
using UnityEngine;

namespace Merusenne.Player
{
    /// <summary>
    /// プレイヤーキャラのアニメーション処理を行うスクリプトコンポーネント
    /// アニメーターの実行タイミングを定義する
    /// 入力やプレイヤーの状態に応じてアニメーションを切り替える
    /// </summary>
    public class PlayerAnimation : MonoBehaviour
    {
        private Animator _animator;                             //プレイヤーのAnimatorを制御する
        private PlayerMove _playerMove;                         //プレイヤーの動きを制御する
        private PlayerCore _playerCore;                         //プレイヤーがDead状態になったときに使用する
        private IInputEventProvider _inputEventProvider;        //入力を取得する、入力に応じてモーションを切り替える

       

        //各種アニメーションの紐づけ
        private string _stopAnime = "PlayerStop";   //静止モーション(未入力時)
        private string _moveAnime = "PlayerMove";   //地上移動モーション(地上で横入力)
        private string _jumpAnime = "PlayerJump";   //空中モーション(空中時)
        private string _deadAnime = "PlayerOver";   //Deadモーション(Deadタグ接触時)

        private string _nowAnime = "";              //現在のフレームのモーション
        private string _oldAnime = "";              //1フレーム前のモーション

        private const float _AXISHBORDER = 0.15f;   //横入力の閾値
        private bool _isClear = false;              //クリアフラグ
        private void Awake()
        {
            _animator = GetComponent<Animator>();                       //プレイヤーのAnimator取得
            _playerMove = GetComponent<PlayerMove>();                   //プレイヤーの動き取得(接地判定を購読)
            _playerCore = GetComponent<PlayerCore>();                   //プレイヤーの状態取得(Dead通知を購読)
            _inputEventProvider = GetComponent<IInputEventProvider>();  //プレイヤーの入力取得

            //Dead通知を受け取ったときにDeadモーションを行うための購読
            _playerCore.OnDead.Subscribe(_ => Dead()).AddTo(this);
        }

        private void Start()
        {
            //アニメーションの初期化
            _nowAnime = _stopAnime;
            _oldAnime = _stopAnime;

            //クリアフラグの初期化
            _isClear = false;
        }
        void Update()
        {
            //Clearフラグが立ったら処理を行わない
            if (_isClear) return;
            //プレイヤーがDead時には処理を行わない
            if (_playerCore.IsDead.Value) return;
            
            //向きの調整、横入力の閾値を超えたら右向き(左向き)にする
            if (_inputEventProvider.AxisH.Value > _AXISHBORDER)         //右向き
            {
                transform.localScale = new Vector2(1, 1);
            }
            else if (_inputEventProvider.AxisH.Value < -_AXISHBORDER)   //左向き
            {
                transform.localScale = new Vector2(-1, 1);
            }

            //プレイヤーの接地判定から地上と空中のモーションを切り替える
            if (_playerMove.IsGrounded.Value)
            {
                //地上
                if (Math.Abs(_inputEventProvider.AxisH.Value) <= _AXISHBORDER)
                {
                    //静止
                    _nowAnime = _stopAnime;
                }
                else
                {
                    //移動
                    _nowAnime = _moveAnime;
                }
            }
            else
            {
                //空中
                _nowAnime = _jumpAnime;
            }

            //アニメーションの切り替えを監視する
            if (_nowAnime != _oldAnime)
            {
                _oldAnime = _nowAnime;
                _animator.Play(_nowAnime);
            }

        }

        //Goal時にクリアフラグを立て、プレイヤーアニメーションを静止状態にする
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "Goal")
            {
                _isClear = true;
                _animator.Play(_stopAnime);
            }
        }

        //プレイヤーがDeadしたときにDeadモーションを行う
        private void Dead()
        {
            _animator.Play(_deadAnime);
        }
    }

}

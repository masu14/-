using Merusenne.Player.InputImpls;
using UnityEngine;
using System;
using UniRx;

namespace Merusenne.StageGimmick
{
    /// <summary>
    /// 
    /// ThroughFroor(すり抜け床)を制御するクラス
    /// すり抜け床がプレイヤーのy座標(足元)よりも高い位置にあるときコライダーが無効になる
    /// プレイヤーがすり抜け床の上に乗っているときは、下向き入力を長押しすることでコライダーが無効になる
    /// 
    /// </summary>
    /// 
    public class ThroughFloorController : MonoBehaviour
    {
        private BoxCollider2D _myCollider;                      //コライダーを切り替えることですり抜けを行う
        private GameObject _player;                             //プレイヤーのy座標をコライダーの切り替えの条件に使用する
        private InputEventProviderImpl _inputEventProvider;     //プレイヤーの下向き入力をコライダーの切り替えの条件に使用する


        private IDisposable _isDown;                            //下向き入力の購読
        private bool _canThroughDown = false;                   //下向き入力長押しですり抜けフラグ
        [SerializeField] private float _through_border = 0.2f;   //長押しの閾値 
        void Start()
        {
            _myCollider = GetComponent<BoxCollider2D>();                            //すり抜け床のコライダー取得
            _player = GameObject.FindWithTag("Player");                             //プレイヤー取得
            _inputEventProvider = _player.GetComponent<InputEventProviderImpl>();   //プレイヤーの入力取得



            //下向き入力を長押しで購読する
            _isDown = _inputEventProvider.IsThrough
                .Throttle(TimeSpan.FromSeconds(_through_border))
                .Subscribe(x => _canThroughDown = x)
                .AddTo(this);

        }

        void Update()
        {
            //すり抜けフラグが立つ または プレイヤーより上側にあるときコライダーを無効化する
            //2つ目の条件はプレイヤーが床にめり込むのを避けるため
            if (_canThroughDown || (_player.transform.position.y < transform.position.y))
            {
                _myCollider.enabled = false;    //無効化
            }
            else
            {
                _myCollider.enabled = true;     //有効化
            }
        }

    }

}

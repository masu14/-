using Merusenne.Player.InputImpls;
using UnityEngine;
using System;
using UniRx;


/// <summary>
/// ThroughFroor(すり抜け床)を制御するクラス
/// </summary>
public class ThroughFloorController : MonoBehaviour
{
    private BoxCollider2D _myCollider;
    private GameObject _player;
    private InputEventProviderImpl _inputEventProvider;
    

    private IDisposable _isDown;                            //下向き入力の購読
    private bool _canThroughDown = false;                   //下向き入力長押しですり抜けフラグ
    [SerializeField] private float _throughBorder = 0.2f;   //長押しの閾値 
    void Start()
    {
        _myCollider = GetComponent<BoxCollider2D>();                            //すり抜け床のコライダー取得
        _player = GameObject.FindWithTag("Player");                             //プレイヤー取得
        _inputEventProvider = _player.GetComponent<InputEventProviderImpl>();   //プレイヤーの入力取得

        

        //下向き入力を長押しで購読する
        _isDown = _inputEventProvider.IsThrough
            .Throttle(TimeSpan.FromSeconds(_throughBorder))
            .Subscribe(x => _canThroughDown = x)
            .AddTo(this);

    }

    void Update()
    {
        //すり抜けフラグが立つ または プレイヤーより上側にあるときコライダーを無効化する
        //2つ目の条件はプレイヤーが床にめり込むのを避けるため
        if(_canThroughDown ||(_player.transform.position.y < transform.position.y))
        {
            _myCollider.enabled = false;
        }
        else
        {
            _myCollider.enabled = true;
        }
    }

}

using UnityEngine;
using UniRx;
using System;

/// <summary>
/// プレイヤーの状態を管理するスクリプトコンポーネント
/// プレイヤーがDeadタグを持つオブジェクトに接触すると、Dead状態になる
/// </summary>
public class PlayerCore : MonoBehaviour
{                              
    private readonly ReactiveProperty<bool> _isDead = new ReactiveProperty<bool>(false);        //Deadフラグ
    private Subject<Unit> _onDead = new Subject<Unit>();                                        //Dead通知

    //Dead状態を送信
    public IReadOnlyReactiveProperty<bool> IsDead => _isDead;
    public IObservable<Unit> OnDead => _onDead;

    void Start()
    {
        //OnDestroy時にDispose()されるように登録
        _isDead.AddTo(this);
        _onDead.AddTo(this);
    }

    //Deadタグのオブジェクトに衝突したらDead状態になる
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Dead"))
        {
            _isDead.Value = true;
            _onDead.OnNext(Unit.Default);
        }
    }


}

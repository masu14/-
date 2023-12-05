using UnityEngine;
using UniRx;
using System;

/// <summary>
/// 
/// ステージの位置をCameraManagerに送信するためのクラス
/// プレイヤーが入るステージの座標はCameraManagerの_nowStageに記録し、カメラの更新に使う
/// 
/// </summary>

public class StageManager : MonoBehaviour
{
    private Subject<Vector2> _playerEnter = new Subject<Vector2>();     //プレイヤーの接触したステージの座標

    public IObservable<Vector2> OnPlayerEnter => _playerEnter;          //プレイヤーがステージのコライダーに接触したタイミングでステージ座標を送信


    //プレイヤーの接触検知
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //プレイヤーを検知したときnowStageを更新
        if(collision.gameObject.tag == "Player")
        {
            _playerEnter.OnNext(transform.position);                    //メインカメラにプレイヤーのいるステージを送信
        }
    }
}

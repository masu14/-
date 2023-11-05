using UnityEngine;
using UniRx;
using TMPro;
using System;
using Merusenne.StageGimmick;

/// <summary>
/// ClearTextControllerはGameClearオブジェクト(テキスト)に付けるスクリプトコンポーネント
/// Goalオブジェクトに触れたとき一定時間経過後、「Game Clear」と表示する
/// </summary>

public class ClearTextController : MonoBehaviour
{
    private GameObject _goal;                   //ステージ奥にあるゴールオブジェクト
    private GoalController _goalController;     //ゴールオブジェクトのスクリプトコンポーネント、ゴール時に購読する

    [SerializeField] private TextMeshProUGUI _clear_text;       //「Game Clear」と表示するテキストオブジェクト
    [SerializeField] private float _wait_game_clear;            //Goalオブジェクトに触れてからテキストを表示するまでの時間

    private Subject<Unit> _onClear = new Subject<Unit>();       //クリアテキストを表示したときに送信
    public IObservable<Unit> OnClear => _onClear;               


    void Start()
    {
        _goal = GameObject.FindWithTag("Goal");                     
        _goalController = _goal.GetComponent<GoalController>();

        //ゴールしたときに送信してくる、購読すると一定時間経過後、クリアテキストを表示する
        _goalController.OnGoal
        .Subscribe(_ =>WaitGameClear())
        .AddTo(this);

        _onClear.AddTo(this);

        _clear_text.alpha = 0.0f;       //クリアテキストは初め透明にしておく
    }

    //ゴール通知が届くと一定時間経過した後にGameClear()メソッドを呼ぶ
    void WaitGameClear()
    {
        Observable.Timer(TimeSpan.FromSeconds(_wait_game_clear)).Subscribe(_ => GameClear());
    }
    
    //クリアテキストを表示する
    void GameClear()
    {
        _clear_text.alpha = 1.0f;
        _onClear.OnNext(Unit.Default);  //テキストの表示を送信する
    }

}

using UnityEngine;
using UnityEngine.Rendering.Universal;
using UniRx;
using System;

namespace Merusenne.StageGimmick
{
    /// <summary>
    /// Goalオブジェクトに付けるスクリプトコンポーネント
    /// プレイヤーが触れるとゴール通知を送信してゲームクリアとなる
    /// Goalオブジェクトは白い光で点滅している球体のオブジェクト
    /// </summary>

    public class GoalController : MonoBehaviour
    {
        [SerializeField] private Light2D _goal_light;           //ライト、白く光って点滅する
        [SerializeField] private float _duration = 1.0f;        //点滅の速さパラメータ
        [SerializeField] private float _max_intensity = 2.0f;   //発光時の強度パラメータ
        [SerializeField] private float _min_intensity = 0.2f;   //消光時の強度パラメータ

        private bool _isGoal = false;       //ゴールフラグ、触れるとフラグが立つ
        private float _time = 0.0f;         //時間をdeltatimeから得る、点滅処理に使う

        private Subject<Unit> _onGoal = new Subject<Unit>();    //ゴール時に通知を送信する
        public IObservable<Unit> OnGoal => _onGoal;

        void Start()
        {
            _isGoal = false;        //フラグの初期化
            _onGoal.AddTo(this);
        }


        void Update()
        {
            //点滅処理
            _time += Time.deltaTime;
            _goal_light.intensity = _min_intensity + _max_intensity * Mathf.Abs(Mathf.Sin(_time * _duration));

        }

        //プレイヤー接触時にゴール通知を送信する
        private void OnTriggerEnter2D(Collider2D collision)
        {

            if (collision.tag == "Player")
            {
                if (_isGoal) return;
                _isGoal = true;
                _onGoal.OnNext(Unit.Default);
                Debug.Log("Goal");
            }
        }
    }

}

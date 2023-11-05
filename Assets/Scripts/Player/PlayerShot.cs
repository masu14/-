using System;
using UnityEngine;
using UniRx;
using Merusenne.Player.Shot;



namespace Merusenne.Player
{
    /// <summary>
    /// プレイヤーのショットの発射の制御を行うスクリプトコンポーネント
    /// ショット本体の制御はShotControllerが行う
    /// </summary>
    public class PlayerShot : MonoBehaviour
    {
        private IInputEventProvider _inputEventProvider;

        //パラメータ
        [SerializeField] private float _go_shot_time = 1.5f;                          //ショット待機時間

        //Prefabの登録
        [SerializeField] private ShotController _shot_blue_prefab;                    //青ショット
        [SerializeField] private ShotController _shot_green_prefab;                   //緑ショット
        [SerializeField] private ShotController _shot_red_prefab;                     //赤ショット
       
        private Vector3 _shotPoint;                                                 //ショットが生成される位置
        private bool _goShot = false;                                               //ショットフラグ
        private bool _shotxDir = false;                                             //ショット発射向き(true:右向き、false:左向き)
        private const float _AXISHBORDER = 0.15f;                                   //横入力の閾値

        private ReactiveProperty<int> _shotSwitch = new ReactiveProperty<int>(0);   //ショットの切り替えを保持

        //ショットの切り替えを送信
        public IReactiveProperty<int> OnShotSwitch => _shotSwitch;

        void Start()
        {
            _inputEventProvider = GetComponent<IInputEventProvider>();      //プレイヤーの入力取得
            _shotPoint = transform.Find("ShotPoint").localPosition;         //ショットの生成される位置を取得

            //初期化
            _shotxDir = true;
            _goShot = true;

            //OnDestroy時にDispose()されるように登録
            _shotSwitch.AddTo(this);
        }


        void Update()
        {
            //入力を受け取ってショット生成の向きを決める
            if (_inputEventProvider.AxisH.Value > _AXISHBORDER)         //右向き
            {
                _shotxDir = true;
            }
            else if (_inputEventProvider.AxisH.Value < -_AXISHBORDER)   //左向き
            {
                _shotxDir = false;

            }

            //左シフトキーのショット切り替え　blue=0, green=1, red=2
            if (_inputEventProvider.IsLeftSwitch.Value)
            {
                _shotSwitch.Value--;
                if(_shotSwitch.Value < 0)
                {
                    _shotSwitch.Value = 2;
                }
            }

            //右シフトキーのショット切り替え　blue=0, green=1, red=2
            if (_inputEventProvider.IsRightSwitch.Value)
            {
                //0=>1=>2=>0の順番で切り替わる
                _shotSwitch.Value++;
                if (_shotSwitch.Value >2)
                {
                    _shotSwitch.Value = 0;
                }
            }

            //ショット発射
            if (_inputEventProvider.IsShot.Value && _goShot == true)
            {
                Shot();
                _goShot = false;
                Observable.Timer(TimeSpan.FromSeconds(_go_shot_time)).Subscribe(_ => GoShot());
            }
        }

        //ショットの生成
        void Shot()
        {
            ShotController shotPrefab = null;

            //色番号から生成するショットを決める
            switch (_shotSwitch.Value)
            {
                case 0: //青ショット
                    shotPrefab = _shot_blue_prefab;
                    break;
                case 1: //緑ショット
                    shotPrefab = _shot_green_prefab;
                    break;
                case 2: //赤ショット
                    shotPrefab = _shot_red_prefab;
                    break;
            }

            //体の向きからショットが生成される位置を決める
            Vector3 shotPosition = transform.position + (_shotxDir ? _shotPoint : new Vector3(-_shotPoint.x, _shotPoint.y, 0));

            //ショット生成
            Instantiate(shotPrefab, shotPosition, Quaternion.identity);
          
        }

        //待機時間が経過したらショットの発射が可能になる
        void GoShot()
        {
            _goShot = true;
        }
    }
}


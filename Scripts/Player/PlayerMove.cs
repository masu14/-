using System;
using UnityEngine;
using UniRx;


namespace Merusenne.Player
{
    /// <summary>
    /// プレイヤーの動きを制御するスクリプトコンポーネント
    /// </summary>
    public sealed class PlayerMove : MonoBehaviour
    {
        private Rigidbody2D _rbody;
        private PlayerCore _playerCore;
        private IInputEventProvider _inputEventProvider;

        //プレイヤーの移動のパラメータ
        [SerializeField] private float _x_dash_speed = 3.0f;                  //地上での速さ
        [SerializeField] private float _x_fri_speed = 2.5f; 　　              //空中での速さ
        [SerializeField] private float _jump_power = 14;                      //ジャンプ力
        [SerializeField] private float _max_jump_speed = 14;                  //ジャンプ速度の最大値
        [SerializeField] private float _max_fall_speed = 14;                  //落下速度の最大値
        [SerializeField] private float _dead_jump = 10f;                     //dead時演出のジャンプ
        [SerializeField] private LayerMask _ground_layer;                    //地面レイヤー指定
        [SerializeField] private AnimationCurve _speed_curve;                //横移動の速度のグラフ

        

        //フラグ
        private bool _isJump = false;                           //ジャンプフラグ
        private bool _onGround = false;                         //接地フラグ
        private bool _isClear = false;
        
        
        private float _dashTime;                                //_speedCurveの定義域
        private float beforeAxisH;                              //前フレームの横入力
        private Vector3 _playerWide = new Vector3(0.35f, 0, 0); //プレイヤーの横幅
        private const float _AXISHBORDER = 0.15f;               //横入力の閾値
        


        private ReactiveProperty<bool> _playerxDir = new ReactiveProperty<bool>(true);  //体の向き,ショットPrefabに送信
        private ReactiveProperty<bool> _isGrounded = new BoolReactiveProperty();        //接地判定,

        //体の向き、接地判定の送信
        public IObservable<bool> Observable
        {
            get { return _playerxDir; }
        }
        public IReactiveProperty<bool> IsGrounded => _isGrounded;

        
        private void Awake()
        {
            
            _rbody = GetComponent<Rigidbody2D>();                       //プレイヤーの動き取得
            _playerCore = GetComponent<PlayerCore>();                   //プレイヤーの状態取得(Deadフラグ)
            _inputEventProvider = GetComponent<IInputEventProvider>();  //プレイヤーの入力取得

            

            //OnDestroy時にDispose()されるように登録
            _isGrounded.AddTo(this);
            _playerCore.OnDead.Subscribe(_ => DeadMove()).AddTo(this);
        }

        private void Start()
        {
            _isClear = false;
            Debug.Log(_isClear);
        }

        //入力の送信の検知はUpdateで処理する
        void Update()
        {
            //Clearフラグが立ったら処理を行わない
            if (_isClear) return;
            //Deadフラグが立ったら処理を行わない
            if (_playerCore.IsDead.Value) return;

            //向きの調整
            if (_inputEventProvider.AxisH.Value > _AXISHBORDER)
            {
                _playerxDir.Value = true;   //右向き
            }
            else if (_inputEventProvider.AxisH.Value < -_AXISHBORDER)
            {
                _playerxDir.Value = false;  //左向き
            }

            //地上でジャンプ入力を押したらジャンプフラグを立てる
            if (_inputEventProvider.IsJump.Value && _isGrounded.Value)
            {
                _isJump = true;
            }
        }

        private void FixedUpdate()
        {
            //Clearフラグが立ったら処理を行わない
            if (_isClear) return;
            //Deadフラグが立ったら処理を行わない
            if (_playerCore.IsDead.Value) return;

            //地上判定
            ChecKGround();

            //速度の更新
            if (_inputEventProvider.AxisH.Value != 0 && _onGround) //横入力有＆地上
            {
                _dashTime += Time.deltaTime;
                _rbody.velocity = new Vector2(_x_dash_speed * _inputEventProvider.AxisH.Value * _speed_curve.Evaluate(_dashTime), _rbody.velocity.y);
            }
            else if(_inputEventProvider.AxisH.Value != 0 && _onGround == false) //横入力有＆空中
            {
                _dashTime += Time.deltaTime;
                _rbody.velocity = new Vector2(_x_fri_speed * _inputEventProvider.AxisH.Value * _speed_curve.Evaluate(_dashTime), _rbody.velocity.y);
            }
            else if(_inputEventProvider.AxisH.Value ==0)　//横入力無
            {
                _dashTime = 0.0f;
                _rbody.velocity = new Vector2(0, _rbody.velocity.y);
            }
           

            //横入力切替時に横移動の速度曲線を初期化
            if(_inputEventProvider.AxisH.Value > 0 && beforeAxisH < 0)
            {
                _dashTime = 0.0f;
            }
            else if(_inputEventProvider.AxisH.Value <0&&beforeAxisH > 0)
            {
                _dashTime = 0.0f;
            }
            beforeAxisH = _inputEventProvider.AxisH.Value;
            


            //ジャンプ処理
            if (IsGrounded.Value && _isJump)    //地面の上＆ジャンプフラグ
            {
                _isJump = false;
                //Debug.Log("ジャンプ");
                _rbody.AddForce(new Vector2(0, _jump_power), ForceMode2D.Impulse);//ジャンプ速度の制限
                
            }

            //ジャンプの最大速度
            if (_rbody.velocity.y > _max_jump_speed)
            {
                _rbody.velocity = new Vector2(_rbody.velocity.x, _max_jump_speed);
            }

            //落下速度の最大速度
            if(_rbody.velocity.y < - _max_fall_speed)
            {
                _rbody.velocity = new Vector2(_rbody.velocity.x, -_max_fall_speed);

            }

        }

        

        //地上判定
        private bool ChecKGround()
        {
            //足元の中心、右端、左端のいずれかが地面に触れているならフラグを立てる
            bool isGround = Physics2D.Linecast(transform.position + _playerWide, transform.position + _playerWide - transform.up * 0.1f, _ground_layer) ||
                            Physics2D.Linecast(transform.position, transform.position - transform.up * 0.1f, _ground_layer) ||
                            Physics2D.Linecast(transform.position - _playerWide, transform.position - _playerWide - transform.up * 0.1f, _ground_layer);

            _onGround = isGround;
            IsGrounded.Value = _onGround;

            return _onGround;
        }

        //Dead時に跳ね上がる動き
        private void DeadMove()
        {
            GetComponent<EdgeCollider2D>().enabled = false;
            _rbody.velocity = Vector2.zero;
            _rbody.velocity = new Vector2(0, _dead_jump);
        }

        //Goal時にプレイヤーを静止させる
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "Goal")
            {
                _isClear = true;
                _rbody.velocity = Vector2.zero;
                Debug.Log("Goal");
            }
        }
    }

}

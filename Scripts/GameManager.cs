using UnityEngine;
using UniRx;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using Merusenne.StageGimmick;

/// <summary>
/// ゲーム全体の管理を行うクラス
/// シーンの遷移、セーブ、ロードの制御を行う
/// </summary>
public class GameManager : MonoBehaviour
{
    private GameObject _player;                                     //プレイヤーキャラの状態(Dead等)を取得するのに使う
    private PlayerCore _playerCore;                                 //プレイヤー自身がDeadタグ付きオブジェクトに触れると状態が送信される

    private SavePointController[] _savePoints;                      //シーン上にある黄色い足場
    private SaveDataManager _save;                                  //ゲーム全体でのセーブデータを書き込む(json形式)

    private GameObject _clearText;                                  //ゲームクリア時に表示するテキストオブジェクト
    private ClearTextController _clearTextController;               //_clearTextが表示されたときクリアフラグを立ててシーン遷移の入力を受け付ける

    [SerializeField] GameObject _tutorial_action;                   //ゲーム初プレイ時に表示するチュートリアルポップアップ
    
    //パラメータ
    [SerializeField] private float _load_wait_time = 2.0f;                  //プレイヤーがDeadしてからロードされるまでの時間
    [SerializeField] private float _pop_up_wait_time = 0.5f;                //ゲーム開始からチュートリアル表示までの時間

    private string _sceneName = "StageScene";                               //ロードするシーン名
    private string _filePath;                                               //セーブデータの保存先
    private Vector2 _playerStartPos = new Vector2(-5.5f, -4);               //セーブデータがないときのプレイヤーの開始位置
    private Vector2 _playerPosUp = new Vector2(0, 0.5f);                    //セーブポイント上空のプレイヤーの開始出現位置
    private bool _isClear = false;                                          //クリアフラグ、フラグが立つとタイトルシーンへ遷移するための入力を受け付ける
    private bool _isTutorial = false;                                       //チュートリアルが開いている状態
    

    private Subject<Vector2> _saveStage = new Subject<Vector2>();
    public IObservable<Vector2> OnSaveStage => _saveStage;
    void Awake()
    {
        _player = GameObject.FindWithTag("Player");                             //プレイヤー取得
        _playerCore = _player.GetComponent<PlayerCore>();                       //プレイヤーの状態取得
        _filePath = Application.dataPath + "/.savedata.json";                   //セーブデータの保存先登録
        _save = new SaveDataManager();                                          //セーブデータの管理先取得
        _savePoints = FindObjectsOfType<SavePointController>();                 //シーン上の全てセーブポイントを取得
        _clearText = GameObject.FindWithTag("ClearText");                       //「Game Clear」と表示するテキストオブジェクト
        _clearTextController = _clearText.GetComponent<ClearTextController>();  //表示されたときフラグを送信するスクリプトコンポーネント

        _sceneName = "StageScene";                                  //StageSceneはプレイヤーDead時にロードされるシーン
       
        Debug.Log($"ロード時のnowStagePos:{_save._nowStagePos}");


        //プレイヤーDead時に一定時間をおいてリロード、OnDestroy時にDispose()されるように登録
        _playerCore.OnDead
            .Subscribe(_ => WaitGameRestart())
            .AddTo(this);

        //全てのセーブポイントを購読
        foreach (var savePoint in _savePoints)
        {
            //セーブポイントに触れたときセーブデータを更新、OnDestroy時にDispose()されるように登録
            savePoint.OnTriggerSave
                .Subscribe(x =>
                {
                    _save._nowSavePos = x;

                    Debug.Log($"セーブポイントの位置を変更しました:{x}");
                    Save();
                })
                .AddTo(this);

            savePoint.OnTriggerStage
                .Subscribe(x =>
                {
                    _save._nowStagePos = x;
                    Debug.Log($"セーブポイントのあるステージの位置を変更しました{x}");
                    Save();
                })
                .AddTo(this);
        }

        //クリアテキストが表示されたときクリアフラグを立てる
        _clearTextController.OnClear
            .Subscribe(_ => _isClear = true)
            .AddTo(this);

        //セーブデータのロード
        Load();
        _saveStage.OnNext(_save._nowStagePos);                              //ロード時のステージの送信、CameraManagerが購読
       
    }

    private void Start()
    {
       
        _player.transform.position = _save._nowSavePos + _playerPosUp;      //ロード時のプレイヤーの開始位置

        //ゲーム初プレイのとき操作チュートリアルを表示する,このポップアップが表示されるのは初回プレイ時のみ
        if (!_save._isFisrtPlay)
        {
            _save._isFisrtPlay = true;                                  //セーブデータに書き込み
            Observable.Timer(TimeSpan.FromSeconds(_pop_up_wait_time))   //一定時間経過後、チュートリアルを表示する
                .Subscribe(_ =>
                {
                    _isTutorial = true;
                    _tutorial_action.GetComponent<PopUpController>().Open();
                    Debug.Log("ゲーム初プレイにつき、操作チュートリアルを表示します");
                })
                .AddTo(this);
        }

    }

    private void Update()
    {
        //クリアフラグが立った時、入力を行うとタイトルシーンに戻る
        if (_isClear)
        {
            if (Input.anyKeyDown)
            {
                _sceneName = "TitleScene";              //TitleSceneはゲーム起動時の最初のシーン、クリア時に戻ってくる
                _save._nowSavePos = _playerStartPos;    //ゲーム開始位置を初期化
                Save();                                 //ロード前にセーブを行う
                SceneManager.LoadScene(_sceneName);     //タイトルシーンをロード
            }
        }

        //操作用のチュートリアル表示中に移動入力を行うと一定時間経過後、チュートリアルが閉じる
        if (_isTutorial)
        {
            if (Input.GetKeyDown(KeyCode.W)||Input.GetKeyDown(KeyCode.A)||Input.GetKeyDown(KeyCode.S)||Input.GetKeyDown(KeyCode.D))
            {
                _isTutorial = false;
                Observable.Timer(TimeSpan.FromSeconds(_pop_up_wait_time))
                    .Subscribe(_=>_tutorial_action.GetComponent<PopUpController>().Close());
            }
        }
    }

    //セーブデータを更新しセーブする
    public void Save()
    {
        
        Debug.Log($"セーブ時のnowSavePos:{_save._nowSavePos}");
        string json = JsonUtility.ToJson(_save);                    //セーブデータをjson文字列に変換
        StreamWriter streamWriter = new StreamWriter(_filePath);    //_filePathにjson文字列を保存するテキストファイル作成
        streamWriter.Write(json); streamWriter.Flush();             //json文字列の書き込み、書き込み操作の確定
        streamWriter.Close();                                       //ファイルを閉じて書き込み終了
    }

    //ゲーム開始時、プレイヤーDead時にロードする
    public void Load()
    {
        if (File.Exists(_filePath))     //ファイルが存在するとき
        {
            StreamReader streamReader = new StreamReader(_filePath);    //_filePathのjson文字列を読み込むファイル作成
            string data = streamReader.ReadToEnd();                     //ファイル全体を読み込みdataに格納
            streamReader.Close();                                       //ファイルを閉じて読み込み終了
            _save = JsonUtility.FromJson<SaveDataManager>(data);        //json文字列をセーブデータに変換
            Debug.Log($"ロード時のnowSavePos:{_save._nowSavePos}");
            
        }
        else                           //ファイルが存在しないとき
        {
            Debug.Log("セーブデータが見つかりません。新しいゲームを開始します。");
            _save._nowSavePos = _playerStartPos;                        //デフォルトのプレイヤーの開始位置を格納
        }

        
    }

    //プレイヤーDead時に一定時間待つ
    void WaitGameRestart()
    {
        Observable.Timer(TimeSpan.FromSeconds(_load_wait_time)).Subscribe(_ => GameRestart());
    }

    //プレイヤーDead時に一定時間経過後、セーブデータをセーブした後リロードする
    private void GameRestart()
    {
        
        SceneManager.LoadScene(_sceneName);
    }


}

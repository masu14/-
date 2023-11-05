using UnityEngine;
using UniRx;

/// <summary>
/// 
/// カメラの位置を制御するクラス、プレイヤーのいるステージの座標をStageManagerから購読し、プレイヤーのいるステージを写す。
/// 
/// 仕様：カメラはプレイヤーがステージのコライダーに接触したらそのステージを写す(StageManagerから購読)
///       ロード時に最後に触れたセーブポイントのあるステージを写す(GameManagerから購読)
///       
/// バグ：ロード時にGameManagerクラスから送信されるセーブデータ(セーブポイントのあるステージの座標)が購読されず、
///     　(x,y)=(cameraWidth,cameraHeight)のステージを写してしまう。
///     　
/// 代替案：Updateメソッドで毎フレーム、プレイヤーとステージの接触の検知を監視してカメラ処理を行う
///         この方法はStartメソッド時に行われないので、一瞬だけ(x,y)=(cameraWidth,cameraHeight)のステージを写してしまう。
///         またUpdateで呼ぶのは無駄の負荷をかけてしまう。
///         
/// </summary>
/// 
public class CameraManager : MonoBehaviour
{
    
    private const int _CAMERAWIDE = 18;                         //画面横幅
    private const int _CAMERAHEIGHT = 10;                       //画面縦幅
    private float _edgeRight, _edgeLeft, _edgeUp, _edgeDown;    //カメラに映る端の座標成分                                   
    private Vector2 _nowStage;                                  //プレイヤーがいるステージ

    void Awake()
    {
        //バグ該当箇所、ロード時に送信されるセーブデータの購読をする処理
        /*
        var _gameManager = FindObjectOfType<GameManager>();

       
        _gameManager.OnSaveStage
           .Subscribe(x =>
           {
               UpdateCameraPos(x);
               Debug.Log($"ロード時のステージの座標{x}");

           })
           .AddTo(this);
        */

        StageManager[] stageManagers = FindObjectsOfType<StageManager>();   //ロード時にシーン上のすべてのステージクラスを取得しておく

        //各ステージクラスのステージの座標の購読
        foreach (StageManager stageManager in stageManagers)
        {
            stageManager.OnPlayerEnter
                .Subscribe(pos => {
               // UpdateCameraPos(pos);                                     //本来はプレイヤーとステージの接触を検知したらカメラ処理のメソッドを呼び出すのが望ましい
                _nowStage = stageManager.transform.position;                //代替案、接触したステージを_nowStageに格納
            } )
                .AddTo(this);
        }


    }


    //  カメラ処理のメソッド、引数の座標からカメラの位置合わせを行う
    private void UpdateCameraPos(Vector2 nowStagePos)
    {
        _edgeLeft = nowStagePos.x - _CAMERAWIDE / 2;        //カメラ左端
        _edgeRight = nowStagePos.x + _CAMERAWIDE / 2;       //カメラ右端
        _edgeUp = nowStagePos.y + _CAMERAHEIGHT / 2;        //カメラ上端
        _edgeDown = nowStagePos.y - _CAMERAHEIGHT / 2;      //カメラ下端

        //カメラ更新、カメラの端に触れたときカメラの座標を更新する
        //水平方向
        if (_edgeLeft >= transform.position.x)          //右のステージに更新
        {
            transform.position += _CAMERAWIDE * Vector3.right;
        }
        else if(_edgeRight <= transform.position.x)     //左のステージに更新
        {
            transform.position -= _CAMERAWIDE * Vector3.right;
        }

        if(_edgeDown >= transform.position.y)           //上のステージに更新
        {
            transform.position += _CAMERAHEIGHT * Vector3.up;
        }
        else if (_edgeUp <= transform.position.y)       //下のステージに更新
        {
            transform.position -= _CAMERAHEIGHT * Vector3.up;
        }
    
    }
    

    //代替案、_nowStageからカメラの位置を更新する
    
    void Update()
    {
        _edgeLeft = _nowStage.x - _CAMERAWIDE / 2;      //カメラ左端
        _edgeRight = _nowStage.x + _CAMERAWIDE / 2;     //カメラ右端
        _edgeUp = _nowStage.y + _CAMERAHEIGHT / 2;      //カメラ上端
        _edgeDown = _nowStage.y - _CAMERAHEIGHT / 2;    //カメラ下端

        //カメラ更新、カメラの端に触れたときカメラの座標を更新する
        //水平方向
        if (_edgeLeft >= transform.position.x)          //右のステージに更新
        {
            transform.position += _CAMERAWIDE * Vector3.right;
        }
        else if (_edgeRight <= transform.position.x)    //左のステージに更新
        {
            transform.position -= _CAMERAWIDE * Vector3.right;
        }

        //垂直方向
        if(_edgeDown >= transform.position.y)           //上のステージに更新
        {
            transform.position += _CAMERAHEIGHT * Vector3.up;
        }
        else if (_edgeUp <= transform.position.y)       //下のステージに更新
        {
            transform.position -= _CAMERAHEIGHT * Vector3.up;
        }
    }
    
    
    
}

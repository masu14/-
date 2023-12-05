using System.Collections;
using UnityEngine;
using UniRx;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Merusenne.StageGimmick;

/// <summary>
/// 
/// ロード時のフェードイン、ゴール時のフェードアウトを行うクラス
/// フェードインはStartから徐々にアルファ値を小さくする
/// フェードアウトはGoalオブジェクトに触れたときにアルファ値を大きくする
/// TitleSceneとStageSceneの両方で使う
/// 
/// </summary>
public class FadeController : MonoBehaviour
{
    private Image _fade;                        //UI>Imageオブジェクト、一番手前のレイヤーで基本透明
    private GameObject _goal;                   //Goalオブジェクト、触れるとゲームクリアとなり、画面がフェードアウト(白)する
    private GoalController _goalController;     //Goalオブジェクトがプレイヤーとの接触を感知したときにその状態を送信し、このクラスがそれを購読する

    [SerializeField] private  float _fade_in_time = 2.0f;   //フェードインするまでの時間
    [SerializeField] private float _fade_out_time = 2.0f;   //フェードアウトするまでの時間


    private Color32 _startInColor = new Color32(0, 0, 0, 255);          //フェードイン前の色(黒)
    private Color32 _startOutColor = new Color32(255, 255, 255, 0);     //フェードアウト前の色(透明)
    private Color32 _endOutColor = new Color32(255, 255, 255, 255);     //フェードアウト後の色(白)

    private void Awake()
    {
        //ステージシーン上のGoalオブジェクトとそのスクリプトを取得し、購読の手続きをする
        if (SceneManager.GetActiveScene().name == "StageScene")
        {
            _goal = GameObject.FindWithTag("Goal");
            _goalController = _goal.GetComponent<GoalController>();

            //Goalオブジェクトに接触した際にFadeOut()の処理を実行
            _goalController.OnGoal
           .Subscribe(_ => StartFadeOut())
           .AddTo(this);
        }

        //フェードイン、アウトを行うオブジェクトを取得
        if (_fade == null)
        {
            _fade = GetComponent<Image>();
        }
    }

    void Start()
    {
        StartCoroutine(FadeIn());       //シーンロード時にフェードインが入る
    }

    private void StartFadeOut()
    {
        StartCoroutine(FadeOut());
    }

    //暗い画面から徐々にアルファ値を下げ透明にする
    private IEnumerator FadeIn()        //StartCoroutineで呼ぶ
    {
        var timer = 0.0f;   //アルファ値を時間変化させるために使う

        //アルファ値の大きさを時間変化により徐々に小さくする
        while (timer < _fade_in_time)
        {
            timer += Time.deltaTime;
            float alpha = 1.0f -timer / _fade_in_time;      //一次関数
            Color newColor = _startInColor;
            newColor.a = alpha;
            _fade.color = newColor;

            yield return null;
        }
    }

    //Goalオブジェクトに触れたら呼び出され、画面が徐々に白くなる
    private IEnumerator FadeOut()
    {
        Debug.Log("FadeOut");
        var timer = 0.0f;
        Color startColor = _startOutColor;  //透明
        Color endColor = _endOutColor;      //白

        //アルファ値の大きさを時間変化により徐々に大きくする
        while(timer<_fade_out_time)
        {
            timer += Time.deltaTime;
            float alpha = timer / _fade_out_time;
            Color newColor = _startOutColor;
            newColor.a = alpha;
            _fade.color = newColor;

            yield return null;

        }
    }
}

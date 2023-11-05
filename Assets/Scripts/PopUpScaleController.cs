using UnityEngine;
using UniRx;
using System;

/// <summary>
/// チュートリアル画像のポップアップのサイズをコントロールするクラス
/// PopUpControllerクラスでインスタンスを生成し、Open(),Close()メソッドが呼ばれたとき
/// ポップアップのサイズを指定のパラメータで変更する
/// パラメータの変更はtutorialオブジェクトのPopUpControllerコンポーネント内で変更できる
/// </summary>

[Serializable]
public class PopUpScaleController
{
    [SerializeField] Vector2 _from_size, _to_size;                                                          //PopUpの最初と最後の大きさ
    [SerializeField] float _duration;                                                                       //PopUpのサイズ変更にかかる時間
    [SerializeField] AnimationCurve _curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));    //サイズ変更のアニメーションを変更できる
    
    private GameObject _target;         //サイズ変更を行うポップアップのオブジェクトを格納
    
    private Subject<Unit> _scaleEndStream = new Subject<Unit>();                                    //ポップアップのサイズ変更後の通知
    public IObservable<Unit> OnscaleEnd { get { return _scaleEndStream.AsObservable(); } }          //ポップアップのサイズ変更後の通知を送信


    //ポップアップを開くとき、閉じるとき用にそれぞれインスタンスから呼び出される
    public void Setup(GameObject t)
    {
        _target = t;    //ポップアップのオブジェクト格納
    }

    //開くとき、閉じるときにサイズ変更を行う
    public void Play()
    {
        //フレーム間の時間を使って処理を行う、サイズ変更が終わったら通知を送信してオブジェクトを破棄する
        Observable.EveryFixedUpdate()                   
            .Take(TimeSpan.FromSeconds(_duration))
            .Select(_ => Time.fixedDeltaTime)
            .Scan((acc, current) => acc + current)
            .Subscribe(time => {
                float t = time / _duration;
                _target.transform.localScale = Vector3.Lerp(_from_size, _to_size, _curve.Evaluate(t));  //サイズ変更を行う処理
            },
            _ => {
            },
            () => _scaleEndStream.OnNext(Unit.Default)
        ).AddTo(_target);
    }
}
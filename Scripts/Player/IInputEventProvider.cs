using UniRx;

namespace Merusenne.Player
{
    /// <summary>
    ///入力を定義するインターフェース、参照から入力イベントに依存したスクリプトに移動できる
    ///入力の追加、削除はここから行う
    /// </summary>

    public interface IInputEventProvider
    {

        IReadOnlyReactiveProperty<float> AxisH { get; }         //水平方向の入力
        IReadOnlyReactiveProperty<bool> IsJump { get; }         //ジャンプの入力
        IReadOnlyReactiveProperty<bool> IsLeftSwitch { get; }   //ショット切り替え(左)の入力
        IReadOnlyReactiveProperty<bool> IsRightSwitch { get; }  //ショット切り替え(右)の入力
        IReadOnlyReactiveProperty<bool> IsShot { get; }         //ショット発射の入力
        IReadOnlyReactiveProperty<bool> IsThrough { get; }      //すり抜け床を降りる入力
    }
}
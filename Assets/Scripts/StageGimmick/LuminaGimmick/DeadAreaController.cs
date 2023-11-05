using UnityEngine;

namespace Merusenne.StageGimmick.LuminaGimmick
{
    /// <summary>
    /// DeadArea(Barrierの子オブジェクト)のスクリプトコンポーネント
    /// 親のBarrierがactive状態のときコライダーをactiveに、非activeのとき非activeにする
    /// BarrierがactiveになったときにBarrierと重なる位置にいるとプレイヤーはDeadする
    /// </summary>

    public class DeadAreaController : MonoBehaviour
    {
        private BoxCollider2D _myCollider;
        private BoxCollider2D _parentCollider;

        void Start()
        {
            _myCollider = GetComponent<BoxCollider2D>();                                //DeadAreaのコライダー
            _parentCollider = transform.parent.GetComponent<BoxCollider2D>();           //親のBarrierのコライダー

            SyncColliderState();        //コライダーの状態を初期化

        }

        void Update()
        {
            //コライダーの状態をそろえる
            if (_myCollider.enabled != _parentCollider.enabled)
            {
                SyncColliderState();
            }
        }

        //DeadAreaのコライダーのactive状態を親のBarrierとそろえる
        void SyncColliderState()
        {
            Debug.Log("SyncColliderState");
            _myCollider.enabled = _parentCollider.enabled;
        }
    }

}

using UnityEngine;

namespace Merusenne.StageGimmick
{

    /// <summary>
    /// ステージの隠し通路用のカバーを制御するクラス
    /// </summary>
    public class GrayCoverController : MonoBehaviour
    {

        //触れたら消える
        private void OnTriggerEnter2D(Collider2D collider)
        {
            Destroy(gameObject);
        }
    }

}

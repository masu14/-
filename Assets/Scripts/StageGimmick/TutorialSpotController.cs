using UnityEngine;


namespace Merusenne.StageGimmick
{
    /// <summary>
    /// TutorialSpotに付けるスクリプトコンポーネント
    /// プレイヤーがTutorialSpot(緑色の床)のコライダーに触れたときにチュートリアルを表示、離れたときにチュートリアルを閉じる
    /// </summary>

    public class TutorialSpotController : MonoBehaviour
    {
        [SerializeField] private GameObject _tutorial;      //表示するチュートリアル画像のオブジェクトを設定する

        //緑色の床に触れたときチュートリアルを開く
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.tag == "Player")
            {
                _tutorial.GetComponent<PopUpController>().Open();
            }
        }

        //緑色の床から離れたときチュートリアルを閉じる
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.tag == "Player")
            {
                _tutorial.GetComponent<PopUpController>().Close();
            }
        }
    }

}

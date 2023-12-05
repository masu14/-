using System;
using UnityEngine;

/// <summary>
/// 保存しておくデータを管理するスクリプト
/// GameManagerクラスでデータの読み書きを行いjson形式でAssetフォルダ内に保存する
/// </summary>
[Serializable]
public class SaveDataManager 
{
    //セーブデータ
    public Vector2 _nowSavePos;                    //セーブポイント座標
    public Vector2 _nowStagePos;                   //セーブポイントのあるステージの座標
    public bool _isFisrtPlay = false;              //ゲーム初プレイ時にアクション操作のチュートリアルを表示するためのフラグ
}

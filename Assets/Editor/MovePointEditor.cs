using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MovePoint))]
public class MovePointEditer : Editor
{
    //変数に格納する
    MovePoint movepoint => target as MovePoint;

    private void OnSceneGUI()
    {
        //色を指定
        Handles.color = Color.yellow;

        for (int i = 0; i < movepoint.points.Length; i++)
        {
            //EndChangeCheckとの間でシーン内での変化がないのか確認する
            EditorGUI.BeginChangeCheck();
            //ポイントの位置を取得
            Vector3 currentWaypoint= movepoint.points[i];

            //ハンドルを生成して変数に格納する
            var fmh_26_35_638977664016769440 = Quaternion.identity; Vector3 newWaypoint = Handles.FreeMoveHandle
                (currentWaypoint, 0.7f, new Vector3(0.3f, 0.3f, 0.3f), Handles.SphereHandleCap);

            //ハンドル番号の生成
            GUIStyle textStyle= new GUIStyle();
            textStyle.fontSize= 18;
            textStyle.normal.textColor = Color.white;
            Vector3 textPos = Vector3.down * 0.35f + Vector3.right * 0.35f;

            //ラベルの発生位置、表示内容、GUIの設定
            Handles.Label(movepoint.points[i] + textPos, $"{i + 1}", textStyle);

            EditorGUI.EndChangeCheck();

            if (EditorGUI.EndChangeCheck())
            {
                //位置の保存CTRL+Zで戻せるようにする
                Undo.RecordObject(target, "Move");

                //動かしたハンドル位置に変更する
                movepoint.points[i] = newWaypoint;
            }

        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

public class AddBoneName : EditorWindow
{
    private Vector2 scrollPosition = Vector2.zero;

    [SerializeField]
    string addName = null;                                      // 付け足す名前

    [SerializeField]
    bool explainFolding = false;                                // ヘルプ（説明）の出し入れ
    [SerializeField]
    bool useFolding = false;                                    // ヘルプ（使い方）の出し入れ

    [SerializeField]
    List<GameObject> targetObjects = new List<GameObject>();    // 対象となるオブジェクト

    [SerializeField]
    int targetNum = 0;                                          // 対象となるオブジェクトのリストの長さ
    [SerializeField]
    int selected = 0;                                           // どこに付け足すのか選択

    int groupID = 0;                                            // Undoをグループにするため

    SerializedObject so;                                        

    [MenuItem("MyTools/AddBoneName")]
    static void Open()
    {
        // inspectorウィンドウの横につきます
        GetWindow<AddBoneName>("AddBoneName",
            typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow"));
    }

    SerializedProperty addNameProperty;
    SerializedProperty explainfoldingProperty;
    SerializedProperty useFoldingProperty;
    SerializedProperty selectedProperty;

    private void OnEnable()
    {
        so = new SerializedObject(this);

        addNameProperty = so.FindProperty("addName");
        explainfoldingProperty = so.FindProperty("explainFolding");
        useFoldingProperty = so.FindProperty("useFolding");
        selectedProperty = so.FindProperty("selected");

        // グループIDを保持
        groupID = Undo.GetCurrentGroup();
    }

    private void OnDisable()
    {
        // Undoをグループ化
        Undo.CollapseUndoOperations(groupID);

    }

    private void OnGUI()
    {
        // ここから描画内容＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
        GUILayout.BeginVertical(GUI.skin.box);
        {
            // 位置の調整
            EditorGUILayout.Space();


            GUILayout.Label("付け足したい名前を記入");

            // 記入変更がされたか調べる
            EditorGUI.BeginChangeCheck();

            addNameProperty.stringValue = GUILayout.TextField(addNameProperty.stringValue);

            // もしもされたら
            if (EditorGUI.EndChangeCheck())
            {
                //Debug.Log("addNameProperty = changed");

                // スペースがあったら強制的に消す
                addNameProperty.stringValue = Regex.Replace(addNameProperty.stringValue, @"\s", "");
            }

            // 頭につけるのか最後につけるのか選択する (Head = 0 , Tail = 1)
            GUILayout.Label("頭につけるのか尾につけるのか選択");
            selectedProperty.intValue = GUILayout.Toolbar (selectedProperty.intValue, new string[] {"Head", "Tail"});

            // 対象オブジェクト取得
            GUILayout.Label("対象オブジェクト");
            if (GUILayout.Button("getObject"))
            {
                //Debug.Log("moveGetObject");
                GetObject();

            }

            // 取得オブジェクト表示
            GUILayout.Label("取得オブジェクト");

            // スクロール可能にする
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            {
                // 取得オブジェクト表示
                ShowSelectObjects();

                // 名前を付け足す
                GUILayout.Label("名前を付け足す");
                if (GUILayout.Button("AddName"))
                {
                    //Debug.Log("MoveAddName");
                    AddName();
                }

                // 付け足したのを消す
                //GUILayout.Label("付け足したのを消す");
                //if (GUILayout.Button("DeleteName"))
                //{
                //    DeleteName();
                //}

                // 説明
                if (explainfoldingProperty.boolValue = EditorGUILayout.Foldout(explainfoldingProperty.boolValue, "説明", true))
                {
                    EditorGUILayout.HelpBox("・スペースの入力は不可能になってます。\n" +
                        "・選択オブジェクトはHierarchy上だけでなく、Projectで選択しているものも対象になります。\n" +
                        "・対象オブジェクトを決めた後にHead、Tailを変更すると取得オブジェクトがバグります。" +
                        "もう一度対象オブジェクトを指定してください。\n" +
                        "・DeleteNameは複数回適用したものも全部消えます。\n" +
                        "・AddName、DeleteNameするときは毎回getObjectしてください。\n" +
                        "・Ctrl + Zで戻すことは不可能です。アンドゥ操作すべてが無効です。\n" +
                        "", MessageType.Info);
                }

                // 使い方
                if(useFoldingProperty.boolValue = EditorGUILayout.Foldout(useFoldingProperty.boolValue, "使い方", true))
                {
                    EditorGUILayout.HelpBox("・付け足したい名前を入力\n" +
                        "・Head,Tailを選択。\n" +
                        "・hierarchyWindowで付け足したいオブジェクトを選択（複数選択可能）\n" +
                        "・hierarchyWindowでの複数選択は shift + 左クリック\n" +
                        "・getObjectを押す\n" +
                        "・取得オブジェクトに入っているか確認。\n" +
                        "・大丈夫ならAddName。前後にアンダーバーが自動でつきます。\n" +
                        "・アンドゥ操作で戻すことはできないのでDeleteNameを使って消してください。", MessageType.Info);
                }
            }
            EditorGUILayout.EndScrollView();
        }
        GUILayout.EndVertical();

        // 内部キャッシュに値を保存する
        so.ApplyModifiedProperties();
    }

    // 対象オブジェクト取得
    void GetObject()
    {
        // 初期化処理
        targetNum = 0;
        targetObjects.Clear();

        targetNum = Selection.gameObjects.Length;
        for(int i = 0; i < targetNum; i++)
        {
            targetObjects.Add(Selection.gameObjects[i]);
        }

        //foreach (var obj in targetObjects)
        //{
        //    Debug.Log(obj);
        //}
    }

    // 取得オブジェクト表示
    void ShowSelectObjects()
    {
        GUILayout.BeginVertical(GUI.skin.box);
        {
            for (int i = 0; i < targetObjects.Count; i++)
            {
                if(targetObjects[i] == null)
                {
                    return;
                }

                GUILayout.Label(targetObjects[i].name);
            }
        }
        GUILayout.EndVertical();
    }

    // 名前を付け足す
    void AddName()
    {
        // 付け足したい名前が無記入なら出る。
        if(addNameProperty.stringValue == null || addNameProperty.stringValue == "")
        {
            return;
        }

        // Headの時
        for (int i = 0; i < targetObjects.Count; i++)
        {
            if (targetObjects[i] == null)
            {
                return;
            }

            // Headの時
            if (selectedProperty.intValue == 0)
            {
                Undo.RecordObject(targetObjects[i], "オブジェクトの名前変更");
                targetObjects[i].name = addNameProperty.stringValue + "_" + targetObjects[i].name;
            }
            // Tailの時
            else if (selectedProperty.intValue == 1)
            {
                Undo.RecordObject(targetObjects[i], "オブジェクトの名前変更");
                targetObjects[i].name = targetObjects[i].name + "_" + addNameProperty.stringValue;
            }
        }

    }

    // 付け足したのを消す(複数回適用したものも全部消える,前後すべて消す。)
    void DeleteName()
    {
        // Headの時
            for (int i = 0; i < targetObjects.Count; i++)
            {
                if (targetObjects[i] == null)
                {
                    return;
                }

                // Headの時
                if (selectedProperty.intValue == 0)
                {
                    targetObjects[i].name = targetObjects[i].name.Replace(addNameProperty.stringValue + "_", "");
                }
                // Tailの時
                else if (selectedProperty.intValue == 1)
                {
                    targetObjects[i].name = targetObjects[i].name.Replace("_" + addNameProperty.stringValue, "");
                }

            }
    }
}

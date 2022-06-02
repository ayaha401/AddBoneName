#pragma warning disable 0414

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

namespace AyahaTools.ABN
{
    public enum AddLocation
    {
        HEAD,
        TAIL,
    }

    public enum ResultMessage
    {
        NOTHING,
        SUCCESS,
        NO_STRING,
        UNSELECT,
    }

    public class AddBoneName : EditorWindow
    {
        private string _version = "1.1.1";

        private Vector2 scrollPosition = Vector2.zero;

        private int groupID = 0;

        [SerializeField] private string addName = null;
        [SerializeField] int selected = 0;

        private SerializedObject so;
        private SerializedProperty addNameProp;
        private SerializedProperty selectedProp;

        private int targetNum = 0;
        private List<GameObject> targetObjects = new List<GameObject>();

        private AddLocation selectedAddLocation = new AddLocation();
        private ResultMessage message = new ResultMessage();
        

        private void OnEnable()
        {
            so = new SerializedObject(this);

            addNameProp = so.FindProperty("addName");
            selectedProp = so.FindProperty("selected");

            groupID = Undo.GetCurrentGroup();

            message = ResultMessage.NOTHING;
        }


        [MenuItem("AyahaTools/AddBoneName")]
        private static void OpenWindow()
        {
            var window = GetWindow<AddBoneName>("AddBoneName", typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow"));
            window.titleContent = new GUIContent("AddBoneName");
        }

        void OnGUI()
        {
            Information();

            GUIPartition();

            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("付け足したい名前を記入");

                EditorGUI.BeginChangeCheck();
                addNameProp.stringValue = GUILayout.TextField(addNameProp.stringValue);
                if (EditorGUI.EndChangeCheck())
                {
                    // スペースがあったら強制的に消す
                    addNameProp.stringValue = Regex.Replace(addNameProp.stringValue, @"\s", "");
                }

                GUIPartition();

                // 頭につけるのか最後につけるのか選択する (Head = 0 , Tail = 1)
                EditorGUILayout.LabelField("頭につけるのか尾につけるのか選択");
                selectedProp.intValue = GUILayout.Toolbar(selectedProp.intValue, new string[] { "Head", "Tail" });
                selectedAddLocation = (AddLocation)selectedProp.intValue;

                GUIPartition();

                // 対象オブジェクト取得
                EditorGUILayout.LabelField("対象オブジェクト");
                if (GUILayout.Button("対象オブジェクト取得"))
                {
                    GetObject();
                }

                GUIPartition();

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                {
                    // 取得オブジェクト表示
                    ShowSelectObjects();
                }
                EditorGUILayout.EndScrollView();

                GUIPartition();

                // 名前を付け足す
                EditorGUILayout.LabelField("名前を付け足す");
                if (GUILayout.Button("付け足す"))
                {
                    AddName();
                }

                GUIPartition();

                // 結果を表示
                ShowResultMessage();
            }

            so.ApplyModifiedProperties();
        }

        private void Information()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Version");
                    EditorGUILayout.LabelField("Version " + _version);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("How to use (Japanese)");
                    if (GUILayout.Button("How to use (Japanese)"))
                    {
                        System.Diagnostics.Process.Start("https://github.com/ayaha401/AddBoneName");
                    }
                }
            }
        }

        private void GUIPartition()
        {
            GUI.color = Color.gray;
            GUILayout.Box("", GUILayout.Height(2), GUILayout.ExpandWidth(true));
            GUI.color = Color.white;
        }

        // 選択したオブジェクトを全取得
        private void GetObject()
        {
            targetNum = Selection.gameObjects.Length;
            targetObjects.Clear();

            for (int i = 0; i < targetNum; i++)
            {
                targetObjects.Add(Selection.gameObjects[i]);
            }
        }

        // 選択したオブジェクトを表示
        private void ShowSelectObjects()
        {
            for (int i = 0; i < targetObjects.Count; i++)
            {
                EditorGUILayout.LabelField(targetObjects[i].name);
            }
        }

        // 結果を表示
        private void ShowResultMessage()
        {
            switch (message)
            {
                case ResultMessage.SUCCESS:
                    EditorGUILayout.HelpBox("成功", MessageType.Info);
                    break;
                case ResultMessage.NO_STRING:
                    EditorGUILayout.HelpBox("失敗 : 付け足す文字が無いです", MessageType.Error);
                    break;
                case ResultMessage.NOTHING:
                    EditorGUILayout.HelpBox("特になし", MessageType.Info);
                    break;
                case ResultMessage.UNSELECT:
                    EditorGUILayout.HelpBox("オブジェクトが選択されていません", MessageType.Error);
                    break;
                default:
                    break;
            }
        }

        // 名前をつけ足す
        private void AddName()
        {
            bool noneString = addNameProp.stringValue == null || addNameProp.stringValue == string.Empty;
            if (noneString)
            {
                message = ResultMessage.NO_STRING;
                return;
            }

            bool unSelect = targetObjects.Count == 0;
            if(unSelect)
            {
                message = ResultMessage.UNSELECT;
                return;
            }

            // undo操作に対応
            GameObject[] undoObjArray = targetObjects.ToArray();
            Undo.RecordObjects(undoObjArray, "Add Name");

            for (int i = 0; i < targetObjects.Count; i++)
            {
                if (targetObjects[i] == null) return;

                switch (selectedAddLocation)
                {
                    case AddLocation.HEAD:
                        targetObjects[i].name = addNameProp.stringValue + "_" + targetObjects[i].name;
                        break;

                    case AddLocation.TAIL:
                        targetObjects[i].name = targetObjects[i].name + "_" + addNameProp.stringValue;
                        break;
                }

                message = ResultMessage.SUCCESS;
            }
        }
    }
}



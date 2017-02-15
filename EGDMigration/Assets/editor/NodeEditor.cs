using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Node))]
public class NodeEditor : Editor {
    bool selected = false;
    Color color = Color.red;
    static Node baseNode = null;
    static Node lastBaseNode = null;
    static int hoveringSomething = 0;
    static Vector3 lastPosition = new Vector3(0, 0, 0);

    public void OnSceneGUI()
    {
        Node t = target as Node;
        if ( t == null )
        {
            SceneView.onSceneGUIDelegate -= SceneUpdate;
            return;
        }

        int id = GUIUtility.GetControlID(FocusType.Passive);

        Vector3 screenPosition = Handles.matrix.MultiplyPoint(t.transform.position);
        Matrix4x4 cachedMatrix = Handles.matrix;

        EventType ev = Event.current.GetTypeForControl(id);
        switch (ev)
        {
            case EventType.MouseDown:
                if (HandleUtility.nearestControl == id && Event.current.button == 0)
                {
                    Debug.Log("Select " + t.Text);
                    selected = true;
                    color = Color.green;
                    baseNode = t;
                    lastBaseNode = t;
                    hoveringSomething = 0;

                    Selection.activeGameObject = t.gameObject;
                    Event.current.Use();
                    EditorGUIUtility.SetWantsMouseJumping(1);
                }
                break;

            case EventType.MouseUp:
                if (Event.current.button == 0)
                {
                    if (color == Color.blue)
                    {
                        Debug.Log("Dragged to "+t.Text);
                        Transition tr = lastBaseNode.MakeTransition(t);
                        Selection.activeGameObject = tr.gameObject;
                    }
                    if (color == Color.green)
                        baseNode = null;
                    selected = false;
                    color = Color.red;
                }
                break;

            case EventType.MouseDrag:
                if (baseNode != t)
                {
                    if (HandleUtility.nearestControl == id && Event.current.button == 0)
                    {
                        hoveringSomething ++ ;
                        color = Color.blue;
                    }
                    else if (color == Color.blue)
                    {
                        hoveringSomething--;
                        color = Color.red;
                    }
                }
                break;

            case EventType.Layout:
                Handles.matrix = Matrix4x4.identity;
                HandleUtility.AddControl(id, HandleUtility.DistanceToCircle(screenPosition, 0.5f));
                Handles.matrix = cachedMatrix;
                break;
        }

        Handles.color = color;
        Handles.SphereCap(0, t.transform.position, t.transform.rotation, 1.0f);
        if (selected)
        {
            SceneView sv = SceneView.lastActiveSceneView;
            float dist = Vector3.Distance(t.transform.position, sv.camera.transform.position);
            Vector2 mp = Event.current.mousePosition;
            lastPosition = sv.camera.ScreenToWorldPoint(new Vector3(mp.x, Screen.height - mp.y, dist));
            Handles.DrawDottedLine(t.transform.position, lastPosition, 10.0f);
            SceneView.RepaintAll();
        }
    }

    void SceneUpdate(SceneView sv)
    {
        OnSceneGUI();
    }

    void OnEnable()
    {
        SceneView.onSceneGUIDelegate += SceneUpdate;
    }

}

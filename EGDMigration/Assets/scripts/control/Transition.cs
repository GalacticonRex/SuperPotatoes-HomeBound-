using UnityEngine;
using System.Collections;

public class Transition : MonoBehaviour {

    public string Option = "";
    public Database db = null;
    public Node Begin = null;
    public Node Finish = null;

    public Node Traverse()
    {
        return Finish;
    }
    public bool Visible()
    {
        return true;
    }
    public bool Selectable()
    {
        return true;
    }
    public void Decouple()
    {
        if (Finish == null)
            return;
        Finish.NotifyDestruction(this);
        Begin = null;
        Finish = null;
    }

    void OnDestroy()
    {
        Decouple();
    }

    void OnDrawGizmos()
    {
        if (Begin == null || Finish == null)
            return;

        transform.position = (Begin.transform.position + Finish.transform.position) / 2.0f;
        transform.rotation = Quaternion.LookRotation(Finish.transform.position - Begin.transform.position);
        float distance = Vector3.Distance(Begin.transform.position, Finish.transform.position);

        Matrix4x4 cache = Gizmos.matrix;
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(1,1,1));
        Gizmos.matrix = rotationMatrix;

        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(new Vector3(0,0,0), new Vector3(0.1f, 0.1f, distance));
    }
}

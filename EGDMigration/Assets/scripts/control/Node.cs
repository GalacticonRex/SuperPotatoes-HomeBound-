using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Node : MonoBehaviour {

    public string Text = "";
    public float TestSpeed = 1.0f;
    public List<Character> Characters = new List<Character>();

    private List<Transition> _transitionTermination = new List<Transition>();

    public Transition[] GetTransitions()
    {
        return GetComponentsInChildren<Transition>();
    }
    public void NotifyDestruction(Transition t)
    {
        _transitionTermination.Remove(t);
    }
    public Transition MakeTransition(Node terminate)
    {
        Transition[] transitions = GetComponentsInChildren<Transition>();
        foreach( Transition tran in transitions)
        {
            if (tran.Traverse() == tran)
                return tran;
        }

        GameObject go = new GameObject("Transition");
        Transition nt = go.AddComponent<Transition>();

        nt.db = FindObjectOfType<Database>();
        nt.Begin = this;
        nt.Finish = terminate;

        go.transform.parent = transform;

        terminate._transitionTermination.Add(nt);

        return nt;
    }

    void OnDestroy()
    {
        Transition[] transitions = transform.GetComponentsInChildren<Transition>();
        foreach (Transition tran in transitions)
        {
            tran.Decouple();
            GameObject.Destroy(tran.gameObject);
        }
        foreach (Transition tran in _transitionTermination)
        {
            tran.Decouple();
            GameObject.Destroy(tran.gameObject);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }

}

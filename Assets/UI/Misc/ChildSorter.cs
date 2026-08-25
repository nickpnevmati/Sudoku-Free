using UnityEngine;

[ExecuteAlways]
public class ChildSorter : MonoBehaviour
{
    void Update()
    {
        // Get all direct children
        var children = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            children[i] = transform.GetChild(i);
        }

        // Sort by name
        System.Array.Sort(children, (a, b) => int.Parse(a.name.Split('.')[1]).CompareTo(int.Parse(b.name.Split('.')[1])));

        // Set sibling index according to sorted order
        for (int i = 0; i < children.Length; i++)
        {
            children[i].SetSiblingIndex(i);
        }
    }
}

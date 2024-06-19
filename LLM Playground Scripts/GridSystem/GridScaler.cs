using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GridScaler : MonoBehaviour
{
    public Vector2Int newScale;

    [SerializeField]
    GameObject gridVisualization;

    [SerializeField]
    GameObject floor;
    void Start()
    {
        gridVisualization.transform.localScale = new Vector3Int(newScale.x, 1, newScale.y);
        floor.transform.localScale = new Vector3Int(newScale.x, 1, newScale.y);
    }
}

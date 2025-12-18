using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AtlasSpace.GUI
{
    public class DrawSittingModel : MonoBehaviour
    {
        [SerializeField] GameObject sittingprefab;




        private void OnDrawGizmos()
        {
            for (int i = 0; i < sittingprefab.transform.childCount; i++)
            {
                Gizmos.matrix = this.transform.localToWorldMatrix;
                Gizmos.color = new Color(1, 0, 0, .5f);
                Gizmos.DrawCube(sittingprefab.transform.GetChild(i).position + new Vector3(0, -0.5f, -0.8f), sittingprefab.transform.GetChild(i).transform.localScale);
            }
        }
    }
}

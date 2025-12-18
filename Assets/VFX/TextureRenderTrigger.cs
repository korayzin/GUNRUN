using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AtlasSpacePUN
{
    public class TextureRenderTrigger : MonoBehaviour
    {
        public Transform sp;

        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        void OnTriggerEnter(Collider nesne)
        {
            if (nesne.gameObject.tag == "Player" && sp != null)
            {
                nesne.gameObject.transform.position = sp.position;
            }
        } 
    }
}
 
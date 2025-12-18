using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AtlasSpacePUN
{
    public class Lookatcamera : MonoBehaviour {

        public Camera my_camera;


        // Update is called once per frame
        void Update()
        {
            if(my_camera == null)
                my_camera = Camera.main;
            if (my_camera == null)
                return;
            transform.LookAt(transform.position + my_camera.transform.rotation * Vector3.forward,
                                my_camera.transform.rotation * Vector3.up);
        }
    }
}

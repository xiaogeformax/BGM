using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    public Transform mario;

    void LateUpdate() {
        if (mario.transform != null)
        {
            this.transform.localPosition = new Vector3(mario.transform.localPosition.x, 0, -20);
        }
    }
}

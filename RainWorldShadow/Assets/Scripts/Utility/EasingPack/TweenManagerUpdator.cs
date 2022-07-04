using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenManagerUpdator : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        TweenManager.Instance.Update(Time.deltaTime);
    }

    private void OnDestroy()
    {
        TweenManager.Instance.Cleanup();
    }
}

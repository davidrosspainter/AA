using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfettiController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(killConfetti());
    }

    private IEnumerator killConfetti()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(this.gameObject);
    }
}

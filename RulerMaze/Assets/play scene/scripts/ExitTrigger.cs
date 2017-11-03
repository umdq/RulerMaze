using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitTrigger : MonoBehaviour {
    public GameObject destroyPSPrefab;
    public ManageGame gameManager;//待统为UnityEvent，message to game manager去处理

    void OnTriggerEnter(Collider other)//这里触发物一定是球，编码很硬
    {
        GameObject ps=Instantiate(destroyPSPrefab, other.transform.position, Quaternion.identity);
        gameManager.HandleOneBallExitMessage();
        Destroy(other.gameObject);
        Destroy(ps,5f);
    }
	

}

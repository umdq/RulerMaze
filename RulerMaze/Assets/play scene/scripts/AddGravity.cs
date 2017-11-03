using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddGravity : MonoBehaviour {

    public Vector3 startPoint;
    public GameObject ballPrefab;
    public int ballNum = 1;
    private List<Rigidbody> balls=new List<Rigidbody>();

    //debug acceleration
    public Text accelerationInfo;

    public float gScale = 1.0f;

	void Start () 
    {
        GameObject go;
        for (int i = 0; i <ballNum; i++)
        {
            go=Instantiate(ballPrefab, startPoint, Quaternion.identity);
            balls.Add(go.GetComponent<Rigidbody>());
        }
	}

    public void ResetBalls()
    {
        GameObject go;
        for (int i = 0; i < ballNum; i++)
        {
            if (balls[i] == null)
            {
                go = Instantiate(ballPrefab, startPoint, Quaternion.identity);
                balls[i] = go.GetComponent<Rigidbody>();
            }
            else
            {
                balls[i].transform.position = startPoint;
            }
        }

    }
	
	void Update () 
    {
        /*
        //本陀螺仪斜面，先用键盘加力试一下
        float maxForce=5.0f;
        Vector3 dir = new Vector3(Input.GetAxis("Horizontal"),  0, Input.GetAxis("Vertical"));
        dir.Normalize();
        //到时除了斜面分力，还要加支持力，因为影响摩擦
        Vector3 gravity=dir*maxForce;
        gravity += new Vector3(0, -9.8f, 0);//virtual world space中的重力，倾斜是在reality world space中
        */

        /*
        //debug acceleration
        if (accelerationInfo != null)
        {
            accelerationInfo.text = "acceleration x:" + Input.acceleration.x.ToString("0.000")
                + "\nacceleration y:" + Input.acceleration.y.ToString("0.000")
                + "\nacceleration z:" + Input.acceleration.z.ToString("0.000");
        }
            
        //要真正match有点困难，暂且先只加斜面分力吧,而且z倾斜即绕x的旋转不符合重力，只有左右倾斜符合…
        //——》可能需要真正倾斜地面，这样的话需要把camera也纳入child
        //——》不用
        //Vector3 dir = new Vector3(Input.acceleration.x,  0, -Input.acceleration.z);
        Vector3 dir = new Vector3(Input.acceleration.x,  0, Input.acceleration.y);
        dir.Normalize();
        Vector3 gravity = dir * 9.8f;
        */

        //physically based obliquity
        //原思路是，XZ切向量叉乘出normal，然后gravity关于斜面投影，再从reality W转到virtual W
        //还不如一开始就分开来别合并
        float g=9.8f*gScale;
        float xAngle=Input.acceleration.x*Mathf.PI/2.0f;
        float Fx = Mathf.Sin(xAngle) * g;
        //X contribution to normal
        float Nx=Mathf.Cos(xAngle)*g;
        float zAngle=Input.acceleration.y*Mathf.PI/2.0f;
        float Fz = Mathf.Sin(zAngle) * g;
        //Z contribution to normal
        float Nz=Mathf.Cos(zAngle)*g;

        foreach (Rigidbody rb in balls)
        {
            if(rb!=null)//可能已出去被销毁了
            {
                //rb.AddForce(gravity);

                Vector3 F = new Vector3(Fx , -Nz, Fz);//note it's not force but acceleration
                F*=rb.mass;
                rb.AddForce(F);
            }
           
        }

	}

}

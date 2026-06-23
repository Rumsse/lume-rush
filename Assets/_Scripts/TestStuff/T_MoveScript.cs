using UnityEngine;

public class T_MoveScript : MonoBehaviour
{
    public Rigidbody rb;
    public float moveSpeed = 20;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W)) rb.AddForce(new Vector3(moveSpeed, 0, 0));
        if (Input.GetKey(KeyCode.A)) rb.AddForce(new Vector3(0, 0, moveSpeed));
        if (Input.GetKey(KeyCode.S)) rb.AddForce(new Vector3(-moveSpeed, 0, 0));
        if (Input.GetKey(KeyCode.D)) rb.AddForce(new Vector3(0, 0, -moveSpeed));

        //if (Input.GetKey(KeyCode.W)) rb.linearVelocity = new Vector3(moveSpeed, 0, 0); else if (Input.GetKeyUp(KeyCode.W)) rb.linearVelocity = new Vector3(0, 0, 0);
        //if (Input.GetKey(KeyCode.A)) rb.linearVelocity = new Vector3(0, 0, moveSpeed); else if (Input.GetKeyUp(KeyCode.A)) rb.linearVelocity = new Vector3(0, 0, 0);
        //if (Input.GetKey(KeyCode.S)) rb.linearVelocity += new Vector3(-moveSpeed, 0, 0); else if (Input.GetKeyUp(KeyCode.S)) rb.linearVelocity = new Vector3(0, 0, 0);
        //if (Input.GetKey(KeyCode.D)) rb.linearVelocity = new Vector3(0, 0, -moveSpeed); else if (Input.GetKeyUp(KeyCode.D)) rb.linearVelocity = new Vector3(0, 0, 0);



    }
}

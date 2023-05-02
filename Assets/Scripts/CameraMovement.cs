using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    [SerializeField] float speedMult;
    [SerializeField] float xlookSensitivity;
    [SerializeField] float ylookSensitivity;
    [SerializeField] GameObject cam;
    private float xRotation;
    private float yRotation;
    private bool focus;


    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    // Update is called once per frame
    void Update()
    { 
        //Left-Right movement
        transform.Translate(Vector3.right * speedMult * Time.deltaTime * Input.GetAxis("Horizontal"), Space.Self);
        //Forward-Backward movement
        transform.Translate(Vector3.forward * speedMult * Time.deltaTime * Input.GetAxis("Vertical"), Space.Self);
        //Up-Down movement
        transform.Translate(Vector3.up * speedMult * Time.deltaTime * Input.GetAxis("Jump"), Space.World);
        transform.Translate(Vector3.down * speedMult * Time.deltaTime * Input.GetAxis("Fire3"), Space.World);

        //Camera
        float mouseInputX = Input.GetAxisRaw("Mouse X") * xlookSensitivity;
        float mouseInputY = Input.GetAxisRaw("Mouse Y") * ylookSensitivity;
        xRotation += mouseInputX;
        yRotation -= mouseInputY;
        transform.rotation = Quaternion.Euler(0, xRotation, 0);
        cam.transform.rotation = Quaternion.Euler(yRotation, xRotation, 0);


    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMover : MonoBehaviour
{

    public float arrowSpeed = 150.0f;
    public float zoomSpeed = 2.0f;
    public float mapSize = 100.0f;
    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;
    public float minimumX = -360F;
    public float maximumX = 360F;
    public float minimumY = -60F;
    public float maximumY = 60F;
    private bool fill = false;

    float rotationY = 0F;

    void Start()
    {
        transform.position = new Vector3(50, 100, 50);
    }

    void Update()
    {
        // find the generate mesh map
        generate_map map = FindObjectOfType<generate_map>();

        // arrow keys or WSAD + ctrls and space for height
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            transform.Translate(new Vector3(arrowSpeed * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            transform.Translate(new Vector3(-arrowSpeed * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            transform.Translate(new Vector3(0, 0, arrowSpeed * Time.deltaTime));
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            transform.Translate(new Vector3(0, 0, -arrowSpeed * Time.deltaTime));
        }
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            transform.Translate(new Vector3(0, -arrowSpeed * Time.deltaTime, 0));
        }
        if (Input.GetKey(KeyCode.Space))
        {
            transform.Translate(new Vector3(0, arrowSpeed * Time.deltaTime, 0));
        }
        // Change fill content or not using Enter
        if (Input.GetMouseButtonDown(0))
        {
            fill = (fill ? false : true);
        }
        // Change level of details using key +/-
        if (Input.GetKey(KeyCode.Equals))
        {
            // delete generated stuff first
            foreach (GameObject o in Object.FindObjectsOfType<GameObject>())
            {
                if (o.tag != "important")
                    Destroy(o);
            }
            map.levelOfDetails = Mathf.Min(map.levelOfDetails + 1, 2);
            map.generateMap(fill);
        }
        if (Input.GetKey(KeyCode.Minus))
        {
            foreach (GameObject o in Object.FindObjectsOfType<GameObject>())
            {
                if (o.tag != "important")
                    Destroy(o);
            }
            map.levelOfDetails = Mathf.Max(map.levelOfDetails - 1, 0);
            map.generateMap(fill);
        }
        // Change seed to create a random new map, using left shift
        if (Input.GetKey(KeyCode.LeftShift))
        {
            foreach (GameObject o in Object.FindObjectsOfType<GameObject>())
            {
                if (o.tag != "important")
                    Destroy(o);
            }
            map.seed++;
            map.generateMap(fill);
        }
        // Change scale using mouse wheel
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            foreach (GameObject o in Object.FindObjectsOfType<GameObject>())
            {
                if (o.tag != "important")
                    Destroy(o);
            }
            map.scale += 3;
            map.generateMap(fill);
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            foreach (GameObject o in Object.FindObjectsOfType<GameObject>())
            {
                if(o.tag != "important")
                    Destroy(o); 
            }
            map.scale = Mathf.Max(1, map.scale - 3);
            map.generateMap(fill);
        }

        // rotate camera with mouse movement
        if (axes == RotationAxes.MouseXAndY)
        {
            float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;

            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

            transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
        }
        else if (axes == RotationAxes.MouseX)
        {
            transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
        }
        else
        {
            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

            transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
        }
        // clamp camera position to avoid going underground and out
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, 0, mapSize),
            Mathf.Clamp(transform.position.y, 2.0f, 1000.0f), 
            Mathf.Clamp(transform.position.z, 0, mapSize));
    }
}
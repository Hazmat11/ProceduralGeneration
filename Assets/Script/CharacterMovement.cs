using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [SerializeField] GameObject locked;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position += -transform.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position += -transform.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right;
        }
        if (!Input.GetKey(KeyCode.E))
        {
            float mouseX = Input.GetAxis("Mouse X") * 2;
            float mouseY = Input.GetAxis("Mouse Y") * 2;

            locked.SetActive(false);

            transform.eulerAngles += new Vector3(-mouseY, mouseX, 0);
        }
        else
        {
            locked.SetActive(true);
        }
    }
}
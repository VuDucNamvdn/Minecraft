using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

static class Constants
{
    public const float maxHumanFallSpeed = -55.5f;
}
public class PlayerController : MonoBehaviour
{
    #region Serialize vars
    [SerializeField] Transform mainCam;
    [SerializeField] World world;
    [SerializeField] float mouseSentivity = 10f;
    [SerializeField] float walkSpeed = 6f;
    [SerializeField] float jumpForce = 5f;
    [SerializeField] float gravity = -9.8f;
    [SerializeField] float playerRadius = 0.4f;
    [SerializeField] float boundTolerance = 0.1f;
    [SerializeField] float playerHeight = 1.8f;
    [SerializeField] float checkIncrement = 0.1f;
    [SerializeField] float reach = 8f;
    [SerializeField] Transform highlightBlock;
    [SerializeField] Transform placeBlock;
    [SerializeField] Text selectedBlockText;
    [SerializeField] byte selectedBlockIndex = 1;
    #endregion
    #region private vars
    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private float verticalMomentum = 0f;
    private bool jumpInput;
    private bool isSprinting;
    private bool isGrounded;
    private Vector3 velocity;

    #endregion
    #region MonoBehaviour
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        selectedBlockText.text = world.blockTypes[selectedBlockIndex].blockName + " block selected";
    }
    private void FixedUpdate()
    {
        CalculateVelocity();
        if (jumpInput&&isGrounded)
        {
            Jump();
        }
        transform.Rotate(Vector3.up * mouseHorizontal * mouseSentivity * Time.deltaTime);
        mainCam.Rotate(Vector3.right * mouseVertical * mouseSentivity * Time.deltaTime);
        mainCam.localRotation = new Quaternion(Mathf.Clamp(mainCam.localRotation.x, -0.5f, 0.5f), mainCam.localRotation.y, mainCam.localRotation.z, mainCam.localRotation.w);
    }
    private void Update()
    {
        GetPlayerInput();
        PlaceCursorBlock();
    }
    #endregion
    #region Movement
    private void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
    }
    private void CalculateVelocity()
    {
        //affect y-momen with gravity
        if (verticalMomentum > Constants.maxHumanFallSpeed)
        {
            verticalMomentum += Time.fixedDeltaTime * gravity;
        }

        if (isSprinting)
        {
            velocity = ((transform.forward * vertical + transform.right * horizontal).normalized * walkSpeed * 2 * Time.fixedDeltaTime);
        }
        else
        {
            velocity = ((transform.forward * vertical + transform.right * horizontal).normalized * walkSpeed * Time.fixedDeltaTime);
        }
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;
        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;
        if (velocity.y < 0)
        {
            velocity.y = CheckFallSpeed(velocity.y);
        }
        else if (velocity.y > 0)
        {
            velocity.y = CheckUpSpeed(velocity.y);
        }
        transform.Translate(velocity, Space.World);
    }
    #region Collision
    private float CheckFallSpeed(float downSpeed)
    {
        if (world.CheckForVoxel(transform.position + new Vector3(playerRadius, downSpeed, playerRadius)) ||
            world.CheckForVoxel(transform.position + new Vector3(-playerRadius, downSpeed, -playerRadius)) ||
            world.CheckForVoxel(transform.position + new Vector3(playerRadius, downSpeed, -playerRadius)) ||
            world.CheckForVoxel(transform.position + new Vector3(-playerRadius, downSpeed, playerRadius)))
        {
            isGrounded = true;
            return 0;
        }
        else
        {
            isGrounded = false;
            return Mathf.Max(downSpeed, -55.5f);
        }
    }
    private float CheckDownSpeed(float downSpeed)
    {
        if (world.CheckForVoxel(transform.position + new Vector3(playerRadius, downSpeed, playerRadius)) ||
            world.CheckForVoxel(transform.position + new Vector3(-playerRadius, downSpeed, -playerRadius)) ||
            world.CheckForVoxel(transform.position + new Vector3(playerRadius, downSpeed, -playerRadius)) ||
            world.CheckForVoxel(transform.position + new Vector3(-playerRadius, downSpeed, playerRadius)))
        {
            isGrounded = true;
            return 0;
        }
        else
        {
            verticalMomentum = 0;
            isGrounded = false;
            return downSpeed;
        }
    }
    private float CheckUpSpeed(float upSpeed)
    {
        if (world.CheckForVoxel(transform.position + new Vector3(playerRadius, upSpeed + playerHeight, playerRadius)) ||
            world.CheckForVoxel(transform.position + new Vector3(-playerRadius, upSpeed + playerHeight, -playerRadius)) ||
            world.CheckForVoxel(transform.position + new Vector3(playerRadius, upSpeed + playerHeight, -playerRadius)) ||
            world.CheckForVoxel(transform.position + new Vector3(-playerRadius, upSpeed + playerHeight, playerRadius)))
        {
            return 0;
        }
        else
        {
            return upSpeed;
        }
    }
    public bool front
    {

        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerRadius)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + playerRadius)))
                return true;
            else
                return false;
        }

    }
    public bool back
    {

        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerRadius)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - playerRadius)))
                return true;
            else
                return false;
        }

    }
    public bool left
    {

        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x - playerRadius, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x - playerRadius, transform.position.y + 1f, transform.position.z)))
                return true;
            else
                return false;
        }

    }
    public bool right
    {

        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x + playerRadius, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x + playerRadius, transform.position.y + 1f, transform.position.z)))
                return true;
            else
                return false;
        }

    }
    #endregion
    #endregion

    private void GetPlayerInput()
    {
        #region Movement Input
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");
        if (Input.GetButtonDown("Sprint"))
        {
            isSprinting = true;
        }
        if (Input.GetButtonUp("Sprint"))
        {
            isSprinting = false;
        }
        if (Input.GetButtonDown("Jump"))
        {
            jumpInput = true;
        }
        if (Input.GetButtonUp("Jump"))
        {
            jumpInput = false;
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            jumpInput = false;
        }
        #endregion
        #region Place/ Destroy Blocks
        float scroll = Input.GetAxis("MouseScrollWheel");
        if(scroll != 0)
        {
            if (scroll > 0)
                selectedBlockIndex++;
            else
                selectedBlockIndex--;
            if (selectedBlockIndex > (byte)(world.blockTypes.Length - 1))
                selectedBlockIndex = 1;
            if (selectedBlockIndex < 1)
                selectedBlockIndex = (byte)(world.blockTypes.Length - 1);
            selectedBlockText.text = world.blockTypes[selectedBlockIndex].blockName + " block selected";
        }

        if(highlightBlock.gameObject.activeSelf)
        {
            //Destroy Block
            if(Input.GetMouseButtonDown(0))
            {
                world.GetChunkFromVector3(highlightBlock.position).EditVoxel(highlightBlock.position, 0);
            }
            //Place Block
            if (Input.GetMouseButtonDown(1))
            {
                world.GetChunkFromVector3(highlightBlock.position).EditVoxel(placeBlock.position, selectedBlockIndex);
            }
        }
        #endregion
    }



    #region Raycast
    private void PlaceCursorBlock()
    {
        float step = checkIncrement;
        Vector3 lastPos = new Vector3();

        while (step < reach)
        {

            Vector3 pos = mainCam.position + (mainCam.forward * step);

            if (world.CheckForVoxel(pos))
            {
                highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                placeBlock.position = lastPos;

                highlightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);

                return;

            }

            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

            step += checkIncrement;

        }

        highlightBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);
    }
    #endregion
}

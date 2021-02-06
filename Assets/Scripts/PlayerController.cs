using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class Constants
{
    public const float maxHumanFallSpeed = -55.5f;
}
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    Transform mainCam;
    [SerializeField]
    World world;
    [SerializeField]
    float mouseSentivity = 10f;
    [SerializeField]
    float walkSpeed = 6f;
    [SerializeField]
    float jumpForce = 5f;
    [SerializeField]
    float gravity = -9.8f;
    [SerializeField]
    float playerRadius = 0.4f;
    [SerializeField]
    float boundTolerance = 0.1f;
    [SerializeField]
    float playerHeight = 1.8f;

    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private float verticalMomentum = 0f;
    private bool jumpInput;
    private bool isSprinting;
    private bool isGrounded;
    private Vector3 velocity;

    private void FixedUpdate()
    {
        CalculateVelocity();
        if (jumpInput&&isGrounded)
        {
            Jump();
        }
        transform.Rotate(Vector3.up * mouseHorizontal * mouseSentivity * Time.deltaTime);
        mainCam.Rotate(Vector3.right * mouseVertical * mouseSentivity * Time.deltaTime);
    }
    private void Update()
    {
        GetPlayerInput();
    }
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

    private void GetPlayerInput()
    {
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
    }
    private float CheckFallSpeed(float downSpeed)
    {
        if (world.CheckVoxel(transform.position + new Vector3(playerRadius, downSpeed, playerRadius)) ||
            world.CheckVoxel(transform.position + new Vector3(-playerRadius, downSpeed, -playerRadius)) ||
            world.CheckVoxel(transform.position + new Vector3(playerRadius, downSpeed, -playerRadius)) ||
            world.CheckVoxel(transform.position + new Vector3(-playerRadius, downSpeed, playerRadius)))
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
        if (world.CheckVoxel(transform.position + new Vector3(playerRadius, downSpeed, playerRadius)) ||
            world.CheckVoxel(transform.position + new Vector3(-playerRadius, downSpeed, -playerRadius)) ||
            world.CheckVoxel(transform.position + new Vector3(playerRadius, downSpeed, -playerRadius)) ||
            world.CheckVoxel(transform.position + new Vector3(-playerRadius, downSpeed, playerRadius)))
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
        if (world.CheckVoxel(transform.position + new Vector3(playerRadius, upSpeed + playerHeight, playerRadius)) ||
            world.CheckVoxel(transform.position + new Vector3(-playerRadius, upSpeed + playerHeight, -playerRadius)) ||
            world.CheckVoxel(transform.position + new Vector3(playerRadius, upSpeed + playerHeight, -playerRadius)) ||
            world.CheckVoxel(transform.position + new Vector3(-playerRadius, upSpeed + playerHeight, playerRadius)))
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
            if (world.CheckVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerRadius)) ||
                world.CheckVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + playerRadius)))
                return true;
            else
                return false;
        }

    }
    public bool back
    {

        get
        {
            if (world.CheckVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerRadius)) ||
                world.CheckVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - playerRadius)))
                return true;
            else
                return false;
        }

    }
    public bool left
    {

        get
        {
            if (world.CheckVoxel(new Vector3(transform.position.x - playerRadius, transform.position.y, transform.position.z)) ||
                world.CheckVoxel(new Vector3(transform.position.x - playerRadius, transform.position.y + 1f, transform.position.z)))
                return true;
            else
                return false;
        }

    }
    public bool right
    {

        get
        {
            if (world.CheckVoxel(new Vector3(transform.position.x + playerRadius, transform.position.y, transform.position.z)) ||
                world.CheckVoxel(new Vector3(transform.position.x + playerRadius, transform.position.y + 1f, transform.position.z)))
                return true;
            else
                return false;
        }

    }
}

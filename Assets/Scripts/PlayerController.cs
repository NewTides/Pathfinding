using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
	private const float Gravity = 9.8f;
	private CharacterController _characterController;
	private Vector3 _characterVelocity;

	[SerializeField, Tooltip("Description of this object")]
	private string description = "";

	[SerializeField][Tooltip("The force applied to the character when jumping")]
	private float JumpForce = 5f;
	
	[SerializeField][Tooltip("The camera for the first person controller")]
	public Camera _Camera;

	[SerializeField]
	[Tooltip("The fall speed modifier, increases gravity")]
	private float GravityMultiplier = 1f;
	
	[SerializeField][Tooltip("Walking speed of the character")]
	private Vector3 WalkingSpeed = new Vector3(2f,0f,4f); 	
	
	//[SerializeField][Tooltip("Sprinting speed of the character")]
	//private Vector3 SprintSpeed = new Vector3(2f,0f,9f); 
	
	[SerializeField][Tooltip("Walking speed of the character")]
	private Vector3 AirStrafeSpeed = new Vector3(0.01f,0f,0.01f); 	
	
	[SerializeField][Tooltip("Minimum angle the camera can look up/down")]
	private float MinCameraVerticalAngle = -90;
	[SerializeField][Tooltip("Maximum angle the camera can look up/down")]
	private float MaxCameraVerticalAngle = 90;
	
	[SerializeField][Tooltip("The speed at which the camera moves")]
	private Vector2 CameraSpeed = new Vector2(.1f,0.1f);

	//[SerializeField][Tooltip("Does the sprint button need to be held? false = toggle")]
	//private bool HoldToSprint;
	
	// Start is called before the first frame update
	private void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		// cache the character controller component
		_characterController = GetComponent<CharacterController>();
	}

	
	
	
	// Update is called once per frame
	private void Update()
	{
		// Hold Esc to stop the camera moving and unlock the cursor
		HandleCameraMovement();
		
		// Was i grounded last frame?
		var wasGrounded = _characterController.isGrounded;

		if (_characterController.isGrounded)
		{
			//ground movement controls
			HandleGroundMovement();
					
			if (Input.GetButtonDown("Jump"))
			{
				// remove vertical velocity
				_characterVelocity.y = 0;

				// add jump velocity
				_characterVelocity += Vector3.up * JumpForce;
			}

		}		
		else
		{
			// add air acceleration this frame
			HandleAirMovement();
		}
		
		// add gravitational acceleration this frame
		_characterVelocity += Vector3.down * (Gravity * GravityMultiplier * Time.deltaTime);
		
		// Apply calculated movement
		_characterController.Move(_characterVelocity * Time.deltaTime);
		
	}

	
	
	
	
	private void HandleAirMovement() 
	{
		// cache the input axes
		Vector3 inputAxes = new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
		
		// convert to a vector representing movement directions
		Vector3 inputAsMovement = new Vector3(inputAxes.x, 0f, inputAxes.y);
		
		// multiply movement direction by speed  
		inputAsMovement.Scale(AirStrafeSpeed);

		// transform the movement vector into world space relative to the player
		Vector3 worldSpaceMovement = transform.TransformVector(inputAsMovement);
		_characterVelocity += worldSpaceMovement;
	}

	private void HandleGroundMovement()
	{
		// cache the input axes
		Vector3 inputAxes = new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
		
		// convert to a vector representing movement directions
		Vector3 inputAsMovement = new Vector3(inputAxes.x, 0f, inputAxes.y);
		
		// multiply movement direction by speed  
		inputAsMovement.Scale(WalkingSpeed);

		// transform the movement vector into world space relative to the player
		Vector3 worldSpaceMovement = transform.TransformVector(inputAsMovement);
		_characterVelocity = worldSpaceMovement;
	}

	private float _cameraVerticalAngle;

	private void HandleCameraMovement()
	{
		//Unlock Cursor
		if (Input.GetKey(KeyCode.Escape))
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		else
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			
			// camera movement:
			
			// cache the camera input axes
			Vector2 inputAxes = new Vector2(Input.GetAxisRaw("Horizontal_Camera"), Input.GetAxisRaw("Vertical_Camera"));
			//Debug.Log($"Hor:{Input.GetAxisRaw("Horizontal_Camera")} Ver:{Input.GetAxisRaw("Vertical_Camera")}");

			// scale the input by the camera speed
			inputAxes.Scale(CameraSpeed);

			// horizontal rotation - rotate the character
			transform.Rotate(Vector3.up, inputAxes.x, Space.Self);

			// vertical rotation - rotate the camera
			// add new input to the accumulated angle
			_cameraVerticalAngle += inputAxes.y;

			// clamp the vertical rotation to be between min and max angles
			_cameraVerticalAngle = Mathf.Clamp(_cameraVerticalAngle, MinCameraVerticalAngle, MaxCameraVerticalAngle);

			// apply to the camera
			_Camera.transform.localEulerAngles = new Vector3(_cameraVerticalAngle, 0f, 0f);
		}
	}

	private void Menu()
	{
		throw new NotImplementedException();
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawSphere(_Camera.transform.position + _Camera.transform.forward,0.01f);
	}
}
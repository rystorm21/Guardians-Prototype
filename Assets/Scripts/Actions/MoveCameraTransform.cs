using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace EV
{
    public class MoveCameraTransform : StateActions
    {
        new Camera camera;
        TransformVariable cameraTransform;
        
        FloatVariable horizontal;
        FloatVariable vertical;
        Vector2 input;

        VariablesHolder varHolder;

        private float cameraZoomSpeed = 20;
        private float cameraWASDSpeed = 20;
        private float cameraMouseSpeed = 40;
        private float cameraRotationSpeed = 100;
        private float camSpeed;
        private bool isOrthographicZoom;

        // Constructor
        public MoveCameraTransform(VariablesHolder holder) 
        {
            camera = Camera.main;
            isOrthographicZoom = camera.orthographic;

            varHolder = holder;
            cameraTransform = varHolder.cameraTransform;
            horizontal = varHolder.horizontalInput;
            vertical = varHolder.verticalInput;
        } 

        public override void Execute(StateManager states, SessionManager sessionManager, Turn turn)
        {
            if (vertical.value != 0 || horizontal.value != 0 || Input.GetMouseButton(2)) 
                MoveCamera(states);
            
            if (Input.GetAxisRaw("Mouse ScrollWheel") != 0) 
                ZoomCamera();

            if (Input.GetMouseButton(1) || Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E))
                RotateCamera(states);
        }

        private void MoveCamera(StateManager states) 
        {
            Vector3 camF = cameraTransform.value.forward;
            Vector3 camR = cameraTransform.value.right;
            camF.y = 0;
            camR.y = 0;
            camF = camF.normalized;
            camR = camR.normalized;
            input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            if (Input.GetMouseButton(2))
            {
                input = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                camSpeed = cameraMouseSpeed;
            }
            else {
                camSpeed = cameraWASDSpeed;
            }
            input = Vector2.ClampMagnitude(input, 1);
            cameraTransform.value.position += (camF * input.y + camR * input.x) * (cameraWASDSpeed * states.delta);            
        }

        private void RotateCamera(StateManager states) 
        {
            if (Input.GetMouseButton(1))
            {
                cameraTransform.value.eulerAngles += (cameraRotationSpeed * states.delta) * new Vector3(x: 0, y: Input.GetAxis("Mouse X"), z: 0);
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Rotate(1);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                Rotate(-1);
            }
        }

        private void Rotate(int clockwise)
        {
            int angleIncrement = 45;
            int angle;
            cameraTransform.value.eulerAngles += new Vector3(x: 0, y: angleIncrement * clockwise, z: 0);
            angle = Mathf.RoundToInt(cameraTransform.value.eulerAngles.y);

            if (angle % angleIncrement != 0)
            {
                int remainder = angle % angleIncrement;
                cameraTransform.value.eulerAngles += new Vector3(x:0, y:-remainder, z: 0);
            }
        }

        private void ZoomCamera()
        {
            if (isOrthographicZoom)
            {
                camera.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * cameraZoomSpeed;
            }
            else
            {
                camera.fieldOfView -= Input.GetAxis("Mouse ScrollWheel") * cameraZoomSpeed;
            }
        }
    }
}

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
        private float maxX;
        private float maxZ;
        private float cameraZoomSpeed = 20;
        private float cameraWASDSpeed = 20;
        private float cameraMouseSpeed = 40;
        private float cameraRotationSpeed = 100;
        private float camSpeed;
        private bool isOrthographicZoom;
        GridPosition[] gridPosition;

        // Constructor
        public MoveCameraTransform(VariablesHolder holder) 
        {
            camera = Camera.main;
            isOrthographicZoom = camera.orthographic;
            varHolder = holder;
            cameraTransform = varHolder.cameraTransform;
            horizontal = varHolder.horizontalInput;
            vertical = varHolder.verticalInput;
            gridPosition = GameObject.FindObjectsOfType<GridPosition>();
            maxX = 0;
            maxZ = 0;

            for (int i = 0; i < gridPosition.Length; i++)
            {
                Transform t = gridPosition[i].transform;
                if (t.position.x > maxX)
                {
                    maxX = t.position.x;
                }
                if (t.position.z > maxZ)
                {
                    maxZ = t.position.z;
                }
            }
        } 

        public override void Execute(StateManager states, SessionManager sessionManager, Turn turn)
        {
            if (SessionManager.currentGameState.ToString() != "Dialog")
            {
                if (!GameObject.FindGameObjectWithTag("TargetingCam"))
                {
                    if (vertical.value != 0 || horizontal.value != 0 || Input.GetMouseButton(2))
                        MoveCamera(states);

                    if (Input.GetAxisRaw("Mouse ScrollWheel") != 0)
                        ZoomCamera();

                    if (Input.GetMouseButton(1) || Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E))
                        RotateCamera(states);
                }
            }
        }

        private void MoveCamera(StateManager states) 
        {
            bool oob = false;
            Vector3 camF = cameraTransform.value.forward;
            Vector3 camR = cameraTransform.value.right;
            float minPos = 0;
            camF.y = 0;
            camR.y = 0;
            camF = camF.normalized;
            camR = camR.normalized;
            input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            if (Input.GetMouseButton(2))
            {
                input = new Vector2(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"));
                camSpeed = cameraMouseSpeed;
            }
            else {
                camSpeed = cameraWASDSpeed;
            }
            input = Vector2.ClampMagnitude(input, 1);
            
            // Debug.Log(camF.normalized * input.y + ", " + camR.normalized * input.x);
            // Debug.Log(cameraTransform.value.position.x + ", " + cameraTransform.value.position.z + ", maxXpos: " + maxX);

            if (cameraTransform.value.position.z <= minPos && camF.normalized.z * input.y < 0)
                oob = true;
            if (cameraTransform.value.position.z <= minPos && camR.normalized.z * input.x < 0)
                oob = true;

            if (cameraTransform.value.position.x <= minPos && camF.normalized.x * input.y < 0)
                oob = true;
            if (cameraTransform.value.position.x <= minPos && camR.normalized.x * input.x < 0)
                oob = true;
            
            if (cameraTransform.value.position.x >= maxX && camR.normalized.x * input.x > 0)
                oob = true;
            if (cameraTransform.value.position.x >= maxX && camF.normalized.x * input.y > 0)
                oob = true;

            if (cameraTransform.value.position.z >= maxZ && camF.normalized.z * input.y > 0)
                  oob = true;
            if (cameraTransform.value.position.z >= maxZ && camR.normalized.z * input.x > 0)
                  oob = true;
                
            if (!oob)
            {
                cameraTransform.value.position += (camF * input.y + camR * input.x) * (cameraWASDSpeed * states.delta);
            }
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
                if (camera.fieldOfView >= 50 && Input.GetAxis("Mouse ScrollWheel") < 0)
                    return;
                if (camera.fieldOfView <= 20 && Input.GetAxis("Mouse ScrollWheel") > 0)
                    return;
                camera.fieldOfView -= Input.GetAxis("Mouse ScrollWheel") * cameraZoomSpeed;
            }
        }
    }
}

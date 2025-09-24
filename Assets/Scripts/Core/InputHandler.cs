using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;
//To fix conflict with UnityEngine.XR
using InputDevice = UnityEngine.InputSystem.InputDevice;

public class InputHandler : MonoBehaviour
{
    private static InputHandler instance;
    private Dictionary<string, GameObject> vrHands = new Dictionary<string, GameObject>();
    /// <summary>
    /// SortedDictionary of all inputDevices, sorted by lowest priority to highest
    /// </summary>
    private SortedDictionary<int, InputDeviceProperties> inputDevices = new SortedDictionary<int, InputDeviceProperties>();
    /// <summary>
    /// Highest priority for the InputHandler (usually VR touch controllers)
    /// </summary>
    private const int HIGHEST_PRIORITY = 100;
    private const float FIND_DEVICE_TIMEOUT = 15.0f;
    /// <summary>
    /// Name Unity InputDevice associated with the headset device when using OpenXR
    /// </summary>
    public const string HEADSET_DEVICE_NAME = "HeadTrackingOpenXR";
    /// <summary>
    /// Name Unity InputDevice associated with the mouse
    /// </summary>
    public const string MOUSE_NAME = "Mouse";
    /// <summary>
    /// Name Unity InputDevice associated with the VR controllers when using OpenXR
    /// </summary>
    public const string TOUCH_CONTROLLER_NAME = "OculusTouchControllerOpenXR";
    /// <summary>
    /// JSON string to use VR controllers
    /// </summary>
    public const string JSON_VR = "vr_controller";

    /// <summary>
    /// Dominant hand to track, either "LeftHand" or "RightHand"
    /// </summary>
    private string domHand = "RightHand";

    public delegate Vector3 InputPositionUpdate();
    public delegate Quaternion InputRotationUpdate();

    public enum InputType
    {
        NONE,
        SPATIAL,
        RAY
    }

    public struct InputDeviceProperties
    {
        public Vector3 position;
        public Quaternion rotation;
        public InputDevice inputDevice;
        public InputType inputType;

        InputPositionUpdate inputPosUpdate;
        InputRotationUpdate inputRotUpdate;

        public InputDeviceProperties(Vector3 position, Quaternion rotation, InputType inputType, InputDevice inputDevice, InputPositionUpdate inputPosUpdate, InputRotationUpdate inputRotUpdate)
        {
            this.position = position;
            this.rotation = rotation;
            this.inputType = inputType;
            this.inputDevice = inputDevice;
            this.inputPosUpdate = inputPosUpdate;
            this.inputRotUpdate = inputRotUpdate;
        }

        public void Update()
        {
            UpdatePosition();
            UpdateRotation();

            //Debug.Log(inputDevice.name + " pos= " + position);
        }

        public void UpdatePosition()
        {
            if (inputPosUpdate != null)
            {
                position = inputPosUpdate();
            }
        }

        public void UpdateRotation()
        {
            if (inputRotUpdate != null)
            {
                rotation = inputRotUpdate();
            }
        }
    }

    public static InputHandler Instance
    {
        get { return instance; }
    }

    public Dictionary<string, GameObject> VRHands
    {
        get { return vrHands; }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var e in inputDevices.ToList())
        {
            InputDeviceProperties r = inputDevices[e.Key];
            r.Update();
            inputDevices[e.Key] = r;
        }
    }

    public InputDevice GetInputDevice()
    {
        int highestPriority = inputDevices.Keys.Max(key => key);
        return inputDevices[highestPriority].inputDevice;
    }

    public Dictionary<string, GameObject> GetVRHands()
    {
        return vrHands;
    }

    public void FindHandAnchors()
    {
        vrHands["LeftHand"] = GameObject.Find("LeftHand Controller");
        vrHands["RightHand"] = GameObject.Find("RightHand Controller");

        if (!vrHands["LeftHand"])
        {
            Debug.LogWarning("No GameObject found for left hand found");
        }
        if (!vrHands["RightHand"])
        {
            Debug.LogWarning("No GameObject found for right hand found");
        }

        //Debug.Log(vrHands["LeftHand"]);
        //Debug.Log(vrHands["RightHand"]);
    }
    
    /*
    public void FindDevices(Dictionary<int, string> devicePref)
    {
        Dictionary<int, InputDevice> currentDevices = new Dictionary<int, InputDevice>();

        //Get all the current devices
        if (inputDevices.Count > 0)
        {
            foreach (var e in inputDevices)
            {
                currentDevices[e.Key] = e.Value.inputDevice;
            }
        }

        foreach (InputDevice d in InputSystem.devices)
        {
            string deviceName = d.name;

            //The device is not in the dictionary and is a device in devicePref
            if (!currentDevices.Values.Contains(d) && devicePref.ContainsValue(deviceName))
            {
                //set the key to the counter by default
                int keyVal = devicePref.FirstOrDefault(x => x.Value == deviceName).Key;
                //Add the new device
                inputDevices[keyVal] = CreateInputDeviceProperty(d);
            }
            //The device is already in the dictionary
            else
            {
                //If the device is in the device preferences, change the priority
                if (devicePref.Values.Contains(d.name))
                {
                    int theKey = devicePref.FirstOrDefault(x => x.Value == deviceName).Key;
                    //InputDevices should have the device so get the key and then change the key
                    int oldKey = currentDevices.FirstOrDefault(x => x.Value.name == deviceName).Key;
                    InputDeviceProperties iDevicePropery = inputDevices[oldKey];
                    inputDevices[theKey] = iDevicePropery;
                    inputDevices.Remove(oldKey);
                }
            }
        }
    }
    */

    /// <summary>
    /// Default setup for finding devices
    /// </summary>
    public void FindDevices()
    {
        Dictionary<int, InputDevice> currentDevices = new Dictionary<int, InputDevice>();

        //Get all the currnet input devices
        if(inputDevices.Count > 0)
        {
            foreach(var e in inputDevices)
            {
                currentDevices[e.Key] = e.Value.inputDevice;
            }
        }

        int counter = 0;

        foreach (InputDevice d in InputSystem.devices)
        {
            string deviceName = d.name;

            if (!currentDevices.Values.Contains(d))
            {
                //Set the priority based on the device
                //For new devices new cases can be added or just have the default
                //Commenting out the default case will add the keyboard which is almost never used
                switch (deviceName)
                {
                    case (HEADSET_DEVICE_NAME):
                        inputDevices[HIGHEST_PRIORITY - 1] = CreateInputDeviceProperty(d);
                        break;
                    case (TOUCH_CONTROLLER_NAME):
                        FindHandAnchors();
                        inputDevices[HIGHEST_PRIORITY] = CreateInputDeviceProperty(d);
                        break;
                    case (MOUSE_NAME):
                    //default:
                        inputDevices[counter] = CreateInputDeviceProperty(d);
                        counter++;
                        break;
                }
            }
            else
            {
                if(d.name != HEADSET_DEVICE_NAME || d.name != TOUCH_CONTROLLER_NAME)
                {
                    counter++;
                }
            }
        }
    }
    /// <summary>
    /// Returns InputDeviceProperty based on the passed InputDevice.
    /// Define specific device names and cases for specific devices here.
    /// </summary>
    /// <param name="inputDevice">InputDevice to create the InputDeviceProperties from</param>
    /// <returns>InputDeviceProperties object based on passed param</returns>
    InputDeviceProperties CreateInputDeviceProperty(InputDevice inputDevice)
    {
        switch (inputDevice.name)
        {
            case (HEADSET_DEVICE_NAME):
                return new InputDeviceProperties(Vector3.zero, Quaternion.identity, InputType.RAY, inputDevice, null, null);
            case (TOUCH_CONTROLLER_NAME):
                return new InputDeviceProperties(Vector3.zero, Quaternion.identity, InputType.SPATIAL, inputDevice, GetHandPosition, GetHandRotation);
            case (MOUSE_NAME):
                return new InputDeviceProperties(Vector3.zero, Quaternion.identity, InputType.RAY, inputDevice, delegate { return Input.mousePosition; }, null);
            default:
                return new InputDeviceProperties(Vector3.zero, Quaternion.identity, InputType.NONE, inputDevice, null, null);
        }
    }

    /*
    public Vector3 GetHandPosition(string handName)
    {
        if (vrHands[handName])
        {
            return vrHands[handName].transform.position;
        }

        return Vector3.zero;
    }
    */

    /*
    public Quaternion GetHandRotation(string handName)
    {
        if (vrHands[handName])
        {
            return vrHands[handName].transform.rotation;
        }

        return Quaternion.identity;
    }
    */

    public void SetDominantHand(string newDomHand)
    {
        domHand = newDomHand;
    }

    public GameObject GetDominantHandGameObject()
    {
        return vrHands[domHand];
    }

    /// <summary>
    /// Get dominant hand in respect to XR InputDevice
    /// </summary>
    /// <returns>The dominant hand XR InputDevice</returns>
    public UnityEngine.XR.InputDevice GetDominantHand()
    {
        if(domHand == "LeftHand")
        {
            return InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        }
        else
        {
            return InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        }
    }

    /// <summary>
    /// Get velocity of hand controller
    /// </summary>
    /// <param name="hand">String name for hand to get velocity (either "LeftHand" or "RightHand")</param>
    /// <returns>Velocity of hand, default is dominant hand if no hand is specified</returns>
    public Vector3 GetHandVelocity(string hand = "")
    {
        if (hand.Length > 0 && hand != null && hand != "null")
        {
            string handToGet = hand.Length > 0 ? hand : domHand;

            UnityEngine.XR.InputDevice vrHand;

            if (handToGet == "LeftHand")
            {
                vrHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            }
            else if(handToGet == "RightHand")
            {
                vrHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            }
            else
            {
                vrHand = InputDevices.GetDeviceAtXRNode(XRNode.GameController);
            }

            vrHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceVelocity, out Vector3 velocity);

            return velocity;
        }
        else
        {
            return Vector3.zero;
        }
    }


    public string GetDominantHandString()
    {
        return domHand;
    }

    /// <summary>
    /// Get dominant hand positions
    /// </summary>
    /// <returns></returns>
    public Vector3 GetHandPosition()
    {
        if (vrHands[domHand])
        {
            return vrHands[domHand].transform.position;
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Get position of hand controller
    /// </summary>
    /// <param name="hand">String name for hand to get position (either "LeftHand" or "RightHand")</param>
    /// <returns>Position of hand, default is dominant hand if no hand is specified</returns>
    public Vector3 GetHandPosition(string hand = "")
    {
        if (hand.Length > 0 && hand != null && hand != "null")
        {
            string handToGet = hand.Length > 0 ? hand : domHand;

            UnityEngine.XR.InputDevice vrHand;

            if (handToGet == "LeftHand")
            {
                vrHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            }
            else if (handToGet == "RightHand")
            {
                vrHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            }
            else
            {
                vrHand = InputDevices.GetDeviceAtXRNode(XRNode.GameController);
            }

            vrHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out Vector3 position);

            return position;
        }
        else
        {
            return Vector3.zero;
        }
    }


    /// <summary>
    /// Get dominant hand rotation
    /// </summary>
    /// <returns></returns>
    public Quaternion GetHandRotation()
    {
        if (vrHands[domHand])
        {
            return vrHands[domHand].transform.rotation;
        }

        return Quaternion.identity;
    }

    /// <summary>
    /// Get rotation of hand controller
    /// </summary>
    /// <param name="hand">String name for hand to get rotation (either "LeftHand" or "RightHand")</param>
    /// <returns>Rotation of hand, default is dominant hand if no hand is specified</returns>
    public Quaternion GetHandRotation(string hand = "")
    {
        if (hand.Length > 0 && hand != null && hand != "null")
        {
            string handToGet = hand.Length > 0 ? hand : domHand;

            UnityEngine.XR.InputDevice vrHand;

            if (handToGet == "LeftHand")
            {
                vrHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            }
            else if (handToGet == "RightHand")
            {
                vrHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            }
            else
            {
                vrHand = InputDevices.GetDeviceAtXRNode(XRNode.GameController);
            }

            vrHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out Quaternion rotation);

            return rotation;
        }
        else
        {
            return Quaternion.identity;
        }
    }

    /// <summary>
    /// Get the highest priority device connected and returns said devices position
    /// </summary>
    /// <returns>If proper device connected returns a Vector3</returns>
    public Vector3 GetPosition()
    {
        int highestPriority = inputDevices.Keys.Max(key => key);
        return inputDevices[highestPriority].position;
    }

    /// <summary>
    /// Get position of device related to passed priority
    /// </summary>
    /// <param name="priority">Priority of device</param>
    /// <returns>If device with given priority return said devices position else return zero vector</returns>
    public Vector3 GetPosition(int priority)
    {
        if (inputDevices.ContainsKey(priority))
        {
            return inputDevices[priority].position;
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Get rotation of device related to passed priority
    /// </summary>
    /// <param name="priority">Priority of device</param>
    /// <returns>If device with given priority return said devices rotation else return identity quaternion</returns>
    public Quaternion GetRotation(int priority)
    {
        if (inputDevices.ContainsKey(priority))
        {
            return inputDevices[priority].rotation;
        }

        return Quaternion.identity;
    }

    /// <summary>
    /// Get the highest priority device connected and returns said devices rotation
    /// </summary>
    /// <returns>If proper device connected returns a Quaternion</returns>
    public Quaternion GetRotation()
    {
        int highestPriority = inputDevices.Keys.Max(key => key);
        return inputDevices[highestPriority].rotation;
    }

    /// <summary>
    /// Get the highest priority device of RAY inputType and returns said devices position
    /// </summary>
    /// <returns>Ray origin position</returns>
    public Vector3 GetRayPosition()
    {
        var sortedDict = from entry in inputDevices orderby entry.Key descending select entry;

        foreach (KeyValuePair<int, InputDeviceProperties> e in sortedDict)
        {
            if (e.Value.inputType == InputType.RAY)
            {
                return e.Value.position;
            }
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Is the ray casting onto the plane
    /// </summary>
    /// <param name="plane">Plane to check collision for</param>
    /// <returns>True if collision, false if not</returns>
    public bool IsRaycastColliding(Plane plane)
    {
        Ray ray = Camera.main.ScreenPointToRay(GetRayPosition());
        float enter;

        return plane.Raycast(ray, out enter);
    }

    /// <summary>
    /// Get the hit position of a ray onto a plane
    /// </summary>
    /// <param name="plane">Plane to cast the ray onto</param>
    /// <returns>If ray hits plan return the hit position else return ray origin position</returns>
    public Vector3 GetRaycastPosition(Plane plane)
    {
        Ray ray = Camera.main.ScreenPointToRay(GetRayPosition());
        float enter;

        if (plane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            return hitPoint;
        }

        return ray.origin;
    }

    /// <summary>
    /// Get the highest priority device of SPATIAL inputType and returns said devices position
    /// </summary>
    /// <returns>If spatial device return position of said device else zero vector</returns>
    public Vector3 GetSpatialPosition()
    {
        var sortedDict = from entry in inputDevices orderby entry.Key descending select entry;

        foreach (KeyValuePair<int, InputDeviceProperties> e in sortedDict)
        {
            if (e.Value.inputType == InputType.SPATIAL)
            {
                return e.Value.position;
            }
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Get the highest priority device of SPATIAL inputType and returns said devices rotation
    /// </summary>
    /// <returns>If spatial device return rotation of said device else identity quaternion</returns>
    public Quaternion GetSpatialRotation()
    {
        var sortedDict = from entry in inputDevices orderby entry.Key descending select entry;

        foreach (KeyValuePair<int, InputDeviceProperties> e in sortedDict)
        {
            if (e.Value.inputType == InputType.SPATIAL)
            {
                return e.Value.rotation;
            }
        }

        return Quaternion.identity;
    }

    /// <summary>
    /// Get the dictionary of devices
    /// </summary>
    /// <returns>Dictionary of devices</returns>
    public SortedDictionary<int, InputDeviceProperties> GetDevices()
    {
        return inputDevices;
    }

    //Current way assumes we will never have 100 devices connected at one time
    /// <summary>
    /// Set highest priority device to the device with given name
    /// </summary>
    /// <param name="deviceName">Device name of connected device within InputDevices. If null or empty string uses FindDevices() default setup.</param>
    public void UseThisDevice(string deviceName)
    {
        StartCoroutine(FindAndSetDevice(deviceName));
    }
    
    /// <summary>
    /// Find the device and than set it as the highest priority
    /// </summary>
    /// <param name="deviceName"></param>
    /// <returns></returns>
    IEnumerator FindAndSetDevice(string deviceName)
    {
        //If a device name was passed
        if (deviceName != null && deviceName.Length > 0 && deviceName != "null")
        {
            //The actual device string
            string deviceString = "";

            //switch case for converting JSON strings into Unity InputDevices string
            switch (deviceName)
            {
                case (JSON_VR):
                    deviceString = TOUCH_CONTROLLER_NAME;
                    break;
                default:
                    deviceString = deviceName;
                    break;
            }

            //Make the first letter upper case just in case
            //the name is written in camel case
            deviceString = deviceString.First().ToString().ToUpper() + deviceString.Substring(1);

            List<string> deviceNames = new List<string>();

            foreach (var e in inputDevices)
            {
                deviceNames.Add(e.Value.inputDevice.name);
            }

            //If the device name was not found in input devices
            if (!deviceNames.Contains(deviceString))
            {
                float timeout = 0.0f;

                Debug.Log(deviceString + " could not be found trying again");
                //TODO Maybe have a coroutine here instead?
                while (!deviceNames.Contains(deviceString) && timeout < FIND_DEVICE_TIMEOUT)
                {
                    //Find all devices again to try and see if the device was connected
                    FindDevices();

                    timeout += Time.deltaTime;

                    yield return null;
                }

                //If for some reason the device still could not be found, throw an exception
                if (!deviceNames.Contains(deviceString))
                {
                    throw new UnassignedReferenceException(deviceString + " could not be found");
                }
            }

            SetTheDevice(deviceString);
        }
        else
        {
            Debug.LogWarning("Device name was null or empty string");
            //FindDevices();
        }


        yield return new WaitForEndOfFrame();
    }

    /// <summary>
    /// Set the passed device name as the highest priority
    /// </summary>
    /// <param name="deviceName"></param>
    private void SetTheDevice(string deviceName)
    {
            //Get the key for device
            int theKey = inputDevices.FirstOrDefault(x => x.Value.inputDevice.name == deviceName).Key;

            //If the highest priority key has a value we need to move some things around 
            //before setting the other device to highest priority
            if (theKey != HIGHEST_PRIORITY && inputDevices.Keys.Max() == HIGHEST_PRIORITY)
            {
                for (int i = HIGHEST_PRIORITY - 1; i > -1; i--)
                {
                    if (!inputDevices.ContainsKey(i))
                    {
                        InputDeviceProperties highestPriorityDevice = inputDevices[HIGHEST_PRIORITY];
                        inputDevices[i] = highestPriorityDevice;
                        inputDevices.Remove(HIGHEST_PRIORITY);

                        inputDevices[HIGHEST_PRIORITY] = inputDevices[theKey];
                        inputDevices.Remove(theKey);
                        break;
                    }
                    else if (i == 0)
                    {
                        Debug.LogWarning("Could not move devices around, highest priority device is " + inputDevices[HIGHEST_PRIORITY].inputDevice.name +
                            " and not the desired " + deviceName);
                    }
                }
            }
            //if no highest priority device exists just set the device to it
            else if (theKey != HIGHEST_PRIORITY && inputDevices.Keys.Max() != HIGHEST_PRIORITY)
            {
                inputDevices[HIGHEST_PRIORITY] = inputDevices[theKey];
                inputDevices.Remove(theKey);
            }
    }

    public void UseThisDevice(InputDevice inputDevice)
    {
        foreach(var e in inputDevices)
        {
            if(e.Value.inputDevice == inputDevice)
            {
                UseThisDevice(e.Value.inputDevice.name);
                break;
            }
        }
    }

    public void UseThisDevice(InputDeviceProperties inputDeviceProperties)
    {
        if (inputDevices.ContainsValue(inputDeviceProperties))
        {
            UseThisDevice(inputDeviceProperties.inputDevice.name);
        }
    }

    public void UseThisDevice(int priority)
    {
        if (inputDevices.ContainsKey(priority))
        {
            UseThisDevice(inputDevices[priority].inputDevice.name);
        }
    }
}

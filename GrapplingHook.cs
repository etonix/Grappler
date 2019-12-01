using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

namespace ShadowedSouls.Items
{
    public class GrapplingHook : MonoBehaviour
    {
    #region Variables
        [Header("Setup and Configuration")]
        [Tooltip("The grappling hook prefab.")]
        public GameObject hookObj;
        [Tooltip("The main scene camera.")]
        public Transform mainCamera;
        [Tooltip("How fast the player will grapple to the hook.")]
        public float speed = 3f;
        [Tooltip("The maximum distance the grappling hook can travel.")]
        public float maxDistance = 55f;
        [Tooltip("Re-enables kinematics on the player once done grappling.")]
        public bool resetKinematic = false;

        private GUIStyle style = new GUIStyle();

        // [CAN BE CHANGED]
        private ThirdPersonCharacter playerControl;         // Unity Standard Assets 
        private float heightOffset;                         // The height of the player transform


        // [LEAVE AS IS BELOW THIS POINT]
        private Vector3 grapplePoint, adjustmentPoint;      // The positions we'll be grappling to
        private GameObject hookTmp;                         // Cloned hook prefab.
        private GameObject hookedObj;                       // The surface we're grappling to.
        private Rigidbody trb;                              // Rigidbody of the grappling hook.


        [SerializeField] private bool isSecured = false, firedHook = false;     // For debug and script purposes.
        [SerializeField] private bool isTargeting = false;                      // For debug and script purposes.
        private Rigidbody rb;                                                   // Player's attached rigidbody.

        private float step;                                                     // Grappling movement broken up
        private float momentum;                                                 // Grappling speed build up

        private bool onGrappleSurface;                                          // Are we standing on a hookable surface?

        private Ray ray;                                                        // Raycasting. Everybody Loves Ray!
        private RaycastHit hit;                                                 // Where'd Ray hit that guy at?
        #endregion

    #region StartUpdates
        void Start()
        {
            // [CAN BE CHANGED]
            playerControl = GetComponent<ThirdPersonCharacter>();

            // [LEAVE ALONE BEYOND HERE]
            rb = GetComponent<Rigidbody>();
            heightOffset = GetComponent<CapsuleCollider>().height;
        }

        void FixedUpdate()
        {
            if (!firedHook && !isSecured)
            {
                Ray ray = new Ray(mainCamera.position, mainCamera.forward);
                if (Physics.Raycast(ray, out hit, maxDistance))
                {
                    if (hit.collider.tag == "Hookable")
                    {
                        if (Vector3.Distance(transform.position, hit.point) > 5f)
                            isTargeting = true;
                        else isTargeting = false;
                    }
                    else isTargeting = false;
                }
                else isTargeting = false;


                if (isTargeting)
                {
                    if (hit.collider.tag == "Hookable")
                    {
                        hookedObj = hit.transform.gameObject;
                        adjustmentPoint = grapplePoint = hit.point;
                        adjustmentPoint.y += 1f + heightOffset;
                        if(!IsCentered(adjustmentPoint))
                        // Add a TEENY bit more to the collision point...
                           adjustmentPoint = AdjustedGrapple(adjustmentPoint);

                        if (adjustmentPoint == Vector3.zero)
                        {
                            isTargeting = false;
                            return;
                        }
                        else
                            isTargeting = true;

                    }

                    if (Input.GetKeyDown(KeyCode.Mouse1) == true)
                    {
                        isTargeting = false;
                        firedHook = true;
                        hookTmp = Instantiate(hookObj);
                        Vector3 worldP;
                        worldP = new Vector3(transform.position.x, transform.position.y + heightOffset / 2, transform.position.z + 0.3f);
                        hookTmp.transform.position = worldP;
                        hookTmp.transform.LookAt(hookedObj.transform);
                        trb = hookTmp.AddComponent<Rigidbody>();
                        trb.isKinematic = false;
                        trb.useGravity = false;
                    }
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                if (Cursor.lockState == CursorLockMode.Locked)
                    Cursor.lockState = CursorLockMode.None;
                else
                    Cursor.lockState = CursorLockMode.Locked;

            if (firedHook || isSecured)
            {
                momentum += Time.deltaTime * speed;
                step = momentum * Time.deltaTime;
            }

            if (firedHook && hookTmp.transform.position != grapplePoint)
            {
                hookTmp.transform.position = Vector3.MoveTowards(hookTmp.transform.position, grapplePoint, step * 5.5f);
            }
            else if (firedHook && hookTmp.transform.position == grapplePoint)
            {
                firedHook = false;
                isSecured = true;
                momentum = 0;
                step = 0;
                if (!rb.isKinematic)
                    rb.isKinematic = true;
            }


            if (isSecured && transform.position != adjustmentPoint)
            {
                rb.position = Vector3.MoveTowards(rb.position, adjustmentPoint, step);
            }
       
            else if (isSecured && transform.position == adjustmentPoint)
            {
                Unhook();
            }
        }
        #endregion
            
        private void Unhook()
        {
            isSecured = false;
            firedHook = false;
            hookedObj = null;
            momentum = 0;
            step = 0;
            Destroy(hookTmp);
            if (resetKinematic)
                rb.isKinematic = false;
        }

        private Vector3 AdjustedGrapple(Vector3 tmp, bool firstPass = true)
        {
            Vector3 ret = tmp;

            ret.x += 0.45f;
            if (IsCentered(ret))
                return ret;
            else
            {
                ret.x += 0.45f;
                if (IsCentered(ret))
                    return ret;
            }

            ret = tmp;
            ret.x -= 0.45f;

            if (IsCentered(ret))
                return ret;
            else
            {
                ret.x -= 0.45f;
                if (IsCentered(ret))
                    return ret;
            }

            ret = tmp;
            ret.z += 0.45f;

            if (IsCentered(ret))
                return ret;
            else
            {
                ret.z += 0.45f;
                if (IsCentered(ret))
                    return ret;
            }

            ret = tmp;
            ret.z -= 0.45f;

            if (IsCentered(ret))
                return ret;
            else
            {
                ret.z -= 0.45f;
                if (IsCentered(ret))
                    return ret;
            }

            if (firstPass)
            {
                ret = tmp;
                ret.z += 0.45f;
                ret.x += 0.45f;
                ret = AdjustedGrapple(ret, false);
                if (ret != Vector3.zero)
                    if (IsCentered(ret))
                        return ret;

                ret = tmp;
                ret.z -= 0.45f;
                ret.x -= 0.45f;
                ret = AdjustedGrapple(ret, false);
                if (ret != Vector3.zero)
                    if(IsCentered(ret))
                        return ret;
            }

            return Vector3.zero;
        }

        private bool IsCentered(Vector3 ret)
        {
            if (Physics.Raycast(new Ray(ret + new Vector3(1f, 1f, 1f), Vector3.down), 2f + heightOffset)
               && Physics.Raycast(new Ray(ret + new Vector3(-1f, 1f, 1f), Vector3.down), 2f + heightOffset)
               && Physics.Raycast(new Ray(ret + new Vector3(1f, 1f, -1f), Vector3.down), 2f + heightOffset)
               && Physics.Raycast(new Ray(ret + new Vector3(-1f, 1f, -1f), Vector3.down), 2f + heightOffset))
                if(Physics.Raycast(new Ray(ret, Vector3.down), out RaycastHit hitCenter, 2f + heightOffset))
                    if(hitCenter.transform.tag == "Hookable")
                       return true;

            return false;
        }
        
        private void OnGUI()
        {
            if(isTargeting)
            {
                GUI.Box(new Rect(new Vector2(Screen.width / 2, Screen.height / 2), new Vector2(150, 20)), "Right Click To Grapple");
            }
        }
    }
}
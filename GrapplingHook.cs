// Written for Shadowed Souls Gaming
// 11/20/2019 - Thomas Pearce
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

namespace ShadowedSouls
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

        // [CAN BE CHANGED]
        private ThirdPersonCharacter playerControl;         // Unity Standard Assets 
        private float heightOffset;                         // The height of the player transform


        // [LEAVE AS IS BELOW THIS POINT]
        private Vector3 grapplePoint, adjustmentPoint;      // The positions we'll be grappling to
        private GameObject hookTmp;                         // Cloned hook prefab.
        private GameObject hookedObj;                       // The surface we're grappling to.
        private Rigidbody trb;                              // Rigidbody of the grappling hook.


        [SerializeField] private bool isSecured = false, firedHook = false;         // For debug and script purposes.
        private Rigidbody rb;                                                       // Player's attached rigidbody.

        private float step;                                                         // Grappling movement broken up
        private float momentum;                                                     // Grappling speed build up

        private bool onGrappleSurface;                                              // Are we standing on a hookable surface?

        private Ray ray;                                                            // Raycasting. Everybody Loves Ray!
        private RaycastHit hit;                                                     // Where'd Ray hit that guy at?
        #endregion

    #region StartUpdates
        void Start()
        {
            // [CAN BE CHANGED]
            playerControl = GetComponentInParent<ThirdPersonCharacter>();

            // [LEAVE ALONE BEYOND HERE]
            rb = playerControl.GetComponent<Rigidbody>();
            heightOffset = playerControl.GetComponent<CapsuleCollider>().height;
        }

        void FixedUpdate()
        {
            if (!firedHook)
            {
                Ray ray = new Ray(mainCamera.position, mainCamera.forward);
                if (Input.GetKeyDown(KeyCode.Mouse1) == true)
                {
                    if (Physics.Raycast(ray, out RaycastHit hitt, maxDistance))
                    {
                        if (hitt.collider.tag == "Hookable" || hitt.collider.tag == "Climbable")
                        {
                            hookedObj = hitt.transform.gameObject;
                            firedHook = true;
                            adjustmentPoint = grapplePoint = hitt.point;
                            adjustmentPoint.y += heightOffset;
                            Debug.DrawLine(mainCamera.position, hitt.point, Color.red);
                            hookTmp = Instantiate(hookObj);
                            hookTmp.transform.position = playerControl.transform.position;
                            hookTmp.transform.localPosition = playerControl.transform.localPosition;
                            hookTmp.transform.LookAt(hookedObj.transform);
                            trb = hookTmp.AddComponent<Rigidbody>();
                            trb.isKinematic = false;
                            trb.useGravity = false;
                        }
                    }
                }
            }
        }

        private void Update()
        {
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


            if (isSecured && Vector3.Distance(playerControl.transform.position, adjustmentPoint) > 0.05f)
            {
                rb.position = Vector3.MoveTowards(rb.position, adjustmentPoint, step);
            }
            else if (isSecured && Vector3.Distance(playerControl.transform.position, adjustmentPoint) <= 0.05f)
            {
                Unhook();
            }
        }

        #endregion

    #region Colliders
        private void OnCollisionExit(Collision collision)
        {
            if (collision.transform.tag == "Hookable" || collision.transform.tag == "Climbable")
                if (isSecured && onGrappleSurface)
                    onGrappleSurface = false;
                else if (isSecured)
                {
                    Unhook();
                    onGrappleSurface = false;
                }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.transform.tag == "Hookable" || collision.transform.tag == "Climbable")
                onGrappleSurface = true;
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
    }
}

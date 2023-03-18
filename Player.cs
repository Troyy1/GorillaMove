namespace GorillaMove

{

    using System.Collections;

    using UnityEngine;

    using UnityEngine.XR;




    public class Player : MonoBehaviour

    {

        private static Player _instance;
        public bool ghostHands = false;



        public static Player Instance
        {
            get
            {
                return _instance;
            }
        }



        public SphereCollider headCollider;

        public CapsuleCollider bodyCollider;



        public Transform leftHandFollower;

        public Transform rightHandFollower;



        public Transform rightHandTransform;

        public Transform leftHandTransform;



        private Vector3 lastLeftHandPosition;

        private Vector3 lastRightHandPosition;

        private Vector3 lastHeadPosition;



        private Rigidbody playerRigidBody;



        public int velocityHistorySize;

        public float maxArmLength = 1.5f;

        public float unStickDistance = 1f;



        public float velocityLimit;

        public float maxJumpSpeed;

        public float jumpMultiplier;

        public float minimumRaycastDistance = 0.05f;

        public float defaultSlideFactor = 0.03f;

        public float defaultPrecision = 0.995f;



        private Vector3[] velocityHistory;

        private int velocityIndex;

        private Vector3 currentVelocity;

        private Vector3 denormalizedVelocityAverage;

        private bool jumpHandIsLeft;

        private Vector3 lastPosition;



        public Vector3 rightHandOffset;

        public Vector3 leftHandOffset;



        public LayerMask locomotionEnabledLayers;



        public bool wasLeftHandTouching;

        public bool wasRightHandTouching;



        public bool disableMovement = false;



        public bool LAndRHandHasTrigger;

        public AudioSource Stone;
        public AudioSource Wood;
        public AudioSource Grass;
        public AudioSource Metal;
        public string CurrentCollison;
        public PhysicMaterial Slip;

        public float hapticWaitSeconds = 0.05f;



        public float vibrationAmmount = 0.15f;



        private void Awake()

        {

            if (_instance != null && _instance != this)

            {

                Destroy(gameObject);

            }

            else

            {

                _instance = this;

            }



            InitializeValues();

        }



        public void InitializeValues()

        {

            playerRigidBody = GetComponent<Rigidbody>();

            velocityHistory = new Vector3[velocityHistorySize];

            lastLeftHandPosition = leftHandFollower.transform.position;

            lastRightHandPosition = rightHandFollower.transform.position;

            lastHeadPosition = headCollider.transform.position;

            velocityIndex = 0;

            lastPosition = transform.position;

        }
        public void OnTriggerEnter(Collider collision)
        {
            if (LAndRHandHasTrigger)
            {
                if (collision.gameObject.CompareTag("Grass"))
                {
                    CurrentCollison = "Grass";
                }
                else if (collision.gameObject.CompareTag("Stone"))
                {
                    CurrentCollison = "Stone";
                }
                else if (collision.gameObject.CompareTag("Wood"))
                {
                    CurrentCollison = "Wood";

                }
                else if (collision.gameObject.CompareTag("Metal"))
                {
                    CurrentCollison = "Metal";

                }
                else if (collision.gameObject.CompareTag("Untagged"))
                {
                    CurrentCollison = "Stone";
                }
                else if (collision.gameObject.CompareTag("Slip"))
                {
                    CurrentCollison = "Slip";
                }
            }
        }
        public void OnCollisionEnter(Collision collision)
        {
            if (LAndRHandHasTrigger)
            {

            }
            else
            {
                if (collision.gameObject.CompareTag("Grass"))
                {
                    CurrentCollison = "Grass";
                }
                else if (collision.gameObject.CompareTag("Stone"))
                {
                    CurrentCollison = "Stone";
                }
                else if (collision.gameObject.CompareTag("Wood"))
                {
                    CurrentCollison = "Wood";

                }
                else if (collision.gameObject.CompareTag("Metal"))
                {
                    CurrentCollison = "Metal";

                }
                else if (collision.gameObject.CompareTag("Untagged"))
                {
                    CurrentCollison = "Stone";
                }
                else if (collision.gameObject.CompareTag("Slip"))
                {
                    CurrentCollison = "Slip";
                }
            }
        }

        private Vector3 CurrentLeftHandPosition()

        {

            if ((PositionWithOffset(leftHandTransform, leftHandOffset) - headCollider.transform.position).magnitude < maxArmLength)

            {

                return PositionWithOffset(leftHandTransform, leftHandOffset);

            }

            else

            {

                return headCollider.transform.position + (PositionWithOffset(leftHandTransform, leftHandOffset) - headCollider.transform.position).normalized * maxArmLength;

            }

        }



        private Vector3 CurrentRightHandPosition()

        {

            if ((PositionWithOffset(rightHandTransform, rightHandOffset) - headCollider.transform.position).magnitude < maxArmLength)

            {

                return PositionWithOffset(rightHandTransform, rightHandOffset);

            }

            else

            {

                return headCollider.transform.position + (PositionWithOffset(rightHandTransform, rightHandOffset) - headCollider.transform.position).normalized * maxArmLength;

            }

        }



        private Vector3 PositionWithOffset(Transform transformToModify, Vector3 offsetVector)

        {

            return transformToModify.position + transformToModify.rotation * offsetVector;

        }

        public void Play(Collider other)
        {
            if (other.gameObject.CompareTag("Stone"))
            {
                Stone.Play();
            }
            if (other.gameObject.CompareTag("Wood"))
            {
                Wood.Play();
            }
            if (other.gameObject.CompareTag("Grass"))
            {
                Grass.Play();
            }
            if (other.gameObject.CompareTag("Metal"))
            {
                Metal.Play();
            }
        }

        private void Update()

        {




            bool leftHandColliding = false;

            bool rightHandColliding = false;

            Vector3 finalPosition;

            Vector3 rigidBodyMovement = Vector3.zero;

            Vector3 firstIterationLeftHand = Vector3.zero;

            Vector3 firstIterationRightHand = Vector3.zero;

            RaycastHit hitInfo;



            bodyCollider.transform.eulerAngles = new Vector3(0, headCollider.transform.eulerAngles.y, 0);



            //left hand



            Vector3 distanceTraveled = CurrentLeftHandPosition() - lastLeftHandPosition + Vector3.down * 2f * 9.8f * Time.deltaTime * Time.deltaTime;



            leftHandFollower.localRotation = leftHandTransform.localRotation;



            if (IterativeCollisionSphereCast(lastLeftHandPosition, minimumRaycastDistance, distanceTraveled, defaultPrecision, out finalPosition, true))

            {

                //this lets you stick to the position you touch, as long as you keep touching the surface this will be the zero point for that hand

                if (wasLeftHandTouching)

                {

                    firstIterationLeftHand = lastLeftHandPosition - CurrentLeftHandPosition();

                }

                else

                {

                    firstIterationLeftHand = finalPosition - CurrentLeftHandPosition();



                    StartVibration(true, vibrationAmmount, 0.15f);

                    if (CurrentCollison == "Grass")
                    {
                        leftHandTransform.GetComponent<SphereCollider>().material = null;
                        rightHandTransform.GetComponent<SphereCollider>().material = null;
                        Grass.Play();
                    }
                    else if (CurrentCollison == "Stone")
                    {
                        leftHandTransform.GetComponent<SphereCollider>().material = null;
                        rightHandTransform.GetComponent<SphereCollider>().material = null;
                        Stone.Play();
                    }
                    else if (CurrentCollison == "Wood")
                    {
                        leftHandTransform.GetComponent<SphereCollider>().material = null;
                        rightHandTransform.GetComponent<SphereCollider>().material = null;
                        Wood.Play();
                    }
                    else if (CurrentCollison == "Metal")
                    {
                        leftHandTransform.GetComponent<SphereCollider>().material = null;
                        rightHandTransform.GetComponent<SphereCollider>().material = null;
                        Metal.Play();
                    }
                    else if (CurrentCollison == "Slip")
                    {
                        leftHandTransform.GetComponent<SphereCollider>().material = Slip;
                        rightHandTransform.GetComponent<SphereCollider>().material = Slip;
                        Stone.Play();
                    }



                }

                playerRigidBody.velocity = Vector3.zero;



                leftHandColliding = true;

            }



            //right hand



            distanceTraveled = CurrentRightHandPosition() - lastRightHandPosition + Vector3.down * 2f * 9.8f * Time.deltaTime * Time.deltaTime;



            rightHandFollower.localRotation = rightHandTransform.localRotation;



            if (IterativeCollisionSphereCast(lastRightHandPosition, minimumRaycastDistance, distanceTraveled, defaultPrecision, out finalPosition, true))

            {

                if (wasRightHandTouching)

                {

                    firstIterationRightHand = lastRightHandPosition - CurrentRightHandPosition();

                }

                else

                {

                    firstIterationRightHand = finalPosition - CurrentRightHandPosition();



                    StartVibration(false, vibrationAmmount, 0.15f);

                    if (CurrentCollison == "Grass")
                    {
                        leftHandTransform.GetComponent<SphereCollider>().material = null;
                        rightHandTransform.GetComponent<SphereCollider>().material = null;
                        Grass.Play();
                    }
                    else if (CurrentCollison == "Stone")
                    {
                        leftHandTransform.GetComponent<SphereCollider>().material = null;
                        rightHandTransform.GetComponent<SphereCollider>().material = null;
                        Stone.Play();
                    }
                    else if (CurrentCollison == "Wood")
                    {
                        leftHandTransform.GetComponent<SphereCollider>().material = null;
                        rightHandTransform.GetComponent<SphereCollider>().material = null;
                        Wood.Play();
                    }
                    else if (CurrentCollison == "Metal")
                    {
                        leftHandTransform.GetComponent<SphereCollider>().material = null;
                        rightHandTransform.GetComponent<SphereCollider>().material = null;
                        Metal.Play();
                    }
                    else if (CurrentCollison == "Slip")
                    {
                        leftHandTransform.GetComponent<SphereCollider>().material = Slip;
                        rightHandTransform.GetComponent<SphereCollider>().material = Slip;
                        Stone.Play();
                    }

                    //    if (target.tag == "Stone")
                    //      {
                    //          Stone.Play();
                    //     }
                    //      if (target.tag == "Wood")
                    //      {
                    //           Wood.Play();
                    //       }
                    //      if (target.tag == "Grass")
                    //        {
                    //           Grass.Play();
                    //       }
                    //       if (target.tag == "Metal")
                    //       {
                    //           Metal.Play();
                    //       }

                }



                playerRigidBody.velocity = Vector3.zero;



                rightHandColliding = true;

            }



            //average or add



            if ((leftHandColliding || wasLeftHandTouching) && (rightHandColliding || wasRightHandTouching))

            {

                //this lets you grab stuff with both hands at the same time

                rigidBodyMovement = (firstIterationLeftHand + firstIterationRightHand) / 2;

            }

            else

            {

                rigidBodyMovement = firstIterationLeftHand + firstIterationRightHand;

            }



            //check valid head movement



            if (IterativeCollisionSphereCast(lastHeadPosition, headCollider.radius, headCollider.transform.position + rigidBodyMovement - lastHeadPosition, defaultPrecision, out finalPosition, false))

            {

                rigidBodyMovement = finalPosition - lastHeadPosition;

                //last check to make sure the head won't phase through geometry

                if (Physics.Raycast(lastHeadPosition, headCollider.transform.position - lastHeadPosition + rigidBodyMovement, out hitInfo, (headCollider.transform.position - lastHeadPosition + rigidBodyMovement).magnitude + headCollider.radius * defaultPrecision * 0.999f, locomotionEnabledLayers.value))

                {

                    rigidBodyMovement = lastHeadPosition - headCollider.transform.position;

                }

            }



            if (rigidBodyMovement != Vector3.zero)

            {

                transform.position = transform.position + rigidBodyMovement;

            }



            lastHeadPosition = headCollider.transform.position;



            //do final left hand position



            distanceTraveled = CurrentLeftHandPosition() - lastLeftHandPosition;



            if (IterativeCollisionSphereCast(lastLeftHandPosition, minimumRaycastDistance, distanceTraveled, defaultPrecision, out finalPosition, !((leftHandColliding || wasLeftHandTouching) && (rightHandColliding || wasRightHandTouching))))

            {

                lastLeftHandPosition = finalPosition;

                leftHandColliding = true;

            }

            else

            {

                lastLeftHandPosition = CurrentLeftHandPosition();

            }



            //do final right hand position



            distanceTraveled = CurrentRightHandPosition() - lastRightHandPosition;



            if (IterativeCollisionSphereCast(lastRightHandPosition, minimumRaycastDistance, distanceTraveled, defaultPrecision, out finalPosition, !((leftHandColliding || wasLeftHandTouching) && (rightHandColliding || wasRightHandTouching))))

            {

                lastRightHandPosition = finalPosition;

                rightHandColliding = true;

            }

            else

            {

                lastRightHandPosition = CurrentRightHandPosition();

            }



            StoreVelocities();



            if ((rightHandColliding || leftHandColliding) && !disableMovement)

            {

                if (denormalizedVelocityAverage.magnitude > velocityLimit)

                {

                    if (denormalizedVelocityAverage.magnitude * jumpMultiplier > maxJumpSpeed)

                    {

                        playerRigidBody.velocity = denormalizedVelocityAverage.normalized * maxJumpSpeed;

                    }

                    else

                    {

                        playerRigidBody.velocity = jumpMultiplier * denormalizedVelocityAverage;

                    }

                }

            }



            //check to see if left hand is stuck and we should unstick it



            if (leftHandColliding && (CurrentLeftHandPosition() - lastLeftHandPosition).magnitude > unStickDistance && !Physics.SphereCast(headCollider.transform.position, minimumRaycastDistance * defaultPrecision, CurrentLeftHandPosition() - headCollider.transform.position, out hitInfo, (CurrentLeftHandPosition() - headCollider.transform.position).magnitude - minimumRaycastDistance, locomotionEnabledLayers.value))

            {

                lastLeftHandPosition = CurrentLeftHandPosition();

                leftHandColliding = false;

            }



            //check to see if right hand is stuck and we should unstick it



            if (rightHandColliding && (CurrentRightHandPosition() - lastRightHandPosition).magnitude > unStickDistance && !Physics.SphereCast(headCollider.transform.position, minimumRaycastDistance * defaultPrecision, CurrentRightHandPosition() - headCollider.transform.position, out hitInfo, (CurrentRightHandPosition() - headCollider.transform.position).magnitude - minimumRaycastDistance, locomotionEnabledLayers.value))

            {

                lastRightHandPosition = CurrentRightHandPosition();

                rightHandColliding = false;

            }



            leftHandFollower.position = lastLeftHandPosition;

            rightHandFollower.position = lastRightHandPosition;



            wasLeftHandTouching = leftHandColliding;

            wasRightHandTouching = rightHandColliding;


            // This is a Anti cheating

        }



        private bool IterativeCollisionSphereCast(Vector3 startPosition, float sphereRadius, Vector3 movementVector, float precision, out Vector3 endPosition, bool singleHand)
        {
            if (!ghostHands)
            {
                RaycastHit hitInfo;
                Vector3 movementToProjectedAboveCollisionPlane;
                Surface gorillaSurface;
                float slipPercentage;
                //first spherecast from the starting position to the final position
                if (CollisionsSphereCast(startPosition, sphereRadius * precision, movementVector, precision, out endPosition, out hitInfo))
                {
                    //if we hit a surface, do a bit of a slide. this makes it so if you grab with two hands you don't stick 100%, and if you're pushing along a surface while braced with your head, your hand will slide a bit

                    //take the surface normal that we hit, then along that plane, do a spherecast to a position a small distance away to account for moving perpendicular to that surface
                    Vector3 firstPosition = endPosition;
                    gorillaSurface = hitInfo.collider.GetComponent<Surface>();
                    slipPercentage = gorillaSurface != null ? gorillaSurface.slipPercentage : (!singleHand ? defaultSlideFactor : 0.001f);
                    movementToProjectedAboveCollisionPlane = Vector3.ProjectOnPlane(startPosition + movementVector - firstPosition, hitInfo.normal) * slipPercentage;
                    if (CollisionsSphereCast(endPosition, sphereRadius, movementToProjectedAboveCollisionPlane, precision * precision, out endPosition, out hitInfo))
                    {
                        //if we hit trying to move perpendicularly, stop there and our end position is the final spot we hit
                        return true;
                    }
                    //if not, try to move closer towards the true point to account for the fact that the movement along the normal of the hit could have moved you away from the surface
                    else if (CollisionsSphereCast(movementToProjectedAboveCollisionPlane + firstPosition, sphereRadius, startPosition + movementVector - (movementToProjectedAboveCollisionPlane + firstPosition), precision * precision * precision, out endPosition, out hitInfo))
                    {
                        //if we hit, then return the spot we hit
                        return true;
                    }
                    else
                    {
                        //this shouldn't really happe, since this means that the sliding motion got you around some corner or something and let you get to your final point. back off because something strange happened, so just don't do the slide
                        endPosition = firstPosition;
                        return true;
                    }
                }
                //as kind of a sanity check, try a smaller spherecast. this accounts for times when the original spherecast was already touching a surface so it didn't trigger correctly
                else if (CollisionsSphereCast(startPosition, sphereRadius * precision * 0.66f, movementVector.normalized * (movementVector.magnitude + sphereRadius * precision * 0.34f), precision * 0.66f, out endPosition, out hitInfo))
                {
                    endPosition = startPosition;
                    return true;
                }
                else
                {
                    endPosition = Vector3.zero;
                    return false;
                }
            }
            else
            {
                endPosition = Vector3.zero;
                return false;
            }
        }



        private bool CollisionsSphereCast(Vector3 startPosition, float sphereRadius, Vector3 movementVector, float precision, out Vector3 finalPosition, out RaycastHit hitInfo)

        {

            //kind of like a souped up spherecast. includes checks to make sure that the sphere we're using, if it touches a surface, is pushed away the correct distance (the original sphereradius distance). since you might

            //be pushing into sharp corners, this might not always be valid, so that's what the extra checks are for



            //initial spherecase

            RaycastHit innerHit;

            if (Physics.SphereCast(startPosition, sphereRadius * precision, movementVector, out hitInfo, movementVector.magnitude + sphereRadius * (1 - precision), locomotionEnabledLayers.value))

            {

                //if we hit, we're trying to move to a position a sphereradius distance from the normal

                finalPosition = hitInfo.point + hitInfo.normal * sphereRadius;



                //check a spherecase from the original position to the intended final position

                if (Physics.SphereCast(startPosition, sphereRadius * precision * precision, finalPosition - startPosition, out innerHit, (finalPosition - startPosition).magnitude + sphereRadius * (1 - precision * precision), locomotionEnabledLayers.value))

                {

                    finalPosition = startPosition + (finalPosition - startPosition).normalized * Mathf.Max(0, hitInfo.distance - sphereRadius * (1f - precision * precision));

                    hitInfo = innerHit;

                }

                //bonus raycast check to make sure that something odd didn't happen with the spherecast. helps prevent clipping through geometry

                else if (Physics.Raycast(startPosition, finalPosition - startPosition, out innerHit, (finalPosition - startPosition).magnitude + sphereRadius * precision * precision * 0.999f, locomotionEnabledLayers.value))

                {

                    finalPosition = startPosition;

                    hitInfo = innerHit;

                    return true;

                }

                return true;

            }

            //anti-clipping through geometry check

            else if (Physics.Raycast(startPosition, movementVector, out hitInfo, movementVector.magnitude + sphereRadius * precision * 0.999f, locomotionEnabledLayers.value))

            {

                finalPosition = startPosition;

                return true;

            }

            else

            {

                finalPosition = Vector3.zero;

                return false;

            }

        }



        public bool IsHandTouching(bool forLeftHand)

        {

            if (forLeftHand)

            {

                return wasLeftHandTouching;

            }

            else

            {

                return wasRightHandTouching;

            }

        }



        public void Turn(float degrees)

        {

            transform.RotateAround(headCollider.transform.position, transform.up, degrees);

            denormalizedVelocityAverage = Quaternion.Euler(0, degrees, 0) * denormalizedVelocityAverage;

            for (int i = 0; i < velocityHistory.Length; i++)

            {

                velocityHistory[i] = Quaternion.Euler(0, degrees, 0) * velocityHistory[i];

            }

        }



        public void StartVibration(bool forLeftController, float amplitude, float duration)

        {

            base.StartCoroutine(this.HapticPulses(forLeftController, amplitude, duration));

        }



        // Token: 0x06000315 RID: 789 RVA: 0x00016512 File Offset: 0x00014712

        private IEnumerator HapticPulses(bool forLeftController, float amplitude, float duration)

        {

            float startTime = Time.time;

            uint channel = 0U;

            InputDevice device;

            if (forLeftController)

            {

                device = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

            }

            else

            {

                device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

            }

            while (Time.time < startTime + duration)

            {

                device.SendHapticImpulse(channel, amplitude, this.hapticWaitSeconds);

                yield return new WaitForSeconds(this.hapticWaitSeconds * 0.9f);

            }

            yield break;

        }



        private void StoreVelocities()

        {

            velocityIndex = (velocityIndex + 1) % velocityHistorySize;

            Vector3 oldestVelocity = velocityHistory[velocityIndex];

            currentVelocity = (transform.position - lastPosition) / Time.deltaTime;

            denormalizedVelocityAverage += (currentVelocity - oldestVelocity) / (float)velocityHistorySize;

            velocityHistory[velocityIndex] = currentVelocity;

            lastPosition = transform.position;

        }



    }

}

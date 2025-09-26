using UnityEngine;
using System.Collections.Generic;
using System;
using AI.Core;

namespace GDD3400.Project01
{
    public class Dog : MonoBehaviour
    {

        private bool _isActive = true;
        public bool IsActive
        {
            get => _isActive;
            set => _isActive = value;
        }

        // Required Variables (Do not edit!)
        private float _maxSpeed = 5f;
        private float _sightRadius = 7.5f;

        [NonSerialized] private float _turnRate = 5f;

        float wanderYaw;

        // Layers - Set In Project Settings
        private LayerMask _targetsLayer;
        private LayerMask _obstaclesLayer;

        // Tags - Set In Project Settings
        private string friendTag = "Friend";
        private string threatTag = "Threat";
        private string safeZoneTag = "SafeZone";

        float maxRotationDegPerSec = 90f;

        //target to chase
        Vector3 _target;

        Vector3 _floatingTarget;

        RaycastHit hit;

        float stoppingDistance = 2.5f;

        float _currSpeed = 0;
        float _maxAcceleration = 0.5f;

        float walkSpeed = 2f;


        float searchRadius = 20f;

        float maxRotation = 10f;
        Vector3 targetDirection;

        Rigidbody _rb;


        private Collider[] _tmpTargets = new Collider[16];

        private float _targetSpeed;

        List<Collider> _trackedTargets;


        Collider _safeZone;

        Vector3 velocity;

        List<Vector3> lastKnownLocs;

        bool reachedPosition;

        Quaternion _targetRotation;

        Vector3 avoidDir;

        Vector3 LeftAngle;
        Vector3 RightAngle; 

        bool hitObject = false;
        public enum Dogstate
        {
            None,
            Chasing,
            Wandering,
            Scaning,
            Loc,
            Friendly,
            Goal
        }

        Dogstate currentState = Dogstate.Wandering;

        public void Awake()
        {
            // Find the layers in the project settings
            _targetsLayer = LayerMask.GetMask("Targets");
            _obstaclesLayer = LayerMask.GetMask("Obstacles");
            _trackedTargets = new List<Collider>();
            _rb = GetComponent<Rigidbody>();
            lastKnownLocs = new List<Vector3>();

        }



        private void Update()
        {
            if (!_isActive) return;
            Debug.DrawRay(transform.position, LeftAngle * 2f, Color.green);
            Debug.DrawRay(transform.position, RightAngle * 2f, Color.blue);
            Debug.DrawRay(transform.position, transform.forward * 2f, Color.red);
            Perception();
            DecisionMaking();
        }

        private void Perception()
        {
            LeftAngle = Quaternion.Euler(0, -30, 0) * transform.forward;
            RightAngle = Quaternion.Euler(0, 30 , 0) * transform.forward;
            


            if (_trackedTargets.Count > 0)
            {
                lastKnownLocs.Clear();
                foreach (Collider target in _trackedTargets)
                {
                    if (target != null)
                    {
                        lastKnownLocs.Add(target.transform.position);
                    }
                }
            }

            //clear our tracked targets 
            _trackedTargets.Clear();




            //get reference to the safe zone
            if (_safeZone == null)
            {
                _safeZone = GameObject.FindGameObjectWithTag(safeZoneTag).GetComponent<Collider>();
                //starts at the goal position



            }

            //do collison here and store data in the _tmpTargets
            int t = Physics.OverlapSphereNonAlloc(transform.position, _sightRadius, _tmpTargets, _targetsLayer);
            

            

            //loop over every target we collected in the sight radius
            for (int i = 0; i < t; i++)
            {
                Collider c = _tmpTargets[i];
                if (c.gameObject.GetComponent<Sheep>() != null && !c.gameObject.GetComponent<Sheep>().InSafeZone)
                {
                    {
                        //track the target if it has the sheep script on it
                        _trackedTargets.Add(c);
                    }
                }


            }

            switch (currentState)
            {
                case Dogstate.None:
                    FindNextObjective();
                    break;
                case Dogstate.Wandering:
                    Wander();
                    break;
                case Dogstate.Scaning:
                    Scan();
                    break;
                case Dogstate.Chasing:
                    Chase();
                    break;
                case Dogstate.Loc:
                    GotoLocation();
                    break;
                case Dogstate.Friendly:
                    SetFriendlyTag();
                    break;
                case Dogstate.Goal:
                    GotoGoal();

                    break;

            }
        }

        private void SetFriendlyTag()
        {
            gameObject.GetComponentInChildren<Collider>().gameObject.tag = friendTag;
            currentState = Dogstate.Goal;
            return;
        }

        private void GotoLocation()
        {
            float dist = (transform.position - _target).magnitude;
            if (transform.position.magnitude > dist)
            {
                _rb.MovePosition(transform.position + velocity * Time.fixedDeltaTime);
            }
            else
            {
                if (_trackedTargets.Count > 0)
                {
                    currentState = Dogstate.Chasing;
                    return;
                }
                else
                {
                    currentState = Dogstate.Wandering;
                }

            }
        }

        private void DecisionMaking()
        {
           
                CalculateMoveToTarget();
           
          
        }

        private void CalculateMoveToTarget()
        {

            reachedPosition = false;
            _floatingTarget = Vector3.Lerp(_floatingTarget, _target, Time.deltaTime * 10f);




        }

        /// <summary>
        /// Make sure to use FixedUpdate for movement with physics based Rigidbody
        /// You can optionally use FixedDeltaTime for movement calculations, but it is not required since fixedupdate is called at a fixed rate
        /// </summary>
        private void FixedUpdate()
        {


            



            if (!_isActive) return;

            if (_rb.linearVelocity.sqrMagnitude <= 0.01)
            {
                Debug.Log(currentState.ToString());
            }

            if (Physics.Raycast(transform.position, LeftAngle, 2, _obstaclesLayer) || Physics.Raycast(transform.position, RightAngle, 2, _obstaclesLayer) || Physics.Raycast(transform.position, transform.forward, 2, _obstaclesLayer))
            {
                hitObject = true;


            }
            else
            {
                hitObject = false;
            }
          


            float dist = Vector3.Distance(transform.position, _target);
            //if we have a target and the distance to the target is still greater than the stopping distance
            if (dist >= stoppingDistance)
            {

                targetDirection = (_floatingTarget - transform.position).normalized;
       
                velocity = targetDirection * Mathf.Min(_targetSpeed, dist);
                

                
              

                _rb.linearVelocity = velocity;

            }


            else
            {
                reachedPosition = true;
                velocity = Vector3.zero;
                _rb.linearVelocity = velocity;
                //if we have reached our target, then set our new state to going toward the goal
                if (Dogstate.Chasing == currentState)
                {
                    currentState = Dogstate.Friendly;
                }
                else
                {
                    currentState = Dogstate.Chasing;
                }
            }

            //if our velocity is not zero
            if (velocity.sqrMagnitude > 0.001f)
            {

                //look in the direction of our velocity
                _targetRotation = Quaternion.LookRotation(velocity);
                //smooth rotation so doesn't snap instantly
                transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, Time.deltaTime * maxRotationDegPerSec);


            }


        }

        void Wander()
        {
          
            GetComponentInChildren<Collider>().gameObject.tag = friendTag;
            targetDirection = UnityEngine.Random.insideUnitSphere * 50f;
            targetDirection.y = 0;
            _target = transform.position + targetDirection;
            _targetSpeed = _maxSpeed;
            currentState = Dogstate.Chasing;
            return;

        }

        void Scan()
        {
        }
        void Chase()
        {
            if (_trackedTargets.Count > 0)
            {
                stoppingDistance = 2.5f;
                _target = FindClosestSheep();
                _targetSpeed = _maxSpeed;
            }
            else
            {
                if (reachedPosition || hitObject)
                {
                    currentState = Dogstate.Wandering;
                    return;
                }

            }


            }
            void FindNextObjective()
        {


            if (avoidDir != Vector3.zero) {
                reachedPosition = true;
            }

            if (reachedPosition || _trackedTargets.Count > 0)
            {


                currentState = Dogstate.Chasing;




            }


        }
        void GotoGoal()
        {
            _target = _safeZone.transform.position;
            _targetSpeed = 4f;
            stoppingDistance = 4f;
            foreach (Collider c in _trackedTargets)
            {
                if (c.GetComponent<Sheep>() != null && c.GetComponent<Sheep>()._targetSpeed >=5f)
                {
                    reachedPosition= true;
                    currentState = Dogstate.Wandering;
                    _trackedTargets.Remove(c);
                    return;
                }
            }
            if (reachedPosition)
            {
                if (_trackedTargets.Count > 0)
                {
                    currentState = Dogstate.Chasing;
                }
                else
                {
                    currentState = Dogstate.Wandering;
                   
                }
            }
            else
            {
                if (_trackedTargets.Count == 0)
                {
                    currentState = Dogstate.Wandering;
                }
            }
           
            }

            Collider FindClosestTarget()
            {
                float closestDistance = float.MaxValue;
                Collider colliderclosest = null;
                if (_trackedTargets.Count > 0)
                {
                    foreach (Collider target in _trackedTargets)
                    {
                        float distance = (transform.position - target.transform.position).magnitude;
                        if (distance < closestDistance) {
                            closestDistance = distance;
                            colliderclosest = target;
                        }
                    }
                }

                return colliderclosest;
            }

        Vector3 FindClosestSheep()
        {
            float closestDistance = float.MaxValue;
            Collider colliderclosest = null;
            if (_trackedTargets.Count > 0)
            {
                foreach (Collider target in _trackedTargets)
                {
                    float distance = (transform.position - target.transform.position).magnitude;
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        colliderclosest = target;
                    }
                }
            }

            return colliderclosest.transform.position;
        }

        float RandomBinomial()
        {
            return 1;
        }



        }
    }


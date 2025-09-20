using UnityEngine;
using System.Collections.Generic;
using System;

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

        // Layers - Set In Project Settings
        private LayerMask _targetsLayer;
        private LayerMask _obstaclesLayer;

        // Tags - Set In Project Settings
        private string friendTag = "Friend";
        private string threatTag = "Threat";
        private string safeZoneTag = "SafeZone";

        Collider _target;

        float stoppingDistance;

        float _currSpeed = 0;
        float _maxAcceleration = 0.5f;


        Rigidbody _rb;


        private Collider[] _tmpTargets = new Collider[16];

        private float _targetSpeed;

        List<Collider> _trackedTargets;


        Collider _safeZone;

        public enum Dogstate
        {
            None,
            Chasing,
            Patroling,
            Scaning
        }

        Dogstate currentState = Dogstate.None;

        public void Awake()
        {
            // Find the layers in the project settings
            _targetsLayer = LayerMask.GetMask("Targets");
            _obstaclesLayer = LayerMask.GetMask("Obstacles");
            _trackedTargets = new List<Collider>();
            _rb= GetComponent<Rigidbody>();
        }

       

        private void Update()
        {
            if (!_isActive) return;
            
            Perception();
            DecisionMaking();
        }

        private void Perception()
        {
            _trackedTargets.Clear();
            if(_safeZone== null)
            {
                _safeZone = GameObject.FindGameObjectWithTag(safeZoneTag).GetComponent<Collider>();
                //starts at the goal position
               

                Debug.Log("found safe zone");
            }

            //do collison here and store data in the _tmpTargets
            int t = Physics.OverlapSphereNonAlloc(transform.position, _sightRadius, _tmpTargets,_targetsLayer);

            //loop over every target we collected in the sight radius
            for(int i=0;i<t; i++)
            {
                var c = _tmpTargets[i];
                if (c.tag == friendTag)
                {
                    _trackedTargets.Add(c);
                }
                
            }
            switch (currentState)
            {
                case Dogstate.None:
                    GotoGoal();
                    break;
                case Dogstate.Patroling:
                    Patrol();
                    break;
                case Dogstate.Scaning:
                    Scan();
                    break;
                case Dogstate.Chasing:
                    Chase();
                    break;

            }
        }

        private void DecisionMaking()
        {
            CalculateMoveToTarget();
        }

        private void CalculateMoveToTarget()
        {
            //calculate the acceleration, velocity and dirrection based upon the desired target
            if (_trackedTargets.Count == 0) return;
            if (_target == null) return;

           

            


        }

        /// <summary>
        /// Make sure to use FixedUpdate for movement with physics based Rigidbody
        /// You can optionally use FixedDeltaTime for movement calculations, but it is not required since fixedupdate is called at a fixed rate
        /// </summary>
        private void FixedUpdate()
        {
            if (!_isActive) return;
            
        }

        void Patrol()
        {
            //pick known tracked colliders to patrol around
        }

        void Scan()
        {
            //find targets by moving around the field
        }
        void Chase()
        {
            _target = FindClosestTarget();
        }
        void GotoGoal()
        {
           _target = _safeZone;

        }

        Collider FindClosestTarget()
        {
            float closestDistance = 99999;
            Collider colliderclosest = null;
            if(_trackedTargets.Count==0) return null;
            foreach(Collider target in _trackedTargets)
            {
                if ((target.transform.position - transform.position).magnitude > closestDistance)
                {
                    closestDistance = (target.transform.position - transform.position).magnitude;
                    colliderclosest= target;
                }
            }
            return colliderclosest;
        }

        

    }
}

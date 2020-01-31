﻿using UnityEngine;
using Utils;

namespace Player
{
    public class PlayerGridInput : MonoBehaviour
    {
        [Header("Player")] public Transform playerTransform;
        public PlayerGridController playerGridController;
        public Vector3 followOffset;

        [Header("Raycast")] public Transform leftRaycast;
        public Transform rightRaycast;
        public Transform forwardRaycast;
        public Transform backRaycast;
        public float raycastDistance;

        #region Unity Functions

        private void Update()
        {
            if (Input.GetKeyDown(ControlConstants.Left) || Input.GetKeyDown(ControlConstants.AltLeft))
            {
                FindTargetRayCast(Vector3.right, leftRaycast.position); // TODO: Check why this is opposite
            }
            else if (Input.GetKeyDown(ControlConstants.Right) || Input.GetKeyDown(ControlConstants.AltRight))
            {
                FindTargetRayCast(Vector3.left, rightRaycast.position); // TODO: Check why this is opposite
            }
            else if (Input.GetKeyDown(ControlConstants.Forward) || Input.GetKeyDown(ControlConstants.AltForward))
            {
                FindTargetRayCast(Vector3.forward, forwardRaycast.position);
            }
            else if (Input.GetKeyDown(ControlConstants.Back) || Input.GetKeyDown(ControlConstants.AltBack))
            {
                FindTargetRayCast(Vector3.back, backRaycast.position);
            }

            FollowPlayer();
        }

        #endregion

        #region Utility Functions

        private void FollowPlayer() => transform.position = playerTransform.position + followOffset;

        private void FindTargetRayCast(Vector3 rayCastDirection, Vector3 position)
        {
            if (Physics.Raycast(position, rayCastDirection, out RaycastHit hit, raycastDistance))
            {
                if (hit.collider.CompareTag(TagManager.GridMarker)
                    || hit.collider.CompareTag(TagManager.InsideOut))
                {
                    Vector3 targetMovementPosition = hit.collider.transform.position;
                    playerGridController.SetPlayerTargetLocation(targetMovementPosition);
                }
                else if (hit.collider.CompareTag(TagManager.WinMarker))
                {
                    Vector3 targetMovementPosition = hit.collider.transform.position;
                    playerGridController.SetPlayerTargetLocation(targetMovementPosition);
                }
            }
        }

        #endregion
    }
}
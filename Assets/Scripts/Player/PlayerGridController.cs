﻿using Scenes.Main;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerGridController : MonoBehaviour
    {
        [Header("Player Movement")] public float movementLerpSpeed;
        public float movementLerpTolerance;
        public Vector3 posiitionOffset;

        [Header("Player Rotation")] public float rotationSpeed;

        [Header("Water Hole")] public float rayCastDistance = 3;

        // Movement
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private Vector3 _lastPosition;
        private float _lerpAmount;
        private bool _positionReached;

        // Components
        private Rigidbody _playerRb;
        private SphereCollider _playerCollider;

        // Player State
        private PlayerState _playerState;

        #region Unity Functions

        private void Start()
        {
            _playerRb = GetComponent<Rigidbody>();
            _playerCollider = GetComponent<SphereCollider>();

            _lastPosition = transform.position;
            _positionReached = true;
            _lerpAmount = 0;

            SetPlayerState(PlayerState.PlayerInControl);
        }

        private void Update()
        {
            switch (_playerState)
            {
                case PlayerState.PlayerInControl:
                {
                    OrientPlayerToPosition();
                    MovePlayer();
                }
                    break;

                case PlayerState.PlayerStatic:
                    // Probably don't do anything here...
                    break;

                case PlayerState.PlayerEndState:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(TagManager.GridMarker))
            {
                transform.SetParent(other.transform.parent.parent);
            }
            else if (other.CompareTag(TagManager.WaterHole))
            {
                SetPlayerEndState(false);
            }
            else if (other.CompareTag(TagManager.WinMarker))
            {
                SetPlayerEndState(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(TagManager.GridMarker))
            {
                transform.SetParent(null);
            }
        }

        #endregion

        #region External Functions

        public void SetPlayerTargetLocation(Vector3 targetPosition)
        {
            // Don't detect inputs when the player is not in control
            if (_playerState != PlayerState.PlayerInControl || !_positionReached)
            {
                return;
            }

            targetPosition += posiitionOffset;
            _targetPosition = targetPosition;

            _startPosition = transform.position;
            _lerpAmount = 0;
            _positionReached = false;

            _playerRb.isKinematic = true;
            _playerRb.useGravity = false;
            _playerCollider.isTrigger = true;
        }

        public bool IsPlayerMoving() => !_positionReached;

        public void PreventPlayerMovement()
        {
            if (_playerState == PlayerState.PlayerEndState)
            {
                return;
            }

            SetPlayerState(PlayerState.PlayerStatic);
            _playerRb.isKinematic = true;
        }

        public void AllowPlayerMovement()
        {
            if (_playerState == PlayerState.PlayerEndState)
            {
                return;
            }

            SetPlayerState(PlayerState.PlayerInControl);
            _playerRb.isKinematic = false;
        }

        #endregion

        #region Utility Functions

        #region Player  Movement

        private void MovePlayer()
        {
            if (_positionReached)
            {
                return;
            }

            _lastPosition = transform.position;
            _lerpAmount += movementLerpSpeed * Time.deltaTime;
            transform.position = Vector3.Lerp(_startPosition, _targetPosition, _lerpAmount);

            if (_lerpAmount >= movementLerpTolerance)
            {
                transform.position = _targetPosition;
                _positionReached = true;

                _playerRb.isKinematic = false;
                _playerRb.useGravity = true;
                _playerCollider.isTrigger = false;

                // Probably do this somewhere else. Should be a better way to do it.
                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, rayCastDistance))
                {
                    Debug.Log(hit.collider.name);
                    if (hit.collider.CompareTag(TagManager.WaterHole))
                    {
                        //Debug.Log( ",??? ");
                        SetPlayerEndState(false);
                    }
                    else if(hit.collider.CompareTag(TagManager.InsideOut))
                    {
                        //hit.collider.transform.parent.transform.rotation = Quaternion.Lerp()
                        Transform parent = hit.collider.transform.parent;
                        Transform currentParent = this.transform.parent;
                        Debug.Log(parent + ", " + currentParent);
                    }
                }
            }
        }

        private void OrientPlayerToPosition()
        {
            if (_positionReached)
            {
                return;
            }

            Vector3 currentPosition = transform.position;
            Vector3 direction = _lastPosition - currentPosition;

            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
        }

        private void FlipCubeFacet(Transform i_parent)
        {

        }

        #endregion

        private void SetPlayerEndState(bool didPlayerWin)
        {
            SetPlayerState(PlayerState.PlayerEndState);

            _playerCollider.isTrigger = true;
            if (!didPlayerWin) // This means currently they have hit the water hole
            {
                _playerRb.isKinematic = false;
                _playerRb.useGravity = true;
            }

            if (didPlayerWin)
            {
                int buildIndex = SceneManager.GetActiveScene().buildIndex;
                MainSceneController.Instance.LoadNextLevel(buildIndex + 1);
            }
            else
            {
                MainSceneController.Instance.ReloadCurrentLevel();
            }
        }

        private void SetPlayerState(PlayerState playerState) => _playerState = playerState;

        #endregion

        #region Enums

        private enum PlayerState
        {
            PlayerInControl,
            PlayerStatic,
            PlayerEndState
        }

        #endregion
    }
}
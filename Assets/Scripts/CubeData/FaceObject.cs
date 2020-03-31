﻿using CubeData;
using WorldCube;
using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TileFunction
{
    Turn, Ramp, Wall, Custom, None
}

public enum TurnDirection
{
    Forward, Right, Back, Left
}

public enum ReachEvent
{
    None, Water, Exit
}
[ExecuteAlways]
public class FaceObject : MonoBehaviour
{
    [Header("Facet Function")]
    public TileFunction faceType = TileFunction.None;
    //public bool applyChange = false;
    

    [Header("Face-specific")]
    public TurnDirection turnTo = TurnDirection.Forward;   //default turn to left if this is a turn-facet
    public bool showWallFace = true;
    public GameObject turnArrow;
    public GameObject wallTile;
    public GameObject water;
    private GameObject arrow_instantiated;
    private GameObject water_instantiated;
    private GameObject exit_instantiated;

    //  Mark the accessable directions starting from this tile object.
    [Header("Custom Access")]
    public bool forward;
    public bool back;
    public bool right;
    public bool left;
    public bool up;
    public bool down;

    [Header("EventAfterReaching")]
    public ReachEvent faceEvent;

    private GameObject instantiated_arrow;
    
    private void Awake()
    {
        if (this.isActiveAndEnabled)
            StartCoroutine(ClearAddOns());
    }

    public CubeLayerMask GetMoveDirection(CubeLayerMask i_direction)
    {
        //Debug.Log(this.transform.GetInstanceID());
        // Return CubeLayerMask.Zero if the path is blocked
        // Return i_direction if the path is accessible and the direction keeps
        // Return another CubeLayerMask if the path is accessible but the direction changes.
        //Debug.Log("i_direction: " + i_direction.ToVector3());

        Vector3 moveDir = i_direction.ToVector3();
        Vector3 playerDir = GetPlayerRelativeDir(moveDir); //mark the direction of the player relative to this tile

        if (AccessAvailable(playerDir))
        {
            if (faceType == TileFunction.Ramp)
            {
                //Debug.Log("Go up ramp - change direction Up");
                if (playerDir == CubeLayerMask.forward.ToVector3())
                    return CubeLayerMask.up;
                else
                    return i_direction;
            }
            else
            {
                //Debug.Log("Don't change dir");
                return i_direction;
            }
                
        }
        else
        {
            //Debug.Log(this.transform.name);
            //Debug.Log(gameObject.transform.parent.name + "Blocked");
            return CubeLayerMask.Zero;
        }
            

        //throw new NotImplementedException();
    }

    public void OnPlayerEnter(Dummy dummy)
    {
        //throw new NotImplementedException();
        if(faceEvent == ReachEvent.Water)
        {
            Debug.Log("Player Died. Reloading the scene");
            CubeInputController.Instance.CloseSocketConnection(); // Very Hacky. But a temp fix
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else if (faceEvent == ReachEvent.Exit)
        {
            Debug.Log("Player Won");

            int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
            if (currentBuildIndex + 1 <= SceneManager.sceneCountInBuildSettings)
            {
                //Something wrong here when loading next level so I commented it out
                //CubeInputController.Instance.CloseSocketConnection(); // Very Hacky. But a temp fix
                SceneManager.LoadScene(currentBuildIndex + 1);
            }
            else
                Debug.Log("This is already the last level");
        }
    }

    public (CubeLayerMask, bool) TryChangeDirection(CubeLayerMask i_direction)
    {
        Vector3 moveDir = i_direction.ToVector3();
        Vector3 playerDir = GetPlayerRelativeDir(new CubeLayerMask(moveDir*-1)); //mark the direction of the player relative to this tile

        if (AccessAvailable(playerDir))
        {
            if (faceType == TileFunction.Ramp)
            {
                //The condition where the player climbs up a ramp
                //Debug.Log("Go up ramp - change direction Up");
                if (i_direction.ToVector3() == this.transform.forward)   //see if the player is climbing the ramp
                    return (new CubeLayerMask(-this.transform.up), false);
                else if (i_direction.ToVector3() == -this.transform.up)
                {
                    return (new CubeLayerMask(-this.transform.forward), false);
                }
                else
                    return (i_direction, false);
            }
            else if (faceType == TileFunction.Turn)
            {
                //The condition where the player meets a turning face
                if (turnTo == TurnDirection.Forward)
                {
                    return (new CubeLayerMask(this.transform.forward), true);
                }
                else if (turnTo == TurnDirection.Back)
                {
                    return (new CubeLayerMask(-this.transform.forward), true);
                }
                else if(turnTo == TurnDirection.Left)
                {
                    return (new CubeLayerMask(-this.transform.right), true);
                }
                else
                {
                    return (new CubeLayerMask(this.transform.right), true);
                }
            }
            else if ((faceType == TileFunction.None || faceType == TileFunction.Custom) 
                && i_direction.ToVector3() != Vector3.up)
            {
                return (CubeLayerMask.down, false);
            }
            else
            {
                //The condition where the player can get through the face
                //Debug.Log("Don't change dir");
                return (i_direction, false);
            }

        }
        else
        {
            //The condition where the player gets blocked
            //Debug.Log(this.transform.name);
            //Debug.Log(gameObject.transform.parent.name + "Blocked");
            return (CubeLayerMask.Zero, true);
        }

    }

    private bool AccessAvailable(Vector3 i_dir)
    {
        if (i_dir.magnitude > 1)
        {
            Debug.LogError("Invalid direction vector! Returned with false");
            return false;
        }

        int axis = GetDirection(i_dir);

        switch (axis) //0 - x; 1 - y; 2 - z
        {
            case 0:
                if (i_dir.x > 0)
                {
                    //Debug.Log("Player on Right");
                    return right;
                }
                else
                {
                    //Debug.Log("Player on Left");
                    return left;
                }
                    
            case 1:
                if (i_dir.y > 0)
                {
                    //Debug.Log("Player on Up");
                    return down;
                }
                else
                {
                    //Debug.Log("Player on Down");
                    return up;
                }
                    
            case 2:
                if (i_dir.z > 0)
                {
                    //Debug.Log("Player on Forward");
                    return forward;
                }
                else
                {
                    //Debug.Log("Player on Back");
                    return back;
                }
                    
        }

        Debug.LogError("Out of conditions error!");
        return false;
    }

    private int GetDirection(Vector3 i_dir)
    {
        if (i_dir.x != 0)
            return 0;
        else if (i_dir.y != 0)
            return 1;
        else if (i_dir.z != 0)
            return 2;
        Debug.Log(i_dir);
        Debug.LogError("Condition broken");
        return -1;
    }

    private Vector3 GetPlayerRelativeDir(Vector3 i_dir)
    {
        if (-i_dir == this.transform.forward)
            return new Vector3(0, 0, 1);
        else if (-i_dir == -this.transform.forward)
            return new Vector3(0, 0, -1);
        else if (-i_dir == this.transform.right)
            return new Vector3(1, 0, 0);
        else if (-i_dir == -this.transform.right)
            return new Vector3(-1, 0, 0);
        else if (-i_dir == this.transform.up)
            return new Vector3(0, 1, 0);
        else if (-i_dir == -this.transform.up)
            return new Vector3(0, -1, 0);

        Debug.LogError("Not valid direction input");
        return Vector3.zero;
    }

    private Vector3 GetPlayerRelativeDir(CubeLayerMask i_direction)
    {
        if (i_direction == new CubeLayerMask(this.transform.forward))
            return new Vector3(0, 0, 1);
        else if (i_direction == new CubeLayerMask(-this.transform.forward))
            return new Vector3(0, 0, -1);
        else if (i_direction == new CubeLayerMask(this.transform.right))
            return new Vector3(1, 0, 0);
        else if (i_direction == new CubeLayerMask(-this.transform.right))
            return new Vector3(-1, 0, 0);
        else if (i_direction == new CubeLayerMask(this.transform.up))
            return new Vector3(0, 1, 0);
        else if (i_direction == new CubeLayerMask(-this.transform.up))
            return new Vector3(0, -1, 0);

        Debug.LogError("Not valid direction input");
        return Vector3.zero;
    }

    public void SetFaceFunction(TileFunction i_function)
    {
        faceType = i_function;
    }

    private void LoadFaceData()
    {
        

        //Load face access related prefabs and data
        if (faceType == TileFunction.Turn)
        {
            forward = true;
            back = true;
            right = true;
            left = true;
            up = false;
            down = false;
            //Turn the face into the right angle (opposite of the tile/face)
            GameObject arrow_instance = null;
            if (turnArrow)
            {
                //showWallFace = true;
                float rotation_y = 90f * (int)turnTo;
                arrow_instance = Instantiate(turnArrow, this.transform) as GameObject;

                arrow_instance.transform.localEulerAngles = new Vector3(0f, rotation_y, 180f);
                instantiated_arrow = arrow_instance;
                //if (!instantiated_arrow)
                //{
                //    arrow_instance = Instantiate(turnArrow, this.transform) as GameObject;

                //    arrow_instance.transform.localEulerAngles = new Vector3(0f, rotation_y, 180f);
                //    instantiated_arrow = arrow_instance;
                //} 
                //else
                //{
                //    instantiated_arrow.transform.localEulerAngles = new Vector3(0f, rotation_y, 180f);
                //}

            }
            arrow_instantiated = arrow_instance;
            //GetComponent<MeshRenderer>().enabled = false;
            SetGroundVisibility(false);

        }
        else if (faceType == TileFunction.Wall)
        {
            forward = true;
            back = true;
            right = true;
            left = true;
            up = false;
            down = false;

            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
            SetGroundVisibility(true);
            //if (wallTile)
            //Instantiate(wallTile, this.transform.position, this.transform.rotation, this.transform);
        }
        else if (faceType == TileFunction.None)
        {
            forward = true;
            back = true;
            right = true;
            left = true;
            up = true;
            down = true;
            GetComponent<MeshRenderer>().enabled = false;
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        //Load event-related prefabs
        if(faceEvent==ReachEvent.Water)
        {
            GameObject water_instance = null;
            if (water)
            {
                water_instance = Instantiate(water, this.transform) as GameObject;
                SetGroundVisibility(false);
                //float rotation_y = 90f * (int)turnTo;
                water_instance.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
            }
            water_instantiated = water_instance;
        }
        
    }

    private void OnValidate()
    {
        //LoadFaceData();
        if(this.isActiveAndEnabled)
            StartCoroutine(ClearAddOns());
        //if (applyChange)
        //{
        
        //    applyChange = false;
        //}
    }

    IEnumerator ClearAddOns()
    {
        yield return null;//new WaitForEndOfFrame();

        Transform move_sign = transform.Find("Move_Sign(Clone)");
        if (move_sign)
        {
            DestroyImmediate(move_sign.gameObject);
            DestroyImmediate(arrow_instantiated);
            arrow_instantiated = null;
        }
        if(water_instantiated)
        {
            DestroyImmediate(water_instantiated);
            water_instantiated = null;
        }
        if(showWallFace)
        {
            SetGroundVisibility(true);
            //Debug.Log("Show wall faces");
        }
            
        LoadFaceData();
    }

    private void SetGroundVisibility(bool i_visible)
    {
        Transform ground_face = this.transform.Find("Grass Ground Variant");
        if (ground_face)
            ground_face.GetComponent<MeshRenderer>().enabled = i_visible;
    }
}

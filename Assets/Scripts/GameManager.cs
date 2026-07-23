using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    static public GameManager instance; 

    void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
    }

    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] CinemachineCamera camera;
    [SerializeField] Transform cameraShopFocus;
    [SerializeField] GameObject shopUI;

    bool shopping = true;
    InputAction upAction;

    void Start()
    {
        upAction = InputSystem.actions.FindAction("Up");
        upAction.performed += StartRocket;

        playerMovement.enabled = false;
        shopping = true;
    }

    async void StartRocket(InputAction.CallbackContext context)
    {
        if(!shopping)
            return;

        shopping = false;
        shopUI.SetActive(false);
        camera.Follow = playerMovement.transform;

        await Countdown(3);

        playerMovement.enabled = true;
    }

    async Awaitable Countdown(int time)
    {
        for(int i = time; i > 0; i--)
        {
            //TODO display numbers
            Debug.Log(i);

            await Awaitable.WaitForSecondsAsync(1);
        }

        //TODO display blastoff or something
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JoystickPlayerExample : NetworkBehaviour
{
    [Header("Jump")]
    public float jumpForce = 5f;
    public float gravity = -9.81f;
    private float verticalVelocity;
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    public bool isGrounded;
    public float speed = 5f;
    public VariableJoystick variableJoystick;
    public CharacterController controller;
    public float rotationSpeed = 10f;

    private void Start()
    {
        // Garante que só o Player local pega input
        if (IsOwner)
        {
            DontDestroyOnLoad(gameObject);
            variableJoystick = FindObjectOfType<VariableJoystick>();
        }
    }

    private void Update()
    {
        //if (!IsOwner) return;

        variableJoystick = FindObjectOfType<VariableJoystick>();
        isGrounded = Physics.CheckSphere(
        groundCheck.position,
        groundDistance,
        groundMask
    );

    if (isGrounded && verticalVelocity < 0)
    {
        verticalVelocity = -2f; // mantém colado no chão
    }

    Vector3 direction = new Vector3( variableJoystick.Horizontal, 0f, variableJoystick.Vertical);

        if (direction.magnitude >= 0.1f)
        {
            Vector3 move = direction.normalized * speed * Time.deltaTime;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            controller.Move(move);
        }
        verticalVelocity += gravity * Time.deltaTime;
        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    public void Jump()
{
    //if (!IsOwner) return;

    if (isGrounded)
    {
        verticalVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
    }
}

    public void GoToPrivateScene(string privateSceneName)
    {
        if (!IsOwner) return;

        HidePlayerForOthersServerRpc();
        //transform.position = new Vector3(0, 0.55f, 0);
        StartCoroutine(LoadSceneAdditive(privateSceneName, true));
    }

    public void ReturnToLobby()
    {
        if (!IsOwner) return;

        //transform.position = new Vector3(0, 0.55f, 0);
        ShowPlayerForOthersServerRpc();
        StartCoroutine(LoadSceneAdditive("lobby_start", false));
    }

    [ServerRpc(RequireOwnership = false)]
    private void HidePlayerForOthersServerRpc(ServerRpcParams rpcParams = default)
    {
        HidePlayerForOthersClientRpc();
    }

    [ClientRpc]
    private void HidePlayerForOthersClientRpc(ClientRpcParams rpcParams = default)
    {
        if (!IsOwner)
        {
            foreach (var r in GetComponentsInChildren<Renderer>())
                r.enabled = false;

            foreach (var c in GetComponentsInChildren<Collider>())
                c.enabled = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShowPlayerForOthersServerRpc(ServerRpcParams rpcParams = default)
    {
        ShowPlayerForOthersClientRpc();
    }

    [ClientRpc]
    private void ShowPlayerForOthersClientRpc(ClientRpcParams rpcParams = default)
    {
        
            foreach (var r in GetComponentsInChildren<Renderer>())
                r.enabled = true;

            foreach (var c in GetComponentsInChildren<Collider>())
                c.enabled = true;
        
    }

    private IEnumerator LoadSceneAdditive(string sceneName, bool hideOthers)
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        //NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        while (!asyncLoad.isDone)
            yield return null;

        Scene newScene = SceneManager.GetSceneByName(sceneName);
        SceneManager.MoveGameObjectToScene(gameObject, newScene);
        SceneManager.SetActiveScene(newScene);

        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(currentSceneIndex);

        // Define posição padrão (pode ajustar)
        transform.position = new Vector3(0, 0.55f, 0);

        while (!asyncUnload.isDone)
            yield return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("mg1"))
        {
            GoToPrivateScene("mg1");
        }
        else if (other.CompareTag("back"))
        {
            ReturnToLobby();
        }
    }
}

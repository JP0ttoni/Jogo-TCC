using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
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
    public Vector3 last_pos;
    public bool death = false;

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

        if(Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

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
            if(SceneManager.GetActiveScene().name == "mg_geo")
            {
                speed = 23f;
                Vector3 camForward = Camera.main.transform.forward;
                Vector3 camRight = Camera.main.transform.right;

                camForward.y = 0;
                camRight.y = 0;

                camForward.Normalize();
                camRight.Normalize();

                Vector3 moveDirection = camForward * direction.z + camRight * direction.x;
                controller.Move(moveDirection * speed * Time.deltaTime);


                // Rotação para onde anda
                if (moveDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        (rotationSpeed/5) * Time.deltaTime
                    );
                }
            }
            else
            {
                speed = 5f;
                Vector3 move = direction.normalized * speed * Time.deltaTime;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                controller.Move(move);
            }

            verticalVelocity += gravity * Time.deltaTime;
            controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);

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
        Debug.Log(other.name);
        if (other.CompareTag("mg1"))
        {
            GoToPrivateScene("mg1");
        }
        else if (other.CompareTag("back"))
        {
            ReturnToLobby();
        }

        if(other.name == "water")
        {
            Debug.Log("caiu na agua");
            controller.enabled = false;
            transform.position = last_pos;
            controller.enabled = true;
        }
    }
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        string state = hit.gameObject.name[..2];
        int current_state = state_check(state);
        Animator map = GameObject.Find("map").GetComponent<Animator>();
        if(hit.gameObject.tag == "death")
        {
            death = true;
            GameObject.Find("timer").GetComponent<timer>().show_death();
        }
        map.SetInteger("estados", current_state);

        GameObject.Find("sigla").GetComponent<TextMeshProUGUI>().text = state;
        
        
        if(isGrounded && verticalVelocity < 0)
        {
            last_pos = new Vector3(hit.transform.position.x, hit.transform.position.y + 5, hit.transform.position.z);
        } 
    }

    private int state_check(string name)
    {
        switch(name)
        {
            case "RS":
                return 1;

            case "SC":
                return 2;

            case "PR":
                return 3;

            case "SP":
                return 4;

            case "RJ":
                return 5;

            case "ES":
                return 6;

            case "MG":
                return 7;

            case "BA":
                return 8;

            case "SE":
                return 9;

            case "AL":
                return 10;

            case "PE":
                return 11;

            case "PB":
                return 12;

            case "RN":
                return 13;

            case "CE":
                return 14;

            case "PI":
                return 15;

            case "MA":
                return 16;

            case "TO":
                return 17;

            case "PA":
                return 18;

            case "AP":
                return 19;

            case "AM":
                return 20;

            case "RR":
                return 21;

            case "AC":
                return 22;

            case "RO":
                return 23;

            case "GO":
                return 24;

            case "MT":
                return 25;

            case "MS":
                return 26;
                
            default:
                return 0;
        }

    }
        
}

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
    public GameObject lobbyMap;
    private bool block_mg1 = false;
    private bool block_mg2 = false;
    private bool block_mg3 = false;
    private bool block_mg4 = false;
    private bool block_mg5 = false;
    public string current_scene;

    private Vector3 minigameOffset = new Vector3(5000f, 0f, 0f);

    private void Start()
    {
        lobbyMap = GameObject.Find("hide");
        // Garante que só o Player local pega input
        if (IsOwner)
        {
            DontDestroyOnLoad(gameObject);
            variableJoystick = FindObjectOfType<VariableJoystick>();
        }

    }

    private void Update()
    {
        variableJoystick = FindObjectOfType<VariableJoystick>();
        //if(!IsOwner) return;
        if(SceneManager.GetActiveScene().name == "mg_mat" || SceneManager.GetActiveScene().name == "mg_port")
        {
            Screen.orientation = ScreenOrientation.Portrait;
        }
        else
        {
            Screen.orientation = ScreenOrientation.LandscapeRight;
        }
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
                speed = 20f;
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
                        rotationSpeed/3 * Time.deltaTime
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
        StartCoroutine(LoadSceneAdditive(privateSceneName));
    }

    public void ReturnToLobby()
    {
        if (!IsOwner) return;

        ShowPlayerForOthersServerRpc();
        
        // Atenção: passe o nome correto da cena privada que o jogador estava
        // Você pode querer guardar esse nome em uma variável tipo 'currentPrivateScene'
        StartCoroutine(UnloadPrivateScene(current_scene));
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Quando spawnar, garante que está visível para todos
        if (IsOwner)
        {
            // Se for o dono, garante que os outros possam ver
            ShowPlayerForOthersServerRpc();
        }
    }
        //transform.position = new Vector3(0, 0.55f, 0);

    [ServerRpc(RequireOwnership = false)]
    private void HidePlayerForOthersServerRpc(ServerRpcParams rpcParams = default)
    {
        HidePlayerForOthersClientRpc();
    }

    [ClientRpc]
    private void HidePlayerForOthersClientRpc()
    {
        // Esconde APENAS para outros players, NÃO para o dono
        if (!IsOwner)
        {
            SetPlayerVisibility(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShowPlayerForOthersServerRpc(ServerRpcParams rpcParams = default)
    {
        ShowPlayerForOthersClientRpc();
    }

    [ClientRpc]
    private void ShowPlayerForOthersClientRpc()
    {
        // Mostra para TODOS (incluindo outros players)
        if (!IsOwner)
        {
            SetPlayerVisibility(true);
        }
    }

    private void SetPlayerVisibility(bool visible)
    {
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = visible;

        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = visible;
    }

    private IEnumerator LoadSceneAdditive(string sceneName)
    {
        float playerOffset = 5000f + (OwnerClientId * 1000f);
        Vector3 uniquePos = new Vector3(961.4f, 534.5f, 0f);
        lobbyMap.SetActive(false);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!asyncLoad.isDone) yield return null;

        Scene newScene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(newScene);

        // Move o cenário do minigame para o slot único desse jogador
        foreach (GameObject rootObj in newScene.GetRootGameObjects())
        {
            rootObj.transform.position = uniquePos;
        }

        // Teleporta o jogador para o seu slot único
        controller.enabled = false;
        transform.position = uniquePos + new Vector3(0, 2.3f, 0); 
        controller.enabled = true;
    }

    private IEnumerator UnloadPrivateScene(string sceneName)
    {
        lobbyMap.SetActive(true);
        GameObject rootObj = GameObject.Find("move away");
        rootObj.SetActive(false);
        // 1. Descarrega APENAS a cena do minigame/privada
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);
        if(lobbyMap != null) lobbyMap.SetActive(true);
        while (!asyncUnload.isDone)
            yield return null;

        // 2. Define o lobby novamente como a cena ativa
        Scene lobbyScene = SceneManager.GetSceneByName("lobby_start");
        if(lobbyScene.IsValid())
        {
            SceneManager.SetActiveScene(lobbyScene);
        }

        // 3. Volta o player para a posição inicial no lobby
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // Aplica a posição desejada
        transform.position = new Vector3(0, 2f, 0); 

        // Aguarda o fim do frame para a física do Unity processar a nova posição
        yield return new WaitForEndOfFrame();

        if (cc != null) cc.enabled = true; 
        // Pode usar a 'last_pos' que você já guarda no script!
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!IsOwner) return;
        Debug.Log(other.name);
        if (other.CompareTag("mg1") && !block_mg1)
        {
            current_scene = "mg_hist";
            GoToPrivateScene("mg_hist");
            block_mg1 = true;
        }else if (other.CompareTag("mg2") && !block_mg2)
        {
            current_scene = "mg_bio";
            GoToPrivateScene("mg_bio");
            block_mg2 = true;
        }else if (other.CompareTag("mg3") && !block_mg3)
        {
            current_scene = "mg_geo";
            GoToPrivateScene("mg_geo");
            block_mg3 = true;
        }else if (other.CompareTag("mg4") && !block_mg4)
        {
            current_scene = "mg_mat";
            GoToPrivateScene("mg_mat");
            block_mg4 = true;
        }else if (other.CompareTag("mg5") && !block_mg5)
        {
            current_scene = "mg_port";
            GoToPrivateScene("mg_port");
            block_mg5 = true;
        }
        else if (other.CompareTag("back"))
        {
            Debug.Log("colidiu com a volta");
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
        if(!IsOwner) return;
        if (hit.gameObject.CompareTag("back"))
        {
            Debug.Log("colidiu com a volta");
            ReturnToLobby();
        }
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
            last_pos = new Vector3(hit.transform.position.x, hit.transform.position.y + 20, hit.transform.position.z);
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

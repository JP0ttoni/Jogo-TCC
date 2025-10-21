using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JoystickPlayerExample : NetworkBehaviour
{
    
    public float speed = 5f;
    public VariableJoystick variableJoystick;
    public CharacterController controller;
    public float rotationSpeed = 10f;

    private void Start()
    {
        // Garante que só o Player local pega input
        if (IsOwner) // se for Mirror: if (isLocalPlayer)
        {
            DontDestroyOnLoad(gameObject);
            // Procura o joystick da cena
            variableJoystick = FindObjectOfType<VariableJoystick>();
        }
    }
    private void Update()
    {
        if (!IsOwner) return;
        // Pega a direção do joystick
        variableJoystick = FindObjectOfType<VariableJoystick>();
        Vector3 direction = new Vector3(variableJoystick.Horizontal, 0f, variableJoystick.Vertical);

        // Se houver entrada no joystick
        if (direction.magnitude >= 0.1f)
        {
            // Normaliza para não aumentar a velocidade na diagonal
            Vector3 move = direction.normalized * speed * Time.deltaTime;

            // Move usando CharacterController
            controller.Move(move);

            // Faz o personagem rotacionar suavemente para a direção do movimento
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }


public void GoToPrivateScene(string privateSceneName)
    {
        if (!IsOwner) return;

        // 1. Oculta o player para os outros
        HidePlayerForOthersServerRpc();

        // 2. Carrega a cena privada localmente
        StartCoroutine(LoadPrivateScene(privateSceneName));
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
            // Desativa os renderers do player
            foreach (var r in GetComponentsInChildren<Renderer>())
                r.enabled = false;

            // Opcional: desativa colliders
            foreach (var c in GetComponentsInChildren<Collider>())
                c.enabled = false;
        }
    }

    private IEnumerator LoadPrivateScene(string privateSceneName)
{
        // 1. Obtenha uma referência à cena que está chamando (a "cena antiga")
        Scene oldScene = gameObject.scene;
        Debug.Log(oldScene);

    // 2. Carrega a nova cena aditivamente
    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(privateSceneName, LoadSceneMode.Additive);
    while (!asyncLoad.isDone)
        yield return null;

    // 3. O resto do seu código para configurar a nova cena
    Scene privateScene = SceneManager.GetSceneByName(privateSceneName);
    SceneManager.MoveGameObjectToScene(gameObject, privateScene);
    SceneManager.SetActiveScene(privateScene);
    AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(1);

    transform.position = new Vector3(0, 1, 0);

    // 4. Descarrega a cena antiga
    // Use UnloadSceneAsync e espere que termine.
        while (!asyncUnload.isDone)
        {
            Debug.Log("ainda não terminou");
            yield return null;
        }
}

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "mg1")
        {
            GoToPrivateScene("mg1");
        }
    }
}
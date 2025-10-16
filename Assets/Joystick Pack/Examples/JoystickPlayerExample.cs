using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

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
            // Procura o joystick da cena
            variableJoystick = FindObjectOfType<VariableJoystick>();
        }
    }
    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            testClientRpc(new ClientRpcParams{Send = new ClientRpcSendParams{TargetClientIds = new List<ulong> {1}}});
        }
        // Pega a direção do joystick
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

    [ServerRpc]
    private void testServerRpc(ServerRpcParams serverRpcParams)
    {
        Debug.Log("id: " + OwnerClientId + "message: " + serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void testClientRpc(ClientRpcParams clientRpcParams)
    {
        Debug.Log("ola");
    }
}
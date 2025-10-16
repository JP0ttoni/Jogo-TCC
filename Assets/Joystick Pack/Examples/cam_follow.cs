using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class CameraFollowSetup : NetworkBehaviour
{
    private void Start()
    {
        if (IsOwner) // só a câmera do player local
        {
            CinemachineVirtualCamera vcam = FindObjectOfType<CinemachineVirtualCamera>();
            vcam.Follow = transform;   // opcional, se quiser olhar sempre pro player
        }
    }
}

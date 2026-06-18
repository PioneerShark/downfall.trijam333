using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] Portal exitNode;
    [SerializeField] AudioSource portalSound;
    [SerializeField] bool timedTeleport = true;
    [SerializeField] float transitionTime = 0.6f;
    [SerializeField] float setRotation = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && exitNode != null)
        {
            portalSound.Play();

            Vector3 panPos = ChunkGen.Instance.player.transform.position-exitNode.transform.position;
            ChunkGen.Instance.TeleportPlayer(exitNode.transform.position);
            if (timedTeleport)
            {
                
                ChunkGen.Instance.SetCameraPan(panPos, transitionTime, false, timedTeleport);
                
            }
            if (!ChunkGen.Instance.SetCameraRotation(setRotation, transitionTime))
            {
                ChunkGen.Instance.SetCameraPan(panPos, transitionTime, false, timedTeleport);
            }
        }
    }
}

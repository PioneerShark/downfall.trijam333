using UnityEngine;

public class Chunk : MonoBehaviour
{
    ChunkGen spawner;
    [SerializeField] public Transform spawnLoc;
    bool isActive = true;
    [SerializeField] bool transitionChunk = false;
    [SerializeField] Color transitionColour;

    private void Update()
    {
        if (Vector2.Distance(transform.position, Vector2.zero) > 200f)
        {
            Destroy(this.gameObject);
        }
    }
    public void SetSpawner(ChunkGen _spawner)
    {
        spawner = _spawner;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && isActive)
        {
            if (transitionChunk)
            {
                Camera.main.backgroundColor = transitionColour;
            }
            spawner.LoadNextChunk();
            isActive = false;
        }
        
    }
}

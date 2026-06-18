using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;

public class ChunkGen : MonoBehaviour
{
    public static ChunkGen Instance;
    [SerializeField] List<Chunk> chunksToSpawn = new List<Chunk>();
    [SerializeField] List<Chunk> chunksToSpawn2nd = new List<Chunk>();
    [SerializeField] List<Chunk> chunksToSpawn3rd = new List<Chunk>();
    private bool twoStart = true, threeStart = true;
    Chunk lastChunkSpawned;
    List<Chunk> chunksInPlay = new List<Chunk>();
    float distanceTravelled;
    [SerializeField] float regionLength = 500f;
    float speed = 8f;
    int lastChunkNum = 0;
    [SerializeField] AudioSource endJingle;

    [SerializeField] public Player player;

    [SerializeField] ProgressBar heatMeter, depthMeter;

    GameObject[] anchor = new GameObject[2];
    int currentAnchor = 0;
    public float pauseDuration = 0f;

    [SerializeField] TextMeshProUGUI score;
    [SerializeField] TextMeshProUGUI scoreTwo;

    Camera cameraMain;
    Vector3 cameraRest;
    Vector3 cameraTempRest;
    Vector3 panStartPos;
    float lastPanSet;
    float panTime;
    float lastRotSet;
    float rotationTime;
    float rotationEnd = 0f;
    float rotationStart = 0f;
    bool doublePan = false;
    bool panActive = false;
    bool rotActive = false;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        cameraMain = Camera.main;
        cameraRest = cameraMain.transform.position;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lastPanSet = 0;
        lastRotSet = 0;
        anchor[0] = new GameObject("anchor");
        anchor[1] = new GameObject("anchor alt");
        anchor[1].transform.position += new Vector3(0, -30, 0);

        lastChunkSpawned = Instantiate(chunksToSpawn[0], anchor[0].transform);
        lastChunkSpawned.SetSpawner(this);
        chunksInPlay.Add(lastChunkSpawned);
        for (int i = 0; i < 0; i++)
        {
            lastChunkSpawned = Instantiate(chunksToSpawn[Random.Range(1, chunksToSpawn.Count)], lastChunkSpawned.spawnLoc.position, Quaternion.identity, anchor[0].transform);
            lastChunkSpawned.SetSpawner(this);
            chunksInPlay.Add(lastChunkSpawned);
        }
        

    }

    // Update is called once per frame
    void Update()
    {
        float distanceMoved = speed + (player.heat * 0.015f);
        if (pauseDuration > 0)
        {
            pauseDuration -= Time.deltaTime;
            distanceMoved = 0f;
        }
        
        

        distanceMoved *= Time.deltaTime;
        player.CheckVertical(distanceMoved);
        anchor[0].transform.position += new Vector3(0, distanceMoved,0);
        anchor[1].transform.position += new Vector3(0, distanceMoved, 0);
        distanceTravelled += distanceMoved;
        heatMeter.UpdateValue((float)player.heat / 400f);
        depthMeter.UpdateValue((float)distanceTravelled / (regionLength*4));
        score.text = "Score: " + (int)distanceTravelled;
        scoreTwo.text = "Score: " + (int)distanceTravelled;


        if (Vector2.Distance(anchor[currentAnchor].transform.position, Vector2.zero) > 120f)
        {
            GameObject anchorTemp = anchor[currentAnchor];
            currentAnchor = currentAnchor == 0 ? 1 : 0;
            foreach(Chunk chunk in chunksInPlay)
            {
                if (chunk == null) continue;
                chunk.transform.SetParent(anchor[currentAnchor].transform);
            }
            anchorTemp.transform.position = Vector3.zero;
        }
        if(panActive)
        PanCamera();
        if (rotActive) RotateCamera();
    }

    public void SetCameraPan(Vector3 start, float timeToPan, bool mirror, bool pause)
    {
        
        doublePan = mirror;
        panStartPos = start;
        if (mirror)
        {
            panStartPos += new Vector3(0, -7, 0);
            cameraTempRest = cameraRest;
        }
        if (doublePan && mirror)
        {
            cameraTempRest = cameraMain.transform.position;
        }
        panStartPos.z = cameraMain.transform.position.z;
        panStartPos.x = 0;
        panTime = timeToPan;
        lastPanSet = Time.time;
        panActive = true;
        if (pause)
        Pause(timeToPan+0.1f);
    }
    public bool SetCameraRotation(float rotation, float timeToRotate)
    {
        Debug.Log(rotation + " | "+ cameraMain.transform.localEulerAngles.z);
        if (rotation == cameraMain.transform.localEulerAngles.z) return false;
        lastRotSet = Time.time;
        rotationTime = timeToRotate;
        rotationStart = cameraMain.transform.localEulerAngles.z;
        rotationEnd = rotation;
        rotActive = true;
        if (rotation == 180)
            player.flipControls = true;
        else 
            player.flipControls = false;
        Pause(timeToRotate + 0.1f);
        return true;

    }

    private void RotateCamera()
    {
        float lerpRot = (Time.time - lastRotSet) / rotationTime;
        float resultantAngle = Mathf.Lerp(rotationStart, rotationEnd, lerpRot);
        cameraMain.transform.localEulerAngles = new Vector3(0, 0, resultantAngle);

        if (lerpRot >= 1)
        {

            rotActive = false;
        }
    }
    private void PanCamera()
    {
        float lerpPos = (Time.time - lastPanSet) / panTime;
        if (doublePan)
        {
            lerpPos = (Time.time - lastPanSet) / (panTime);
            cameraMain.transform.position = Vector3.Lerp(cameraTempRest, panStartPos, lerpPos);
        }
        else
        {
            cameraMain.transform.position = Vector3.Lerp(panStartPos, cameraRest, lerpPos);
        }
        
        if (lerpPos >= 1)
        {
            if (doublePan)
            {
            doublePan = false;
            SetCameraPan(panStartPos, panTime*0.5f, false, false);
            }
            else
            {
                panActive = false;
            }
            
        }
    }

    public void Pause(float duration)
    {
        pauseDuration = duration;
    }
    public void LoadNextChunk()
    {
        float regionPercent = distanceTravelled/(regionLength * 4);
        int mult = (int)Time.time % 2 == 1 ? -1 : 1;
        if (regionPercent < 0.25f)
        {

                int numChunk = Random.Range(1, chunksToSpawn.Count);
            if (numChunk == lastChunkNum)
            {
                lastChunkNum += Random.Range(1-lastChunkNum, chunksToSpawn.Count-1-lastChunkNum);
            }
            else
            {
                lastChunkNum = numChunk;
            }
            lastChunkSpawned = Instantiate(chunksToSpawn[lastChunkNum], lastChunkSpawned.spawnLoc.position, Quaternion.identity, anchor[currentAnchor].transform);
            //lastChunkSpawned = Instantiate(chunksToSpawn[Random.Range(1, chunksToSpawn.Count)], lastChunkSpawned.spawnLoc.position, Quaternion.identity, anchor[currentAnchor].transform);
            lastChunkSpawned.transform.localScale = new Vector3(mult, 1f, 1f);
            lastChunkSpawned.SetSpawner(this);
            chunksInPlay.Add(lastChunkSpawned);
        }
        else if (regionPercent >= 0.5f)
        {
            if (threeStart)
            {
                lastChunkNum = 0;
                threeStart = false;
                lastChunkSpawned = Instantiate(chunksToSpawn3rd[0], lastChunkSpawned.spawnLoc.position, Quaternion.identity, anchor[currentAnchor].transform);
                lastChunkSpawned.SetSpawner(this);
                chunksInPlay.Add(lastChunkSpawned);
            }
            else
            {
                int numChunk = Random.Range(1, chunksToSpawn3rd.Count);
                if (numChunk == lastChunkNum)
                {
                    lastChunkNum += Random.Range(1 - lastChunkNum, chunksToSpawn3rd.Count - 1 - lastChunkNum);
                }
                else
                {
                    lastChunkNum = numChunk;
                }
                
                lastChunkSpawned = Instantiate(chunksToSpawn3rd[lastChunkNum], lastChunkSpawned.spawnLoc.position, Quaternion.identity, anchor[currentAnchor].transform);
                lastChunkSpawned.transform.localScale = new Vector3(mult, 1f, 1f);
                lastChunkSpawned.SetSpawner(this);
                chunksInPlay.Add(lastChunkSpawned);
            }
            
        }

        else if (regionPercent >= 0.25f)
        {
            if (twoStart)
            {
                lastChunkNum = 0;
                twoStart = false;
                lastChunkSpawned = Instantiate(chunksToSpawn2nd[0], lastChunkSpawned.spawnLoc.position, Quaternion.identity, anchor[currentAnchor].transform);
                lastChunkSpawned.SetSpawner(this);
                chunksInPlay.Add(lastChunkSpawned);
            }
            else
            {
                int numChunk = Random.Range(1, chunksToSpawn2nd.Count);
                if (numChunk == lastChunkNum)
                {
                    lastChunkNum += Random.Range(1 - lastChunkNum, chunksToSpawn2nd.Count - 1 - lastChunkNum);
                }
                else
                {
                    lastChunkNum = numChunk;
                }

                lastChunkSpawned = Instantiate(chunksToSpawn2nd[lastChunkNum], lastChunkSpawned.spawnLoc.position, Quaternion.identity, anchor[currentAnchor].transform);
                lastChunkSpawned.transform.localScale = new Vector3(mult, 1f, 1f);
                lastChunkSpawned.SetSpawner(this);
                chunksInPlay.Add(lastChunkSpawned);
            }

        }


    }

    public void TeleportPlayer(Vector3 location)
    {
        
        foreach (GameObject _anchor in anchor)
        {
            Vector3 displacement = new Vector3(0,location.y,0) - new Vector3(0, player.transform.position.y, 0);
            _anchor.transform.position -= displacement;
        }
        player.transform.position = new Vector3(location.x, player.transform.position.y, player.transform.position.z);
    }

    public void Stop()
    {
        endJingle.Play();
        speed = 0f;
    }
}

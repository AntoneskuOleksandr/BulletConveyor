using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private Vector3 dragOffset;
    [SerializeField] private float blockYOffSet = 0.925f;
    [SerializeField] private int coinsForLevel = 1000;
    [SerializeField] private BulletGenerator bulletGenerator;
    [SerializeField] private GameObject[] blocks;
    [SerializeField] private UIManager UIManager;
    [SerializeField] private MergePlatformsManager platformsManager;
    [SerializeField] private SpawnGuns spawnGuns;
    [SerializeField] private HandCursor handCursor;

    [SerializeField] private Vector2Int gridSize;

    public List<Gun> guns;
    public bool isTutorial = false;

    [Header("Tutorial")]
    public GunSettings defaultGunSettings;
    public UnityEvent onBlockPlace;
    public UnityEvent onEnoughMoneyToBuyBlock;
    public UnityEvent onBlockBuy;
    public UnityEvent onMerge;

    public GameObject confetti;

    private Camera currentCamera;
    private Block currentlyDraggingBlock;
    private GameObject currentlyDraggingGO;
    private BlockParent blockParent;
    private Tile[,] tiles;

    private TileGenerator tileGenerator;
    private TileManager tileManager;
    private List<Tile> availableTileList = new List<Tile>();

    private bool wasOnBoardBeforeDragging;
    private bool canMoveBlocks = true;
    private bool canMergeBlocks = false;

    private int additionBullets;
    private float coins;
    private float reward;
    private int highestBlockRank;
    private int currentLevel;

    private void Awake()
    {
        Instance = this;
        currentCamera = Camera.main;

        tileGenerator = GetComponent<TileGenerator>();
        tileManager = GetComponent<TileManager>();
        tileGenerator.GenerateGrid(gridSize.x, gridSize.y);
        tiles = tileGenerator.GetTiles();

        currentLevel = SceneManager.GetActiveScene().buildIndex - 2;
    }

    private void Start()
    {
        AudioManager.Instance.PlayMusic(AudioManager.Instance.InGameMusic);
        CreateBlock();

        AbilityManager.Instance.OnBulletToGenerateAbilitySelected.AddListener(IncreaseAdditionBulletsCount);

        if (isTutorial)
            onBlockBuy.AddListener(ChangeAbilityToPutBlock);
    }

    private void OnDisable()
    {
        AbilityManager.Instance.OnBulletToGenerateAbilitySelected.RemoveListener(IncreaseAdditionBulletsCount);
    }

    private void Update()
    {
        if (!canMoveBlocks)
            return;

        RaycastHit hit;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            if (currentlyDraggingGO == null)
            {
                if (Physics.Raycast(ray, out hit, 100))
                {
                    if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Block") && !EventSystem.current.IsPointerOverGameObject())
                    {
                        currentlyDraggingGO = hit.transform.parent.gameObject;
                        blockParent = currentlyDraggingGO.GetComponent<BlockParent>();

                        if (isTutorial)
                            if (canMergeBlocks && !blockParent.isOnBoard)
                            {
                                currentlyDraggingGO = null;
                                blockParent = null;
                                return;
                            }

                        currentlyDraggingBlock = currentlyDraggingGO.GetComponentInChildren<Block>();

                        wasOnBoardBeforeDragging = blockParent.isOnBoard;

                        if (currentlyDraggingBlock.positionIndex != -Vector2Int.one)
                        {
                            foreach (var tile in blockParent.myTiles)
                                tile.isAvailable = true;
                        }

                        bulletGenerator.blockParents.Remove(blockParent);
                        blockParent.transform.parent = null;
                        blockParent.isOnBoard = false;
                        return;
                    }
                }
            }
        }

        if (currentlyDraggingGO != null)
        {
            DraggingBlockPosition(ray);
            tileManager.HighLightTiles(tiles.Cast<Tile>().ToList(), false);

            RaycastHit blockHit;
            Ray blockRay = new Ray(currentlyDraggingBlock.transform.position, Vector3.down);

            Debug.DrawRay(blockRay.origin, blockRay.direction * 5, Color.red);

            if (Physics.Raycast(blockRay, out blockHit, 5))
            {
                GameObject hitGO = blockHit.transform.gameObject;

                if (hitGO.layer == LayerMask.NameToLayer("Block"))
                {
                    if (Input.GetMouseButtonUp(0))
                        if (hitGO.transform.parent.GetComponent<BlockParent>() != null)
                            if (blockParent.rank == hitGO.transform.parent.GetComponent<BlockParent>().rank
                                && !hitGO.transform.parent.GetComponent<BlockParent>().isOnBoard)
                                Merge(currentlyDraggingGO, hitGO.transform.parent.gameObject);
                            else
                            {
                                blockParent.SetPosition(blockParent.previousPosition);
                                blockParent.isOnBoard = wasOnBoardBeforeDragging;
                                blockParent.transform.SetParent(bulletGenerator.blocksPool.transform);

                                if (blockParent.isOnBoard)
                                    bulletGenerator.blockParents.Add(blockParent);

                                currentlyDraggingBlock = null;
                                currentlyDraggingGO = null;
                                blockParent = null;
                            }

                    availableTileList.Clear();
                    return;
                }
                else if (hitGO.layer == LayerMask.NameToLayer("Platform"))
                {
                    if (canMergeBlocks)
                        return;

                    if (Input.GetMouseButtonUp(0))
                        if (hitGO.transform.GetComponent<Platform>().isFree)
                            PlaceOnPlatform(hitGO.GetComponent<Platform>());
                        else
                        {
                            blockParent.SetPosition(blockParent.previousPosition);
                            blockParent.transform.SetParent(bulletGenerator.blocksPool.transform);

                            currentlyDraggingBlock = null;
                            currentlyDraggingGO = null;
                            blockParent = null;
                        }

                    availableTileList.Clear();
                    return;
                }
                else if (hitGO.layer == LayerMask.NameToLayer("Tile"))
                {
                    if (canMergeBlocks)
                        return;

                    if (!CanPlaceBlock(hitGO))
                    {
                        if (Input.GetMouseButtonUp(0))
                        {
                            blockParent.SetPosition(blockParent.previousPosition);
                            blockParent.transform.SetParent(bulletGenerator.blocksPool.transform);
                            blockParent.isOnBoard = wasOnBoardBeforeDragging;

                            if (blockParent.isOnBoard)
                                bulletGenerator.blockParents.Add(blockParent);

                            currentlyDraggingBlock = null;
                            currentlyDraggingGO = null;
                            blockParent = null;
                        }

                        availableTileList.Clear();
                        return;
                    }
                }
                else
                {
                    if (canMergeBlocks)
                        return;

                    if (Input.GetMouseButtonUp(0))
                    {
                        blockParent.SetPosition(blockParent.previousPosition);
                        blockParent.transform.SetParent(bulletGenerator.blocksPool.transform);
                        blockParent.isOnBoard = wasOnBoardBeforeDragging;

                        if (blockParent.isOnBoard)
                            bulletGenerator.blockParents.Add(blockParent);

                        currentlyDraggingBlock = null;
                        currentlyDraggingGO = null;
                        blockParent = null;
                    }

                    availableTileList.Clear();
                    return;
                }
            }


            tileManager.HighLightTiles(availableTileList);

            if (Input.GetMouseButtonUp(0))
            {
                PlaceOnField();
                tileManager.HighLightTiles(tiles.Cast<Tile>().ToList(), false);
            }

            availableTileList.Clear();
        }
    }

    private void PlaceOnPlatform(Platform platform)
    {
        currentlyDraggingBlock.positionIndex = -Vector2Int.one;

        if (blockParent.platform != -1)
            platformsManager.platforms[blockParent.platform].isFree = true;

        blockParent.SetPosition(platform.transform.position + new Vector3(0f, blockYOffSet, 0f));
        blockParent.previousPosition = platform.transform.position + new Vector3(0f, blockYOffSet, 0f);

        blockParent.platform = platform.index;
        platform.isFree = false;
        blockParent.isOnBoard = false;

        if (isTutorial)
        {
            handCursor.pathPoints[2].position = blockParent.transform.position + new Vector3(0f, -0.75f, 0f);
        }

        currentlyDraggingBlock = null;
        currentlyDraggingGO = null;
        blockParent = null;
    }

    private void PlaceOnField()
    {
        if (currentlyDraggingBlock != null)
        {
            Vector3 firstTilePosition = Vector3.zero;
            Vector2Int firstTileIndex = Vector2Int.zero;

            RaycastHit blockHit;
            Ray blockRay = new Ray(currentlyDraggingBlock.transform.position, Vector3.down);

            if (Physics.Raycast(blockRay, out blockHit, 100))
            {
                Tile firstTile = blockHit.transform.gameObject.GetComponent<Tile>();

                if (firstTile != null)
                {
                    firstTileIndex = LookupTileIndex(firstTile);
                    firstTilePosition += firstTile.transform.position;

                    currentlyDraggingBlock.positionIndex = LookupTileIndex(firstTile);

                    foreach (var tile in availableTileList)
                        tile.isAvailable = false;
                }
            }

            Vector3 lastTilePosition = tiles[firstTileIndex.x + blockParent.scaleX - 1, firstTileIndex.y + blockParent.scaleZ - 1].transform.position;
            Vector3 finalPosition = new Vector3((firstTilePosition.x + lastTilePosition.x) / 2, blockYOffSet, (firstTilePosition.z + lastTilePosition.z) / 2);

            blockParent.SetPosition(finalPosition);
            blockParent.previousPosition = finalPosition;

            bulletGenerator.blockParents.Add(blockParent);
            blockParent.transform.SetParent(bulletGenerator.blocksPool.transform);

            if (blockParent.platform != -1)
                platformsManager.platforms[blockParent.platform].isFree = true;

            UIManager.UpdateUI();

            if (isTutorial)
            {
                onBlockPlace.Invoke();
                canMoveBlocks = false;
                handCursor.pathPoints[2].position = blockParent.transform.position + new Vector3(0f, -0.75f, 0f);
            }

            blockParent.platform = -1;
            blockParent.isOnBoard = true;

            currentlyDraggingBlock = null;
            currentlyDraggingGO = null;
            blockParent = null;
        }
    }

    private bool CanPlaceBlock(GameObject gameObject)
    {
        Tile tile = gameObject.GetComponent<Tile>();

        for (int i = 0; i < blockParent.shape.Count; i++)
        {
            for (int j = 0; j < blockParent.shape[0].row.Length; j++)
            {
                if (tile.indexInMatrix.x + j >= tiles.GetLength(0) || tile.indexInMatrix.y + i >= tiles.GetLength(1))
                {
                    availableTileList.Clear();
                    return false;
                }

                if (blockParent.shape[i].row[j] == 1)
                {
                    if (!tiles[tile.indexInMatrix.x + j, tile.indexInMatrix.y + i].isAvailable)
                    {
                        availableTileList.Clear();
                        return false;
                    }
                    else
                    {
                        availableTileList.Add(tiles[tile.indexInMatrix.x + j, tile.indexInMatrix.y + i]);
                    }
                }
            }
        }

        blockParent.myTiles = availableTileList.ToArray();

        return true;
    }


    private void Merge(GameObject block1, GameObject block2)
    {

        BlockParent newBlock = Instantiate(block2.GetComponent<BlockParent>().nextRangBlock, block2.transform.position, Quaternion.identity).GetComponent<BlockParent>();
        newBlock.platform = block2.GetComponent<BlockParent>().platform;
        newBlock.bulletsToGenerate += additionBullets;
        newBlock.rank = block2.GetComponent<BlockParent>().rank + 1;

        if (blockParent.platform != -1)
            platformsManager.platforms[blockParent.platform].isFree = true;

        if (newBlock.rank > highestBlockRank)
            highestBlockRank = newBlock.rank;

        currentlyDraggingBlock = null;
        currentlyDraggingGO = null;
        blockParent = null;

        UIManager.UpdateUI();

        Destroy(block1);
        Destroy(block2);

        if (isTutorial)
            onMerge.Invoke();
    }

    private void IncreaseAdditionBulletsCount()
    {
        additionBullets++;
    }

    private void DraggingBlockPosition(Ray ray)
    {
        Plane horizontalPlane = new Plane(Vector3.up, Vector3.up);
        float distance;
        if (horizontalPlane.Raycast(ray, out distance))
            currentlyDraggingGO.GetComponent<BlockParent>().SetPosition(ray.GetPoint(distance) + dragOffset);
    }

    private Vector2Int LookupTileIndex(Tile hitInfo)
    {
        for (int x = 0; x < tileManager.rows; x++)
            for (int y = 0; y < tileManager.columns; y++)
                if (tiles[x, y] == hitInfo)
                    return new Vector2Int(x, y);

        return -Vector2Int.one;
    }

    public bool CanCreateBlock()
    {
        if (platformsManager.IsFreePlatform())
            return true;

        return false;
    }

    public void CreateBlock(int index = 0)
    {
        int platformIndex = platformsManager.GetFreePlatformIndex();

        BlockParent parent = Instantiate(blocks[index], platformsManager.GetFreePlatformPosition() + new Vector3(0f, blockYOffSet, 0f), Quaternion.identity).GetComponent<BlockParent>();

        platformsManager.platforms[platformIndex].isFree = false;

        parent.platform = platformIndex;
        parent.bulletsToGenerate += additionBullets;
        parent.rank = index;

        if (parent.rank > highestBlockRank)
            highestBlockRank = parent.rank;

        UIManager.UpdateUI();

        return;
    }

    public void CreateBlockWithHighestRank()
    {
        if (CanCreateBlock())
            CreateBlock(highestBlockRank);
    }

    private List<Enemy> GetAllEnemies()
    {
        return Spawner.Instance.enemies;
    }

    public void GunDestoyed(Gun destroyedGun)
    {
        guns.Remove(destroyedGun);

        foreach (Enemy enemy in GetAllEnemies())
        {
            if (enemy.TargetGun == destroyedGun)
            {
                enemy.GetNewGunPosition();
            }
        }

        if (guns.Count <= 0)
            Lose();
    }

    public List<Gun> GetAllGuns()
    {
        List<Gun> tempGuns = new List<Gun>();

        for (int i = 0; i < guns.Count; i++)
            if (guns[i] != null)
                tempGuns.Add(guns[i]);

        return tempGuns;
    }

    public void Win()
    {
        Time.timeScale = 1f;
        confetti.SetActive(true);
        coins = PlayerPrefs.GetFloat("Coins");

        reward += coinsForLevel;
        coins += reward;

        PlayerPrefs.SetFloat("Coins", coins);
        UIManager.CoinsReward(reward);

        //GAManager.instance.OnLevelComplete(currentLevel);

        currentLevel++;

        PlayerPrefs.SetInt("CurrentLevel", currentLevel);

        StartCoroutine(UIManager.ShowWinerScreen());
    }

    public void Lose()
    {
        coins = PlayerPrefs.GetInt("Coins");

        float completedWavePercentage = (float)(Spawner.Instance.currentWaveCount - 1) / Spawner.Instance.totalWaves;

        reward += (int)(coinsForLevel * completedWavePercentage);
        coins += reward;

        PlayerPrefs.SetFloat("Coins", coins);

        UIManager.CoinsReward(reward);
        UIManager.ShowLoserScreen();
    }

    public void ContinueForAd()
    {
        ShowAdd();
        KillAllEnemies();
        SpawnAllGuns();
        Time.timeScale = 1f;
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ShowAdd()
    {
        Debug.Log("Ad");
    }

    public void KillAllEnemies()
    {
        Time.timeScale = 1f;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            enemy.GetComponent<Enemy>().Die();
        }
    }

    public void SpawnAllGuns()
    {
        spawnGuns.SpawnAllGuns();
    }

    public void ChangeTimeSpeed()
    {
        if (Time.timeScale == 1)
            Time.timeScale = 5f;
        else
            Time.timeScale = 1f;
    }

    public void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }

    public void MultiplyReward(int muliplier)
    {
        float newReward = reward * (muliplier - 1);

        ShowAdd();

        coins = PlayerPrefs.GetFloat("Coins");
        coins += newReward;

        PlayerPrefs.SetFloat("Coins", coins);

        UIManager.CoinsReward(reward * muliplier);
        UIManager.HideMultiplyRewardButton();
    }

    private void ChangeAbilityToPutBlock()
    {
        Debug.Log("ChangeAbilityToPutBlock");
        canMoveBlocks = !canMoveBlocks;
        canMergeBlocks = !canMergeBlocks;
    }
}


using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CoinBurst : MonoBehaviour
{
    [Header("Burst Settings")]
    public GameObject coinPrefab;
    public int burstCount = 100;
    public float burstRadius = 2f;
    public float minForce = 5f;
    public float maxForce = 10f;
    public float minTorque = 1f;
    public float maxTorque = 5f;

    [Header("Optimization")]
    public bool useObjectPooling = true;
    public int poolSize = 200;
    public float lifeTime = 3f;
    public float fadeDuration = 0.5f;

    private Queue<GameObject> coinPool = new Queue<GameObject>();

    void Start()
    {
        if (useObjectPooling)
        {
            InitializePool();
        }
    }

    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject coin = Instantiate(coinPrefab);
            coin.SetActive(false);
            coinPool.Enqueue(coin);
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            BurstCoins(200);
        }
    }
    public void BurstCoins(int numberOfCoins)
    {
        StartCoroutine(BurstCoinsCoroutine(numberOfCoins));
    }

    private IEnumerator BurstCoinsCoroutine(int numberOfCoins)
    {
        int coinsPerFrame = Mathf.Max(1, numberOfCoins / 10); // Spread over 10 frames
        int coinsCreated = 0;

        while (coinsCreated < numberOfCoins)
        {
            int coinsThisFrame = Mathf.Min(coinsPerFrame, numberOfCoins - coinsCreated);

            for (int i = 0; i < coinsThisFrame; i++)
            {
                CreateCoin();
                coinsCreated++;
            }

            yield return null; // Wait for next frame
        }
    }

    void CreateCoin()
    {
        GameObject coin;

        if (useObjectPooling && coinPool.Count > 0)
        {
            coin = coinPool.Dequeue();
            coin.SetActive(true);
            coin.transform.position = transform.position;
        }
        else
        {
            coin = Instantiate(coinPrefab, transform.position, Quaternion.identity);

            if (useObjectPooling)
            {
                coinPool.Enqueue(coin); // Add to pool for future use
            }
        }

        // Set random rotation
        coin.transform.rotation = Random.rotation;

        // Add random force
        Rigidbody rb = coin.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 randomDirection = Random.onUnitSphere;
            randomDirection.y = Mathf.Abs(randomDirection.y); // Ensure some upward force
            float force = Random.Range(minForce, maxForce);
            rb.AddForce(randomDirection * force, ForceMode.Impulse);

            // Add random torque
            Vector3 torque = new Vector3(
                Random.Range(minTorque, maxTorque),
                Random.Range(minTorque, maxTorque),
                Random.Range(minTorque, maxTorque)
            );
            rb.AddTorque(torque, ForceMode.Impulse);
        }

        // Random position within burst radius
        Vector3 randomPosition = transform.position + Random.insideUnitSphere * burstRadius;
        coin.transform.position = randomPosition;

        // Start lifetime coroutine if pooling is enabled
        if (useObjectPooling)
        {
            StartCoroutine(DeactivateAfterTime(coin, lifeTime, fadeDuration));
        }
    }

    private IEnumerator DeactivateAfterTime(GameObject coin, float delay, float fadeTime = 0f)
    {
        yield return new WaitForSeconds(delay);

        if (fadeTime > 0)
        {
            // Optional: Add fade out effect here
            float elapsed = 0f;
            Renderer renderer = coin.GetComponent<Renderer>();
            Material mat = renderer.material;
            Color originalColor = mat.color;

            while (elapsed < fadeTime)
            {
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
                mat.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        coin.SetActive(false);
        if (useObjectPooling)
        {
            coinPool.Enqueue(coin);

            // Reset alpha if fading was used
            Renderer renderer = coin.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = renderer.material;
                mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 1f);
            }
        }
        else
        {
            Destroy(coin);
        }
    }
}
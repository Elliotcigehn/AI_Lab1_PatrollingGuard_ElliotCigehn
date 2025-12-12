using UnityEngine;

public class AgentSpawner : MonoBehaviour
{
    public SteeringAgent agentPrefab;
    public int numberOfAgents = 10;
    public Vector2 spawnAreaSize = new Vector2(10f, 10f);

    void Start()
    {
        for (int i = 0; i < numberOfAgents; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2), 0f, Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2));
            Vector3 spawnPosition = transform.position + offset;
            Instantiate(agentPrefab, spawnPosition, Quaternion.identity);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnAreaSize.x, 0.1f, spawnAreaSize.y));
    }
}

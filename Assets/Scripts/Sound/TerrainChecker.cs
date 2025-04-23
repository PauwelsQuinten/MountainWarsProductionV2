using System;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChecker
{
    public Dictionary<string, float> GetLayerMixes(Vector3 playerPos, Terrain t)
    {
        Vector3 tPos = t.transform.position;
        TerrainData tData = t.terrainData;

        if (tData == null || tData.terrainLayers == null || tData.terrainLayers.Length == 0)
        {
            return new Dictionary<string, float>();
        }

        int mapX = Mathf.RoundToInt((playerPos.x - tPos.x) / tData.size.x * tData.alphamapWidth);
        int mapZ = Mathf.RoundToInt((playerPos.z - tPos.z) / tData.size.z * tData.alphamapHeight);

        float[,,] splatMapData = tData.GetAlphamaps(mapX, mapZ, 1, 1);
        Dictionary<string, float> layerMixes = new Dictionary<string, float>();

        for (int i = 0; i < splatMapData.GetLength(2); i++)
        {
            if (i >= tData.terrainLayers.Length || tData.terrainLayers[i] == null)
            {
                Debug.LogWarning($"Terrain layer at index {i} is null or out of bounds.");
                continue;
            }

            string layerName = tData.terrainLayers[i].name;
            float mixValue = splatMapData[0, 0, i];
            layerMixes[layerName] = mixValue;
        }

        return layerMixes;
    }
}

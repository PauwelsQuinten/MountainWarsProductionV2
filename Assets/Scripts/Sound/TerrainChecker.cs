using System;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChecker
{
    // private float[] GetTextureMix(Vector3 playerPos, Terrain t)
    // {
    //     Vector3 _tPos = t.transform.position;
    //     TerrainData _tData = t.terrainData;
    //     
    //     int _mapX = Mathf.RoundToInt((playerPos.x - _tPos.x) / _tData.size.x * _tData.alphamapWidth);
    //     int _mapZ = Mathf.RoundToInt((playerPos.z - _tPos.z) / _tData.size.z* _tData.alphamapHeight);
    //     float[,,] splatMapData = _tData.GetAlphamaps(_mapX, _mapZ, 1, 1);
    //     float[] _cellMix = new float[splatMapData.GetUpperBound(2) + 1];
    //     for (int i = 0; i < _cellMix.Length; i++)
    //     {
    //         _cellMix[i] = splatMapData[0, 0, i];
    //     }
    //     return _cellMix;
    // }
    //
    // public String GetLayerName(Vector3 playerPos, Terrain t)
    // {
    //     float[] _cellMix = GetTextureMix(playerPos, t);
    //     float _strongest = 0;
    //     int _maxIndex = 0;
    //     for (int i = 0; i < _cellMix.Length; i++)
    //     {
    //         if (_cellMix[i] > _strongest)
    //         {
    //             _maxIndex = i;
    //             _strongest = _cellMix[i];
    //         }
    //     }
    //     return t.terrainData.terrainLayers[_maxIndex].name;
    // }
    public Dictionary<string, float> GetLayerMixes(Vector3 playerPos, Terrain t)
    {
        Vector3 tPos = t.transform.position;
        TerrainData tData = t.terrainData;

        int mapX = Mathf.RoundToInt((playerPos.x - tPos.x) / tData.size.x * tData.alphamapWidth);
        int mapZ = Mathf.RoundToInt((playerPos.z - tPos.z) / tData.size.z * tData.alphamapHeight);

        float[,,] splatMapData = tData.GetAlphamaps(mapX, mapZ, 1, 1);
        Dictionary<string, float> layerMixes = new Dictionary<string, float>();

        for (int i = 0; i < splatMapData.GetLength(2); i++)
        {
            string layerName = tData.terrainLayers[i].name;
            float mixValue = splatMapData[0, 0, i];
            layerMixes[layerName] = mixValue;
        }

        return layerMixes;
    }
}

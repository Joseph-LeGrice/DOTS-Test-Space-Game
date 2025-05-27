using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

public struct AsteroidSettingsBlob
{
    // public WeakObjectReference<GameObject> AsteroidPrefab; // Not yet possible :(
    
    public Hash128 AssetPrefabHash;
    public BlobArray<AsteroidDataBlob> AsteroidTypes;
    public float AsteroidFieldMinimumSpacing;
}

public struct AsteroidDataBlob
{
    // public BlobArray<WeakObjectReference<Mesh>> PossibleMeshes; // Not yet possible :(

    public BlobArray<Hash128> PossibleMeshHashes;
    public float Size;
    public float MinRotateSpeed;
    public float MaxRotateSpeed;
}


[System.Serializable]
public struct AsteroidData
{
    public List<Mesh> PossibleMeshHashes;
    public float Size;
    public float MinRotateSpeed;
    public float MaxRotateSpeed;
}

[CreateAssetMenu(menuName = "_Game/AsteroidSettings")]
public class AsteroidSettings : BlobAssetScriptableObject<AsteroidSettingsBlob>
{
    public GameObject AsteroidPrefab;
    public List<AsteroidData> AsteroidTypes;
    public float AsteroidFieldMinimumSpacing;

    protected override void PopulateBlob(IBaker baker, BlobBuilder builder, ref AsteroidSettingsBlob blobData)
    {
        blobData.AsteroidFieldMinimumSpacing = AsteroidFieldMinimumSpacing;
        BlobBuilderArray<AsteroidDataBlob> asteroidTypesArray = builder.Allocate(ref blobData.AsteroidTypes, AsteroidTypes.Count);
        for (int i = 0; i < AsteroidTypes.Count; i++)
        {
            asteroidTypesArray[i] = new AsteroidDataBlob()
            {
                Size = AsteroidTypes[i].Size,
                MinRotateSpeed = AsteroidTypes[i].MinRotateSpeed,
                MaxRotateSpeed = AsteroidTypes[i].MaxRotateSpeed,
            };
            
            BlobBuilderArray<Hash128> meshHashes = builder.Allocate(ref asteroidTypesArray[i].PossibleMeshHashes, AsteroidTypes[i].PossibleMeshHashes.Count);
            for (int j=0; j<AsteroidTypes[i].PossibleMeshHashes.Count; j++)
            {
                Mesh mesh = AsteroidTypes[i].PossibleMeshHashes[j];
                meshHashes[j] = RegisterAssetReference(baker, mesh);
            }
        }

        blobData.AssetPrefabHash = RegisterAssetReference(baker, AsteroidPrefab);
    }
}
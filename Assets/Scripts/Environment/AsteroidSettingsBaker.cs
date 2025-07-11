using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct AsteroidTypeBufferData : IBufferElementData
{
    public Entity Prefab;
}

public class AsteroidSettingsBaker : MonoBehaviour
{
    [System.Serializable]
    public class MonoAsteroidData
    {
        public GameObject Prefab;
        public float Size;
        public float MinRotateSpeed;
        public float MaxRotateSpeed;
    }
    
    public List<MonoAsteroidData> AsteroidTypes;
    
    public class Baker : Baker<AsteroidSettingsBaker>
    {
        public override void Bake(AsteroidSettingsBaker authoring)
        {
            Entity asteroidSettingsEntity = GetEntity(TransformUsageFlags.None);
            
            DynamicBuffer<AsteroidTypeBufferData> asteroidTypes = AddBuffer<AsteroidTypeBufferData>(asteroidSettingsEntity);
            for (int i=0; i<authoring.AsteroidTypes.Count; i++)
            {
                AsteroidSettingsBaker.MonoAsteroidData asteroidTypeMono = authoring.AsteroidTypes[i];
                asteroidTypes.Add(new AsteroidTypeBufferData()
                {
                    Prefab = GetEntity(asteroidTypeMono.Prefab, TransformUsageFlags.Dynamic),
                });
            }
        }
    }
}

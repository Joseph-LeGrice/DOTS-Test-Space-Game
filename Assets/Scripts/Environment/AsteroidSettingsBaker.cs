using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

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
    public float AsteroidFieldMinimumSpacing;
    
    public class Baker : Baker<AsteroidSettingsBaker>
    {
        public override void Bake(AsteroidSettingsBaker authoring)
        {
            Entity asteroidSettingsEntity = GetEntity(TransformUsageFlags.None);
            AddComponent(asteroidSettingsEntity, new AsteroidFieldSettings());
            
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

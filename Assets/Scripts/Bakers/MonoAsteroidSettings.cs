using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class MonoAsteroidSettings : MonoBehaviour
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
}

class MonoAsteroidSettingsBaker : Baker<MonoAsteroidSettings>
{
    public override void Bake(MonoAsteroidSettings authoring)
    {
        Entity asteroidSettingsEntity = GetEntity(TransformUsageFlags.None);
        AddComponent(asteroidSettingsEntity, new AsteroidFieldSettings());
        
        DynamicBuffer<AsteroidTypeBufferData> asteroidTypes = AddBuffer<AsteroidTypeBufferData>(asteroidSettingsEntity);
        for (int i=0; i<authoring.AsteroidTypes.Count; i++)
        {
            MonoAsteroidSettings.MonoAsteroidData asteroidTypeMono = authoring.AsteroidTypes[i];
            asteroidTypes.Add(new AsteroidTypeBufferData()
            {
                Prefab = GetEntity(asteroidTypeMono.Prefab, TransformUsageFlags.Dynamic),
            });
        }
    }
}

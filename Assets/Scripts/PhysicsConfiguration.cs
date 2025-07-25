using Unity.Physics;
// ReSharper disable MemberCanBePrivate.Global

public static class PhysicsConfiguration
{
    public static readonly uint DefaultCollisionMask = 1 << 0;
    public static readonly uint DamageReceiverCollisionMask = 1 << 1;
    public static readonly uint DamageDealerCollisionMask = 1 << 2;

    public static CollisionFilter GetDefaultFilter()
    {
        return new CollisionFilter()
        {
            BelongsTo = DefaultCollisionMask,
            CollidesWith = DefaultCollisionMask | DamageReceiverCollisionMask | DamageDealerCollisionMask,
            GroupIndex = -1,
        };
    }
    
    public static CollisionFilter GetDamageReceiverFilter()
    {
        return new CollisionFilter()
        {
            BelongsTo = DamageReceiverCollisionMask,
            CollidesWith = DamageReceiverCollisionMask | DefaultCollisionMask | DamageDealerCollisionMask,
            GroupIndex = 0,
        };
    }
    
    public static CollisionFilter GetDamageDealerFilter()
    {
        return new CollisionFilter()
        {
            BelongsTo = DamageDealerCollisionMask,
            CollidesWith = DamageReceiverCollisionMask | DefaultCollisionMask,
            GroupIndex = 0,
        };
    }
    
    public static CollisionFilter GetGravityTetherFilter()
    {
        return new CollisionFilter()
        {
            BelongsTo = DamageDealerCollisionMask,
            CollidesWith = DamageReceiverCollisionMask | DefaultCollisionMask,
            GroupIndex = -1,
        };
    }
}

using DAFP.GAME.Assets;
using DAFP.TOOLS.ECS;
using DAFP.TOOLS.ECS.BuiltIn;
using NRandom;
using NRandom.Unity;
using Optional;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public static class EntityCommonExtensions
    {
        public static IEntity AddEmptyEntity(this GameObject obj,IAssetFactory factory)
        {
            var ent =obj.AddComponent<EmptyEntity>();
            factory.InjectD(obj);
            return ent;
        }
        public static Option<Vector3> TryToFindARandomWaypoint3D(
            this IEntity ent, IRandom rng, float radius,
            LayerMask mask = default,
            int iterations = 100)
        {
            ent.RecalculateBounds();
            var _bounds = ent.Bounds;
            var _pos = _bounds.center;

            for (int _i = 0; _i < iterations; _i++)
            {
                // Random point in sphere
                var _offset = rng.NextVector3() * rng.NextFloat(0f, radius);

                // Check collisions
                if (mask != default)
                {
                    if (!Physics.CheckBox(_pos + _offset, _bounds.extents, Quaternion.identity, mask))
                    {
                        return _offset.Some();
                    }
                }

                if (!Physics.CheckBox(_pos + _offset, _bounds.extents, Quaternion.identity))
                {
                    return _offset.Some();
                }
            }

            return Option.None<Vector3>();
        }

        public static Option<Vector2> TryToFindARandomWaypoint2D( this IEntity ent, IRandom rng, float radius,
            LayerMask mask = default,
            int iterations = 100)
        {
            ent.RecalculateBounds();
            var _bounds = ent.Bounds;
            var _pos = _bounds.center;
            for (int _i = 0; _i < iterations; _i++)
            {
                var _x =
                    rng.NextVector2() * rng.NextFloat(0f, radius);
                if (mask != default)
                {
                    if (!Physics2D.OverlapBox(_pos + (Vector3)_x, _bounds.size, 0, mask))
                    {
                        return _x.Some();
                    }
                }

                if (!Physics2D.OverlapBox(_pos + (Vector3)_x, _bounds.size, 0))
                {
                    return _x.Some();
                }
            }


            return Option.None<Vector2>();
        }
    }
}
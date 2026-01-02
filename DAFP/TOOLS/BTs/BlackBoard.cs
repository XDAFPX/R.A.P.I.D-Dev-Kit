using System.Collections.Generic;
using DAFP.GAME.Essential;
using DAFP.TOOLS.ECS;
using UnityEngine;

namespace DAFP.TOOLS.BTs
{
    public interface IBlackBoard
    {
        void Set<T>(string key, T value);
        T Get<T>(string key);
        bool Has(string key);
    }

    public class BlackBoard : IBlackBoard
    {
        private readonly Dictionary<string, object> data = new();


        public BlackBoard(IEntity target, IEntity self)
        {
            data.Clear();
            data.Add("Target", target);
            data.Add("Self", self);
        }

        public float GetDistanceToTarget(Vector2 curpos)
        {
            return GetTarget() != null
                ? Vector2.Distance(GetTarget().GetWorldRepresentation().transform.position, curpos)
                : Mathf.Infinity;
        }

        public void Set<T>(string key, T value)
        {
            data[key] = value;
        }

        public T Get<T>(string key)
        {
            if (key == default)
                return default;
            if (!Has(key)) return default;
            if (data.TryGetValue(key, out var _value) && _value is T _castValue) return _castValue;

            return default;
        }

        public bool Has(string key)
        {
            if (key == default)
                return default;
            return data.ContainsKey(key);
        }

        internal IEntity GetSelf()
        {
            return Get<IEntity>("Self");
        }

        internal IEntity GetTarget()
        {
            return Get<IEntity>("Target");
        }

        public Dictionary<string, object> GetFullData()
        {
            return data;
        }
    }
}
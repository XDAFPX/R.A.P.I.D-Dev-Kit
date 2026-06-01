using System;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common.Utill;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;

namespace DAFP.TOOLS.AssetManagement
{
    public interface IAssetPool<T, TP> : IAssetPoolBase
        where TP : Component, IGamePoolable<TP> where T : IGamePoolable<TP>
    {
        //PooledObject<TP> Get(out T v);
    }

    public interface IAssetPoolBase : IDisposable
    {
        int Count { get; }
        void Clear();
        Type GetPoolType();
        void ReleaseGeneric(IGamePoolableBase element);
        internal string Prefix { get; }
        UniTask<IGamePoolableBase> Get(GameAssetInfo info);
    }

    //--Basically the story goes like this. 
    //--You have an Address like "Effects.Explosion01"
    //--And then you go like UName = Explosion01
    //--And then you go like Prefix = Effects which is the pool
    //--And then you go like FullAddress = GAME.Assets.Effects.Explosion01 which is the actual address for spawning
    public struct GameAssetInfo
    {
        public const string ASSETS_PREFIX = "GAME.Assets.";

        public GameAssetInfo(string address) //-- can contain GAME.Assets. or not
        {
            Address = address;
        }

        public GameAssetInfo(IGamePoolableBase poolable)
        {
            var _address = poolable.Prefix + "." + poolable.UName;
            Address = FormatAddress(_address);
        }

        public string Address { get; }

        public string FullAddress => FormatAddress(Address);
        public string Prefix => GetPrefix(FormatAddress(Address));
        public string UName => FormatUName(GetUName(FormatAddress(Address)));

        public static string GetUName(string input)
        {
            input = FormatAddress(input);
            string[] _parts = input.Split('.');
            return string.Join(".", _parts.Skip(3));
        }

        public static GameAssetInfo From(string UName)
        {
            foreach (var _address in GameAssetsRegistry.AllAddresses)
            {
                if (FormatUName(GetUName(FormatAddress(_address))) == UName)
                    return new GameAssetInfo(_address);
            }

            throw new Exception($"Failed to find an asset with name {UName}");
        }

        public static GameAssetInfo From(IGamePoolableBase poolable) => new GameAssetInfo(poolable);
        public static implicit operator GameAssetInfo(string name) => From(name);

        public static string FormatAddress(string input)
        {
            if (input[^1].Equals('.'))
                input.Remove(input.Length - 1);
            if (input[0].Equals('.'))
                input.Remove(0);

            if (!input.StartsWith(ASSETS_PREFIX))
                input = input.Insert(0, ASSETS_PREFIX);
            input = input.Replace("..", ".");
            input = input.Replace("...", ".");
            return input;
        }


        public static string GetPrefix(string input)
        {
            input = FormatAddress(input);
            string[] _parts = input.Split('.');
            return _parts[2];
        }

        public static string FormatUName(string input)
        {
            if (!string.IsNullOrEmpty(input) && input[0] == '.')
            {
                input = input.Remove(0, 1);
            }

            return input;
        }
    }
}
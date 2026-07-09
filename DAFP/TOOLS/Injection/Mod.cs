using System;
using DAFP.TOOLS.Common.TextSys;
using MessagePipe;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.Injection
{
    public abstract class Mod : ScriptableObject, IMod
    {
        public string Name
        {
            get => name;
            set => name = value;
        }

        public abstract IMessage Description { get; set; }
        public abstract string Author { get; set; }


        private IDisposable disposables;

        public void Initialize()
        {
            var _bag = DisposableBag.CreateBuilder();
            RegisterSubscriptions(_bag);
            disposables = _bag.Build();
        }

        public void Dispose() => disposables?.Dispose();

        protected abstract void RegisterSubscriptions(DisposableBagBuilder bag);
    }
}
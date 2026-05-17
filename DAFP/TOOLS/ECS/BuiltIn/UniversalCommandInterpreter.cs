using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Audio;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.Serialization;
using DAFP.TOOLS.ECS.Services;
using ModestTree;
using NRandom;
using RapidLib.DAFP.TOOLS.Common;
using UGizmo;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class UniversalCommandInterpreter : ICommandInterpreter
    {
        public UniversalCommandInterpreter(IEnumerable<IConsoleCommand> commands)
        {
            Children = new List<ICommandInterpreter>();
            foreach (var _ownable in commands
                         .Cast<IOwnedBy<ICommandInterpreter>>())
                _ownable.ChangeOwner(this);
        }

        public virtual ITextProcess Process(string input)
        {
            foreach (var _ownable in Children)
            {
                var result = _ownable.Process(input);
                if (result != null)
                    return result;
            }

            return null;
        }

        public List<ICommandInterpreter> Children { get; } 
        public List<ICommandInterpreter> Owners { get; } = new();

    }
}
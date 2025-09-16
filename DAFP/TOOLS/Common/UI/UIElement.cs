using UnityEngine;

namespace DAFP.TOOLS.Common.UI
{
    /*
    public class UIElement : MonoBehaviour
    {
        public Animator Animator { private set;  get; }
        public bool IsEnabled { get; private set; }
        public string Name;
        private string animatorName;
    
        private void Start()
        {
    
    
            if (Name.StartsWith("GUIE_")) {
    
                animatorName = "GenericUIElement";
                Name = Name.Replace("GUIE_", string.Empty);
            }
            else
                animatorName = Name;
    
            UIManager.Singleton.RegisterUie(this);
            Animator = GetComponent<Animator>();
        }
        private void NoAnimFound(string hash)
        {
            Debug.LogWarning($"({this.GetType().FullName}): No animation found: ({hash}) On Object: ({animatorName}) ");
        }
        public void Disable()
        {
            if (!IsEnabled) return;
    
            if(Animator== null)
            {
                Kill();
                return;
            }
            var _hash = Animator.StringToHash($"{animatorName}_UIE_Disable");
            if (!Animator.HasState(0, _hash)) { NoAnimFound($"{animatorName}_UIE_Disable"); return; }
    
            Animator.Play(_hash);
    
        }
        public void DoCustomAnimation(string anName)
        {
            var _hash = Animator.StringToHash($"{animatorName}_UIE_{anName}");
            if (!Animator.HasState(0, _hash)) { NoAnimFound($"{animatorName}_UIE_{anName}"); return; }
    
            Animator.Play(_hash);
        }
        public void Enable()
        {
            if (IsEnabled) return;
            if (Animator == null)
            {
                Restore();
                return;
            }
            var _hash = Animator.StringToHash($"{animatorName}_UIE_Enable");
    
            Restore();
            IsEnabled = true;
            if (!Animator.HasState(0, _hash)) { NoAnimFound($"{animatorName}_UIE_Enable"); return; }
            Animator.Play(_hash);
        }
        
        public void Kill()
        {
    
            if (!IsEnabled) return;
    
            for (int _i = 0; _i < transform.childCount; _i++)
            {
                transform.GetChild(_i).gameObject.SetActive(false);
            }
            IsEnabled = false;
        }
        public void Restore()
        {
            if (IsEnabled) return;
            for (int _i = 0; _i < transform.childCount; _i++)
            {
                transform.GetChild(_i).gameObject.SetActive(true);
            }
            IsEnabled = true;
        }
        public void DoRattleAnimation()
        {
            var _hash = Animator.StringToHash($"{animatorName}_UIE_Rattle");
            if (Animator == null) return;
            if (!Animator.HasState(0, _hash)) { NoAnimFound($"{animatorName}_UIE_Rattle"); return; }
            Animator.Play(_hash);
        }
        public void DoIdleAnimation()
        {
            var _hash = Animator.StringToHash($"{animatorName}_UIE_Idle");
            if (Animator == null) return;
            if (!Animator.HasState(0, _hash)) { NoAnimFound($"{animatorName}_UIE_Idle"); return; }
            Animator.Play(_hash);
        }
        */
       
    }
    
    
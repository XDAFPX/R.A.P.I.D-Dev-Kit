using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DAFP.GAME.Essential;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DAFP.TOOLS.Common.UI
{
    // public class UIManager : Manager<UIManager>
    // {
    //     public bool EnabledHealthMechanic;
    //     public bool EnabledShieldMechanic;
    //     public bool EnabledMoneyMechanic;
    //     public Slider HealthBar;
    //     public Slider ShieldBar;
    //     public Animator BatteryCounter;
    //     public TextMeshProUGUI BatteryCounterText;
    //     public GameObject PauseMenu;
    //     private List<UIElement> elements = new List<UIElement>();
    //     public int Batteries {  get; private set; }
    //     private int displayebleBatteries;
    //     private int addedbateries;
    //     private bool isinmoneyanimation;
    //     private bool washurtrecently;
    //     public bool IshieldDecaying;
    //     
    //     public void Start()
    //     {
    //         EnableEveryThing();
    //     }
    //     private void FixedUpdate()
    //     {
    //         if (EnabledShieldMechanic&&ShieldBar.value ==0) { 
    //             GetElement("ShieldBar").Disable();
    //         }
    //     }
    //     public void EnableEveryThing()
    //     {
    //         EnabledHealthMechanic = true;
    //         EnabledShieldMechanic = true;   
    //         EnabledMoneyMechanic = true;
    //
    //         foreach (UIElement _element in elements) { _element.Enable();}
    //         DisplayBatteries(displayebleBatteries);
    //         Invoke(nameof(TryHideBatteries), 30);
    //     }
    //     public void DisableEveryThing()
    //     {
    //         EnabledHealthMechanic = false;
    //         EnabledShieldMechanic = false;
    //         EnabledMoneyMechanic = false;
    //
    //         foreach (UIElement _element in elements) { _element.Disable(); }
    //     }
    //     public void RegisterUie(UIElement element)
    //     {
    //         elements.Add(element);
    //     }
    //     public UIElement GetElement(string elementName)
    //     {
    //         return elements.FirstOrDefault((UIElement e) => e.Name == elementName);
    //     }
    //
    //
    //     public void AddBattery(int ammount) {
    //         if (EnabledMoneyMechanic) {
    //             Batteries += ammount;
    //
    //             addedbateries = Batteries - displayebleBatteries;
    //             Debug.Log($"ADDed: {ammount}");
    //             StartCoroutine(CheckBatteryCoolDown(addedbateries));
    //         }
    //
    //     }
    //
    //     public IEnumerator CheckBatteryCoolDown(int bat)
    //     {
    //         GetElement("Batteries").Enable();
    //         if (isinmoneyanimation||bat==0) {
    //             yield break;
    //         }
    //         yield return new WaitForSeconds(1.5f);
    //         if (addedbateries > bat) {
    //             yield break;
    //         }
    //         
    //         StartCoroutine(DoBatteriesCountAnimation());
    //     }
    //     public IEnumerator DoBatteriesCountAnimation() {
    //         BatteryCounter.Play("UI_BatteryCounter_Open");
    //         yield return new WaitForSeconds(0.75f);
    //         BatteryCounter.Play("UI_BatteryCounter_Count");
    //         int _max = Batteries - displayebleBatteries;
    //         isinmoneyanimation = true;
    //         for (int _i = 0; _i < _max; _i++) { 
    //             float _ratio = (float)_i / _max;
    //             float _reverseratio = 1 - _ratio;
    //
    //             yield return new WaitForSeconds(_reverseratio*0.3f);
    //             displayebleBatteries++;
    //             DisplayBatteries(displayebleBatteries);
    //         }
    //         SetDisplayebleBatteries(Batteries);
    //         BatteryCounter.Play("UI_BatteryCounter_Close");
    //         isinmoneyanimation = false;
    //         Invoke(nameof(TryHideBatteries), 30f);
    //     }
    //     public void TryHideBatteries()
    //     {
    //         if (Batteries - displayebleBatteries != 0)
    //         {
    //             return;
    //         }
    //         GetElement("Batteries").Disable();
    //
    //     }
    //
    //     public void SetDisplayebleBatteries(int bat)
    //     {
    //         displayebleBatteries = bat;
    //         DisplayBatteries(bat);
    //     }
    //
    //     public void DisplayBatteries(int bat)
    //     {
    //         BatteryCounterText.text = $"x{bat}";
    //     }
    //
    //
    //     public void DecayShield()
    //     {
    //         if (!EnabledShieldMechanic)
    //             return;
    //         if (ShieldBar.value  < 1)
    //             return;
    //         ShieldBar.value -= 0.013f;
    //         GetElement("ShieldBar").Enable();
    //     }
    //     public bool IsMaxHealth()
    //     {
    //         if (!EnabledHealthMechanic)
    //             return true;
    //         return HealthBar.value == HealthBar.maxValue;
    //     }
    //     public bool IsMaxShield()
    //     {
    //         if (!EnabledShieldMechanic)
    //             return true;
    //         return ShieldBar.value == ShieldBar.maxValue;
    //     }
    //     public float GetHealth()
    //     {
    //         if (!EnabledHealthMechanic)
    //         {
    //             if (EnabledShieldMechanic)
    //                 return ShieldBar.maxValue + HealthBar.maxValue;
    //             return ShieldBar.value;
    //         }
    //         else if (!EnabledShieldMechanic)
    //         {
    //             if(EnabledHealthMechanic)
    //                 return ShieldBar.maxValue + HealthBar.maxValue;
    //             return HealthBar.value;
    //         }
    //         return ShieldBar.value + HealthBar.value;
    //     }
    //     public void Setmaxhealth(float max)
    //     {
    //         if (!EnabledHealthMechanic)
    //             return;
    //         HealthBar.maxValue = max;
    //         HealthBar.value = max;
    //     }
    //     public void SetHealth(float hal)
    //     {
    //         if (!EnabledHealthMechanic)
    //             return;
    //         HealthBar.value = hal;
    //     }
    //     public void HealShield(float value)
    //     {
    //         if (!EnabledShieldMechanic)
    //             return;
    //
    //         GetElement("ShieldBar").Enable();
    //         value *= Mathf.Clamp(1.6f - HealthBar.value / HealthBar.maxValue, 0.7f, 2f);
    //
    //         ShieldBar.value += value;
    //         if(ShieldBar.value == ShieldBar.maxValue)
    //         {
    //             GetElement("ShieldBar").DoRattleAnimation();
    //         }
    //     }
    //     public void HealHealth(float value)
    //     {
    //         if (!EnabledHealthMechanic)
    //             return;
    //         GetElement("HealthBar").Enable();
    //         HealthBar.value += value;
    //         if (HealthBar.value == HealthBar.maxValue)
    //         {
    //             GetElement("HealthBar").DoRattleAnimation();
    //         }
    //     }
    //     public void TakeDamage(float he)
    //     {
    //         if (!EnabledHealthMechanic && !EnabledShieldMechanic) return;
    //         washurtrecently = true;
    //         CancelInvoke(nameof(HurtRecentlyTimer));
    //         Invoke(nameof(HurtRecentlyTimer), 13);
    //
    //
    //
    //         
    //         if (!EnabledHealthMechanic)
    //         {
    //             TakeDamageShield(he);
    //             return;
    //
    //         }
    //         if (!EnabledShieldMechanic)
    //         {
    //             TakeDamageHealth(he);
    //             return;
    //         }
    //         if(EnabledHealthMechanic && EnabledShieldMechanic)
    //         {
    //             if(ShieldBar.value > 0)
    //             {
    //                 TakeDamageShield(he);
    //                 return;
    //             }
    //             TakeDamageHealth(he);
    //         }
    //     }
    //     private void HurtRecentlyTimer()
    //     {
    //         washurtrecently = false;
    //     }
    //     private void TryHideHealthTimer()
    //     {
    //         if (!washurtrecently)
    //         {
    //             
    //             GetElement("HealthBar").Disable();
    //         }
    //     }
    //     private void TryHideShieldTimer()
    //     {
    //         if (!washurtrecently&&!IshieldDecaying)
    //         {
    //
    //             GetElement("ShieldBar").Disable();
    //         }
    //     }
    //     private void TakeDamageShield(float damage)
    //     {
    //         GetElement("ShieldBar").Enable();
    //         ShieldBar.value -= damage * 0.75f;
    //         
    //         Invoke(nameof(TryHideShieldTimer), 16);
    //     }
    //     private void TakeDamageHealth(float damage)
    //     {
    //
    //         GetElement("HealthBar").Enable();
    //         HealthBar.value -= damage;
    //         Invoke(nameof(TryHideHealthTimer), 16);
    //
    //     }
    //     // public void Pause()
    //     // {
    //     //     if(GameSystem.STATE == GameSystem.Gamestate.Playing)
    //     //     {
    //     //         Time.timeScale = 0f;
    //     //         GameSystem.STATE = GameSystem.Gamestate.Paused;
    //     //         PauseMenu.gameObject.SetActive(true);
    //     //     }
    //     //
    //     //
    //     // }
    //     // public void UnPause()
    //     // {
    //     //     if (GameSystem.STATE == GameSystem.Gamestate.Paused)
    //     //     {
    //     //         Time.timeScale = 1f;
    //     //         GameSystem.STATE = GameSystem.Gamestate.Playing;
    //     //         PauseMenu.gameObject.SetActive(false);
    //     //     }
    //     // }
    //     // public void Update()
    //     // {
    //     //     if (Input.GetKeyDown(KeyCode.Escape))
    //     //     {
    //     //         if(GameSystem.STATE == GameSystem.Gamestate.Paused)
    //     //             UnPause();
    //     //         else if (GameSystem.STATE == GameSystem.Gamestate.Playing)
    //     //             Pause();
    //     //     }
    //     // }
    // }
}
using System;
using System.Collections.Generic;
using System.Text;
using XRL.Language;
using XRL.Rules;
using XRL.World.Capabilities;
// MyActivatedAbility(ActivatedAbilityID,GetActivePartFirstSubject()) returns the ability correctly
// but CooldownMyActivatedAbility(ActivatedAbilityID, GetDowntime(MyPowerLoadLevel())) isnt showing the cooldown
// on the ability bar. Investigate

namespace XRL.World.Parts {

	[Serializable]
	public class Cherman0_BalanceTweaks_ForceEmitterTimed : ForceEmitter
	{
		public int Uptime = 10;

		public int UptimeOverloadBonus = 5;

		public int Downtime = 50;

		public int DowntimeOverloadBonus = 10;

		public int UptimeRemaining = 10;

		public Cherman0_BalanceTweaks_ForceEmitterTimed() : base()
		{

		}

		public int GetUptime(int PowerLoad)
		{
			//10 normallly; 15 with overloaded
			return Uptime + (MyPowerLoadBonus(PowerLoad) / 2) * UptimeOverloadBonus;
		}

		public int GetDowntime(int PowerLoad)
		{
			//50 normally; 40 with overload
			return Downtime - (MyPowerLoadBonus(PowerLoad) / 2) * DowntimeOverloadBonus;
		}

		new public void SetUpActivatedAbility(GameObject Actor)
		{
			//hiding the parent version with one that configures vars to be a cooldownable ability
			if (Actor != null)
			{
				ActivatedAbilityID = Actor.AddActivatedAbility(
					GetActivatedAbilityName(Actor),
					COMMAND_NAME,
					(Actor == ParentObject) ? "Maneuvers" : "Items",
					null,
					"è",
					AffectedByWillpower : false,
					Toggleable : true,
					DefaultToggleState : false,
					ActiveToggle : true,
					IsAttack : false
					);
			}
		}

		public override bool HandleEvent(EndTurnEvent E)
		{
			if(UptimeRemaining > 0)
			{
				UptimeRemaining--;
			}
			if (UptimeRemaining <= 0 && IsActive())
			{
				DestroyBubble();
				CooldownMyActivatedAbility(ActivatedAbilityID, GetDowntime(MyPowerLoadLevel()), GetActivePartFirstSubject());
				MyActivatedAbility(ActivatedAbilityID, GetActivePartFirstSubject()).ToggleState = false;
			}
			return base.HandleEvent(E);
		}
		public override bool HandleEvent(GetInventoryActionsEvent E)
		{
			if (ParentObject.Understood())
			{
				GameObject activePartFirstSubject = GetActivePartFirstSubject();
				if (activePartFirstSubject != null && activePartFirstSubject.IsPlayer())
				{
					if (IsActive())
					{
						E.AddAction("Deactivate", "deactivate", COMMAND_NAME, null, 'a', FireOnActor: false, 10);
					}
					else if(!IsMyActivatedAbilityCoolingDown(ActivatedAbilityID, activePartFirstSubject))
					{
						E.AddAction("Activate", "activate", COMMAND_NAME, null, 'a', FireOnActor: false, 10);
					}
				}
			}

			return false;
		}

		public override bool HandleEvent(InventoryActionEvent E)
		{
			if (IsActive())
			{
				DestroyBubble();
				UptimeRemaining = 0;
				CooldownMyActivatedAbility(ActivatedAbilityID, GetDowntime(MyPowerLoadLevel()), GetActivePartFirstSubject());
				MyActivatedAbility(ActivatedAbilityID, GetActivePartFirstSubject()).ToggleState = false;
				return false; //to prevent it from immediately turning back on when handled by the base force bracelet
			}
			else
			{
				UptimeRemaining = GetUptime(MyPowerLoadLevel());
				MyActivatedAbility(ActivatedAbilityID, GetActivePartFirstSubject()).ToggleState = true;
				return base.HandleEvent(E);
			}
		}

		public override bool HandleEvent(CommandEvent E)
		{
			if (IsActive())
			{
				DestroyBubble();
				UptimeRemaining = 0;
				CooldownMyActivatedAbility(ActivatedAbilityID, GetDowntime(MyPowerLoadLevel()), GetActivePartFirstSubject());
				MyActivatedAbility(ActivatedAbilityID, GetActivePartFirstSubject()).ToggleState = false;
				return false; //to prevent it from immediately turning back on when handled by the base force bracelet
			}
			else
			{
				UptimeRemaining = GetUptime(MyPowerLoadLevel());
				MyActivatedAbility(ActivatedAbilityID, GetActivePartFirstSubject()).ToggleState = true;
				return base.HandleEvent(E);
			}
		}

		public override bool HandleEvent(ExamineSuccessEvent E)
		{
			if (E.Object == ParentObject && E.Complete)
			{
				SetUpActivatedAbility(ParentObject.Equipped);
			}
			//dont call parent since we hid SetUpActivatedAbility() and dont want to call the parent version of it
			return false;
		}

		public override bool HandleEvent(ObjectCreatedEvent E)
		{
			if (WorksOnSelf)
			{
				SetUpActivatedAbility(ParentObject);
			}
			//dont call parent since we hid SetUpActivatedAbility() and dont want to call the parent version of it
			return false;
		}

		public override bool HandleEvent(EquippedEvent E)
		{
			E.Actor.RegisterPartEvent(this, "BeginMove");
			E.Actor.RegisterPartEvent(this, "EffectApplied");
			E.Actor.RegisterPartEvent(this, "EnteredCell");
			E.Actor.RegisterPartEvent(this, "MoveFailed");
			if (ParentObject.Understood())
			{
				SetUpActivatedAbility(E.Actor);
			}
			//dont call parent since we hid SetUpActivatedAbility() and dont want to call the parent version of it
			return false;
		}
	}
}
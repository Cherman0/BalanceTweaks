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
    public class ForceEmitterTimed : ForceEmitter
    {
        public int Uptime = 10;

        public int Downtime = 50;

        public int UptimeRemaining = 10;

        public int DowntimeRemaining = 0;

        public ForceEmitterTimed() : base()
        {

        }

        public int GetUptime(int PowerLoad)
        {
            //10 normallly; 15 with overloaded
            return Uptime + (MyPowerLoadBonus(PowerLoad) * 5 / 2);
        }

        public int GetDowntime(int PowerLoad)
        {
            //50 normally; 40 with overload
            return Downtime + (MyPowerLoadBonus(PowerLoad) * 5);
        }

        public override bool HandleEvent(EndTurnEvent E)
        {
            if(DowntimeRemaining > 0)
            {
                DowntimeRemaining--;
                if(DowntimeRemaining <= 0)
                {
                    IComponent<GameObject>.AddPlayerMessage(ParentObject + "'s capacitor has finished recharging!");
                }
            }
            if(UptimeRemaining > 0)
            {
                UptimeRemaining--;
            }
            if (UptimeRemaining <= 0 && IsActive())
            {
                DestroyBubble();
                DowntimeRemaining = GetDowntime(MyPowerLoadLevel());
                IComponent<GameObject>.AddPlayerMessage(ParentObject + "'s capacitor is expended and it shuts down! It will take " + DowntimeRemaining + " turns to recharge.");
            }
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(InventoryActionEvent E)
        {
            if (IsActive())
            {
                DestroyBubble();
                UptimeRemaining = 0;
                DowntimeRemaining = GetDowntime(MyPowerLoadLevel());
                IComponent<GameObject>.AddPlayerMessage(ParentObject + "'s capacitor is expended and it shuts down! It will take " + DowntimeRemaining + " turns to recharge.");
                return false; //to prevent it from immediately turning back on when handled by the base force bracelet
            }
            else if (DowntimeRemaining > 0)
            {
                IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("are") + " on cooldown for " + DowntimeRemaining + " more turns!");
                return false; //we're turned off and on cooldown so we shouldn't toggle
            }
            UptimeRemaining = GetUptime(MyPowerLoadLevel());
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(CommandEvent E)
        {
            if (IsActive())
            {
                DestroyBubble();
                UptimeRemaining = 0;
                DowntimeRemaining = GetDowntime(MyPowerLoadLevel());
                CooldownMyActivatedAbility(ActivatedAbilityID, GetDowntime(MyPowerLoadLevel())); //since we have an activated ability we can use real cooldowns
                IComponent<GameObject>.AddPlayerMessage(ParentObject + "'s capacitor is expended and it shuts down! It will take " + DowntimeRemaining + " turns to recharge.");
                return false; //to prevent it from immediately turning back on when handled by the base force bracelet
            }
            else if (DowntimeRemaining > 0)
            {
                IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("are") + " on cooldown for " + DowntimeRemaining + " more turns!");
                return false; //we're turned off and on cooldown so we shouldn't toggle
            }
            UptimeRemaining = GetUptime(MyPowerLoadLevel());
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(ExamineSuccessEvent E)
        {
            bool parentValue = base.HandleEvent(E);
            if (E.Object == ParentObject && E.Complete)
            {
                MyActivatedAbility(ActivatedAbilityID, GetActivePartFirstSubject()).AffectedByWillpower = false;
            }
            return parentValue;
        }

        public override bool HandleEvent(ObjectCreatedEvent E)
        {
            bool parentValue = base.HandleEvent(E);
            if (WorksOnSelf)
            {
                MyActivatedAbility(ActivatedAbilityID, GetActivePartFirstSubject()).AffectedByWillpower = false;
            }
            return parentValue;
        }

        public override bool HandleEvent(EquippedEvent E)
        {
            bool parentValue = base.HandleEvent(E);
            if (ParentObject.Understood())
            {
                MyActivatedAbility(ActivatedAbilityID, GetActivePartFirstSubject()).AffectedByWillpower = false;
            }
            return parentValue;
        }
    }
}
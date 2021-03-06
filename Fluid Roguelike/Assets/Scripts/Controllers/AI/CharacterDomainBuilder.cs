﻿using Fluid.Roguelike.AI.Conditions;
using Fluid.Roguelike.AI.Effects;
using Fluid.Roguelike.AI.Operators;
using Fluid.Roguelike.Character.State;
using FluidHTN;
using FluidHTN.Factory;
using FluidHTN.PrimitiveTasks;

namespace Fluid.Roguelike.AI
{
    public class CharacterDomainBuilder : BaseDomainBuilder<CharacterDomainBuilder, CharacterContext>
    {
        public CharacterDomainBuilder(string domainName) : base(domainName, new DefaultFactory())
        {
        }

        public CharacterDomainBuilder HasState(CharacterWorldState state)
        {
            var condition = new HasWorldStateCondition(state);
            Pointer.AddCondition(condition);
            return this;
        }

        public CharacterDomainBuilder HasState(CharacterWorldState state, byte value)
        {
            var condition = new HasWorldStateCondition(state, value);
            Pointer.AddCondition(condition);
            return this;
        }

        public CharacterDomainBuilder SetState(CharacterWorldState state, EffectType type)
        {
            if (Pointer is IPrimitiveTask task)
            {
                var effect = new SetWorldStateEffect(state, type);
                task.AddEffect(effect);
            }
            return this;
        }

        public CharacterDomainBuilder SetState(CharacterWorldState state, bool value, EffectType type)
        {
            if (Pointer is IPrimitiveTask task)
            {
                var effect = new SetWorldStateEffect(state, value, type);
                task.AddEffect(effect);
            }
            return this;
        }

        public CharacterDomainBuilder SetState(CharacterWorldState state, byte value, EffectType type)
        {
            if (Pointer is IPrimitiveTask task)
            {
                var effect = new SetWorldStateEffect(state, value, type);
                task.AddEffect(effect);
            }
            return this;
        }

        public CharacterDomainBuilder IncrementState(CharacterWorldState state, EffectType type)
        {
            if (Pointer is IPrimitiveTask task)
            {
                var effect = new IncrementWorldStateEffect(state, type);
                task.AddEffect(effect);
            }
            return this;
        }

        public CharacterDomainBuilder IncrementState(CharacterWorldState state, byte value, EffectType type)
        {
            if (Pointer is IPrimitiveTask task)
            {
                var effect = new IncrementWorldStateEffect(state, value, type);
                task.AddEffect(effect);
            }
            return this;
        }

        public CharacterDomainBuilder FindEnemyTarget()
        {
            Action("Find enemy target");
            if (Pointer is IPrimitiveTask task)
            {
                task.SetOperator(new FindEnemyTargetOperator());
                SetState(CharacterWorldState.HasEnemyTarget, true, EffectType.PlanAndExecute);
            }
            End();
            return this;
        }

        public CharacterDomainBuilder FindSpellToCast()
        {
            Action("Find spell to cast");
            if (Pointer is IPrimitiveTask task)
            {
                CanCastAnySpell();
                task.SetOperator(new FindSpellToCastOperator());
                SetState(CharacterWorldState.HasSpellToCast, true, EffectType.PlanAndExecute);
            }

            End();
            return this;
        }

        public CharacterDomainBuilder MeleeEnemy()
        {
            Action("Melee enemy");
            if (Pointer is IPrimitiveTask task)
            {
                HasState(CharacterWorldState.IsStunned, 0);
                HasState(CharacterWorldState.HasEnemyTargetInMeleeRange);
                task.SetOperator(new MeleeOperator(MeleeOperator.TargetType.Enemy));
            }
            End();
            return this;
        }

        public CharacterDomainBuilder CanCastSpell()
        {
            var condition = new CanCastSpellCondition();
            Pointer.AddCondition(condition);
            return this;
        }

        public CharacterDomainBuilder CanCastAnySpell()
        {
            var condition = new CanCastAnySpellCondition();
            Pointer.AddCondition(condition);
            return this;
        }

        public CharacterDomainBuilder CastSpellEffect(EffectType type)
        {
            if (Pointer is IPrimitiveTask task)
            {
                var effect = new CastSpellEffect(type);
                task.AddEffect(effect);
            }
            return this;
        }

        public CharacterDomainBuilder CastSpellOnEnemy()
        {
            Action("Cast spell on enemy");
            if (Pointer is IPrimitiveTask task)
            {
                HasState(CharacterWorldState.IsStunned, 0);
                HasState(CharacterWorldState.HasEnemyTargetAtSpellCastRange);
                CanCastSpell();
                task.SetOperator(new CastSpellOperator(CastSpellOperator.TargetType.Enemy));
                CastSpellEffect(EffectType.PlanOnly);
            }
            End();
            return this;
        }

        public CharacterDomainBuilder MoveToEnemy()
        {
            Action("Move to enemy");
            if (Pointer is IPrimitiveTask task)
            {
                task.SetOperator(new MoveToOperator(MoveToOperator.TargetType.Enemy));
                SetState(CharacterWorldState.HasEnemyTargetInMeleeRange, true, EffectType.PlanAndExecute);
            }
            End();
            return this;
        }

        public CharacterDomainBuilder MoveAwayFromEnemy()
        {
            Action("Move away from enemy");
            if (Pointer is IPrimitiveTask task)
            {
                HasState(CharacterWorldState.HasEnemyTargetInMeleeRange, 0);
                task.SetOperator(new MoveAwayOperator(MoveAwayOperator.TargetType.Enemy));
                SetState(CharacterWorldState.HasEnemyTargetInMeleeRange, false, EffectType.PlanOnly);
            }
            End();
            return this;
        }

        // ----------------------------------------------------------------------- COMPLEX OPTIONS

        public CharacterDomainBuilder EngageEnemyMeleeSequence()
        {
            Sequence("Engage enemy");
            {
                FindEnemyTarget();
                Select("Move or attack");
                {
                    HasState(CharacterWorldState.HasEnemyTarget);
                    MeleeEnemy();
                    Sequence("Move to enemy");
                    {
                        MoveToEnemy();
                        MeleeEnemy();
                    }
                    End();
                }
                End();
            }
            End();
            return this;
        }

        public CharacterDomainBuilder FleeOrEngageSequence()
        {
            Sequence("Flee enemy");
            {
                FindEnemyTarget();
                Select("Flee or attack");
                {
                    HasState(CharacterWorldState.HasEnemyTarget);
                    MoveAwayFromEnemy();
                    MeleeEnemy();
                }
                End();
            }
            End();
            return this;
        }

        public CharacterDomainBuilder CastSpellKeepDistanceSequence()
        {
            Sequence("Cast spells and keep distance");
            {
                FindEnemyTarget();
                FindSpellToCast();
                Select("Flee or cast spells");
                {
                    HasState(CharacterWorldState.HasEnemyTarget);
                    HasState(CharacterWorldState.HasSpellToCast);
                    CastSpellOnEnemy();
                    MoveAwayFromEnemy();
                }
                End();
            }
            End();
            return this;
        }
    }
}
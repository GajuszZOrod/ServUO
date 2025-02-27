using Server.Engines.MyrmidexInvasion;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("zwloki wojownika myrmideksow")]
    public class MyrmidexWarrior : BaseCreature
    {
        [Constructable]
        public MyrmidexWarrior()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, .2, .4)
        {
            Name = "wojownik myrmideksow";

            Body = 1403;
            BaseSoundID = 959;
            //Hue = 2676;

            SetStr(500, 600);
            SetDex(82, 95);
            SetInt(130, 140);

            SetDamage(18, 22);

            SetHits(2800, 3000);
            SetMana(40, 50);

            SetResistance(ResistanceType.Physical, 1, 10);
            SetResistance(ResistanceType.Fire, 1, 10);
            SetResistance(ResistanceType.Cold, 1, 10);
            SetResistance(ResistanceType.Poison, 1, 10);
            SetResistance(ResistanceType.Energy, 1, 10);

            SetDamageType(ResistanceType.Physical, 50);
            SetDamageType(ResistanceType.Poison, 50);

            SetSkill(SkillName.Wrestling, 90, 100);
            SetSkill(SkillName.Tactics, 90, 100);
            SetSkill(SkillName.MagicResist, 70, 80);
            SetSkill(SkillName.Poisoning, 70, 80);
            SetSkill(SkillName.Magery, 80, 90);
            SetSkill(SkillName.EvalInt, 70, 80);

            Fame = 8000;
            Karma = -8000;
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich, 2);
            AddLoot(LootPack.LootItemCallback(TryDropMoonstone, 0.25, Utility.RandomMinMax(1, 5), false, false));
        }

        public static Item TryDropMoonstone(IEntity e)
        {
            if (Region.Find(e.Location, e.Map).IsPartOf("MyrmidexBattleground"))
            {
                return new MoonstoneCrystalShard();
            }

            return null;
        }

        public override Poison HitPoison => Poison.Deadly;
        public override Poison PoisonImmune => Poison.Deadly;
        public override int TreasureMapLevel => 2;

        public override bool IsEnemy(Mobile m)
        {
            if (MyrmidexInvasionSystem.Active && MyrmidexInvasionSystem.IsAlliedWithEodonTribes(m))
                return true;

            if (MyrmidexInvasionSystem.Active && MyrmidexInvasionSystem.IsAlliedWithMyrmidex(m))
                return false;

            return base.IsEnemy(m);
        }

        public MyrmidexWarrior(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}

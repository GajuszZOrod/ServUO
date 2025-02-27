namespace Server.Items
{
    [Flipable(0x1B09, 0x1B10)]
    public class BonePile : Item, IScissorable
    {
        [Constructable]
        public BonePile()
            : base(0x1B09 + Utility.Random(8))
        {
            Stackable = false;
            Weight = 10.0;
            Name = "kupa kosci";
        }

        public BonePile(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public bool Scissor(Mobile from, Scissors scissors)
        {
            if (Deleted || !from.CanSee(this))
                return false;

            base.ScissorHelper(from, new Bone(), Utility.RandomMinMax(10, 15));

            return true;
        }
    }
}
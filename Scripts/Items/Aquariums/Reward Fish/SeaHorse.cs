namespace Server.Items
{
    public class SeaHorseFish : BaseFish
    {
        [Constructable]
        public SeaHorseFish()
            : base(0x3B10)
        {
            Name = "konik morski";
        }

        public SeaHorseFish(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1074414;// A sea horse
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
    }
}
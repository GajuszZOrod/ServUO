using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    public class Gardener : BaseVendor
    {
        private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();
        [Constructable]
        public Gardener()
            : base("- ogrodnik")
        {
        }

        public Gardener(Serial serial)
            : base(serial)
        {
        }

        public override VendorShoeType ShoeType => VendorShoeType.ThighBoots;
        protected override List<SBInfo> SBInfos => m_SBInfos;
        public override void InitSBInfo()
        {
            m_SBInfos.Add(new SBGardener());
        }

        public override int GetShoeHue()
        {
            return 0;
        }

        public override void InitOutfit()
        {
            base.InitOutfit();

            SetWearable(new WideBrimHat(), Utility.RandomNeutralHue(), 1);
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
    }
}

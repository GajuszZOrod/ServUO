////////////////////////////////////////
//                                     //
//   Generated by CEO's YAAAG - Ver 2  //
// (Yet Another Arya Addon Generator)  //
//    Modified by Hammerhand for       //
//      SA & High Seas content         //
//                                     //
////////////////////////////////////////

namespace Server.Items
{
	public class LargePondAddon : BaseAddon
	{
		private static readonly int[,] m_AddOnSimpleComponents =
		{
			{ 6040, -5, 4, 0 }, { 6059, 1, 8, 0 }, { 933, -2, 6, 0 } // 1	2	3	
			,
			{ 6058, 3, 8, 0 }, { 6058, 4, 7, 0 }, { 6058, 5, 6, 0 } // 4	5	6	
			,
			{ 1217, 0, 4, 5 }, { 1217, 0, 5, 5 }, { 1217, 0, 6, 5 } // 7	8	9	
			,
			{ 6039, -2, 5, 0 }, { 1217, -1, 1, 5 }, { 6039, -3, 5, 0 } // 10	11	12	
			,
			{ 13573, 2, 1, 1 }, { 6055, 8, 6, 0 }, { 13567, 1, 1, 1 } // 13	14	15	
			,
			{ 6040, -2, 4, 0 }, { 6049, -2, 6, 0 }, { 935, 2, 3, 11 } // 16	17	18	
			,
			{ 6056, 1, 9, 0 }, { 6056, 0, 8, 0 }, { 6056, -1, 7, 0 } // 19	20	21	
			,
			{ 6039, -5, 5, 0 }, { 6055, 10, 2, 0 }, { 935, -1, 4, 11 } // 22	23	24	
			,
			{ 935, -1, 3, 11 }, { 935, -1, 1, 11 }, { 935, -1, 2, 11 } // 25	26	27	
			,
			{ 1217, 1, 5, 5 }, { 935, 2, 2, 11 }, { 6055, 5, 7, 0 } // 28	29	30	
			,
			{ 1217, -1, 6, 5 }, { 1217, -1, 4, 5 }, { 1217, -1, 5, 5 } // 31	32	33	
			,
			{ 1217, -1, 2, 5 }, { 1217, -1, 3, 5 }, { 6039, 9, 1, 0 } // 34	35	36	
			,
			{ 935, -1, 5, 11 }, { 8109, 2, 4, 0 }, { 3255, 7, 3, 1 } // 37	38	39	
			,
			{ 3255, -3, 4, 1 }, { 935, 2, 6, 11 }, { 938, -2, 6, 5 } // 40	41	42	
			,
			{ 942, 1, 6, 0 }, { 935, -1, 6, 11 }, { 3220, -5, 4, 1 } // 43	44	45	
			,
			{ 4943, 4, 8, 0 }, { 4944, 5, 8, 0 }, { 3334, 4, 2, 4 } // 46	47	48	
			,
			{ 3209, -3, 6, 1 }, { 3336, -4, 3, 1 }, { 3257, 3, 4, 1 } // 49	50	51	
			,
			{ 4945, 5, 7, 0 }, { 13449, 2, 6, 2 }, { 3338, -3, 2, 1 } // 52	53	54	
			,
			{ 3376, 6, 7, 1 }, { 3234, -5, 6, 1 }, { 3259, 7, 5, 1 } // 55	56	57	
			,
			{ 3235, 3, 9, 1 }, { 3259, 2, 7, 1 }, { 8109, 2, 1, 0 } // 58	59	60	
			,
			{ 6042, 2, 8, 0 }, { 6042, 8, 1, 0 }, { 3220, 6, 6, 2 } // 61	62	63	
			,
			{ 6045, 8, 5, 0 }, { 6046, 8, 4, 0 }, { 6058, 8, 2, 0 } // 64	65	66	
			,
			{ 1217, 1, 6, 5 }, { 6042, 1, 7, 0 }, { 6040, 0, 6, 0 } // 67	68	69	
			,
			{ 6040, 3, 7, 0 }, { 6041, 3, 6, 0 }, { 6042, 4, 6, 0 } // 70	71	72	
			,
			{ 6044, 2, 6, 0 }, { 6045, 8, 3, 0 }, { 6045, 10, 1, 0 } // 73	74	75	
			,
			{ 6043, 2, 7, 0 }, { 6039, 1, 6, 0 }, { 6043, 6, 2, 0 } // 76	77	78	
			,
			{ 1849, -1, 8, 0 }, { 1849, 1, 8, 0 }, { 938, 1, 6, 5 } // 79	80	81	
			,
			{ 6044, 4, 1, 0 }, { 6044, -1, 1, 0 }, { 1849, 0, 8, 0 } // 82	83	84	
			,
			{ 6041, 2, 4, 0 }, { 6041, 0, 4, 0 }, { 6041, 1, 4, 0 } // 85	86	87	
			,
			{ 6041, 3, 4, 0 }, { 6043, 2, 2, 0 }, { 6050, -3, 6, 0 } // 88	89	90	
			,
			{ 6050, -5, 6, 0 }, { 6044, -3, 1, 0 }, { 6044, 0, 1, 0 } // 91	92	93	
			,
			{ 6049, -4, 6, 0 }, { 6049, 2, 9, 0 }, { 6044, -5, 1, 0 } // 94	95	96	
			,
			{ 6044, 3, 1, 0 }, { 6044, 2, 1, 0 }, { 6043, 7, 2, 0 } // 97	98	99	
			,
			{ 6043, -2, 2, 0 }, { 6043, 1, 2, 0 }, { 6043, -1, 2, 0 } // 100	101	102	
			,
			{ 6043, -3, 2, 0 }, { 6043, -4, 2, 0 }, { 6059, -1, 6, 0 } // 103	104	105	
			,
			{ 6044, 7, 1, 0 }, { 1217, 1, 4, 5 }, { 1848, -1, 7, 0 } // 106	107	108	
			,
			{ 6044, -4, 1, 0 }, { 17155, -5, 3, 22 }, { 6055, 4, 8, 0 } // 109	110	111	
			,
			{ 6042, 0, 3, 0 }, { 6042, -3, 3, 0 }, { 6042, -2, 3, 0 } // 112	113	114	
			,
			{ 6043, 0, 2, 0 }, { 6044, 6, 1, 0 }, { 6044, 5, 1, 0 } // 115	116	117	
			,
			{ 6043, -5, 2, 0 }, { 6043, 5, 2, 0 }, { 6043, 4, 2, 0 } // 118	119	120	
			,
			{ 6049, 6, 6, 0 }, { 6050, 7, 6, 0 }, { 6041, 5, 4, 0 } // 121	122	123	
			,
			{ 1217, 0, 2, 5 }, { 6055, 3, 9, 0 }, { 6040, -3, 4, 0 } // 124	125	126	
			,
			{ 6042, 5, 3, 0 }, { 6042, 4, 3, 0 }, { 6042, 1, 3, 0 } // 127	128	129	
			,
			{ 6044, -2, 1, 0 }, { 935, 2, 5, 11 }, { 6041, 7, 4, 0 } // 130	131	132	
			,
			{ 935, 2, 4, 11 }, { 6042, -1, 3, 0 }, { 1217, 0, 1, 5 } // 133	134	135	
			,
			{ 6041, 4, 4, 0 }, { 6059, 0, 7, 0 }, { 6049, 9, 2, 0 } // 136	137	138	
			,
			{ 6040, -4, 4, 0 }, { 6039, -4, 5, 0 }, { 4964, 9, 2, 0 } // 139	140	141	
			,
			{ 3516, 4, 3, 1 }, { 3339, -4, 3, 1 }, { 6039, 7, 5, 0 } // 142	143	144	
			,
			{ 4963, -4, 6, 1 }, { 3518, 4, 2, 1 }, { 4968, 3, 1, 3 } // 145	146	147	
			,
			{ 1217, 0, 3, 5 }, { 6039, 6, 5, 0 }, { 6041, 6, 4, 0 } // 148	149	150	
			,
			{ 6042, 7, 3, 0 }, { 1848, 0, 7, 0 }, { 6042, 3, 3, 0 } // 151	152	153	
			,
			{ 6042, -4, 3, 0 }, { 6042, -5, 3, 0 }, { 6041, -1, 4, 0 } // 154	155	156	
			,
			{ 6043, 3, 2, 0 }, { 1848, 1, 7, 0 }, { 1217, 1, 3, 5 } // 157	158	159	
			,
			{ 6039, -1, 5, 0 }, { 1217, 1, 1, 5 }, { 6044, 1, 1, 0 } // 160	161	162	
			,
			{ 6042, 2, 3, 0 }, { 6042, 6, 3, 0 }, { 6039, 5, 5, 0 } // 163	164	165	
			,
			{ 935, 2, 1, 11 }, { 1217, 1, 2, 5 }, { 6039, 0, 5, 0 } // 166	167	168	
			,
			{ 6039, 4, 5, 0 }, { 6039, 1, 5, 0 }, { 6039, 3, 5, 0 } // 169	170	171	
			,
			{ 6039, 2, 5, 0 }, { 6060, 0, -7, 0 }, { 935, -1, -5, 11 } // 172	173	174	
			,
			{ 1217, 0, -5, 5 }, { 1217, -1, 0, 5 }, { 6057, 2, -7, 0 } // 175	176	177	
			,
			{ 1217, -1, -1, 5 }, { 1217, 1, -6, 5 }, { 1217, 1, -5, 5 } // 178	179	180	
			,
			{ 1217, 1, -4, 5 }, { 13573, 2, -1, 1 }, { 1217, -1, -2, 5 } // 181	182	183	
			,
			{ 1217, 1, -2, 5 }, { 1217, 1, -1, 5 }, { 1217, 1, 0, 5 } // 184	185	186	
			,
			{ 1217, 1, -3, 5 }, { 6039, 0, -6, 0 }, { 6039, 1, -6, 0 } // 187	188	189	
			,
			{ 6042, 1, -7, 0 }, { 6042, 2, -6, 0 }, { 6054, -1, -7, 0 } // 190	191	192	
			,
			{ 6053, 3, -7, 0 }, { 6039, 2, -5, 0 }, { 6040, 1, -5, 0 } // 193	194	195	
			,
			{ 6043, 0, -5, 0 }, { 6039, -1, -5, 0 }, { 6053, 10, 0, 0 } // 196	197	198	
			,
			{ 6053, 4, -6, 0 }, { 6048, -5, -5, 0 }, { 6047, -4, -5, 0 } // 199	200	201	
			,
			{ 6048, -3, -5, 0 }, { 6048, 5, -5, 0 }, { 6047, 6, -5, 0 } // 202	203	204	
			,
			{ 6047, 7, -5, 0 }, { 1217, -1, -5, 5 }, { 1217, -1, -4, 5 } // 205	206	207	
			,
			{ 1217, -1, -3, 5 }, { 6053, 9, -1, 0 }, { 6053, 8, -5, 0 } // 208	209	210	
			,
			{ 6054, -2, -6, 0 }, { 1848, 1, -7, 0 }, { 1848, 0, -7, 0 } // 211	212	213	
			,
			{ 1851, -1, -8, 0 }, { 1217, 0, -4, 5 }, { 1217, 0, -6, 5 } // 214	215	216	
			,
			{ 935, -1, -3, 11 }, { 935, -1, -2, 11 }, { 935, -1, -4, 11 } // 217	218	219	
			,
			{ 935, 2, -5, 11 }, { 13567, 1, 0, 1 }, { 1851, 1, -8, 0 } // 220	221	222	
			,
			{ 1851, 0, -8, 0 }, { 935, -1, -1, 11 }, { 1848, -1, -7, 0 } // 223	224	225	
			,
			{ 6053, 2, -8, 0 }, { 6039, 3, -5, 0 }, { 4970, -3, 0, 1 } // 226	227	228	
			,
			{ 8109, -3, 0, 0 }, { 3256, 8, -1, 1 }, { 942, 1, -6, 0 } // 229	230	231	
			,
			{ 933, -2, -6, 0 }, { 938, -2, -6, 5 }, { 3207, 9, -2, 1 } // 232	233	234	
			,
			{ 3220, -3, -5, 1 }, { 3209, 9, -1, 1 }, { 3255, -5, -4, 1 } // 235	236	237	
			,
			{ 8109, 2, 0, 0 }, { 8109, 3, 0, 0 }, { 13452, 2, -1, 1 } // 238	239	240	
			,
			{ 4946, 6, -1, 1 }, { 4947, 7, -1, 1 }, { 4948, 7, -2, 1 } // 241	242	243	
			,
			{ 13488, 7, 0, 1 }, { 8108, -3, 0, 0 }, { 3255, 7, -4, 1 } // 244	245	246	
			,
			{ 13573, 2, 0, 1 }, { 3234, 5, -5, 1 }, { 3259, 2, -6, 1 } // 247	248	249	
			,
			{ 3259, 6, 0, 1 }, { 6041, 8, 0, 0 }, { 6046, 8, -2, 0 } // 250	251	252	
			,
			{ 6046, 8, -4, 0 }, { 6045, 8, -3, 0 }, { 4969, 2, -7, 0 } // 253	254	255	
			,
			{ 6043, 1, -4, 0 }, { 6043, 2, -4, 0 }, { 6043, 3, -4, 0 } // 256	257	258	
			,
			{ 1217, -1, -6, 5 }, { 935, 2, -3, 11 }, { 6042, -3, -3, 0 } // 259	260	261	
			,
			{ 6042, -2, -3, 0 }, { 6042, -1, -3, 0 }, { 6042, 4, -3, 0 } // 262	263	264	
			,
			{ 6042, 3, -3, 0 }, { 6042, 2, -3, 0 }, { 6042, 1, -3, 0 } // 265	266	267	
			,
			{ 6042, 0, -3, 0 }, { 6042, 6, -3, 0 }, { 6042, 5, -3, 0 } // 268	269	270	
			,
			{ 6041, 6, -2, 0 }, { 6042, -5, -3, 0 }, { 6041, 7, -2, 0 } // 271	272	273	
			,
			{ 6042, 7, -3, 0 }, { 6043, -5, -4, 0 }, { 13567, 1, -1, 1 } // 274	275	276	
			,
			{ 6040, 3, -1, 0 }, { 6039, 6, 0, 0 }, { 6041, -5, -2, 0 } // 277	278	279	
			,
			{ 6039, 4, 0, 0 }, { 6040, 2, -1, 0 }, { 6039, 1, 0, 0 } // 280	281	282	
			,
			{ 6039, 5, 0, 0 }, { 6039, -4, 0, 0 }, { 6040, 0, -1, 0 } // 283	284	285	
			,
			{ 6040, -1, -1, 0 }, { 6039, 3, 0, 0 }, { 6040, 7, -1, 0 } // 286	287	288	
			,
			{ 6040, 1, -1, 0 }, { 6039, -1, 0, 0 }, { 6040, 6, -1, 0 } // 289	290	291	
			,
			{ 6039, 2, 0, 0 }, { 6040, -3, -1, 0 }, { 6048, 1, -8, 0 } // 292	293	294	
			,
			{ 935, -1, 0, 11 }, { 6039, -5, 0, 0 }, { 3515, 4, -3, 1 } // 295	296	297	
			,
			{ 6040, -2, -1, 0 }, { 6057, 3, -6, 0 }, { 935, 2, -1, 11 } // 298	299	300	
			,
			{ 935, 2, 0, 11 }, { 935, 2, -2, 11 }, { 6043, -2, -4, 0 } // 301	302	303	
			,
			{ 6043, -1, -4, 0 }, { 6040, -5, -1, 0 }, { 1217, 0, -1, 5 } // 304	305	306	
			,
			{ 6041, -4, -2, 0 }, { 1217, 0, 0, 5 }, { 6043, 5, -4, 0 } // 307	308	309	
			,
			{ 6057, 8, -1, 0 }, { 6039, 0, 0, 0 }, { 6041, 2, -2, 0 } // 310	311	312	
			,
			{ 6041, 3, -2, 0 }, { 6043, 0, -4, 0 }, { 6039, -3, 0, 0 } // 313	314	315	
			,
			{ 6041, 4, -2, 0 }, { 6041, 5, -2, 0 }, { 6042, -4, -3, 0 } // 316	317	318	
			,
			{ 3514, -3, -4, 1 }, { 6054, 0, -8, 0 }, { 6043, -3, -4, 0 } // 319	320	321	
			,
			{ 6043, 7, -4, 0 }, { 3258, 2, -3, 1 }, { 6057, 9, 0, 0 } // 322	323	324	
			,
			{ 3257, -4, -2, 1 }, { 1217, 0, -2, 5 }, { 6060, -2, -5, 0 } // 325	326	327	
			,
			{ 935, 2, -4, 11 }, { 6039, 7, 0, 0 }, { 6040, 5, -1, 0 } // 328	329	330	
			,
			{ 6060, -1, -6, 0 }, { 6043, 4, -4, 0 }, { 6057, 4, -5, 0 } // 331	332	333	
			,
			{ 6040, -4, -1, 0 }, { 6040, 4, -1, 0 }, { 1217, 0, -3, 5 } // 334	335	336	
			,
			{ 6039, -2, 0, 0 }, { 938, 1, -6, 5 }, { 6043, 6, -4, 0 } // 337	338	339	
			,
			{ 6043, -4, -4, 0 }, { 6041, 0, -2, 0 }, { 6041, -2, -2, 0 } // 340	341	342	
			,
			{ 6041, -3, -2, 0 }, { 6041, 1, -2, 0 }, { 6041, -1, -2, 0 } // 343	344	345	
			,
			{ 3211, -10, 4, 0 }, { 6041, -7, 1, 0 }, { 6039, -6, 2, 0 } // 346	347	348	
			,
			{ 6040, -6, 1, 0 }, { 6052, -6, 5, 0 }, { 6052, -8, 1, 0 } // 349	350	351	
			,
			{ 6056, -8, 2, 0 }, { 6056, -7, 3, 0 }, { 4973, -6, 6, 1 } // 352	353	354	
			,
			{ 4962, -7, 3, 22 }, { 8109, -6, 1, 0 }, { 4945, -7, 2, 1 } // 355	356	357	
			,
			{ 3209, -8, 4, 1 }, { 13446, -6, 2, 28 }, { 13555, -7, 1, 0 } // 358	359	360	
			,
			{ 13451, -6, 3, 29 }, { 4947, -8, 2, 1 }, { 4946, -9, 2, 1 } // 361	362	363	
			,
			{ 4948, -8, 1, 1 }, { 4952, -9, 3, 1 }, { 4951, -10, 3, 1 } // 364	365	366	
			,
			{ 3308, -7, 4, 1 }, { 8108, -6, 1, 0 }, { 4943, -8, 3, 1 } // 367	368	369	
			,
			{ 6051, -6, 4, 0 }, { 4949, -8, 2, 16 }, { 4950, -8, 1, 16 } // 370	371	372	
			,
			{ 6059, -7, 2, 0 }, { 4944, -7, 3, 1 }, { 4970, -7, 3, 17 } // 373	374	375	
			,
			{ 13488, -7, 2, 27 }, { 13485, -7, 2, 22 }, { 3258, -6, 3, 1 } // 376	377	378	
			,
			{ 4967, -6, 2, 32 }, { 6059, -6, 3, 0 }, { 4960, -6, 2, 22 } // 379	380	381	
			,
			{ 6056, -6, 6, 0 }, { 4961, -7, 2, 25 }, { 4961, -7, 1, 18 } // 382	383	384	
			,
			{ 4955, -6, -2, 1 }, { 6041, -7, 0, 0 }, { 6040, -6, 0, 0 } // 385	386	387	
			,
			{ 6043, -6, -1, 0 }, { 6051, -8, 0, 0 }, { 6052, -6, -4, 0 } // 388	389	390	
			,
			{ 6051, -6, -3, 0 }, { 6054, -8, -1, 0 }, { 6054, -7, -2, 0 } // 391	392	393	
			,
			{ 6054, -6, -5, 0 }, { 4954, -6, -1, 1 }, { 3377, -7, -2, 1 } // 394	395	396	
			,
			{ 3259, -6, -3, 1 }, { 6060, -7, -1, 0 }, { 4948, -7, -1, 1 } // 397	398	399	
			,
			{ 6060, -6, -2, 0 }, { 4963, -7, 0, 17 }, { 4947, -7, 0, 1 } // 400	401	402	
		};


		public override BaseAddonDeed Deed
		{
			get
			{
				return new LargePondAddonDeed();
			}
		}

		[Constructable]
		public LargePondAddon()
		{
			for (int i = 0; i < m_AddOnSimpleComponents.Length / 4; i++)
				AddComponent(new AddonComponent(m_AddOnSimpleComponents[i, 0]), m_AddOnSimpleComponents[i, 1],
					m_AddOnSimpleComponents[i, 2], m_AddOnSimpleComponents[i, 3]);
		}

		public LargePondAddon(Serial serial) : base(serial)
		{
		}


		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0); // Version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}

	public class LargePondAddonDeed : BaseAddonDeed
	{
		public override BaseAddon Addon
		{
			get
			{
				return new LargePondAddon();
			}
		}

		[Constructable]
		public LargePondAddonDeed()
		{
			Name = "LargePond";
		}

		public LargePondAddonDeed(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0); // Version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}
}

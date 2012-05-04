﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Loki.IO;
using Loki.Data;

namespace Loki.Maple.Characters
{
    public class CharacterSPTable : Dictionary<byte, byte>
    {
        public Character Parent { get; private set; }

        public CharacterSPTable(Character parent)
            : base()
        {
            this.Parent = parent;
        }

        public short AvailableSP
        {
            get
            {
                short sp;

                if (this.GetSPType(this.Parent.Job) == ExtendedSPType.Regular)
                {
                    sp = this.Parent.AvailableSP;
                }
                else
                {
                    byte adv = (byte)((short)this.Parent.Job % 100 == 0 ? 1 : ((short)this.Parent.Job % 10) + 2);
                    sp = this[adv];
                }

                return sp;
            }
            set
            {
                if (this.GetSPType(this.Parent.Job) == ExtendedSPType.Regular)
                {
                    this.Parent.AvailableSP = value;
                }
                else
                {
                    byte adv = (byte)((short)this.Parent.Job % 100 == 0 ? 1 : ((short)this.Parent.Job % 10) + 2);

                    this[adv] = (byte)value;

                    if (this.Parent.IsInitialized)
                    {
                        this.Parent.UpdateStatistics(StatisticType.AvailableSP);
                    }
                }
            }
        }

        public short GetAvailableSPByJob(Job job)
        {
            short sp;

            if (this.GetSPType(job) == ExtendedSPType.Regular)
            {
                sp = this.Parent.AvailableSP;
            }
            else
            {
                byte adv = (byte)((short)job % 100 == 0 ? 1 : ((short)job % 10) + 2);
                sp = this[adv];
            }

            return sp;
        }

        public void SetAvailableSPByJob(Job job, short sp)
        {
            if (this.GetSPType(this.Parent.Job) == ExtendedSPType.Regular)
            {
                this.Parent.AvailableSP = sp;
            }
            else
            {
                byte adv = (byte)((short)job % 100 == 0 ? 1 : ((short)job % 10) + 2);

                this[adv] = (byte)sp;

                if (this.Parent.IsInitialized)
                {
                    this.Parent.UpdateStatistics(StatisticType.AvailableSP);
                }
            }
        }

        public void Load()
        {
            foreach (dynamic datum in new Datums("sptables").Populate("CharacterID = '{0}'", this.Parent.ID))
            {
                this.Add(datum.Advancement, datum.AvailableSP);
            }
        }

        public void Delete()
        {
            Database.Delete("sptables", "CharacterID = '{0}'", this.Parent.ID);
        }

        public void Save()
        {
            foreach (KeyValuePair<byte, byte> advance in this)
            {
                dynamic datum = new Datum("sptables");

                datum.CharacterID = this.Parent.ID;
                datum.Advancement = advance.Key;
                datum.AvailableSP = advance.Value;

                if (Database.Exists("sptables", "CharacterID = '{0}' AND Advancement = '{1}'", this.Parent.ID, advance.Key))
                {
                    datum.Update("CharacterID = '{0}' AND Advancement = '{1}'", this.Parent.ID, advance.Key);
                }
                else
                {
                    datum.Insert();
                }
            }
        }

        public ExtendedSPType GetSPType(Job job)
        {
            if (job == Job.Farmer || (short)job / 100 == 22)
            {
                return ExtendedSPType.Evan;
            }
            else if ((short)job / 1000 == 3)
            {
                return ExtendedSPType.Resistance;
            }
            else
            {
                return ExtendedSPType.Regular;
            }
        }

        public byte[] ToByteArray()
        {
            using (ByteBuffer buffer = new ByteBuffer())
            {
                if (this.GetSPType(this.Parent.Job) == ExtendedSPType.Regular)
                {
                    buffer.WriteShort(this.Parent.AvailableSP);
                }
                else
                {
                    buffer.WriteByte((byte)this.Count);

                    foreach (KeyValuePair<byte, byte> advance in this)
                    {
                        buffer.WriteByte((byte)(advance.Key));
                        buffer.WriteByte(advance.Value);
                    }
                }

                buffer.Flip();
                return buffer.GetContent();
            }
        }
    }
}
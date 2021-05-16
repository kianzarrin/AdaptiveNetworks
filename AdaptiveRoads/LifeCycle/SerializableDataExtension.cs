namespace AdaptiveRoads.LifeCycle
{
    using JetBrains.Annotations;
    using ICities;
    using System;
    using System.IO;
    using AdaptiveRoads.Manager;
    using KianCommons;
    using ColossalFramework.IO;

    [UsedImplicitly]
    public class SerializableDataExtension
        : SerializableDataExtensionBase
    {
        private const string DATA_ID = "AdaptiveRoads";
        static Version DataVersion => typeof(SerializableDataExtensionBase).VersionOf();

        public override void OnLoadData()
        {
            try {
                Log.Info("OnLoadData() ...", true);
                byte[] data = serializableDataManager.LoadData(DATA_ID);
                if (data == null) {
                    NetworkExtensionManager.Deserialize(null);
                } else {
                    var s = Serializer.Reader(data);
                    NetworkExtensionManager.Deserialize(s);
                }

                Log.Info("OnLoadData() was successful!", true);
            } catch (Exception ex) {
                ex.Log();
            }
        }

        public override void OnSaveData()
        {
            try {
                Log.Info("OnSaveData() ...", true);
                var man = NetworkExtensionManager.Instance;
                Assertion.AssertNotNull(DataVersion, "DataVersion");
                var s = Serializer.Writer(DataVersion, man.SerializationCapacity);
                man.Serialize(s);
                serializableDataManager.SaveData(DATA_ID, s.GetBytes());
                Log.Info("OnSaveData() was successful!", true);
            } catch (Exception ex) {
                ex.Log();
            }
        }
    }

    public class Serializer {
        public readonly Version Version;
        public readonly MemoryStream Stream;

        // make deserailizer
        Serializer(byte []data) {
            Stream = new MemoryStream(data, false);
            Version = ReadVersion();
        }

        // make serailizer
        Serializer(Version version, int capacity) {
            Stream = new MemoryStream(capacity);
            Assertion.AssertNotNull(version);
            Version = version;
            WriteVersion(version);
        }

        public static Serializer Reader(byte[] data) => new Serializer(data);
        public static Serializer Writer(Version version, int capacity) =>
            new Serializer(version, capacity);

        public byte[] GetBytes() => Stream.ToArray();


        // from ColossalFramework.IO.DataSerializer
        public int ReadInt32() {
            int num = (this.Stream.ReadByte() & 255) << 24;
            num |= (this.Stream.ReadByte() & 255) << 16;
            num |= (this.Stream.ReadByte() & 255) << 8;
            return num | (this.Stream.ReadByte() & 255);
        }
        public void WriteInt32(int value) {
            this.Stream.WriteByte((byte)(value >> 24 & 255));
            this.Stream.WriteByte((byte)(value >> 16 & 255));
            this.Stream.WriteByte((byte)(value >> 8 & 255));
            this.Stream.WriteByte((byte)(value & 255));
        }

        public uint ReadUInt32() {
            uint num = (uint)((uint)(this.Stream.ReadByte() & 255) << 24);
            num |= (uint)((uint)(this.Stream.ReadByte() & 255) << 16);
            num |= (uint)((uint)(this.Stream.ReadByte() & 255) << 8);
            return num | (uint)(this.Stream.ReadByte() & 255);
        }
        public void WriteUInt32(uint value) {
            this.Stream.WriteByte((byte)(value >> 24 & 255u));
            this.Stream.WriteByte((byte)(value >> 16 & 255u));
            this.Stream.WriteByte((byte)(value >> 8 & 255u));
            this.Stream.WriteByte((byte)(value & 255u));
        }

        public Version ReadVersion() =>
            new Version(ReadInt32(), ReadInt32(), ReadInt32(), ReadInt32());
        public void WriteVersion(Version version) {
            Assertion.AssertNotNull(version);
            WriteInt32(version.Major);
            WriteInt32(version.Minor);
            WriteInt32(version.Build);
            WriteInt32(version.Revision);
        }
    }


}

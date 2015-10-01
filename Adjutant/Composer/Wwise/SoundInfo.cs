﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Composer.IO;
using Adjutant.Library.Endian;

namespace Composer.Wwise
{
    public enum SoundPositionType : sbyte
    {
        Position2D,
        Position3D,
    }

    public enum SoundPositionSourceType : int
    {
        UserDefined = 2,
        GameDefined = 3
    }

    public enum SoundPlayType : int
    {
        SequenceStep,
        RandomStop,
        SequenceContinuous,
        RandomContinuous,
        SequenceStepPickPathAtStart,
        RandomStepPickPathAtStart
    }

    public enum SoundLimitMethod : sbyte
    {
        PerGameObject,
        Global
    }

    public enum SoundVirtualVoiceBehavior : sbyte
    {
        ContinuePlaying,
        Kill,
        SendToVirtualVoice
    }

    /// <summary>
    /// Represents information about a sound.
    /// </summary>
    public class SoundInfo
    {
        public SoundInfo(EndianReader reader)
        {
            OverrideParentEffectSettings = (reader.ReadByte() != 0);

            ReadEffects(reader);

            BusID = reader.ReadUInt32();
            ParentID = reader.ReadUInt32();

            OverrideParentPrioritySettings = (reader.ReadByte() != 0);
            OffsetPriorityAtMaxDistance = (reader.ReadByte() != 0);

            byte numParameters = reader.ReadByte();
            // TODO: actually store the parameter values instead of skipping over them
            reader.Skip(numParameters);
            reader.Skip(numParameters * 4);

            sbyte unknownCount = reader.ReadSByte();
            if (unknownCount > 0)
            {
                reader.Skip(unknownCount);
                reader.Skip(unknownCount * 8);
            }

            ReadPositioningInfo(reader);

            // Read auxiliary send settings
            OverrideParentGameDefinedAuxiliarySendSettings = (reader.ReadByte() != 0);
            UseGameDefinedAuxiliarySends = (reader.ReadByte() != 0);
            OverrideParentUserDefinedAuxiliarySendSettings = (reader.ReadByte() != 0);
            ReadUserDefinedAuxiliarySends(reader);

            bool unknown = (reader.ReadByte() != 0);
            /*if (unknown)
                reader.Skip(4);*/

            // Read voice settings
            LimitMethod = (SoundLimitMethod)reader.ReadSByte();
            VirtualVoiceBehavior = (SoundVirtualVoiceBehavior)reader.ReadSByte();
            OverrideParentPlaybackLimitSettings = (reader.ReadByte() != 0);
            OverrideParentVirtualVoiceSettings = (reader.ReadByte() != 0);

            ReadStateGroups(reader);
            ReadRTPCs(reader);

            reader.Skip(4); // I think this is part of the sound info data...
        }

        public bool OverrideParentEffectSettings { get; private set; }
        public byte EffectBypassMask { get; private set; }
        public uint[] EffectIDs { get; private set; }
        
        public uint BusID { get; private set; }
        public uint ParentID { get; private set; }

        public bool OverrideParentPrioritySettings { get; private set; }
        public bool OffsetPriorityAtMaxDistance { get; private set; }

        public bool HasPositioning { get; private set; }
        public SoundPositionType PositionType { get; private set; }
        public bool EnablePanner { get; private set; }
        public SoundPositionSourceType PositionSourceType { get; private set; }
        public uint AttenuationID { get; private set; }
        public bool EnableSpatialization { get; private set; }
        public SoundPlayType PlayType { get; private set; }
        public bool Loop { get; private set; }
        public uint TransitionTime { get; private set; }
        public bool FollowListenerOrientation { get; private set; }
        public bool UpdateEachFrame { get; private set; }

        public SoundPathPoint[] PathPoints { get; private set; }
        public SoundPath[] Paths { get; private set; }
        public SoundPathRandomRange[] PathRandomness { get; private set; }

        public bool OverrideParentGameDefinedAuxiliarySendSettings { get; private set; }
        public bool UseGameDefinedAuxiliarySends { get; private set; }
        public bool OverrideParentUserDefinedAuxiliarySendSettings { get; private set; }
        public bool HasUserDefinedAuxiliarySends { get; private set; }
        public uint[] AuxiliaryBusIDs { get; private set; }

        public SoundLimitMethod LimitMethod { get; private set; }
        public SoundVirtualVoiceBehavior VirtualVoiceBehavior { get; private set; }

        public bool OverrideParentPlaybackLimitSettings { get; private set; }
        public bool OverrideParentVirtualVoiceSettings { get; private set; }

        public StateGroup[] StateGroups { get; private set; }
        public RTPC[] RTPCs { get; private set; }

        private void ReadPositioningInfo(EndianReader reader)
        {
            HasPositioning = (reader.ReadByte() != 0);
            if (!HasPositioning)
                return;

            PositionType = (SoundPositionType)reader.ReadByte();
            if (PositionType == SoundPositionType.Position2D)
            {
                EnablePanner = (reader.ReadByte() != 0);
            }
            else
            {
                PositionSourceType = (SoundPositionSourceType)reader.ReadInt32();
                AttenuationID = reader.ReadUInt32();
                EnableSpatialization = (reader.ReadByte() != 0);

                if (PositionSourceType == SoundPositionSourceType.UserDefined)
                {
                    PlayType = (SoundPlayType)reader.ReadInt32();
                    Loop = (reader.ReadByte() != 0);
                    TransitionTime = reader.ReadUInt32();
                    FollowListenerOrientation = (reader.ReadByte() != 0);

                    ReadPaths(reader);
                }
                else if (PositionSourceType == SoundPositionSourceType.GameDefined)
                {
                    UpdateEachFrame = (reader.ReadByte() != 0);
                }
            }
        }

        private void ReadPaths(EndianReader reader)
        {
            ReadPathPoints(reader);
            ReadPathDefinitions(reader);
            ReadPathRandomness(reader);
        }

        private void ReadPathPoints(EndianReader reader)
        {
            int numPathPoints = reader.ReadInt32();
            PathPoints = new SoundPathPoint[numPathPoints];
            for (int i = 0; i < numPathPoints; i++)
                PathPoints[i] = new SoundPathPoint(reader);
        }

        private void ReadPathDefinitions(EndianReader reader)
        {
            int numPaths = reader.ReadInt32();
            Paths = new SoundPath[numPaths];
            for (int i = 0; i < numPaths; i++)
                Paths[i] = new SoundPath(reader);
        }

        private void ReadPathRandomness(EndianReader reader)
        {
            PathRandomness = new SoundPathRandomRange[Paths.Length];
            for (int i = 0; i < Paths.Length; i++)
                PathRandomness[i] = new SoundPathRandomRange(reader);
        }

        private void ReadUserDefinedAuxiliarySends(EndianReader reader)
        {
            HasUserDefinedAuxiliarySends = (reader.ReadByte() != 0);
            if (HasUserDefinedAuxiliarySends)
            {
                AuxiliaryBusIDs = new uint[4];
                for (int i = 0; i < 4; i++)
                    AuxiliaryBusIDs[i] = reader.ReadUInt32();
            }
            else
            {
                AuxiliaryBusIDs = new uint[0];
            }
        }

        private void ReadEffects(EndianReader reader)
        {
            byte numEffects = reader.ReadByte();
            if (numEffects > 0)
            {
                EffectBypassMask = reader.ReadByte();
                EffectIDs = new uint[numEffects];
                for (byte i = 0; i < numEffects; i++)
                {
                    reader.Skip(1); // Effect index, useless
                    EffectIDs[i] = reader.ReadUInt32();
                    reader.Skip(2);
                }
            }
            else
            {
                EffectIDs = new uint[0];
            }
        }

        private void ReadStateGroups(EndianReader reader)
        {
            int numStateGroups = reader.ReadInt32();
            StateGroups = new StateGroup[numStateGroups];
            for (int i = 0; i < numStateGroups; i++)
                StateGroups[i] = new StateGroup(reader);
        }

        private void ReadRTPCs(EndianReader reader)
        {
            short numRtpcs = reader.ReadInt16();
            RTPCs = new RTPC[numRtpcs];
            for (short i = 0; i < numRtpcs; i++)
                RTPCs[i] = new RTPC(reader);
        }
    }
}

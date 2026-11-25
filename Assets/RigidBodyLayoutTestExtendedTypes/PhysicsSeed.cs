using System;
using Unity.Entities;

namespace Unity.Physics
{
    public struct PhysicsSeed : IComponentData, IEquatable<PhysicsSeed>
    {
        public const int TypeSize = sizeof(short) + sizeof(ushort);
        public static readonly PhysicsSeed MaxValue = new PhysicsSeed
        {
            playbackIndex = short.MaxValue,
            globalInvPriority = ushort.MaxValue,
        };

        ///important:
        /// This value may be serialized with value 0..255, marking an offset from the TrackEntityRef.PlaybackIndex.
        /// When setting the value initially, add the serialized value to get the intended playbackIndex. So don't overwrite, just += the value.
        /// See TrackRecorderOrigin.physicsSeedPlaybackIndexWidth>
        public short playbackIndex;
        public ushort globalInvPriority;

        public static sbyte Compare(PhysicsSeed lhs, PhysicsSeed rhs)
        {
            if (lhs.globalInvPriority != rhs.globalInvPriority)
            {
                return lhs.globalInvPriority > rhs.globalInvPriority
                    ? (sbyte)-1
                    : (sbyte)1;
            }

            return lhs.playbackIndex == rhs.playbackIndex
                ? (sbyte)0
                : lhs.playbackIndex > rhs.playbackIndex
                    ? (sbyte)-1
                    : (sbyte)1;
        }

        public static void IsBigger(PhysicsSeed lhs, PhysicsSeed rhs, out bool result)
        {
            result = rhs.globalInvPriority > lhs.globalInvPriority || (rhs.globalInvPriority == lhs.globalInvPriority && rhs.playbackIndex > lhs.playbackIndex);
        }

        public bool Equals(PhysicsSeed other)
        {
            return globalInvPriority == other.globalInvPriority && playbackIndex == other.playbackIndex;
        }

        public override bool Equals(object obj)
        {
            return obj is PhysicsSeed other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(playbackIndex, globalInvPriority);
        }
    }
}